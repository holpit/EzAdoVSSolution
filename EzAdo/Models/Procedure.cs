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
    [Serializable]
    public class Procedure
    {
        #region Private Variables

        private string _connectionString;
        private string _procedureName;
        private bool _isJsonResult;
        private bool _isSingleResult;
        private bool _isNonQuery;
        private string _dynamicQuery = null;
        private int _rowCount = 0;
        private Dictionary<string, Parameter> _parameters;
        private Dictionary<string, string> _parameterNameMap;

        #endregion

        #region |Loaders|

        /// <summary>
        /// Checks to see if the newton soft token provides a value
        /// </summary>
        /// <param name="tkn">JsonToken</param>
        /// <returns>True if the token type is a value type</returns>
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
        /// Sets the parameters from the query string
        /// Note that multiple values (s=1&s=2) are not supported
        /// Multple records are supported with LoadFromJson method
        /// </summary>
        /// <param name="keyValuePairs">From HttpRequest</param>
        public void LoadFromQuery(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> kp in keyValuePairs)
            {
                SetValue<object>(kp.Key, kp.Value);
            }
        }

        /// <summary>
        /// Sets the parameters values from json
        /// </summary>
        /// <param name="json">json string</param>
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
        /// Sets the parameter values from the given object where the object properties map to the parameters
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="value">object</param>
        public void LoadFromObject<T>(T value)
        {
            ObjectPropertyToParameterNameMapping[] mappings = ProcedureFactory.GetParameterMapping<T>(_procedureName);
            for (int idx = 0, len = mappings.Length; idx != len; idx++)
            {
                mappings[idx].SetValue(value, this);
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="procedureName"></param>
        /// <param name="isJsonResult"></param>
        /// <param name="isSingleResult"></param>
        /// <param name="isNonQuery"></param>
        /// <param name="parameters"></param>
        /// <param name="parameterNameMap"></param>
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

        /// <summary>
        /// Builds the SqlCommand based on the Parameter Collection
        /// Binds an open connection to the command
        /// </summary>
        public SqlCommand buildCommand(SqlConnection cnn)
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

        /// <summary>
        /// Executes procedure and returns json string
        /// Requires a stored procedure to be marked with /*Returns Json*/
        /// </summary>
        /// <returns>Returns the string of json</returns>
        public string ExecuteJson()
        {
            if (_isJsonResult == false)
            {
                throw new MethodAccessException("Procedure does not produce a json result - mark the stored procedure with /*Returns Json*/ if that is the intention");
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


        /// <summary>
        /// Sets the parameter values to the results from the out parameters
        /// </summary>
        /// <param name="cmd"></param>
        private void setReturnValues(SqlCommand cmd)
        {
            foreach(SqlParameter parameter in cmd.Parameters)
            {
                if(parameter.Direction != ParameterDirection.Input)
                {
                    if(parameter.Value != DBNull.Value)
                    {
                        GetParameter(parameter.ParameterName).SetValue<object>(parameter.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Executes procedure and returns Data Table
        /// </summary>
        /// <returns>Data as DataTable</returns>
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
                            _rowCount = result.Rows.Count;
                            return result;
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Executes procedure and returns List<T>
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <returns>Data as List<T></T></returns>
        public List<T> ExecuteList<T>()
        {
            if (_isSingleResult) throw new TypeAccessException("Single result annotated procedure cannot produce List<T>");

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
        /// Executes procedure and returns T
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <returns>Data as T</returns>
        public T Execute<T>()
        {
            if (!_isSingleResult) throw new TypeAccessException("Multiple result procedure cannot produce single object T - add the *Single Result* annotation");

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
        /// Executes procedure with no result set
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
        /// Short cut to getting or setting the values through indexer
        /// </summary>
        /// <param name="name">@FULL_NAME, SHORT_NAME, ProperCase, or camelCase parameter name</param>
        /// <returns>Parameter value</returns>
        public object this[string name]
        {
            set
            {
                SetValue<object>(mpn(name), value);
            }
            get
            {
                return GetValue<object>(mpn(name));
            }

        }

        /// <summary>
        /// Gets the return value of the function or procedure
        /// </summary>
        /// <returns>return value</returns>
        public T ReturnValue<T>()
        {
            return GetValue<T>("@RETURN_VALUE");
        }

        /// <summary>
        /// Gets the parameter value as T based on the parameter name
        /// </summary>
        /// <typeparam name="T">type of return value</typeparam>
        /// <param name="position">ordinal position of parameter</param>
        /// <returns>value as T</returns>
        public T GetValue<T>(string name)
        {
            string parameterName = mpn(name);
            return _parameters[parameterName].GetValue<T>();

        }

        /// <summary>
        /// Sets the value as T based on the name
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="name">name of parameter</param>
        /// <param name="value">value of parameter</param>
        public void SetValue<T>(string name, T value)
        {
            if(!_parameterNameMap.ContainsKey(name))
            {
                throw new ArgumentException($"Provided parameter {name} was not found in procedures parameter mapping");
            }
            string parameterName = mpn(name);
            _parameters[parameterName].SetValue<T>(value);
        }

        /// <summary>
        /// Checks all the procedures for null values against isNullable
        /// </summary>
        private void checkValues()
        {
            foreach (Parameter parameter in _parameters.Values)
            {
                parameter.ValidateNull();
            }
        }
        
        /// <summary>
        /// private method to get @FULL_NAME from any of the name variations
        /// </summary>
        /// <param name="name">@FULL_NAME, SHORT_NAME, ProperCase, or camelCase parameter name</param>
        /// <returns>@FULL_NAME</returns>
        private string mpn(string name)
        {
            return _parameterNameMap[name];
        }

        /// <summary>
        /// Finds a parameter by any of the name variations
        /// </summary>
        /// <param name="paramaterName"></param>
        /// <returns></returns>
        public Parameter GetParameter(string paramaterName)
        {
            return _parameters[mpn(paramaterName)];
        }

        /// <summary>
        /// Cehcks it the parameter is in the collection
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public bool ContainsParameter(string parameterName)
        {
            return _parameterNameMap.ContainsKey(parameterName);
        }

        /// <summary>
        /// Builds List of T either via reflection or supported IExplicitMap
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
                        if(!builtFromExplicit)
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

        /// <summary>
        /// Builds a list when the object implements IExplicitMap
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr"></param>
        /// <returns></returns>
        private List<T> buildListExplicit<T>(SqlDataReader rdr)
        {
            List<T> result = new List<T>();
            while (rdr.Read())
            {
                _rowCount++;
                T item = Activator.CreateInstance<T>();
                ((IExplicitMap)item).Map(_procedureName, rdr);
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Builds a list using reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rdr"></param>
        /// <returns></returns>
        private List<T> buildListReflection<T>(SqlDataReader rdr)
        {
            List<T> result = new List<T>();

            ReaderColumnToObjectPropertyMapping[] columnMappings = ProcedureFactory.GetReaderColumnToObjectPropertyMappings<T>(_procedureName, rdr);

            while (rdr.Read())
            {
                _rowCount++;
                T item = Activator.CreateInstance<T>();
                for (int idx = 0, len = columnMappings.Length; idx != len; idx++)
                {
                    columnMappings[idx].SetValue(item, rdr);
                }
                result.Add(item);
            }
            return result;
        }
    }
}
