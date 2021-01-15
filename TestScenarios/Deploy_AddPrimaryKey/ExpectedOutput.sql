ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [PK__Person__Id__Name]
PRIMARY KEY ([Id], [Name])
GO
DECLARE @newName VARCHAR(1024)
SELECT @newName = 'PK__Person__' + col.name + '__' + FORMAT(k.OBJECT_ID, 'X')
FROM sys.key_constraints k
INNER JOIN sys.index_columns ic
ON ic.object_id = k.parent_object_id
INNER JOIN sys.columns col
ON col.column_id = ic.column_id
AND col.object_id = k.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = k.parent_object_id
WHERE tab.name = 'Person'
AND SCHEMA_NAME(tab.schema_id) = 'dbo'
AND k.name = 'PK__Person__Id__Name'

EXEC sp_rename 
    @objname = 'dbo.PK__Person__Id__Name', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO
