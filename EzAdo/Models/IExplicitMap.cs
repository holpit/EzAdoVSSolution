using System.Data.SqlClient;

namespace EzAdo.Models
{
    /// <summary>   Interface for explicit map.  Objects marked as implementing this interface can be mapped explicitly in the Execute methods of the procedures. </summary>
    public interface IExplicitMap
    {
        /// <summary>   Implementations of this method will map object.Property to rdr.GetType(idx) </summary>
        /// <param name="procedureName">    Name of the procedure where this mapping is applicable. </param>
        /// <param name="rdr">              The reader. </param>
        void Map(string procedureName, SqlDataReader rdr);

        /// <summary>   This method asks the object if it supports the particular mapping. </summary>
        /// <param name="procedureName">    Name of the procedure where this mapping is applicable. </param>
        /// <returns>   true if the procedure is supported, false if not. </returns>
        bool SupportsMap(string procedureName);
    }
}
