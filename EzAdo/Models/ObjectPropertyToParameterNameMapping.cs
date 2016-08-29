using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EzAdo.Models
{
    /// <summary>
    /// A ObjectPropertyToParameterNameMapping is utilized in the LoadFromObject T calls when the procedure parameters are loaded from an object.
    /// The mapping aligns the Procedures ParmaterName to a property in a C# object where
    /// object.ParameterName = Procedure["@PARAMETER_NAME"].
    public class ObjectPropertyToParameterNameMapping
    {
        //Procedure ParmaterName
        private string _parameterName;

        //Object Property
        private PropertyInfo _property;

        /// <summary>
        /// Constructor takes the parmeter name and object property
        /// </summary>
        /// <param name="parameterName">Procedure Parameter Name</param>
        /// <param name="property">Object Property</param>
        public ObjectPropertyToParameterNameMapping(string parameterName, PropertyInfo property)
        {
            _parameterName = parameterName;
            _property = property;
        }

        /// <summary>
        /// Sets the value of the parmeter for the procedure
        /// </summary>
        /// <param name="obj">Object supplying the value</param>
        /// <param name="proc">Procedure receiving the value</param>
        public void SetValue(object obj, Procedure proc)
        {
            proc.SetValue(_parameterName, _property.GetValue(obj));
        }
    }
}
