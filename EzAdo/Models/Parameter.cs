using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EzAdo.Models
{
    /// <summary>   A parameter. </summary>
    public class Parameter
    {
        #region |Private Variables|

        /// <summary>   Properties of a System.Data.SqlClient.SqlParameter. </summary>
        private string _parameterName;

        /// <summary>   The parameter direction. </summary>
        private ParameterDirection _parameterDirection;

        /// <summary>   Type of the SQL database. </summary>
        private SqlDbType _sqlDbType;

        /// <summary>   The date time precision. </summary>
        private int? _dateTimePrecision;

        /// <summary>   The numeric scale. </summary>
        private int? _numericScale;

        /// <summary>   The numeric precision. </summary>
        private int? _numericPrecision;

        /// <summary>   true if this object is nullable. </summary>
        public bool _isNullable;


        /// <summary>   Length of the character maximum. </summary>
        private int? _characterMaximumLength;

        /// <summary>   Properties of a EzAdo.Parameter. </summary>
        private long? _numericMinimumValue;

        /// <summary>   The numeric maximum value. </summary>
        private long? _numericMaximumValue;

        /// <summary>   The regular expression. </summary>
        public string _regularExpression;


        /// <summary>   The value. </summary>
        private object _value = null;

        /// <summary>   The type. </summary>
        private Type _type;

        /// <summary>   Name of the data type. </summary>
        private string _dataTypeName;

        #endregion


        /// <summary>   Makes a deep copy of this object. </summary>
        /// <returns>   A copy of this object. </returns>
        public Parameter Clone()
        {
            return new Parameter(_parameterName, _parameterDirection, _sqlDbType, _characterMaximumLength, _dateTimePrecision, _numericScale, _numericPrecision, _numericMaximumValue, _numericMaximumValue, _regularExpression, _isNullable, _type, _dataTypeName);
        }

        /// <summary>   Exposing the nullable vs value state to the parent procedure. </summary>
        /// <exception cref="ArgumentException"> Non-nullable parameter is null. </exception>
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

        /// <summary>   Exposing the DataTypeName so the Data Table can be prebuilt. </summary>
        /// <value> The name of the data type. </value>
        public string DataTypeName
        {
            get
            {
                return _dataTypeName;
            }
        }

        /// <summary>   Constructor. </summary>
        /// <param name="parameterName">         Properties of a System.Data.SqlClient.SqlParameter. </param>
        /// <param name="parameterDirection">       The parameter direction. </param>
        /// <param name="sqlDbType">                Type of the SQL database. </param>
        /// <param name="characterMaximumLength">   Character Maximum Length. </param>
        /// <param name="dateTimePrecision">        The date time precision. </param>
        /// <param name="numericScale">             The numeric scale. </param>
        /// <param name="numericPrecision">         The numeric precision. </param>
        /// <param name="numericMinimumValue">      The numeric minum value. </param>
        /// <param name="numericMaximumValue">      The numeric maximum value. </param>
        /// <param name="regularExpression">        The regular expression. </param>
        /// <param name="isNullable">               true if this object is nullable. </param>
        /// <param name="type">                     The type. </param>
        /// <param name="dataTypeName">             Name of the data type. </param>
        public Parameter(string parameterName, ParameterDirection parameterDirection, SqlDbType sqlDbType, int? characterMaximumLength, int? dateTimePrecision, int? numericScale, int? numericPrecision, long? numericMinimumValue, long? numericMaximumValue, string regularExpression, bool isNullable, Type type, string dataTypeName )
        {
            _parameterName = parameterName;
            _parameterDirection = parameterDirection;
            _sqlDbType = sqlDbType;
            _characterMaximumLength = characterMaximumLength;
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


        /// <summary>   Sets the value of the parameter. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="value">    T value to set. </param>
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

        /// <summary>   Returns the value of the parameter as T. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <returns>   T value. </returns>
        public T GetValue<T>()
        {
            if (_value == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(_value, typeof(T));
        }

        /// <summary>
        /// Checks the value against _isNullable, throws ArgumentNullException on failure.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="value">    T value to test. </param>
        private void checkNullValue<T>(T value)
        {
            if(!_isNullable)
            {
                if(value == null)
                {
                    throw new ArgumentNullException($"Attempt to set non-nullable parameter to null at Models.Parameter.checkNullValue()", _parameterName);
                }
            }
        }

        /// <summary>
        /// Checks the value against _regularExpression, throws ArgumentOutOfRangeException on failure.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Parameter value fails regular expression test</exception>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="value">    T value to test. </param>
        private void checkRegEx<T>(T value)
        {
            if (_regularExpression == null) return;
            if (!Regex.IsMatch(value.ToString(), _regularExpression))
            {
                throw new ArgumentOutOfRangeException($"Attempt to set regular expression parameter to invalid match of {_regularExpression} at Models.Parameter.checkRegEx()", _parameterName);
            }
        }

        /// <summary>
        /// Checks the value against _characterMaximumLength, throws argument exception on failure.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Character data exceeds maximum length</exception>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="value">    T value to test. </param>
        private void checkMaxLength<T>(T value)
        {
            if(_characterMaximumLength.HasValue)
            {
                string s = value.ToString();
                if (s.Length > _characterMaximumLength.Value)
                {
                    throw new ArgumentOutOfRangeException($"Value exceeds character maximum length of {_characterMaximumLength.Value}", _parameterName);
                }
            }
        }

        /// <summary>   Checks the value against _type, throws invalid cast exception on failure. </summary>
        /// <exception cref="InvalidCastException"> Thrown when an object cannot be cast to a required
        /// type. </exception>
        /// <typeparam name="T"> Generic type parameter.</typeparam>
        /// <param name="value">    T value to test. </param>
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
        /// Checks the value against _numericMinimumValue and _numericMaximumValue, throws argument
        /// exception on failure.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        /// the required range. </exception>
        /// <typeparam name="T">    System.Type - type of parameter. </typeparam>
        /// <param name="value">    T value to test. </param>
        private void checkNumericRange<T>(T value)
        {
            if (_numericMinimumValue.HasValue || _numericMaximumValue.HasValue)
            {
                long test = Convert.ToInt64(value);
                if (_numericMinimumValue.HasValue)
                {
                    if (test < _numericMinimumValue.Value)
                    {
                        throw new ArgumentOutOfRangeException($"Numeric Minumum Value failure for value {value} against limit of {_numericMaximumValue.Value}", _parameterName);
                    }
                }
                if (_numericMaximumValue.HasValue)
                {
                    if (test > _numericMaximumValue.Value)
                    {
                        throw new ArgumentOutOfRangeException($"Numeric Maximum Value failure for value {value} against limit of {_numericMaximumValue.Value}", _parameterName);
                    }
                }
            }
        }

        /// <summary>   Creates an SqlParameter based on the Parameter properties. </summary>
        /// <returns>   Sql Parameter. </returns>
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