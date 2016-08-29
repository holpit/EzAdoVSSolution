using System;
using System.Collections.Generic;
using System.Data;
using EzAdo.Extensions;
using EzAdo.Models;

namespace EzAdo.Converters
{
    public static class DtoProcedureToModelProcedure
    {
        /// <summary>
        /// Routine to handle the conversion of the dto representation to actual implementation
        /// </summary>
        /// <param name="dtoProc">dto.Procedure to convert</param>
        /// <returns>models.Procedure</returns>
        public static Procedure Convert(dto.Procedure dtoProc)
        {
            //Only place this should ever be set!
            string procedureName = $"[{dtoProc.SpecificSchema}].[{dtoProc.SpecificName}]";

            Dictionary<string, Models.Parameter> parameters = buildParameters(dtoProc);
            Dictionary<string, string> parameterNameMapping = buildParameterNameMapping(dtoProc);

            Procedure proc = new Procedure
                (
                    dtoProc.ConnectionString,
                    procedureName, dtoProc.IsJsonResult,
                    dtoProc.IsSingleResult,
                    dtoProc.IsNonQuery,
                    parameters,
                    parameterNameMapping
                );

            return proc;
        }

        /// <summary>
        /// Builds the models.Parameters from the dto.Parameters
        /// </summary>
        /// <param name="dtoProc">dto.Procedure to convert</param>
        /// <returns>models.Parameters</returns>
        private static Dictionary<string, Models.Parameter> buildParameters(dto.Procedure dtoProc)
        {
            Dictionary<string,Parameter> parms = new Dictionary<string, Parameter>();

            foreach (dto.Parameter dtoParm in dtoProc.Parameters)
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
                    dtoParm.NumericMaximumValue, dtoParm.NumericMaximumValue, dtoParm.RegularExpression,
                    dtoParm.IsNullable, type, dataTypeName );
                parms.Add(dtoParm.ParameterName, mdlParm);
            }

            return parms;
        }

        /// <summary>
        /// Builds the models.ParameterNameMaps
        /// </summary>
        /// <param name="dtoProc">dto.Procedure to convert</param>
        /// <returns>models.</returns>
        private static Dictionary<string, string> buildParameterNameMapping(dto.Procedure dtoProc)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            foreach (dto.Parameter dtoParm in dtoProc.Parameters)
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
