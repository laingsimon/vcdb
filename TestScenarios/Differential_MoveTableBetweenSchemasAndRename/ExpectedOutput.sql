EXEC sp_rename
    @objname = 'dbo.Person',
    @newname = 'People',
    @objtype = 'OBJECT'
GO
ALTER SCHEMA [NewSchema]
TRANSFER [dbo].[People]
GO