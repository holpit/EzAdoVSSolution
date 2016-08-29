using System.Collections.Generic;

namespace EzAdo.dto
{
    public class Procedure
    {
        public string SpecificSchema { set; get; }
        public string SpecificName { set; get; }
        public string RoutineType { set; get; }
        public bool IsJsonResult { set; get; }
        public bool IsSingleResult { set; get; }
        public bool IsAlwaysEncrypted { set; get; }
        public bool IsNonQuery { set; get; }
        public List<Parameter> Parameters { set; get; }
        public string ConnectionString { set; get; }
        public Dictionary<string,string> ParameterNameMap { set; get; }
    }
}
