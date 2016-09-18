namespace EzAdo.dto
{
    /// <summary>A dto.Parameter populated in the ProcedureFactory and utilized in building the model.Parameters.</summary>
    public class Parameter
    {
        /// <summary>Gets or sets the ordinal position.</summary>
        public int OrdinalPosition { set; get; }

        /// <summary>Gets or sets the parameter mode.</summary>
        public string ParameterMode { set; get; }

        /// <summary>Gets or sets a value indicating whether this parameter is a result.</summary>
        public string IsResult { set; get; }

        /// <summary>Gets or sets the name of the parameter.</summary>
        public string ParameterName { set; get; }

        /// <summary>Gets or sets the SqlServer data type name.</summary>
        public string DataType { set; get; }

        /// <summary>Gets or sets the numeric precision.</summary>
        public byte? NumericPrecision { set; get; }

        /// <summary>Gets or sets the numeric scale.</summary>
        public int? NumericScale { set; get; }

        /// <summary>Gets or sets the maximum character length.</summary>
        public int? CharacterMaximumLength { set; get; }

        /// <summary>Gets or sets the date time precision.</summary>
        public short? DateTimePrecision { set; get; }

        /// <summary>Gets or sets the numeric minimum value.</summary>
        public long? NumericMinimumValue { set; get; }

        /// <summary>Gets or sets the numeric maximum value.</summary>
        public long? NumericMaximumValue { set; get; }

        /// <summary>Gets or sets the regular expression.</summary>
        public string RegularExpression { set; get; }

        /// <summary>Gets or sets a value indicating whether this object is nullable.</summary>
        public bool IsNullable { set; get; }

        /// <summary>Gets or sets the user defined type schema.</summary>
        public string UserDefinedTypeSchema { set; get; }

        /// <summary>Gets or sets the name of the user defined type.</summary>
        public string UserDefinedTypeName { set; get; }
    }
}
