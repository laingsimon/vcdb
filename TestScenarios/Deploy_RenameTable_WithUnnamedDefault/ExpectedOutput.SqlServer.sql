EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename
    @objname = '[dbo].[DF__Person__Name__/[A-F0-9]{8}/]',
    @newname = '[DF__People__Name__/[A-F0-9]{8}/]',
    @objtype = 'OBJECT'
GO
