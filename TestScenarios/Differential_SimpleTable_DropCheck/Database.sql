CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null CHECK(len([Name])>3)
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_IdNotTooBig]
CHECK ([Id]<1000)
