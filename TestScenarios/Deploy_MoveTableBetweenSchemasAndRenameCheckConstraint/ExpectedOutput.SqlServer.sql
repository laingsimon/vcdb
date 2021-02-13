ALTER SCHEMA [NewSchema]
TRANSFER [dbo].[Person]
GO
EXEC sp_rename
    @objname = 'NewSchema.CK_Person_ValidName', 
    @newname = 'CK_Person_StillValidName', 
    @objtype = 'OBJECT'
GO