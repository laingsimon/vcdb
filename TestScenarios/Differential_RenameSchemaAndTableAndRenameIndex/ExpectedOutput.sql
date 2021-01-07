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
EXEC sp_rename 
	@objname = 'NewSchema.People.IX_Person_Name',
	@newname = 'IX_People_Name',
	@objtype = 'INDEX'
GO
