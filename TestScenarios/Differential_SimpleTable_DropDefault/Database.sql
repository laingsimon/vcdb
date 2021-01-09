CREATE TABLE [dbo].[Person] (
	[Id] int not null default(0),
	[Name] nvarchar(255) not null
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_Person_DefaultName]
DEFAULT ('John Doe')
FOR [Name]