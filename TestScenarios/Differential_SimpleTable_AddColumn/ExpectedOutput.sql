ALTER TABLE [dbo].[Person]
ADD [NewDefaultedColumn] bit NOT NULL
GO
ALTER TABLE [dbo].[Person]
ADD [NewCheckedColumn] int NOT NULL
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF__Person__NewDefaultedColumn__0]
DEFAULT (0)
FOR [NewDefaultedColumn]
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__Person__NewDefaultedColumn__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND def.name = 'DF__Person__NewDefaultedColumn__0'

EXEC sp_rename
    @objname = 'DF__Person__NewDefaultedColumn__0',
    @newname = @newName,
    @objtype = 'OBJECT'
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK__Person__NewCheckedColumn__0]
CHECK ([NewCheckedColumn]<5)
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__Person__NewCheckedColumn__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND chk.name = 'CK__Person__NewCheckedColumn__0'

EXEC sp_rename
    @objname = 'CK__Person__NewCheckedColumn__0',
    @newname = @newName,
    @objtype = 'OBJECT'
GO