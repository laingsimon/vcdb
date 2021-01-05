ALTER TABLE [dbo].[Person]
ADD [NewColumn] bit NOT NULL
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF__Person__NewColumn__0]
DEFAULT (0)
FOR [NewColumn]
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__Person__NewColumn__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND def.name = 'DF__Person__NewColumn__0'

EXEC sp_rename
    @objname = 'DF__Person__NewColumn__0',
    @newname = @newName,
    @objtype = 'OBJECT'
GO