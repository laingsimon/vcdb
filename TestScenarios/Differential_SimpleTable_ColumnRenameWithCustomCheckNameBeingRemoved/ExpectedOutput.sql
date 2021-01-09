EXEC sp_rename 
    @objname = 'dbo.Person.FullName', 
    @newname = 'Name',
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK_Person_ValidName]
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK__Person__Name__0]
CHECK (len([Name])>3)
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__Person__Name__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND chk.name = 'CK__Person__Name__0'

EXEC sp_rename 
    @objname = 'CK__Person__Name__0', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO