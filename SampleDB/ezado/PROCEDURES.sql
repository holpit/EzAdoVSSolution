﻿CREATE PROCEDURE [ezado].[PROCEDURES]
WITH EXECUTE AS OWNER
AS
BEGIN
SELECT
	SPECIFIC_SCHEMA specificSchema,
	SPECIFIC_NAME specificName,
	ROUTINE_TYPE routineType,
	CASE 
		WHEN CHARINDEX('*Returns Json*',ROUTINE_DEFINITION) > 0 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END isJsonResult,
	CASE 
		WHEN CHARINDEX('*Single Result*',ROUTINE_DEFINITION) > 0 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
		END isSingleResult,
	CASE 
		WHEN CHARINDEX('*Always Encrypted*',ROUTINE_DEFINITION) > 0 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END isAlwaysEncrypted,
	CASE 
		WHEN CHARINDEX('*Non Query*',ROUTINE_DEFINITION) > 0 THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END isNonQuery,
	(SELECT
		ISP.ORDINAL_POSITION ordinalPosition,
		ISP.PARAMETER_MODE parameterMode,
		ISP.IS_RESULT isResult,
		CASE ISP.PARAMETER_NAME
			WHEN '' THEN '@RETURN_VALUE'
			ELSE ISP.PARAMETER_NAME
		END parameterName,
		ISP.DATA_TYPE dataType,
		ISP.CHARACTER_MAXIMUM_LENGTH characterMaximumLength,
		ISP.NUMERIC_PRECISION numericPrecision,
		ISP.NUMERIC_SCALE numericScale,
		ISP.DATETIME_PRECISION datetimePrecision,
		ISP.USER_DEFINED_TYPE_SCHEMA userDefinedTypeSchema,
		ISP.USER_DEFINED_TYPE_NAME userDefinedTypeName,
		CASE 
		WHEN CHARINDEX(CONCAT('IF ', ISP.PARAMETER_NAME, ' IS NULL THROW'),ROUTINE_DEFINITION) > 0 THEN CAST(0 AS BIT)
		ELSE CAST(1 AS BIT)
		END isNullable,
		RSV.NUMERIC_MINIMUM_VALUE numericMinimumValue,
		RSV.NUMERIC_MAXIMUM_VALUE numericMaximumValue,
		RSV.REGULAR_EXPRESSION regularExpression
	FROM
		INFORMATION_SCHEMA.PARAMETERS ISP LEFT JOIN dbo.REST_SQL_VALIDATORS RSV ON 
		ISP.SPECIFIC_SCHEMA = RSV.SPECIFIC_SCHEMA AND
		ISP.SPECIFIC_NAME = RSV.SPECIFIC_NAME AND
		ISP.PARAMETER_NAME = RSV.PARAMETER_NAME
	WHERE
		ISR.SPECIFIC_SCHEMA = ISP.SPECIFIC_SCHEMA AND
		ISR.SPECIFIC_NAME = ISP.SPECIFIC_NAME FOR JSON PATH) parameters
	FROM INFORMATION_SCHEMA.ROUTINES ISR
	WHERE [SPECIFIC_SCHEMA] IN ('open', 'trusted')
	FOR JSON PATH;

	RETURN 200;
END;