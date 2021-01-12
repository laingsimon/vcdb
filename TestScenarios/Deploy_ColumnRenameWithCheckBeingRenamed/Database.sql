CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[FullName] nvarchar(255) not null
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_ValidName]
CHECK (LEN([FullName])>3)
GO