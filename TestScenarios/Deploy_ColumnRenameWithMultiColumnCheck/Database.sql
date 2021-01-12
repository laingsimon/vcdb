CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[FullName] nvarchar(255) not null
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_Valid]
CHECK (LEN([FullName])>3 AND [Id] > 0)
GO