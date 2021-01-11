EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename
    @objname = '[dbo].[DF__Person__Name__35BCFE0A]',
    @newname = '[DF__People__Name__35BCFE0A]',
    @objtype = 'OBJECT'
GO