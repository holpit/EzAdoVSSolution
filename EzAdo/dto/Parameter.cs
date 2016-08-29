namespace EzAdo.dto
{
    public class Parameter
    {
        public int OrdinalPosition { set; get; }
        public string ParameterMode { set; get; }
        public string IsResult { set; get; }
        public string ParameterName { set; get; }
        public string DataType { set; get; }
        public byte? NumericPrecision { set; get; }
        public int? NumericScale { set; get; }
        public int? CharacterMaximumLength { set; get; }
        public short? DateTimePrecision { set; get; }
        public long? NumericMinimumValue { set; get; }
        public long? NumericMaximumValue { set; get; }
        public string RegularExpression { set; get; }
        public bool IsNullable { set; get; }
        public string UserDefinedTypeSchema { set; get; }
        public string UserDefinedTypeName { set; get; }
    }
}
