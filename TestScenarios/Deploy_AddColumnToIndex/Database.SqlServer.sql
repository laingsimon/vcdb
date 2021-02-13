CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
	[Age] int
)
GO
CREATE INDEX [IX_Person_Id]
ON [dbo].[Person] ([Id])
GO
CREATE INDEX [IX_Person_Name]
ON [dbo].[Person] ([Name])
GO