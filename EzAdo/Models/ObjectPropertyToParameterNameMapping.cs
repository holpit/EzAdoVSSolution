using System.Reflection;

namespace EzAdo.Models
{
    /// <summary>A ObjectPropertyToParameterNameMapping is utilized in the LoadFromObject T calls when the procedure parameters are loaded from an object. The mapping aligns the Procedures ParamaterName to a property in a C# object where object.ParameterName = Procedure["@PARAMETER_NAME"].</summary>
    public class ObjectPropertyToParameterNameMapping
    {
        private string _parameterName;
        private PropertyInfo _property;

        /// <summary>Constructor.</summary>
        /// <param name="parameterName">Name of the parameter that is mapped.</param>
        /// <param name="property">The property of the object that is mapped.</param>
        public ObjectPropertyToParameterNameMapping(string parameterName, PropertyInfo property)
        {
            _parameterName = parameterName;
            _property = property;
        }

        /// <summary>Sets the value of the parameter from the object.Property.</summary>
        /// <param name="obj">Object supplying the value.</param>
        /// <param name="proc">Procedure receiving the value.</param>
        public void SetValue(object obj, Procedure proc)
        {
            proc.SetValue(_parameterName, _property.GetValue(obj));
        }
    }
}
