EXEC sp_rename 
    @objname = 'dbo.Person.Name', 
    @newname = 'FullName',
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = '[dbo].[DF__Person__Name__35BCFE0A]',
    @newname = '[DF__Person__FullName__35BCFE0A]',
    @objtype = 'OBJECT'
GO