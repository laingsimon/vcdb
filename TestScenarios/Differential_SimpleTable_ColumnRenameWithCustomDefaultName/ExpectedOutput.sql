EXEC sp_rename 
    @objname = 'dbo.Person.Deleted', 
    @newname = 'IsDeleted',
    @objtype = 'COLUMN'
GO