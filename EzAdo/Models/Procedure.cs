using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EzAdo.Extensions;
using System.Linq;
using System.IO;

namespace EzAdo.Models
{
    /// <summary>A Procedure in the context of EzAdo is definition and wrapper around an MSSQL stored procedure including the in, out, and return parameters.  The ProcedureFactory.init() call utilizes [ezado].[PROCEDURES] to retrieve the procedures that align with the schema's provided by the connection strings.</summary>
    public class Procedure
    {
        #region |Private variables|

        /// <summary>   The connection string that this procedure requires.  Procedures annotated with *Always Encrypted* will append the connection with "Column Encryption Setting=enabled". </summary>
        private string _connectionString;

        /// <summary>   Name of the procedure in the format [specificSchema].[SPECIFIC_NAME]. </summary>
        private string _procedureName;

        /// <summary>   Stored procedure annotation *Returns Json*. Informs the execution methods that the procedure returns Json.</summary>
        private bool _isJsonResult;

        /// <summary>   Stored procedure annotation *Single Result*.  Informs the execution methods that the result is a single entity allowing Execute&lt;T&gt; vs. ExecuteList&lt;T&gt;.  </summary>
        private bool _isSingleResult;

        /// <summary>   Stored procedure annotation *Non Query*. Informs the execution methods that the procedure does not return a value. </summary>
        private bool _isNonQuery;

        /// <summary>   Contains the definition of all the procedures associated parameters. </summary>
        private Dictionary<string, Parameter> _parameters;

        /// <summary>   A ParameterNameMap is a dictionary that maps a parameter name in the format @PARAMETER_NAME to any of the followin variations
        ///             FIRST_NAME, FirstName, and firstName.  The mapping prevents case conversion on every set or get. </summary>
        private Dictionary<string, string> _parameterNameMap;

        #endregion

        #region |Loaders|

        //Checks to see if the newton soft token provides a value.
        private bool isTokenValue(JsonToken tkn)
        {
            return tkn == JsonToken.Boolean ||
                tkn == JsonToken.Bytes ||
                tkn == JsonToken.Date ||
                tkn == JsonToken.Float ||
                tkn == JsonToken.Integer ||
                tkn == JsonToken.String;
        }


        /// <summary>Sets the parameter values from the query parameters. Note that multiple values (s=1 s=2 s=3) that evaluate to a list are not supported. For multiple records use the LoadFromJson method.</summary>
        /// <param name="keyValuePairs">Query string parameters.</param>
        public void LoadFromQuery(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kp in keyValuePairs)
            {
                SetValue<object>(kp.Key, kp.Value);
            }
        }


        /// <summary>Sets the parameters values from Json. Note that when the Json provides a list the parameter must have a user defined table type as the parameter.</summary>
        /// <param name="Json">Json string.</param>
        public void LoadFromJson(string Json)
        {
            using (var rdr = new JsonTextReader(new StringReader(Json)))
            {
                string propertyName = null;
                object propertyValue = null;

                while (rdr.Read())
                {
                    JsonToken tkn = rdr.TokenType;

                    if (tkn == JsonToken.PropertyName)
                    {
                        propertyName = rdr.Value.ToString();
                        continue;
                    }
                    if (isTokenValue(tkn))
                    {
                        propertyValue = rdr.Value;
                        SetValue<object>(propertyName, propertyValue);
                        propertyName = null;
                        propertyValue = null;
                        continue;
                    }
                    if (tkn == JsonToken.StartArray)
                    {
                        Parameter parameter = GetParameter(propertyName);
                        DataTable dataTable = ProcedureFactory.GetDataTable(parameter.DataTypeName);
                        DataRow dataRow = null;

                        string columnName = null;
                        object columnValue = null;

                        while (rdr.Read())
                        {
                            JsonToken child = rdr.TokenType;

                            if (child == JsonToken.StartObject)
                            {
                                dataRow = dataTable.NewRow();
                                continue;
                            }

                            if (child == JsonToken.PropertyName)
                            {
                                columnName = rdr.Value.ToString().ToUnderscore();
                                continue;
                            }
                            if (isTokenValue(child))
                            {
                                columnValue = rdr.Value;
                                dataRow[columnName] = columnValue;
                                continue;
                            }
                            if (child == JsonToken.EndObject)
                            {
                                dataTable.Rows.Add(dataRow);
                                continue;
                            }
                            if (child == JsonToken.EndArray)
                            {
                                SetValue<object>(propertyName, dataTable);
                                propertyName = null;
                                propertyValue = null;
                                break;
                            }
                        }

                    }
                }
            }
        }


