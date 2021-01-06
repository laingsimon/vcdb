CREATE SCHEMA [NewSchema]
GO
EXEC sp_rename 
	@objname = 'OldSchema.Person',
	@newname = 'People',
	@objtype = 'OBJECT'
GO
ALTER SCHEMA [NewSchema]
TRANSFER [OldSchema].[People]
GO
EXEC sp_rename 
	@objname = 'NewSchema.People.Name',
	@newname = 'FullName',
	@objtype = 'COLUMN'
GO
DROP SCHEMA [OldSchema]
GO