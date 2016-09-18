using System.Data.SqlClient;
using System.Reflection;

namespace EzAdo.Models
{
    /// <summary>A ReaderColumnToObjectPropertyMapping object is utilized in the Procedure.Execute&lt;T&gt; calls when the response is not a Json result and the object does not implement IExplicit. The mapping aligns the SqlDataReader(index) to a property in a C# object where case resolves from COLUMN_NAME to PropertyName. These mappings are then stored for future use based on the object type and procedure name.</summary>
    public class ReaderColumnToObjectPropertyMapping
    {
        //The ordinal position of the data reader.
        private int _position;

        //The property info responsible for the set value call.
        private PropertyInfo _property;

        /// <summary>Constructor.</summary>
        /// <param name="position">The ordinal position of the data reader.</param>
        /// <param name="property">The property info responsible for the set value call.</param>
        public ReaderColumnToObjectPropertyMapping(int position, PropertyInfo property)
        {
            _position = position;
            _property = property;
        }

        /// <summary>Sets the property of object represented by property info to value of reader at position.</summary>
        /// <param name="obj">Object to set the property on.</param>
        /// <param name="rdr">SqlDataReader with value at index of position.</param>
        public void SetValue(object obj, SqlDataReader rdr)
        {
            if(!rdr.IsDBNull(_position))
            {
                _property.SetValue(obj, rdr[_position]);
            }
        }
    }
}
