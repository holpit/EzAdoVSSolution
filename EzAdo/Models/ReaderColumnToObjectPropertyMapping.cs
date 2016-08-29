using System.Data.SqlClient;
using System.Reflection;

namespace EzAdo.Models
{
    /// <summary>
    /// A ReaderColumnToObjectPropertyMapping is utilized in the Execute T calls when the response is not a json result and the object does not implement IExplicit.
    /// The mapping aligns the SqlDataReader column to a property in a C# object where
    /// object.ColumnName = SqlDataReader["COLUMN_NAME"]. That alignment is used to set the ordinal position in the mapping.
    /// </summary>
    public class ReaderColumnToObjectPropertyMapping
    {
        private int _position;
        private PropertyInfo _property;
       
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Ordinal position of the reader</param>
        /// <param name="property">Property info of the class</param>
        public ReaderColumnToObjectPropertyMapping(int position, PropertyInfo property)
        {
            _position = position;
            _property = property;
        }

        /// <summary>
        /// Sets the property of object represented by property info to value of reader at position
        /// </summary>
        /// <param name="obj">Object to set the property on</param>
        /// <param name="rdr">SqlDataReader with value at index of position</param>
        public void SetValue(object obj, SqlDataReader rdr)
        {
            if(!rdr.IsDBNull(_position))
            {
                _property.SetValue(obj, rdr[_position]);
            }
        }
    }
}