        /// <summary>Utilizes ObjectPropertyToParameterNameMapping to set the values of the parameters in the procedure</summary>
        /// <typeparam name="T">Type of object containing the values.</typeparam>
        /// <param name="value">Object containing values.</param>
        public void LoadFromObject<T>(T value)
        {
            ObjectPropertyToParameterNameMapping[] mappings = ProcedureFactory.GetParameterMapping<T>(_procedureName);
            for (int idx = 0, len = mappings.Length; idx != len; idx++)
            {
                mappings[idx].SetValue(value, this);
            }
        }


        #endregion

        #region |Constructor - Clone|

        /// <summary>Object Constructor. </summary>
        /// <param name="connectionString">The connection string that this procedure requires.  Procedures annotated with *Always Encrypted* will append the connection with "Column Encryption Setting=enabled"</param>
        /// <param name="procedureName">Name of the procedure in the format [specificSchema].[PROCEDURE_NAME].</param>
        /// <param name="isJsonResult">Stored procedure annotation *Returns Json*. Informs the execution methods that the procedure returns Json.</param>
        /// <param name="isSingleResult">Stored procedure annotation *Single Result*.  Informs the execution methods that the result is a single entity allowing Execute&lt;T&gt; vs. ExecuteList&lt;T&gt;.</param>
        /// <param name="isNonQuery">Stored procedure annotation *Non Query*. Informs the execution methods that the procedure does not return a value.</param>
        /// <param name="parameters">Collection of parameters associated with the stored procedure.</param>
        /// <param name="parameterNameMap">A ParameterNameMap is a dictionary that maps a parameter name in the format @PARAMETER_NAME to any of the following variations FIRST_NAME, FirstName, and firstName.  The mapping prevents case conversion on every set or get.</param>
        public Procedure(string connectionString, string procedureName, bool isJsonResult, bool isSingleResult, bool isNonQuery, Dictionary<string, Models.Parameter> parameters, Dictionary<string, string> parameterNameMap)
        {
            _connectionString = connectionString;
            _procedureName = procedureName;
            _isJsonResult = isJsonResult;
            _isSingleResult = isSingleResult;
            _isNonQuery = isNonQuery;
            _parameters = parameters;
            _parameterNameMap = parameterNameMap;
        }

        /// <summary>Creates a deep copy of the procedure.</summary>
        public Procedure Clone()
        {
            Dictionary<string, string> parameterNameMap = new Dictionary<string, string>();
            Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();

            foreach (KeyValuePair<string, string> kp in _parameterNameMap)
            {
                parameterNameMap.Add(kp.Key, kp.Value);
            }
            foreach (KeyValuePair<string, Parameter> kp in _parameters)
            {
                parameters.Add(kp.Key, kp.Value.Clone());
            }
            return new Procedure(
                _connectionString,
                _procedureName,
                _isJsonResult,
                _isSingleResult,
                _isNonQuery,
                parameters,
                parameterNameMap);
        }

        #endregion

        #region |Procedure Execution|

