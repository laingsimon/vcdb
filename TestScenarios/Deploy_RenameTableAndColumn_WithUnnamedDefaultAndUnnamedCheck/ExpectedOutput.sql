EXEC sp_rename 
    @objname = 'dbo.Person', 
    @newname = 'People', 
    @objtype = 'OBJECT'
GO
ALTER TABLE [dbo].[People]
DROP CONSTRAINT [CK__Person__Name__35BCFE0A]
GO
EXEC sp_rename 
    @objname = 'dbo.People.Name', 
    @newname = 'FullName', 
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[People]
ADD CONSTRAINT [CK__People__7024CCA]
CHECK (len([FullName])>3)
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__People__' + col.name + '__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = chk.parent_object_id
WHERE tab.name = 'People'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND chk.name = 'CK__People__7024CCA'

EXEC sp_rename 
    @objname = 'dbo.CK__People__7024CCA', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO