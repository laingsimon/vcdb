CREATE SCHEMA ChangeDescriptionSchema
GO
CREATE SCHEMA AddDescriptionSchema
GO
CREATE SCHEMA DropDescriptionSchema
GO
CREATE TABLE ChangeDescriptionSchema.ChangeDescriptionTable (
	Id 			int
)
GO
CREATE TABLE AddDescriptionSchema.AddDescriptionTable (
	Id 			int
)
GO
CREATE TABLE DropDescriptionSchema.DropDescriptionTable (
	Id 			int
)
GO
CREATE INDEX IX_ChangeDescriptionTable_ChangeDescriptionIndex
ON ChangeDescriptionSchema.ChangeDescriptionTable (Id)
GO
CREATE INDEX IX_AddDescriptionTable_AddDescriptionIndex
ON AddDescriptionSchema.AddDescriptionTable (Id)
GO
CREATE INDEX IX_DropDescriptionTable_DropDescriptionIndex
ON DropDescriptionSchema.DropDescriptionTable (Id)
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'database description'
GO

exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'old description',
		@level0type = N'SCHEMA', @level0name = 'ChangeDescriptionSchema'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'old description',
		@level0type = N'SCHEMA', @level0name = 'ChangeDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'ChangeDescriptionTable'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'old description',
		@level0type = N'SCHEMA', @level0name = 'ChangeDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'ChangeDescriptionTable',
		@level2type = N'COLUMN', @level2name = 'Id'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'old description',
		@level0type = N'SCHEMA', @level0name = 'ChangeDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'ChangeDescriptionTable',
		@level2type = N'INDEX', @level2name = 'IX_ChangeDescriptionTable_ChangeDescriptionIndex'
GO

exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'to drop description',
		@level0type = N'SCHEMA', @level0name = 'DropDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'DropDescriptionTable'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'to drop description',
		@level0type = N'SCHEMA', @level0name = 'DropDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'DropDescriptionTable',
		@level2type = N'COLUMN', @level2name = 'Id'
GO
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'to drop description',
		@level0type = N'SCHEMA', @level0name = 'DropDescriptionSchema', 
		@level1type = N'TABLE',  @level1name = 'DropDescriptionTable',
		@level2type = N'INDEX', @level2name = 'IX_DropDescriptionTable_DropDescriptionIndex'
GO