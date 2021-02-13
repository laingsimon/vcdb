EXEC sp_rename 
    @objname = 'dbo.Person.Name', 
    @newname = 'FullName',
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = '[dbo].[DF__Person__Name__/[A-F0-9]{8}/]',
    @newname = '[DF__Person__FullName__/[A-F0-9]{8}/]',
    @objtype = 'OBJECT'
GO
