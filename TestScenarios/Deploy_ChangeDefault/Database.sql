CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Age] int not null default(0)
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_Person_DefaultName]
DEFAULT ('John Doe')
FOR [Name]
GO