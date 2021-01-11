CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Deleted] bit not null
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_PersonActiveByDefault]
DEFAULT (0)
FOR [Deleted]
GO