CREATE TABLE [dbo].[Person] (
	[Id] int identity NOT NULL,
	[Name] nvarchar(255) NOT NULL,
	[Title] varchar(10),
	[Age] int,
	[Price] decimal(18, 2),
	[DoB] date,
	[Deleted] bit NOT NULL,
	[UniqueId] uniqueidentifier
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_Person_Deleted]
DEFAULT (0)
FOR [Deleted]
GO