using System.Collections.Generic;

namespace EzAdo.dto
{
    /// <summary>A dto.Procedure populated in the ProcedureFactory and utilized in building the model.Procedures.</summary>
    public class Procedure
    {
        /// <summary>Gets or sets the specific schema the procedures is a child of.</summary>
        public string SpecificSchema { set; get; }

        /// <summary>   Gets or sets the specific name of the procedure. </summary>
        public string SpecificName { set; get; }

        /// <summary>   Gets or sets the type of the routine function|procedure. </summary>
        public string RoutineType { set; get; }

        /// <summary>   Gets or sets a value indicating whether this Procedure returns Json. </summary>
        public bool isJsonResult { set; get; }

        /// <summary>   Gets or sets a value indicating whether this procedures returns a single row. </summary>
        public bool IsSingleResult { set; get; }

        /// <summary>Gets or sets a value indicating whether this procedure touches columns that use always encrypted.</summary>
        public bool IsAlwaysEncrypted { set; get; }

        /// <summary>Gets or sets a value indicating whether this procedure is non query.</summary>
        public bool IsNonQuery { set; get; }

        /// <summary>Collection of parameters associated with the procedure.</summary>
        public List<Parameter> Parameters { set; get; }

        /// <summary>Gets or sets the connection string.  The connection string is retrieved from app.config where the name maps to the schema name of the procedure.</summary>
        public string ConnectionString { set; get; }
        
    }
}
