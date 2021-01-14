ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [DF_Person_DefaultName]
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_Person_DefaultName]
DEFAULT ('Jane Doe')
FOR [Name]
GO
ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [DF__Person__Age__35BCFE0A]
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF__Person__Age]
DEFAULT (-1)
FOR [Age]
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__Person__Age__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND def.name = 'DF__Person__Age'

EXEC sp_rename 
    @objname = 'DF__Person__Age', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO
