CREATE TABLE [dbo].[Person] (
	[Id] int identity NOT NULL,
	[Name] nvarchar(255) NOT NULL,
	[Title] varchar(10),
	[Age] int,
	[Price] decimal(18, 2),
	[DoB] date,
	[Deleted] bit NOT NULL,
	[UniqueId] uniqueidentifier
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF__Person__Deleted]
DEFAULT (0)
FOR [Deleted]
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__Person__Deleted__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND def.name = 'DF__Person__Deleted'

EXEC sp_rename
    @objname = 'DF__Person__Deleted',
    @newname = @newName,
    @objtype = 'OBJECT'
GO