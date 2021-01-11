CREATE SCHEMA [OldSchema]
GO
CREATE TABLE [OldSchema].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null
)
GO
CREATE UNIQUE INDEX [IX_Person_Name] 
ON [OldSchema].[Person] ([Name])
GO