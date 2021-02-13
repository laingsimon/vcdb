EXEC sp_rename
    @objname = 'dbo.RegularProcedure',
    @newname = 'NewRegularProcedure',
    @objtype = 'OBJECT'
GO
EXEC sp_rename
    @objname = 'dbo.EncryptedProcedure',
    @newname = 'NewEncryptedProcedure',
    @objtype = 'OBJECT'
GO
ALTER PROCEDURE [dbo].[NewEncryptedProcedure]
(
    @name varchar(255)
)
WITH ENCRYPTION
AS
BEGIN
    SELECT @name
END
GO