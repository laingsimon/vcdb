CREATE SCHEMA [OldSchema]
GO
CREATE TABLE [OldSchema].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null
)
GO
