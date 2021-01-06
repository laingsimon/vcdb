CREATE TABLE dbo.Person (
	Id 			int
)
GO
CREATE SCHEMA MySchema
GO
CREATE INDEX IX_Person_Id
ON dbo.Person (Id)
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'database description'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'schema description',
		@level0type = N'SCHEMA', @level0name = 'MySchema'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'table description',
		@level0type = N'SCHEMA', @level0name = 'dbo', 
		@level1type = N'TABLE',  @level1name = 'Person'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'column description',
		@level0type = N'SCHEMA', @level0name = 'dbo', 
		@level1type = N'TABLE',  @level1name = 'Person',
		@level2type = N'COLUMN', @level2name = 'Id'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'index description',
		@level0type = N'SCHEMA', @level0name = 'dbo', 
		@level1type = N'TABLE',  @level1name = 'Person',
		@level2type = N'INDEX', @level2name = 'IX_Person_Id'
GO