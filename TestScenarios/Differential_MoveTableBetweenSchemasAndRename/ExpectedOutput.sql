ALTER SCHEMA [NewSchema]
TRANSFER [dbo].[Person]
GO
EXEC sp_rename
    @objname = 'NewSchema.Person',
    @newname = 'People',
    @objtype = 'OBJECT'
GO
