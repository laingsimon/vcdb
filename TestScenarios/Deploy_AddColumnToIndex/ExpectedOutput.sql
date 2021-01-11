DROP INDEX [IX_Person_Id] ON [dbo].[Person]
GO
CREATE INDEX [IX_Person_Id] 
ON [dbo].[Person] ([Id])
INCLUDE ([Name])
GO
DROP INDEX [IX_Person_Name] ON [dbo].[Person]
GO
CREATE INDEX [IX_Person_NameAndAge] 
ON [dbo].[Person] ([Name], [Age] DESC)
GO