CREATE SCHEMA [NewSchema]
GO
ALTER SCHEMA [NewSchema]
TRANSFER [OldSchema].[Person]
GO
DROP SCHEMA [OldSchema]
GO