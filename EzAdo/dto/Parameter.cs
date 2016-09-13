namespace EzAdo.dto
{
    /// <summary>   A dto.Parameter populated in the ProcedureFactory and utilized in building the model.Parameters. </summary>
    public class Parameter
    {
        /// <summary>   Gets or sets the ordinal position. </summary>
        /// <value> The ordinal position. </value>
        public int OrdinalPosition { set; get; }

        /// <summary>   Gets or sets the parameter mode. </summary>
        /// <value> The parameter mode. </value>
        public string ParameterMode { set; get; }

        /// <summary>   Gets or sets a value indicating whether this parameter is a result. </summary>
        /// <value> true if this parameter is a result, false if not.. </value>
        public string IsResult { set; get; }

        /// <summary>   Gets or sets the name of the parameter. </summary>
        /// <value> The name of the parameter. </value>
        public string ParameterName { set; get; }

        /// <summary>   Gets or sets the SqlServer data type name. </summary>
        /// <value> The data type name. </value>
        public string DataType { set; get; }

        /// <summary>   Gets or sets the numeric precision. </summary>
        /// <value> The numeric precision. </value>
        public byte? NumericPrecision { set; get; }

        /// <summary>   Gets or sets the numeric scale. </summary>
        /// <value> The numeric scale. </value>
        public int? NumericScale { set; get; }

        /// <summary>   Gets or sets the maximum character length. </summary>
        /// <value> The maximum character length. </value>
        public int? CharacterMaximumLength { set; get; }

        /// <summary>   Gets or sets the date time precision. </summary>
        /// <value> The date time precision. </value>
        public short? DateTimePrecision { set; get; }

        /// <summary>   Gets or sets the numeric minimum value. </summary>
        /// <value> The numeric minimum value. </value>
        public long? NumericMinimumValue { set; get; }

        /// <summary>   Gets or sets the numeric maximum value. </summary>
        /// <value> The numeric maximum value. </value>
        public long? NumericMaximumValue { set; get; }

        /// <summary>   Gets or sets the regular expression. </summary>
        /// <value> The regular expression. </value>
        public string RegularExpression { set; get; }

        /// <summary>   Gets or sets a value indicating whether this object is nullable. </summary>
        /// <value> true if this object is nullable, false if not. </value>
        public bool IsNullable { set; get; }

        /// <summary>   Gets or sets the user defined type schema. </summary>
        /// <value> The user defined type schema. </value>
        public string UserDefinedTypeSchema { set; get; }

        /// <summary>   Gets or sets the name of the user defined type. </summary>
        /// <value> The name of the user defined type. </value>
        public string UserDefinedTypeName { set; get; }
    }
}
