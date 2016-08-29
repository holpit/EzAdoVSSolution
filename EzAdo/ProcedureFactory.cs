using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EzAdo.Extensions;
using EzAdo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EzAdo
{
    public static class ProcedureFactory
    {
        #region |Private Collections|

        //All the procedures that were loaded from ezado.PROCEDURES
        private static Dictionary<string, Procedure> _procedures;

        //The set of connection strings from app config
        private static Dictionary<string, string> _connectionStrings;

        //Cache of result column -> object property - used to  set object values from result sets
        private static Dictionary<string, ReaderColumnToObjectPropertyMapping[]> _readerColumnToObjectPropertyMappings;

        //All the data tables that were loaded from ezado.USER_DEFINED_TABLES
        private static Dictionary<string, DataTable> _dataTables;

        //Cache of object.property -> parameter - used to set parameter values from object
        private static Dictionary<string, ObjectPropertyToParameterNameMapping[]> _objectPropertyToParameterNameMappings;
        
        #endregion

        #region |Constructors|

        /// <summary>
        /// Calls init()
        /// </summary>
        static ProcedureFactory()
        {
            init();
        }

        #endregion

        #region |Access Methods|

        /// <summary>
        /// Clones a cached Procedure where the procedure is identified by the combination of schema and name.
        /// </summary>
        /// <param name="specificSchema">Left side of sql stored procedure [schema].[procedure]</param>
        /// <param name="specificName">Right side of sql stored procedure [schema].[procedure]</param>
        /// <returns>REST-SQL.Procedure</returns>
        public static Procedure GetProcedure(string specificSchema, string specificName)
        {
            string procedureName = $"[{specificSchema}].[{specificName}]";
            if(!_procedures.ContainsKey(procedureName))
            {
                throw new IndexOutOfRangeException($"No Procedure was found by the converted name {procedureName}");
            }
            Procedure source = _procedures[procedureName];
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (Procedure)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Clones a cached RestProcedure where the procedure is identified by the combination of method, schema and name in the format METHOD_SCHEMA.NAME
        /// </summary>
        /// <param name="method">GET|PUT|POST|DELETE</param>
        /// <param name="specificSchema">Left side of sql stored procedure [schema].[method_procedure]</param>
        /// <param name="specificName">procedure part of sql stored procedure [schema].[method_procedure]</param>
        /// <returns>REST-SQL.RestProcedure</returns>
        public static Procedure GetRestProcedure(string method, string specificSchema, string specificName)
        {
            return GetProcedure(specificSchema, $"{method}_{specificName.ToUnderscore()}");
        }


        /// <summary>
        /// Gets the ReaderColumnToObjectPropertyMapping[] for the given reader and procedure.
        /// </summary>
        /// <typeparam name="T">Type of object the reader is mapping to.</typeparam>
        /// <param name="specificSchema">Left side of sql stored procedure [schema].[procedure]</param>
        /// <param name="specificName">Right side of sql stored procedure [schema].[procedure]</param>
        /// <param name="rdr">An SqlDataReader that was populate via the procedure call represented by schmema and name</param>
        /// <returns>The generated or cached array of column mappings</returns>
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


        /// <summary>
        /// Gets the ObjectPropertyToParameterNameMapping[] for the given object type and procedure.
        /// </summary>
        /// <typeparam name="T">Type of object the procedure parameters are mapping to.</typeparam>
        /// <param name="specificSchema">Left side of sql stored procedure [schema].[procedure]</param>
        /// <param name="specificName">Right side of sql stored procedure [schema].[procedure]</param>
        /// <returns>The generated or cached array of ObjectPropertyToParameterNameMapping</returns>
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


        /// <summary>
        /// Clones a cached DataTable where the data table is identified by the combination of schema and name in the format SCHEMA.USER_DEFINED_TABLE_TYPE
        /// </summary>
        /// <param name="specificSchema">Left side of sql user defined table type [schema].[USER_DEFINED_TABLE_TYPE]</param>
        /// <param name="specificName">Right side of sql user defined table type [schema].[USER_DEFINED_TABLE_TYPE]</param>
        /// <returns>System.Data.DataTable</returns>
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

        /// <summary>
        /// Clears all cached data and repopulates the collections
        /// </summary>
        public static void Rebuild()
        {
            init();
        }

        #endregion

        #region |Private Methods|

        /// <summary>
        /// Initializes the collections
        /// </summary>
        private static void init()
        {
            _procedures = new Dictionary<string, Procedure>();
            _readerColumnToObjectPropertyMappings = new Dictionary<string, ReaderColumnToObjectPropertyMapping[]>();
            _objectPropertyToParameterNameMappings = new Dictionary<string, ObjectPropertyToParameterNameMapping[]>();

            getConnectionStrings();

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            List<dto.Procedure> dtoProcedures = JsonConvert.DeserializeObject<List<dto.Procedure>>(getJsonProcedures(), jsonSerializerSettings);
            loadProcedures(dtoProcedures);

            loadDataTables();
        }

        /// <summary>
        /// Populates _connectionStrings
        /// </summary>
        private static void getConnectionStrings()
        {
            _connectionStrings = new Dictionary<string, string>();
            ConfigurationManager.RefreshSection("connectionStrings");

            foreach (ConnectionStringSettings connectionStringSetting in ConfigurationManager.ConnectionStrings)
            {
                _connectionStrings.Add(connectionStringSetting.Name, connectionStringSetting.ConnectionString);
            }
        }

        /// <summary>
        /// Gets the json result for the ezado.PROCEDURES call
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Loads the DataTables from the [ezado].[USER_DEFEFINED_TABLES] call
        /// </summary>
        private static void loadDataTables()
        {
            _dataTables = new Dictionary<string, DataTable>();

            using (SqlConnection cnn = new SqlConnection(_connectionStrings["ezado"]))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "[ezado].[USER_DEFEFINED_TABLES]";
                    cmd.CommandType = CommandType.StoredProcedure;
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
                        allowDBNull = rdr.GetBoolean(4);
                        characterMaxLength = rdr.GetInt16(5);

                        DataColumn column = new DataColumn(rdr.GetString(2), rdr.GetString(3).ToCSharpTypeFromSqlDataType());
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

        /// <summary>
        /// Populates the _procedures dictionary
        /// </summary>
        /// <param name="tempProcedures"></param>
        private static void loadProcedures(List<dto.Procedure> dtoProcs)
        {
            //Need to do make adjustments to procedures prior to converting

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