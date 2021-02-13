EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename 
    @objname = 'dbo.CK__Person__Name__/[A-F0-9]{8}/',
    @newname = 'CK__People__Name__/[A-F0-9]{8}/', 
    @objtype = 'OBJECT'
GO
