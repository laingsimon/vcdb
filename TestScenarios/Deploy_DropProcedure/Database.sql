CREATE PROCEDURE [dbo].[EncryptedProcedure]
(
    @id int
)
WITH ENCRYPTION 
AS
BEGIN
    SELECT @id * 2
END
GO
