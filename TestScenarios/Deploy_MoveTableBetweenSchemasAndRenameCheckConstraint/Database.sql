CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null
)
GO
CREATE SCHEMA [NewSchema]
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_ValidName]
CHECK (LEN([Name])>3)
GO