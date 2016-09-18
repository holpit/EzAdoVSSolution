using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EzAdo.Extensions;
using EzAdo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using EzAdo.Converters;

namespace EzAdo
{
    /// <summary>Procedure factory queries db for stored procedures, and UDT's available based on the connection strings provided.  It then builds out the objects that will eventually be cloned for use.</summary>
    public static class ProcedureFactory
    {
        #region |Private Collections|

        private static Dictionary<string, Procedure> _procedures;
        private static Dictionary<string, string> _connectionStrings;
        private static Dictionary<string, ReaderColumnToObjectPropertyMapping[]> _readerColumnToObjectPropertyMappings;
        private static Dictionary<string, DataTable> _dataTables;
        private static Dictionary<string, ObjectPropertyToParameterNameMapping[]> _objectPropertyToParameterNameMappings;
        private static DataTable _schemas;

        #endregion

        #region |Constructors|

        /// <summary>Calls init()</summary>
        static ProcedureFactory()
        {
            init();
        }

        #endregion

        #region |Access Methods|
        /// <summary>Clones a cached Procedure where the procedure is identified by the combination of schema and name.</summary>
        /// <param name="specificSchema">Left side of Sql stored procedure [schema].[PROCEDURE_NAME].</param>
        /// <param name="specificName">Right side of Sql stored procedure [schema].[PROCEDURE_NAME].</param>
        public static Procedure GetProcedure(string specificSchema, string specificName)
        {
            string procedureName = $"[{specificSchema}].[{specificName}]";
            if(!_procedures.ContainsKey(procedureName))
            {
                throw new IndexOutOfRangeException($"No Procedure was found by the converted name {procedureName}");
            }
            return _procedures[procedureName].Clone();
        }

        /// <summary>Clones a cached Procedure where the procedure is identified by the combination of method, schema and name in the format schema.METHOD_PROCEDURE_NAME.</summary>
        /// <param name="method">GET|PUT|POST|DELETE.</param>
        /// <param name="specificSchema">Left side of sql stored procedure [schema].[METHOD_PROCEDURE_NAME].</param>
        /// <param name="specificName">procedure part of sql stored procedure [schema].[METHOD_PROCEDURE_NAME].</param>
        public static Procedure GetRestProcedure(string method, string specificSchema, string specificName)
        {
            return GetProcedure(specificSchema, $"{method}_{specificName.ToUnderscore()}");
        }

        /// <summary>Gets the ReaderColumnToObjectPropertyMapping[] for the given reader and procedure.</summary>
        /// <typeparam name="T">Type of object the reader is mapping to.</typeparam>
        /// <param name="procedureName">Left side of sql stored procedure [schema].[PROCEDURE_NAME].</param>
        /// <param name="rdr">An SqlDataReader that was populate via the procedure call represented by schema and name.</param>
        public static ReaderColumnToObjectPropertyMapping[] GetReaderColumnToObjectPropertyMappings<T>(string procedureName, SqlDataReader rdr)
        {
            string mappingName = $"[{procedureName}].T{typeof(T).ToString()}";

            if (!_readerColumnToObjectPropertyMappings.ContainsKey(mappingName))
            {
                var properties = typeof(T).GetProperties();
                List<ReaderColumnToObjectPropertyMapping> mappings = new List<ReaderColumnToObjectPropertyMapping>();

                for (int idx = 0, cnt = rdr.FieldCount; idx != cnt; idx++)
                {
                    string columnName = rdr.GetName(idx);
                    string propertyName = columnName.ToProperCase();

                    foreach (var property in properties)
                    {
                        if (property.Name == propertyName)
                        {
                            mappings.Add(new ReaderColumnToObjectPropertyMapping(idx, property));
                            break;
                        }
                    }
                }
                _readerColumnToObjectPropertyMappings.Add(mappingName, mappings.ToArray());
            }
            return _readerColumnToObjectPropertyMappings[mappingName];
        }

        /// <summary>Gets the ObjectPropertyToParameterNameMapping[] for the given object type and procedure.</summary>
        /// <typeparam name="T">Type of object the procedure parameters are mapping to.</typeparam>
        /// <param name="procedureName">Left side of sql stored procedure [schema].[PROCEDURE_NAME].</param>
        public static ObjectPropertyToParameterNameMapping[] GetParameterMapping<T>(string procedureName)
        {
            string mappingName = $"{procedureName}].T{typeof(T).ToString()}";

            if (!_objectPropertyToParameterNameMappings.ContainsKey(mappingName))
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                Procedure proc = _procedures[procedureName];
                
                List<ObjectPropertyToParameterNameMapping> mappings = new List<ObjectPropertyToParameterNameMapping>();

                for (int idx = 0, len=properties.Length; idx != len; idx++)
                {
                    PropertyInfo property = properties[idx];
                    string parameterName = $"@{property.Name.ToUnderscore()}";
                    if (proc.ContainsParameter(parameterName))
                    {
                        mappings.Add(new ObjectPropertyToParameterNameMapping(parameterName, property));
                    }
                }
                _objectPropertyToParameterNameMappings.Add(mappingName, mappings.ToArray());
            }
            return _objectPropertyToParameterNameMappings[mappingName];
        }

        /// <summary>Clones a cached DataTable where the data table is identified by the combination of schema and name in the format SCHEMA.USER_DEFINED_TABLE_TYPE.</summary>
        /// <param name="dataTableName"> Fully qualified UDT [schema].[USER_DEFINED_TABLE_TYPE]. </param>
        public static DataTable GetDataTable(string dataTableName)
        {
            DataTable source = _dataTables[dataTableName];
            DataTable target = new DataTable();
            foreach(DataColumn sourceColumn in source.Columns)
            {
                DataColumn targetColumn = new DataColumn(sourceColumn.ColumnName, sourceColumn.DataType);
                targetColumn.AllowDBNull = sourceColumn.AllowDBNull;
                targetColumn.MaxLength = sourceColumn.MaxLength;
                target.Columns.Add(targetColumn);
            }
            return target;
        }

