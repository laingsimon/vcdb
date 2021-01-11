CREATE SCHEMA [NewSchema]
GO
ALTER SCHEMA [NewSchema]
TRANSFER [OldSchema].[Person]
GO
DROP SCHEMA [OldSchema]
GO
EXEC sp_rename 
	@objname = 'NewSchema.Person',
	@newname = 'People',
	@objtype = 'OBJECT'
GO
