EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename 
    @objname = '[dbo].[CK__Person__Name__35BCFE0A]', 
    @newname = '[CK__People__Name__35BCFE0A]', 
    @objtype = 'OBJECT'
GO
