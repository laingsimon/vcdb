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
	@objname = 'NewSchema.People.IX_Person_Name',
	@newname = 'IX_People_Name',
	@objtype = 'INDEX'
GO
DROP SCHEMA [OldSchema]
GO