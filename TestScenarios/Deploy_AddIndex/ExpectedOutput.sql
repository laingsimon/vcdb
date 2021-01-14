CREATE UNIQUE CLUSTERED INDEX [IX_Person_Id]
ON [dbo].[Person] ([Id])
GO
CREATE UNIQUE INDEX [IX_Person_Name]
ON [dbo].[Person] ([Name])
GO
CREATE INDEX [IX_Person_Age]
ON [dbo].[Person] ([Age] DESC)
GO
CREATE INDEX [IX_Person_NameAndAge]
ON [dbo].[Person] ([Name], [Age] DESC)
GO
CREATE INDEX [IX_Person_IdIncName]
ON [dbo].[Person] ([Id])
INCLUDE ([Name])
GO