using System.Collections.Generic;

namespace EzAdo.dto
{
    /// <summary>   A dto.Procedure populated in the ProcedureFactory and utilized in building the model.Procedures.  </summary>
    public class Procedure
    {
        /// <summary>   Gets or sets the specific schema the procedures is a child of. </summary>
        /// <value> The specific schema name. </value>
        public string SpecificSchema { set; get; }

        /// <summary>   Gets or sets the specific name of the procedure. </summary>
        /// <value> The specific name of the procedure. </value>
        public string SpecificName { set; get; }

        /// <summary>   Gets or sets the type of the routine function|procedure. </summary>
        /// <value> The type of the routine. </value>
        public string RoutineType { set; get; }

        /// <summary>   Gets or sets a value indicating whether this Procedure returns Json. </summary>
        /// <value> true if the procedure is annotated with *Returns Json*, false if not. </value>
        public bool IsJsonResult { set; get; }

        /// <summary>   Gets or sets a value indicating whether this procedures returns a single row. </summary>
        /// <value> true if the procedure is annotated with *Single Result*, false if not. </value>
        public bool IsSingleResult { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this procedure touches columns that use always encrypted.
        /// </summary>
        /// <value> true if this procedure is annotated with *Always Encrypted* , false if not. </value>
        public bool IsAlwaysEncrypted { set; get; }

        /// <summary>   Gets or sets a value indicating whether this procedure is non query. </summary>
        /// <value> true if this procedure is annotated with *Non Query*, false if not. </value>
        public bool IsNonQuery { set; get; }

        /// <summary>   Collection of parameters associated with the procedure. </summary>
        /// <value> Parameters Collection. </value>
        public List<Parameter> Parameters { set; get; }

        /// <summary>   Gets or sets the connection string.  The connection string is retrieved from app.config where the name
        ///             maps to the schema name of the procedure. </summary>
        /// <value> The connection string. </value>
        public string ConnectionString { set; get; }
        
    }
}
