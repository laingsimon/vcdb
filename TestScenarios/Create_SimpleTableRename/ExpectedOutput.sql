EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'dbo.People', 
    @objtype = 'OBJECT'
GO