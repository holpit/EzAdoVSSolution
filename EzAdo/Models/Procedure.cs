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
    /// <summary>   A Procedure in the context of EzAdo is a wrapper around a stored procedure including the in, out, and return parameters.
    ///             All procedures are prebuilt using the ezado.Procedures stored procedure as the data source. </summary>
    public class Procedure
    {
        #region private variables

        /// <summary>   The connection string. </summary>
        private string _connectionString;

        /// <summary>   Name of the procedure. </summary>
        private string _procedureName;

        /// <summary>   True if this object is JSON result, based on the procedure containing the *Returns Json* annotation </summary>
        private bool _isJsonResult;

        /// <summary>   True if this object is single result, based on the procedure containing the *Single Result* annotation </summary>
        private bool _isSingleResult;

        /// <summary>   true if this object is non query, based on the procedure containing the *Non Query* annotation  </summary>
        private bool _isNonQuery;

        /// <summary>   The dynamic query entered by the user. </summary>
        private string _dynamicQuery = null;

        /// <summary>   Parameter Collection. </summary>
        private Dictionary<string, Parameter> _parameters;

        /// <summary>   Parameter name is the dictionary that contains mappings for different naming conventions.  As an example the 
        ///             parameter @FIRST_NAME would have mappings of @FIRST_NAME, FIRST_NAME, FirstName, and firstName.  The mapping allows
        ///             for quick parameter lookups in the get and set methods </summary>
        private Dictionary<string, string> _parameterNameMap;

        #endregion

        #region |Loaders|

        /// <summary>   Checks to see if the newton soft token provides a value. </summary>
        /// <param name="tkn">  JsonToken. </param>
        /// <returns>   True if the token type is a value type. </returns>
        private bool isTokenValue(JsonToken tkn)
        {
            return tkn == JsonToken.Boolean ||
                tkn == JsonToken.Bytes ||
                tkn == JsonToken.Date ||
                tkn == JsonToken.Float ||
                tkn == JsonToken.Integer ||
                tkn == JsonToken.String;
        }


        /// <summary>
        /// Sets the parameters from the query string Note that multiple values (s=1 s=2 s=3) are not
        /// supported. For multiple records use LoadFromJson method.
        /// </summary>
        /// <param name="keyValuePairs">    From HttpRequest. </param>
        public void LoadFromQuery(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kp in keyValuePairs)
            {
                SetValue<object>(kp.Key, kp.Value);
            }
        }
        

        /// <summary>   Sets the parameters values from json. </summary>
        /// <param name="json"> json string. </param>
        public void LoadFromJson(string json)
        {
            using (var rdr = new JsonTextReader(new StringReader(json)))
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
        

        /// <summary>
        /// Sets the parameter values from the given object where the object properties map to the
        /// parameters.
        /// </summary>
        /// <typeparam name="T">    Type of object. </typeparam>
        /// <param name="value">    object. </param>
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
        
        /// <summary>   Create a new procedure. </summary>
        /// <param name="connectionString"> Connection string with permissions to the appropriate schema. </param>
        /// <param name="procedureName">    Name of Stored Procedure always in the format
        /// [schema].[PROCEDURE_NAME]. </param>
        /// <param name="isJsonResult">     Stored procedure is annotated with *Returns Json*. </param>
        /// <param name="isSingleResult">   Stored procedure is annotated with *Single Result*. </param>
        /// <param name="isNonQuery">       Stored procedure is annotated with *Non Query*. </param>
        /// <param name="parameters">       Collection of parameters associated with the stored
        /// procedure. </param>
        /// <param name="parameterNameMap"> Collection of name mappings ie. @FIRST_NAME: {@FIRST_NAME,
        /// FIRST_NAME, FirstName, firstName} </param>
        public Procedure(string connectionString, string procedureName, bool isJsonResult, bool isSingleResult, bool isNonQuery, Dictionary<string,Models.Parameter> parameters, Dictionary<string,string> parameterNameMap)
        {
            _connectionString = connectionString;
            _procedureName = procedureName;
            _isJsonResult = isJsonResult;
            _isSingleResult = isSingleResult;
            _isNonQuery = isNonQuery;
            _parameters = parameters;
            _parameterNameMap = parameterNameMap;
        }
        
        /// <summary>   Create a new instance of the procedure. </summary>
        /// <returns>   Models.Procedure cloned procedure. </returns>
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

        /// <summary>
        /// Executes procedure and returns json - works exclusively with procedures annotated with
        /// *Returns Json*!
        /// </summary>
        /// <exception cref="InvalidOperationException"> Procedure is not annotated with *Returns Json*. </exception>
        /// <returns>   System.String json result. </returns>
        public string ExecuteJson()
        {
            if (_isJsonResult == false)
            {
                throw new InvalidOperationException("Procedure is not annotated with *Returns Json*");
            }

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

        /// <summary>   Executes procedure and returns Data Table. </summary>
        /// <returns>   System.Data.DataTable result. </returns>
        public DataTable ExecuteDataTable()
        {
            if (_isJsonResult)
            {
                string JsonResult = ExecuteJson();
                return (DataTable)JsonConvert.DeserializeObject(JsonResult, typeof(DataTable));
            }
            else
            {
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
        }

        /// <summary>
        /// Executes SqlDataReader and creates List T by populating from reader either explicitly or via
        /// reflection.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Procedure annotated with *Single Result* cannot
        /// produce List. </exception>
        /// <typeparam name="T">    System.Type of resulting List T. </typeparam>
        /// <returns>   List of T. </returns>
        public List<T> ExecuteList<T>()
        {
            if (_isSingleResult) throw new InvalidOperationException("Procedure annotated with *Single Result* cannot produce List<T>");

            if (_isJsonResult)
            {
                string jsonResult = ExecuteJson();
                var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                return (List<T>)JsonConvert.DeserializeObject(jsonResult, typeof(List<T>), jsonSerializerSettings);
            }
            else
            {
                return buildList<T>();
            }
        }

        /// <summary>
        /// Executes SqlDataReader and creates List T by populating from reader either explicitly or via
        /// refelction.  Converts resulting List to json.
        /// </summary>
        /// <typeparam name="T">    System.Type of resulting List T. </typeparam>
        /// <returns>   System.String json result. </returns>
        public string ExecuteJsonList<T>()
        {
            List<T> temp = ExecuteList<T>();
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(temp, jsonSerializerSettings);
        }

        /// <summary>
        /// Executes SqlDataReader and creates T by populating from reader either explicitly or via
        /// reflection.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Procedure requires annotation of *Single Result*
        /// to return a single object. </exception>
        /// <typeparam name="T">    System.Type of resulting T. </typeparam>
        /// <returns>   Object T. </returns>
        public T Execute<T>()
        {
            if (!_isSingleResult) throw new InvalidOperationException("Procedure requires annotation of *Single Result* to return a single object");

            if (_isJsonResult)
            {
                string jsonResult = ExecuteJson();
                var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                return (T)JsonConvert.DeserializeObject(jsonResult, typeof(T), jsonSerializerSettings);
            }
            else
            {
                List<T> lst = buildList<T>();
                if (lst.Count == 1)
                {
                    return lst[0];
                }
                return default(T);
            }
        }

        /// <summary>
        /// Executes SqlDataReader and creates T by populating from reader either explicitly or
        /// dynamically.  Converts resulting object to json.
        /// </summary>
        /// <typeparam name="T">    Type of object to populate from reader. </typeparam>
        /// <returns>   System.String json result. </returns>
        public string ExecuteJson<T>()
        {
            T temp = Execute<T>();
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(temp,jsonSerializerSettings);
        }

        /// <summary>
        /// Executes procedure with no results.  Ses the ouput parameters and return value;
        /// </summary>
        public void ExecuteNonQuery()
        {
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

        /// <summary>
        /// Builds the SqlCommand based on the Parameter Definitions and values, binds the connection
        /// passed in to the command.
        /// </summary>
        /// <param name="cnn">  The cnn. </param>
        /// <returns>   A SqlCommand. </returns>
        private SqlCommand buildCommand(SqlConnection cnn)
        {
            SqlCommand cmd = new SqlCommand();

            if (_dynamicQuery != null)
            {
                cmd.CommandText = _dynamicQuery;
                cmd.CommandType = CommandType.Text;
            }
            else
            {
                cmd.CommandText = _procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
            }

            foreach (Parameter parameter in _parameters.Values)
            {
                cmd.Parameters.Add(parameter.ToSqlParameter());
            }

            cmd.Connection = cnn;

            return cmd;
        }

        /// <summary>   Builds List of T either via reflection or supported IExplicitMap. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <returns>   List T; </returns>
        private List<T> buildList<T>()
        {
            checkValues();

            List<T> result = null;

            bool builtFromExplicit = false;

            using (SqlConnection cnn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = buildCommand(cnn))
                {
                    cnn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (typeof(T).GetInterfaces().Contains(typeof(IExplicitMap)))
                        {
                            T temp = Activator.CreateInstance<T>();
                            if (((IExplicitMap)temp).SupportsMap(_procedureName))
                            {
                                result = buildListExplicit<T>(rdr);
                                builtFromExplicit = true;
                            }
                        }
                        if (!builtFromExplicit)
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

        /// <summary>   Builds a list when the object implements IExplicitMap. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="rdr">  The reader. </param>
        /// <returns>   List T </returns>
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

        /// <summary>   Builds a list using reflection. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="rdr">  The reader. </param>
        /// <returns>   List T </returns>
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

        /// <summary>   Sets parameters to return and output parameter values. </summary>
        /// <param name="cmd">  The command. </param>
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

        /// <summary>   Short cut to getting or setting the values through indexer. </summary>
        /// <param name="name"> Name see Parameter Name Map </param>
        /// <returns>   Parameter value. </returns>
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


        /// <summary>   Returns the Command Return_Value. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <returns>   The value. </returns>
        public T ReturnValue<T>()
        {
            return GetValue<T>("@RETURN_VALUE");
        }


        /// <summary>   Gets the parameter value as T based on the parameter name. </summary>
        /// <typeparam name="T">    type of return value. </typeparam>
        /// <param name="name"> name of parameter. </param>
        /// <returns>   value as T. </returns>
        public T GetValue<T>(string name)
        {
            return _parameters[mapParameterName(name)].GetValue<T>();
        }


        /// <summary>   Sets the value as T based on the name. </summary>
        /// <typeparam name="T">    type of value. </typeparam>
        /// <param name="name">     name of parameter. </param>
        /// <param name="value">    value of parameter. </param>
        public void SetValue<T>(string name, T value)
        {
            string parameterName = mapParameterName(name);
            _parameters[parameterName].SetValue<T>(value);
        }


        /// <summary>   Finds a parameter by any of the name variations. </summary>
        /// <param name="paramaterName">    . </param>
        /// <returns>   The parameter. </returns>
        public Parameter GetParameter(string paramaterName)
        {
            return _parameters[mapParameterName(paramaterName)];
        }


        /// <summary>   Cehcks it the parameter is in the collection. </summary>
        /// <param name="parameterName">    . </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool ContainsParameter(string parameterName)
        {
            return _parameterNameMap.ContainsKey(parameterName);
        }

        #endregion

        #region |Helpers}|


        /// <summary>   Checks all the procedures for null values against isNullable. </summary>
        private void checkValues()
        {
            foreach (Parameter parameter in _parameters.Values)
            {
                parameter.ValidateNull();
            }
        }


        /// <summary>   Private method to get @FULL_NAME from any of the name variations. </summary>
        /// <exception cref="IndexOutOfRangeException"> Thrown when the index is outside the required
        /// range. </exception>
        /// <param name="name"> @FULL_NAME, SHORT_NAME, ProperCase, or camelCase parameter name. </param>
        /// <returns>   @FULL_NAME. </returns>
        private string mapParameterName(string name)
        {
            if(!_parameterNameMap.ContainsKey(name))
            {
                throw new IndexOutOfRangeException($"Index out of range Procedure.mapParameterName({name})");
            }
            return _parameterNameMap[name];
        }

        #endregion

    }
}
