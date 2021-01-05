EXEC sp_rename 
    @objname = 'dbo.Person.Name', 
    @newname = 'FullName',
    @objtype = 'COLUMN'
GO