        /// <summary>For *Returns Json* procedures where the raw Json result is the desired return type.</summary>
        public string ExecuteJson()
        {
            if (!_isJsonResult) throw new InvalidOperationException("Procedure is not annotated with *Returns Json*.");
            if (_isNonQuery) throw new InvalidOperationException("Procedures is annotated with *Non Query*.");

            checkValues();

            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = buildCommand(cnn))
                {
                    cnn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        StringBuilder bldr = new StringBuilder();
                        while (rdr.Read())
                        {
                            bldr.Append(rdr.GetString(0));
                        }
                        rdr.Close();
                        setReturnValues(cmd);
                        cnn.Close();
                        return bldr.ToString();
                    }
                }
            }
        }

        /// <summary>For *Returns Json* procedures where an object of T is the desired return type.</summary>
        /// <typeparam name="T">Destination type of deserialization.</typeparam>
        public T ExecuteJson<T>()
        {
            if (!_isSingleResult) throw new InvalidOperationException("Procedures is not annotated with *Single Result*");
            string Json = ExecuteJson();
            var JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.DeserializeObject<T>(Json);
        }

        /// <summary>For *Returns Json* procedures where a List&lt;T&gt; is the desired return type.</summary>
        /// <typeparam name="T">Destination type of deserialization.</typeparam>
        public List<T> ExecuteJsonList<T>()
        {
            if (_isSingleResult) throw new InvalidOperationException("Procedures is annotated with *Single Result*.");
            string Json = ExecuteJson();
            var JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.DeserializeObject<List<T>>(Json);
        }

        /// <summary>For result set procedures where the desired return type is a Data Table.</summary>
        public DataTable ExecuteReaderDataTable()
        {
            if (_isJsonResult) throw new InvalidOperationException("Procedure is annotated with *Returns Json*");
            if (_isNonQuery) throw new InvalidOperationException("Procedures is annotated with *Non Query*");

            checkValues();

            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = buildCommand(cnn))
                {
                    cnn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        DataTable result = new DataTable();
                        result.Load(rdr);
                        rdr.Close();
                        setReturnValues(cmd);
                        cnn.Close();
                        return result;
                    }

                }
            }
        }

        /// <summary>For result set procedures where an object of T is the desired return type.  Uses implicit or explicit mapping.</summary>
        /// <typeparam name="T">Destination type of mapping.</typeparam>
        public T ExecuteReader<T>()
        {
            if (_isJsonResult) throw new InvalidOperationException("Procedures is annotated with *Returns Json* - use the ExecuteJson method instead");
            if (!_isSingleResult) throw new InvalidOperationException("Procedure requires annotation of *Single Result* to return a single object");
            List<T> lst = buildList<T>();
            if (lst.Count == 1)
            {
                return lst[0];
            }
            return default(T);
        }

        /// <summary>For result set procedures where a List of object of T is the desired return type.  Uses implicit or explicit mapping.</summary>  
        /// <typeparam name="T">Destination type of mapped objects.</typeparam>
        public List<T> ExecuteReaderList<T>()
        {
            if (_isJsonResult) throw new InvalidOperationException("Procedures is annotated with *Returns Json* - use the ExecuteJson method instead");
            if (_isSingleResult) throw new InvalidOperationException("Procedures is annotated with *Single Result*");
            return buildList<T>();
        }

        /// <summary>Utilizes ExecuteReader and serializes result to Json.</summary>
        /// <typeparam name="T">Destination type of execute reader.</typeparam>
        public string ExecuteReaderAsJson<T>()
        {
            T temp = ExecuteReader<T>();
            var JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(temp);
        }

        /// <summary>Utilizes ExecuteReaderList serializes result to Json.</summary>
        /// <typeparam name="T">Destination type of execute reader.</typeparam>
        public string ExecuteReaderListAsJson<T>()
        {
            List<T> temp = buildList<T>();
            var JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(temp);
        }

        /// <summary>Executes procedure with no results.  Sets the output parameters and return value.</summary>
        public void ExecuteNonQuery()
        {
            if (!_isNonQuery) throw new InvalidOperationException("Procedure requires annotation of *Non Query*.");

            checkValues();

            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = buildCommand(cnn))
                {
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                    setReturnValues(cmd);
                    cnn.Close();
                }
            }
        }

        //Builds the SqlCommand based on the Parameter Definitions and values, binds the connection passed in to the command.
        private SqlCommand buildCommand(SqlConnection cnn)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = _procedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (Parameter parameter in _parameters.Values)
            {
                cmd.Parameters.Add(parameter.ToSqlParameter());
            }

            cmd.Connection = cnn;

            return cmd;
        }

        //Builds List of T either via reflection or supported IExplicitMap.
        private List<T> buildList<T>()
        {
            checkValues();

            List<T> result = null;

            bool buildFromExplicit = false;
            T temp = Activator.CreateInstance<T>();
            if (typeof(T).GetInterfaces().Contains(typeof(IExplicitMap)))
            {
                if (((IExplicitMap)temp).SupportsMap(_procedureName))
                {
                    buildFromExplicit = true;
                }
            }

            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = buildCommand(cnn))
                {
                    cnn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if(buildFromExplicit)
                        {
                            result = buildListExplicit<T>(rdr);
                        }
                        else
                        {
                            result = buildListReflection<T>(rdr);
                        }
                        rdr.Close();
                        setReturnValues(cmd);
                        cnn.Close();
                    }
                }
            }
            return result;
        }

        //Builds a list when the object implements IExplicitMap. 
        private List<T> buildListExplicit<T>(SqlDataReader rdr)
        {
            List<T> result = new List<T>();
            while (rdr.Read())
            {
                T item = Activator.CreateInstance<T>();
                ((IExplicitMap)item).Map(_procedureName, rdr);
                result.Add(item);
            }

            return result;
        }

        //Builds a list using reflection.
        private List<T> buildListReflection<T>(SqlDataReader rdr)
        {
            List<T> result = new List<T>();

            ReaderColumnToObjectPropertyMapping[] columnMappings = ProcedureFactory.GetReaderColumnToObjectPropertyMappings<T>(_procedureName, rdr);

            while (rdr.Read())
            {
                T item = Activator.CreateInstance<T>();
                for (int idx = 0, len = columnMappings.Length; idx != len; idx++)
                {
                    columnMappings[idx].SetValue(item, rdr);
                }
                result.Add(item);
            }
            return result;
        }

        //Sets parameters to return and output parameter values.
        private void setReturnValues(SqlCommand cmd)
        {
            foreach (SqlParameter parameter in cmd.Parameters)
            {
                if (parameter.Direction != ParameterDirection.Input)
                {
                    if (parameter.Value != DBNull.Value)
                    {
                        GetParameter(parameter.ParameterName).SetValue<object>(parameter.Value);
                    }
                }
            }
        }

        #endregion

        #region |Accessors|

        /// <summary>Gets or sets the value of parameter at [name].</summary>
        public object this[string name]
        {
            set
            {
                SetValue<object>(mapParameterName(name), value);
            }
            get
            {
                return GetValue<object>(mapParameterName(name));
            }
        }

        /// <summary>Short cut to get parameter named RETURN_VALUE.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        public T ReturnValue<T>()
        {
            return GetValue<T>("@RETURN_VALUE");
        }

        /// <summary>Gets the parameter value as T based on the parameter name.</summary>
        /// <typeparam name="T">Type of return value.</typeparam>
        /// <param name="name">Name of parameter.</param>
        public T GetValue<T>(string name)
        {
            return _parameters[mapParameterName(name)].GetValue<T>();
        }

        /// <summary>Sets the value as T based on the name.</summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        public void SetValue<T>(string name, T value)
        {
            string parameterName = mapParameterName(name);
            _parameters[parameterName].SetValue<T>(value);
        }

        /// <summary>Finds a parameter by any of the name variations.</summary>
        /// <param name="paramaterName">Name or name variation @PARAMETER_NAME, PARAMETER_NAME, ParameterName, parameterName</param>
        public Parameter GetParameter(string paramaterName)
        {
            return _parameters[mapParameterName(paramaterName)];
        }

        /// <summary>Checks if the parameter is in the collection.</summary>
        /// <param name="parameterName">Name or name variation @PARAMETER_NAME, PARAMETER_NAME, ParameterName, parameterName</param>
        public bool ContainsParameter(string parameterName)
        {
            return _parameterNameMap.ContainsKey(parameterName);
        }

        #endregion

        #region |Helpers|

        //Checks all the procedures for null values against isNullable.
        private void checkValues()
        {
            foreach (Parameter parameter in _parameters.Values)
            {
                parameter.ValidateNull();
            }
        }

        //Private method to get @FULL_NAME from any of the name variations. 
        private string mapParameterName(string name)
        {
            if (!_parameterNameMap.ContainsKey(name))
            {
                throw new IndexOutOfRangeException($"Index out of range Procedure.mapParameterName({name})");
            }
            return _parameterNameMap[name];
        }

        #endregion

    }
}