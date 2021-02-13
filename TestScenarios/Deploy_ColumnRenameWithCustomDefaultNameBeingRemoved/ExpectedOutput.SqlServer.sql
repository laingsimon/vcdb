EXEC sp_rename 
    @objname = 'dbo.Person.Deleted', 
    @newname = 'IsDeleted',
    @objtype = 'COLUMN'
GO
EXEC sp_rename 
    @objname = '[dbo].[DF_PersonActiveByDefault]', 
    @newname = '[DF__Person__IsDeleted__/[A-F0-9]{8}/]', 
    @objtype = 'OBJECT'
GO
