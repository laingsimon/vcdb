CREATE OR ALTER PROCEDURE [dbo].[EncryptedProcedure]
(
    @name varchar(255)
)
WITH ENCRYPTION
AS
BEGIN
    SELECT @name
END
