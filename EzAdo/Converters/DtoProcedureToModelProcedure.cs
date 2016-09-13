using System;
using System.Collections.Generic;
using System.Data;
using EzAdo.Extensions;
using EzAdo.Models;

namespace EzAdo.Converters
{
    /// <summary>   A dto.procedure to model.procedure converter </summary>
    public static class DtoProcedureToModelProcedure
    {
        /// <summary>   Routine to handle the conversion of the dto.Procedure to model.Procedure. </summary>
        /// <param name="source">   Source dto.Procedure to convert. </param>
        /// <returns>   Converted Procedure. </returns>
        public static Procedure Convert(dto.Procedure source)
        {
            //Only place this should ever be set!
            string procedureName = $"[{source.SpecificSchema}].[{source.SpecificName}]";

            Dictionary<string, Models.Parameter> parameters = buildParameters(source);
            Dictionary<string, string> parameterNameMapping = buildParameterNameMapping(source);

            Procedure proc = new Procedure
                (
                    source.ConnectionString,
                    procedureName, source.IsJsonResult,
                    source.IsSingleResult,
                    source.IsNonQuery,
                    parameters,
                    parameterNameMapping
                );

            return proc;
        }

        /// <summary>   Builds the model.Parameters from the dto.Parameter definitions. </summary>
        /// <param name="source">   Source dto.Procedure to convert. </param>
        /// <returns>   Dictionary of model.Parameters </returns>
        private static Dictionary<string, Models.Parameter> buildParameters(dto.Procedure source)
        {
            Dictionary<string,Parameter> parms = new Dictionary<string, Parameter>();

            foreach (dto.Parameter dtoParm in source.Parameters)
            {
                string dataTypeName = null;
                if(dtoParm.UserDefinedTypeSchema != null)
                {
                    dataTypeName = $"[{dtoParm.UserDefinedTypeSchema}].[{dtoParm.UserDefinedTypeName}]";
                }
                ParameterDirection direction = SqlDataTypeMappings.SqlParameterModeToParameterDirection(dtoParm.ParameterMode);
                Type type = SqlDataTypeMappings.SqlDataTypeToCSharpType(dtoParm.DataType);
                SqlDbType sqlDbType = SqlDataTypeMappings.SqlDataTypeToSqlDbType(dtoParm.DataType);

                Parameter mdlParm = new Parameter(
                    dtoParm.ParameterName, direction, sqlDbType, dtoParm.CharacterMaximumLength,
                    dtoParm.DateTimePrecision, dtoParm.NumericScale, dtoParm.NumericPrecision,
                    dtoParm.NumericMinimumValue, dtoParm.NumericMaximumValue, dtoParm.RegularExpression,
                    dtoParm.IsNullable, type, dataTypeName );
                parms.Add(dtoParm.ParameterName, mdlParm);
            }

            return parms;
        }

        /// <summary>   Builds parameter name mapping.  The parameter name map allows quick mapping of friendly names to the actual parameter name
        ///             ie. @FIRST_NAME maps to @FIRST_NAME, FIRST_NAME, FirstName and firstName </summary>
        /// <param name="source">   Source dto.Procedure containing the native parameter name. </param>
        /// <returns>   Dictionary of name mappings with the native parameter name as the key; </returns>
        private static Dictionary<string, string> buildParameterNameMapping(dto.Procedure source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            foreach (dto.Parameter dtoParm in source.Parameters)
            {
                string parameterName = dtoParm.ParameterName;
                string shortName = dtoParm.ParameterName.Substring(1); //pulls off @
                string jsonName = shortName.ToCamel();
                string propertyName = shortName.ToProperCase();

                result.Add(parameterName, parameterName);
                result.Add(shortName, parameterName);
                result.Add(jsonName, parameterName);
                result.Add(propertyName, parameterName);
            }
            return result;
        }

    }
}
