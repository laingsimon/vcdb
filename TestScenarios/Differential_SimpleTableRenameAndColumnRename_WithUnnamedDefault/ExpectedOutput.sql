EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename 
    @objname = 'dbo.People.Name', 
    @newname = 'FullName', 
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = '[dbo].[DF__Person__Name__35BCFE0A]',
    @newname = '[DF__People__FullName__35BCFE0A]',
    @objtype = 'OBJECT'
GO