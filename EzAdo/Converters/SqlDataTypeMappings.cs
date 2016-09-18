using System;
using System.Collections.Generic;
using System.Data;

namespace EzAdo.Converters
{
    /// <summary>Class for mapping values retrieved from SQL Server information views to c# data types.</summary>
    public static class SqlDataTypeMappings
    {

        #region Mapping of SqlServer data type names to SqlDbType enum
        private static readonly Dictionary<string, SqlDbType> SqlDataTypeToSqlDbTypes = new Dictionary<string, SqlDbType>
        {
            { "bigint", SqlDbType.BigInt },
            { "binary", SqlDbType.Binary },
            { "bit", SqlDbType.Bit },
            { "char", SqlDbType.Char },
            { "date", SqlDbType.Date },
            { "datetime", SqlDbType.DateTime},
            { "datetime2", SqlDbType.DateTime2},
            { "datetimeoffset", SqlDbType.DateTimeOffset },
            { "decimal", SqlDbType.Decimal },
            { "float", SqlDbType.Float },
            { "image", SqlDbType.Image },
            { "int", SqlDbType.Int },
            { "money", SqlDbType.Money },
            { "nchar", SqlDbType.NChar },
            { "ntext", SqlDbType.NText },
            { "numeric", SqlDbType.Decimal },
            { "nvarchar", SqlDbType.NVarChar },
            { "real", SqlDbType.Real },
            { "rowversion", SqlDbType.Timestamp },
            { "smalldatetime", SqlDbType.SmallDateTime },
            { "smallint", SqlDbType.SmallInt },
            { "smallmoney", SqlDbType.SmallMoney },
            { "structured", SqlDbType.Structured },
            { "table type", SqlDbType.Structured },
            { "text", SqlDbType.Text },
            { "time" , SqlDbType.Time },
            { "timestamp", SqlDbType.Timestamp },
            { "tinyint", SqlDbType.TinyInt},
            { "utd", SqlDbType.Udt },
            { "uniqueidentfier", SqlDbType.UniqueIdentifier },
            { "varbinary", SqlDbType.VarBinary },
            { "varchar", SqlDbType.VarChar },
            { "variant", SqlDbType.Variant },
            { "xml", SqlDbType.Xml }
        };

        #endregion

        #region Mapping of SqlServer data type names to CSharps types.
        private static readonly Dictionary<string, Type> SqlDataTypeToCSharpTypes = new Dictionary<string, Type>
        {
            { "bigint", typeof(long) },
            { "binary", typeof(byte[]) },
            { "bit", typeof(bool) },
            { "char", typeof(string) },
            { "date", typeof(DateTime) },
            { "datetime", typeof(DateTime) },
            { "datetime2", typeof(DateTime) },
            { "datetimeoffset", typeof(DateTimeOffset) },
            { "decimal", typeof(DateTime) },
            { "float", typeof(float) },
            { "image", typeof(byte[]) },
            { "int", typeof(int) },
            { "money", typeof(decimal) },
            { "nchar", typeof(string) },
            { "ntext", typeof(string) },
            { "numeric", typeof(decimal) },
            { "nvarchar", typeof(string) },
            { "real", typeof(float) },
            { "rowversion", typeof(byte[]) },
            { "smalldatetime", typeof(DateTime) },
            { "smallint", typeof(short) },
            { "smallmoney", typeof(decimal) },
            { "structured", typeof(DataTable) },
            { "table type", typeof(DataTable) },
            { "text", typeof(string) },
            { "time" , typeof(DateTime) },
            { "timestamp", typeof(byte[]) },
            { "tinyint", typeof(byte) },
            { "utd", typeof(object) },
            { "uniqueidentfier", typeof(Guid) },
            { "varbinary", typeof(byte[]) },
            { "varchar", typeof(string) },
            { "variant", typeof(object) },
            { "xml", typeof(string) }
        };

        #endregion

        #region Mapping of SqlDbType enum to CSharp types.
        private static readonly Dictionary<SqlDbType, Type> SqlDbTypeToCSharpTypes = new Dictionary<SqlDbType, Type>
        {
            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.Binary, typeof(byte[]) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.Date, typeof(DateTime) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTime2, typeof(DateTime) },
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Float, typeof(float) },
            { SqlDbType.Image, typeof(byte[]) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.NText, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Real, typeof(float) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.Structured, typeof(DataTable) },
            { SqlDbType.Text, typeof(string) },
            { SqlDbType.Time, typeof(DateTime) },
            { SqlDbType.Timestamp, typeof(byte[]) },
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.Udt, typeof(object) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.VarBinary, typeof(byte[]) },
            { SqlDbType.VarChar, typeof(string) },
            { SqlDbType.Variant, typeof(object) },
            { SqlDbType.Xml, typeof(string) }
        };

        #endregion

        #region Mapping of SqlServer parameter mode to SqlCommand Parameter Direction enum.
        private static readonly Dictionary<string, ParameterDirection> SqlParameterModeToSqlParameterDirections = new Dictionary<string, ParameterDirection>
        {
            { "in", ParameterDirection.Input },
            { "out", ParameterDirection.ReturnValue },
            { "inout", ParameterDirection.Output }
        };

        #endregion

        #region Conversion Methods

        /// <summary>Converts SqlDataType string to SqlDbType enum. </summary>
        /// <param name="dataType"> String data type bigint, int ... </param>
        public static SqlDbType SqlDataTypeToSqlDbType(string dataType)
        {
            return SqlDataTypeToSqlDbTypes[dataType.ToLower()];
        }

        /// <summary>Converts SqlDataType string to C# Type. </summary>
        /// <param name="dataType"> String data type ie bigint, int ... </param>
        public static Type SqlDataTypeToCSharpType(string dataType)
        {
            return SqlDataTypeToCSharpTypes[dataType.ToLower()];
        }

        /// <summary>Converts SqlParameterMode to ParameterDirection enum. </summary>
        /// <param name="mode"> Parameter mode  string ie in, out, inout. </param>
        public static ParameterDirection SqlParameterModeToParameterDirection(string mode)
        {
            return SqlParameterModeToSqlParameterDirections[mode.ToLower()];
        }

        #endregion
    }
}