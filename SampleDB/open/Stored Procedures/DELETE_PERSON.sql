﻿CREATE PROCEDURE [open].[DELETE_PERSON]
(
	/*Non Query*/

	@PERSON_ID INT,

	@SQL_ERROR_ID BIGINT OUTPUT,
	@MESSAGE_RESULT VARCHAR(256) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON;

	IF @PERSON_ID IS NULL THROW 50001, '@PERSON_ID', 1;
	
	BEGIN TRY

		DELETE FROM dbo.PERSON WHERE PERSON_ID = @PERSON_ID;

		RETURN 200;

	END TRY
	BEGIN CATCH

		SET @SQL_ERROR_ID = 21;
		SET @MESSAGE_RESULT = (SELECT 500 AS messageId FOR Json PATH, WITHOUT_ARRAY_WRAPPER);
		RETURN 500;

	END CATCH

END;