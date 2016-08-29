using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EzAdo.Models
{
    [Serializable]
    public class Parameter
    {
        #region |Private Variables|

        //properties of an SqlPamameter
        private string _parameterName;
        private ParameterDirection _parameterDirection;
        private SqlDbType _sqlDbType;
        private int? _characterMaximumLength;
        private int? _dateTimePrecision;
        private int? _numericScale;
        private int? _numericPrecision;

        //Properties REST_SQL.Parameter
        private long? _numericMinimumValue;
        private long? _numericMaximumValue;
        public string _regularExpression;
        public bool _isNullable;
        private object _value = null;
        private Type _type;
        private string _dataTypeName;

        #endregion

        /// <summary>
        /// Exposing the nullable vs value state to the parent procedure
        /// </summary>
        public void ValidateNull()
        {
            if(!_isNullable)
            {
                if(_parameterDirection == ParameterDirection.Input || _parameterDirection == ParameterDirection.InputOutput)
                {
                    if (_value == DBNull.Value)
                    {
                        throw new ArgumentException($"Non-nullable parameter is null at Models.Parameter.checkNullValue()", _parameterName);
                    }
                }
            }
        }

        /// <summary>
        /// Exposing the DataTypeName so the Data Table can be prebuilt
        /// </summary>
        public string DataTypeName
        {
            get
            {
                return _dataTypeName;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterDirection"></param>
        /// <param name="charcterMaximumLength"></param>
        /// <param name="dateTimePrecision"></param>
        /// <param name="numericScale"></param>
        /// <param name="numericPrecision"></param>
        /// <param name="numericMinimumValue"></param>
        /// <param name="numericMaximumValue"></param>
        /// <param name="regularExpression"></param>
        /// <param name="isNullable"></param>
        public Parameter(string parameterName, ParameterDirection parameterDirection, SqlDbType sqlDbType, int? charcterMaximumLength, int? dateTimePrecision, int? numericScale, int? numericPrecision, long? numericMinimumValue, long? numericMaximumValue, string regularExpression, bool isNullable, Type type, string dataTypeName )
        {
            _parameterName = parameterName;
            _parameterDirection = parameterDirection;
            _sqlDbType = sqlDbType;
            _characterMaximumLength = charcterMaximumLength;
            _dateTimePrecision = dateTimePrecision;
            _numericScale = numericScale;
            _numericPrecision = numericPrecision;
            _numericMinimumValue = numericMinimumValue;
            _numericMaximumValue = numericMaximumValue;
            _regularExpression = regularExpression;
            _isNullable = isNullable;
            _type = type;
            _dataTypeName = dataTypeName;
        }

        /// <summary>
        /// Sets the value for the parameter
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter</typeparam>
        /// <param name="value">T value to test</param>
        public void SetValue<T>(T value)
        {
            if (_parameterDirection == ParameterDirection.Input || _parameterDirection == ParameterDirection.InputOutput)
            {
                checkNullValue<T>(value);
                checkType<T>(value);
                checkRegEx<T>(value);
                checkMaxLength<T>(value);
                checkNumericRange<T>(value);
            }
            _value = value;
        }

        /// <summary>
        /// Returns value as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>()
        {
            if (_value == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(_value, typeof(T));
        }

        /// <summary>
        /// Checks the value against _isNullable, throws argument exception on failure.
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter</typeparam>
        /// <param name="value">T value to test</param>
        private void checkNullValue<T>(T value)
        {
            if(!_isNullable)
            {
                if(value == null)
                {
                    throw new ArgumentException($"Attempt to set non-nullable parameter to null at Models.Parameter.checkNullValue()", _parameterName);
                }
            }
        }

        /// <summary>
        /// Checks the value against _regularExpression, throws argument exception on failure.
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter</typeparam>
        /// <param name="value">T value to test</param>
        private void checkRegEx<T>(T value)
        {
            if (_regularExpression == null) return;
            if (!Regex.IsMatch(value.ToString(), _regularExpression))
            {
                throw new ArgumentException($"Attempt to set regular expression parameter to invalid match of {_regularExpression} at Models.Parameter.checkRegEx()", _parameterName);
            }
        }

        /// <summary>
        /// Checks the value against _characterMaximumLength, throws argument exception on failure.
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter</typeparam>
        /// <param name="value">T value to test</param>
        private void checkMaxLength<T>(T value)
        {
            if(_characterMaximumLength.HasValue)
            {
                string s = value.ToString();
                if (s.Length > _characterMaximumLength.Value)
                {
                    throw new ArgumentException($"Value exceeds character maximum length of {_characterMaximumLength.Value}", _parameterName);
                }
            }
        }

        /// <summary>
        /// Checks the value against _type, throws invalid cast exception on failure.
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter - convert is attempted on typeof(object)</typeparam>
        /// <param name="value">T value to test</param>
        private void checkType<T>(T value)
        {
            if (typeof(T) == typeof(object))
            {
                try
                {
                    Convert.ChangeType(value, _type);
                }
                catch
                {
                    throw new InvalidCastException($"Parameter {_parameterName} expects C# type {_type.ToString()} at Models.Parameter.SetValue<T>(), the value {value.ToString()} provided could not be converted.");
                }
            }
            else
            {
                if (typeof(T) != _type)
                {
                    throw new InvalidCastException($"Parameter {_parameterName} expects C# type {_type.ToString()} at Models.Parameter.SetValue<T>(), the value provided was type {typeof(T).ToString()}.");
                }
            }
        }

        /// <summary>
        /// Checks the value against _numericMinimumValue and _numericMaximumValue, throws argument exception on failure.
        /// </summary>
        /// <typeparam name="T">System.Type - type of parameter</typeparam>
        /// <param name="value">T value to test</param>
        private void checkNumericRange<T>(T value)
        {
            if (_numericMinimumValue.HasValue || _numericMaximumValue.HasValue)
            {
                long test = Convert.ToInt64(value);
                if (_numericMinimumValue.HasValue)
                {
                    if (test < _numericMinimumValue.Value)
                    {
                        throw new ArgumentException($"Numeric Minumum Value failure for value {value} against limit of {_numericMaximumValue.Value}", _parameterName);
                    }
                }
                if (_numericMaximumValue.HasValue)
                {
                    if (test > _numericMaximumValue.Value)
                    {
                        throw new ArgumentException($"Numeric Maximum Value failure for value {value} against limit of {_numericMaximumValue.Value}", _parameterName);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an SqlParameter based on the Parameter properties 
        /// </summary>
        /// <returns>Sql Parameter</returns>
        public SqlParameter ToSqlParameter()
        {
            SqlParameter result = new SqlParameter();
            result.ParameterName = _parameterName;
            result.Direction = _parameterDirection;
            result.SqlDbType = _sqlDbType;

            if (_characterMaximumLength.HasValue) result.Size = _characterMaximumLength.Value;
            if (_dateTimePrecision.HasValue) result.Scale = (byte)_dateTimePrecision.Value;
            if (_numericScale.HasValue) result.Scale = (byte)_numericScale.Value;
            if (_numericPrecision.HasValue) result.Precision = (byte)_numericPrecision.Value;

            result.Value = _value;

            return result;
        } 
    }
}