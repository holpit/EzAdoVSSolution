using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzAdo.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a camel case to underscore case
        /// </summary>
        /// <param name="value">this</param>
        /// <returns></returns>
        public static string ToUnderscore(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len * 2;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharWasLower = false;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (previousCharWasLower)
                {
                    if (char.IsUpper(current))
                    {
                        output[outPos++] = '_';
                    }
                }
                output[outPos++] = char.ToUpper(input[idx]);
                previousCharWasLower = char.IsLower(current);
            }
            return new string(output, 0, outPos);
        }

        public static string ToProperCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharUnderscore = true;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (current == '_')
                {
                    previousCharUnderscore = true;
                    continue;
                }
                if (previousCharUnderscore)
                {
                    output[outPos++] = char.ToUpper(input[idx]);
                }
                else
                {
                    output[outPos++] = char.ToLower(input[idx]);
                }
                previousCharUnderscore = false;
            }
            return new string(output, 0, outPos);

        }
        /// <summary>
        /// Converts underscore case to camel case
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCamel(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            int len = value.Length;
            int outLen = len;
            int outPos = 0;
            char[] input = value.ToCharArray();
            char[] output = new char[outLen];
            bool previousCharUnderscore = true;

            for (int idx = 0; idx != len; idx++)
            {
                char current = input[idx];
                if (current == '_')
                {
                    previousCharUnderscore = true;
                    continue;
                }
                if (previousCharUnderscore)
                {
                    if (idx == 0)
                    {
                        output[outPos++] = char.ToLower(input[idx]);
                    }
                    else
                    {
                        output[outPos++] = char.ToUpper(input[idx]);
                    }
                }
                else
                {
                    output[outPos++] = char.ToLower(input[idx]);
                }
                previousCharUnderscore = false;
            }
            return new string(output, 0, outPos);
        }

        public static string ToJsonName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.StartsWith("@")) value = value.Substring(1);
            return ToCamel(value);
        }

        public static string ToSqlName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (!value.StartsWith("@")) value = $"@{value}";
            return ToUnderscore(value);
        }

        public static string FromSqlParameterName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            string temp = value.StartsWith("@") ? value.Substring(1) : value;
            return ToCamel(temp);
        }

        public static string FromSqlProcedureName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string prefix = value.Substring(0, value.IndexOf('_'));
            string temp = value;
            if (prefix == "GET" || prefix == "POST" || prefix == "PUT" || prefix == "DELETE")
            {
                temp = value.Substring(value.IndexOf('_'));
            }
            return ToCamel(temp);
        }

        public static Type ToCSharpTypeFromSqlDbType(this SqlDbType value)
        {
            switch (value)
            {
                case SqlDbType.BigInt:
                    return typeof(long);
                case SqlDbType.Int:
                    return typeof(int);
                case SqlDbType.SmallInt:
                    return typeof(short);
                case SqlDbType.TinyInt:
                    return typeof(byte);
                case SqlDbType.Bit:
                    return typeof(bool);
                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                    return typeof(string);
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.Time:
                    return typeof(DateTime);
                case SqlDbType.DateTimeOffset:
                    return typeof(TimeSpan);
                case SqlDbType.Decimal:
                    return typeof(decimal);
                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);
                case SqlDbType.Structured:
                    return typeof(DataTable);
                default:
                    throw new ArgumentException($"Data type {value.ToString()} is not yet supported at Procedure.getCSharpTypeSqlDbType.");
            }
        }

        public static SqlDbType ToSqlDbTypeFromSqlDataType(this string value)
        {
            switch (value)
            {
                case "bigint":
                    return SqlDbType.BigInt;
                case "int":
                    return SqlDbType.Int;
                case "smallint":
                    return SqlDbType.SmallInt;
                case "tinyint":
                    return SqlDbType.TinyInt;
                case "bit":
                    return SqlDbType.Bit;
                case "char":
                    return SqlDbType.Char;
                case "varchar":
                    return SqlDbType.VarChar;
                case "nchar":
                    return SqlDbType.NChar;
                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "smalldatetime":
                    return SqlDbType.SmallDateTime;
                case "date":
                    return SqlDbType.Date;
                case "datetime":
                    return SqlDbType.DateTime;
                case "datetime2":
                    return SqlDbType.DateTime;
                case "time":
                    return SqlDbType.Time;
                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;
                case "numeric":
                case "decimal":
                    return SqlDbType.Decimal;
                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;
                case "table type":
                    return SqlDbType.Structured;
                default:
                    throw new ArgumentException($"data type {value} is not yet supported, add data type to Parameter.cs setDataType()");
            }
        }

        public static Type ToCSharpTypeFromSqlDataType(this string value)
        {
            switch (value)
            {
                case "bigint":
                    return typeof(long);
                case "int":
                    return typeof(int);
                case "smallint":
                    return typeof(short);
                case "tinyint":
                    return typeof(byte);
                case "bit":
                    return typeof(bool);
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                    return typeof(string);
                case "smalldatetime":
                case "date":
                case "datetime":
                case "datetime2":
                case "time":
                    return typeof(DateTime);
                case "datetimeoffset":
                    return typeof(TimeSpan);
                case "numeric":
                case "decimal":
                    return typeof(decimal);
                case "uniqueidentifier":
                    return typeof(Guid);
                case "table type":
                    return typeof(DataTable);
                default:
                    throw new ArgumentException($"Data type {value.ToString()} is not yet supported at Procedure.getCSharpTypeSqlDbType.");
            }
        }

    }
}