        /// <summary>Clears all cached data and repopulates the collections.</summary>
        public static void Rebuild()
        {
            init();
        }

        #endregion

        #region |Private Methods|

        //Initializes the collections and builds out objects for cloning
        private static void init()
        {
            _procedures = new Dictionary<string, Procedure>();
            _readerColumnToObjectPropertyMappings = new Dictionary<string, ReaderColumnToObjectPropertyMapping[]>();
            _objectPropertyToParameterNameMappings = new Dictionary<string, ObjectPropertyToParameterNameMapping[]>();

            getConnectionStrings();

            var JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            List<dto.Procedure> dtoProcedures = JsonConvert.DeserializeObject<List<dto.Procedure>>(getJsonProcedures(), JsonSerializerSettings);
            loadProcedures(dtoProcedures);

            loadDataTables();
        }

        //Populates _connectionStrings and _schemas 
        private static void getConnectionStrings()
        {
            _connectionStrings = new Dictionary<string, string>();
            ConfigurationManager.RefreshSection("connectionStrings");

            _schemas = new DataTable();
            _schemas.Columns.Add("SCHEMA_NAME", typeof(string));

            foreach (ConnectionStringSettings connectionStringSetting in ConfigurationManager.ConnectionStrings)
            {
                DataRow newRow = _schemas.NewRow();
                newRow["SCHEMA_NAME"] = connectionStringSetting.Name;
                _schemas.Rows.Add(newRow);
                _connectionStrings.Add(connectionStringSetting.Name, connectionStringSetting.ConnectionString);
            }
        }

        //Gets the Json result for the ezado.PROCEDURES call.
        private static string getJsonProcedures()
        {
            StringBuilder bldr = new StringBuilder();
            
            using (SqlConnection cnn = new SqlConnection(_connectionStrings["ezado"]))
            {
                cnn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "ezado.PROCEDURES";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SCHEMAS", _schemas);
                    cmd.Connection = cnn;

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        bldr.Append(rdr.GetString(0));
                    }
                }
            }
            return bldr.ToString();
        }

        //Loads the DataTables from the [ezado].[USER_DEFEFINED_TABLES] call.
        private static void loadDataTables()
        {
            _dataTables = new Dictionary<string, DataTable>();

            using (SqlConnection cnn = new SqlConnection(_connectionStrings["ezado"]))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "[ezado].[USER_DEFEFINED_TABLES]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SCHEMAS", _schemas);
                    cmd.Connection = cnn;
                    cnn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    string specificSchema = "";
                    string specificName = "";
                    string columnName = "";
                    string dataType = "";
                    bool allowDBNull = false;
                    int characterMaxLength = 0;

                    DataTable currentDataTable = null;
                    while (rdr.Read())
                    {
                        if(rdr.GetString(0) != specificSchema || rdr.GetString(1) != specificName)
                        {
                            specificSchema = rdr.GetString(0);
                            specificName = rdr.GetString(1);
                            currentDataTable = new DataTable(specificName, specificSchema);
                            _dataTables.Add($"[{specificSchema}].[{specificName}]", currentDataTable);
                        }
                        columnName = rdr.GetString(2);
                        dataType = rdr.GetString(3);
                        Type type = SqlDataTypeMappings.SqlDataTypeToCSharpType(dataType);
                        allowDBNull = rdr.GetBoolean(4);
                        characterMaxLength = rdr.GetInt16(5);

                        DataColumn column = new DataColumn(columnName, type);
                        column.AllowDBNull = rdr.GetBoolean(4);
                        if (dataType.Contains("char"))
                        {
                            column.MaxLength = rdr.GetInt16(5);
                        }
                        currentDataTable.Columns.Add(column);
                    }
                }
            }
        }

        //Populates the _procedures dictionary.
        private static void loadProcedures(List<dto.Procedure> dtoProcs)
        {
            //Logic here is
            //  1. Procedures annotated with always encrypted get appended with Column Encryption Setting=enabled";
            //  2. Procedures always return a value, this value is not present in the schema views so it is manually added

            foreach (dto.Procedure dtoProc in dtoProcs)
            {
                // Set the connection string for the procedure
                // Procedures annotated with *Always Encrypted* need the column encryption setting 
                if (dtoProc.IsAlwaysEncrypted)
                {
                    dtoProc.ConnectionString = _connectionStrings[dtoProc.SpecificSchema] + ";Column Encryption Setting=enabled";
                }
                else
                {
                    dtoProc.ConnectionString = _connectionStrings[dtoProc.SpecificSchema];
                }

                //Procedures have return values by default, these return values do not show up in the system views
                //Adding that value here eliminates guessing if it has been added elsewhere
                if (dtoProc.RoutineType != "FUNCTION")
                {
                    if (dtoProc.Parameters == null) dtoProc.Parameters = new List<dto.Parameter>();
                    dtoProc.Parameters.Insert(0, new dto.Parameter() { IsResult = "YES", ParameterName = "@RETURN_VALUE", DataType = "int", ParameterMode = "OUT" });
                }

                //Using the converter to build a usable procedure Model.Procedure
                string procedureName = $"[{dtoProc.SpecificSchema}].[{dtoProc.SpecificName}]";
                Procedure proc = Converters.DtoProcedureToModelProcedure.Convert(dtoProc);

                _procedures.Add(procedureName, proc);
            }
        }

        #endregion
    }
}