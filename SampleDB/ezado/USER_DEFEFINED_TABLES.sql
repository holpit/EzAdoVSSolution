﻿CREATE PROCEDURE [ezado].[USER_DEFEFINED_TABLES]
(
	@SCHEMAS ezado.Schema_Name_Table READONLY
)
WITH EXECUTE AS OWNER
AS
BEGIN
	WITH CTE AS (SELECT
		SCHEMA_NAME(TT.schema_id) SPECIFIC_SCHEMA,
		TT.name SPECIFIC_NAME,
		C.name AS COLUMN_NAME,
		ST.name DATA_TYPE,
		C.Is_Nullable IS_NULLABLE,
		C.max_length CHARACTER_MAXIMUM_LENGTH,
		C.precision NUMERIC_PRECISION,
		C.scale NUMERIC_SCALE,
		C.column_id COLUMN_ID
	FROM sys.table_types TT
	JOIN sys.columns C
		ON TT.type_table_object_id = C.object_id
	JOIN sys.systypes AS ST  
		ON ST.xtype = C.system_type_id
	WHERE TT.is_user_defined = 1)
	SELECT 
		SPECIFIC_SCHEMA,
		SPECIFIC_NAME,
		COLUMN_NAME,
		DATA_TYPE,
		IS_NULLABLE,
		CHARACTER_MAXIMUM_LENGTH,
		NUMERIC_PRECISION,
		NUMERIC_SCALE
	FROM CTE WHERE CTE.DATA_TYPE != 'sysname' AND
	CTE.SPECIFIC_SCHEMA IN (SELECT [SCHEMA_NAME] FROM @SCHEMAS)
	ORDER BY SPECIFIC_SCHEMA, SPECIFIC_NAME, COLUMN_ID;
END;