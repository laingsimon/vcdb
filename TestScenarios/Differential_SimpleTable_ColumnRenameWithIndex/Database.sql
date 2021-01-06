CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
	[Age] int
)
GO
CREATE INDEX [IX_Person_NameIncAge]
ON [dbo].[Person] ([Name]) INCLUDE (Age)