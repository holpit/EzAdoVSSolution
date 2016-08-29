﻿CREATE PROCEDURE [open].[PERSON_BY_ID_JSON]
(
	/*Single Result*/
	/*Returns Json*/

	@PERSON_ID INT,

	@SQL_ERROR_ID BIGINT OUTPUT,
	@MESSAGE_RESULT VARCHAR(256) OUTPUT
)
AS
BEGIN

	BEGIN TRY

		IF @PERSON_ID IS NULL THROW 50001, '@PERSON_ID', 1;

		SELECT
			PERSON_ID,
			LAST_NAME,
			FIRST_NAME,
			EMAIL
		FROM
			dbo.PERSON
		WHERE
			PERSON_ID = @PERSON_ID
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;

		RETURN 200;

	END TRY
	BEGIN CATCH
	
		SET @SQL_ERROR_ID = 1;
		SET @MESSAGE_RESULT = 'A MESSAGE';

		RETURN 500;

	END CATCH

END;