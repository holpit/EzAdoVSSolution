using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EzAdo.Models
{
    /// <summary>In the context of ezado a parameter is the definition that will ultimately build an SqlParameter.  This object is populated via the results of the ezado.PROCEDURES call.</summary>
    public class Parameter
    {
        #region |Private Variables|

        private string _parameterName;
        private ParameterDirection _parameterDirection;
        private SqlDbType _sqlDbType;
        private int? _dateTimePrecision;
        private int? _numericScale;
        private int? _numericPrecision;
        private bool _isNullable;
        private int? _characterMaximumLength;
        private long? _numericMinimumValue;
        private long? _numericMaximumValue;
        private string _regularExpression;
        private object _value = null;
        private Type _type;
        private string _dataTypeName;

        #endregion

        #region |Constructor Clone|

        /// <summary>   Constructor. </summary>
        /// <param name="parameterName">Properties of a System.Data.SqlClient.SqlParameter.</param>
        /// <param name="parameterDirection">The parameter direction.</param>
        /// <param name="sqlDbType">Type of the SQL database.</param>
        /// <param name="characterMaximumLength">Character Maximum Length.</param>
        /// <param name="dateTimePrecision">The date time precision.</param>
        /// <param name="numericScale">The numeric scale.</param>
        /// <param name="numericPrecision">The numeric precision.</param>
        /// <param name="numericMinimumValue">The numeric minimum value.</param>
        /// <param name="numericMaximumValue">The numeric maximum value.</param>
        /// <param name="regularExpression">The regular expression.</param>
        /// <param name="isNullable">true if this object is nullable.</param>
        /// <param name="type">The type.</param>
        /// <param name="dataTypeName">Name of the data type.</param>
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

        /// <summary>Makes a deep copy of this object.</summary>
        public Parameter Clone()
        {
            return new Parameter(_parameterName, _parameterDirection, _sqlDbType, _characterMaximumLength, _dateTimePrecision, _numericScale, _numericPrecision, _numericMaximumValue, _numericMaximumValue, _regularExpression, _isNullable, _type, _dataTypeName);
        }

        #endregion

        #region |Public Methods Exposed To Parent Procedure|

        /// <summary>Exposing the nullable vs value state to the parent procedure.</summary>
        public void ValidateNull()
        {
            if (!_isNullable)
            {
                if (_parameterDirection == ParameterDirection.Input || _parameterDirection == ParameterDirection.InputOutput)
                {
                    if (_value == DBNull.Value)
                    {
                        throw new ArgumentException($"Non-nullable parameter is null at Models.Parameter.checkNullValue()", _parameterName);
                    }
                }
            }
        }

        /// <summary>Exposing the DataTypeName so the Data Table can be prebuilt.</summary>
        /// <value>The name of the data type.</value>
        public string DataTypeName
        {
            get
            {
                return _dataTypeName;
            }
        }

        /// <summary>Sets the value of the parameter.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="value">T value to set.</param>
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

        /// <summary>Returns the value of the parameter as T.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        public T GetValue<T>()
        {
            if (_value == null)
            {
                return default(T);
            }
            return (T)Convert.ChangeType(_value, typeof(T));
        }

        /// <summary>Creates an SqlParameter based on the Parameter properties.</summary>
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

        #endregion

        #region |Validate Methods|

        //Checks the value against _isNullable, throws ArgumentNullException on failure.
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

        //Checks the value against _regularExpression, throws ArgumentOutOfRangeException on failure.
        private void checkRegEx<T>(T value)
        {
            if (_regularExpression == null) return;
            if (!Regex.IsMatch(value.ToString(), _regularExpression))
            {
                throw new ArgumentOutOfRangeException($"Attempt to set regular expression parameter to invalid match of {_regularExpression} at Models.Parameter.checkRegEx()", _parameterName);
            }
        }

        //Checks the value against _characterMaximumLength, throws argument exception on failure.
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

        //Checks the value against _type, throws invalid cast exception on failure.
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

        //Checks the value against _numericMinimumValue and _numericMaximumValue, throws argument exception on failure.
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

        #endregion
        
    }
}