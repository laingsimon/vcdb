EXEC sp_rename 
    @objname = 'dbo.Person.Deleted', 
    @newname = 'IsDeleted',
    @objtype = 'COLUMN'
GO
EXEC sp_rename 
    @objname = '[dbo].[DF_PersonActiveByDefault]', 
    @newname = '[DF__Person__IsDeleted__35BCFE0A]', 
    @objtype = 'OBJECT'
GO