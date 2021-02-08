EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'updated database description',
@level0type = null, @level0name = null, 
@level1type = null,  @level1name = null,
@level2type = null, @level2name = null
GO
EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'updated schema description',
@level0type = N'SCHEMA', @level0name = N'ChangeDescriptionSchema', 
@level1type = null,  @level1name = null,
@level2type = null, @level2name = null
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'new schema description',
@level0type = N'SCHEMA', @level0name = N'AddDescriptionSchema', 
@level1type = null,  @level1name = null,
@level2type = null, @level2name = null
GO
EXEC sp_dropextendedproperty 
@name = N'MS_Description',
@level0type = N'SCHEMA', @level0name = N'DropDescriptionSchema', 
@level1type = null,  @level1name = null,
@level2type = null, @level2name = null
GO
EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'updated column description',
@level0type = N'SCHEMA', @level0name = N'ChangeDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'ChangeDescriptionTable',
@level2type = N'COLUMN', @level2name = N'Id'
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'new column description',
@level0type = N'SCHEMA', @level0name = N'AddDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'AddDescriptionTable',
@level2type = N'COLUMN', @level2name = N'Id'
GO
EXEC sp_dropextendedproperty
@name = N'MS_Description',
@level0type = N'SCHEMA', @level0name = N'DropDescriptionSchema',
@level1type = N'TABLE',  @level1name = N'DropDescriptionTable',
@level2type = N'COLUMN', @level2name = N'Id'
GO
EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'updated index description',
@level0type = N'SCHEMA', @level0name = N'ChangeDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'ChangeDescriptionTable',
@level2type = N'INDEX', @level2name = N'IX_ChangeDescriptionTable_ChangeDescriptionIndex'
GO
EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'updated table description',
@level0type = N'SCHEMA', @level0name = N'ChangeDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'ChangeDescriptionTable',
@level2type = null, @level2name = null
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'new index description',
@level0type = N'SCHEMA', @level0name = N'AddDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'AddDescriptionTable',
@level2type = N'INDEX', @level2name = N'IX_AddDescriptionTable_AddDescriptionIndex'
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'new table description',
@level0type = N'SCHEMA', @level0name = N'AddDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'AddDescriptionTable',
@level2type = null, @level2name = null
GO
EXEC sp_dropextendedproperty 
@name = N'MS_Description',
@level0type = N'SCHEMA', @level0name = N'DropDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'DropDescriptionTable',
@level2type = N'INDEX', @level2name = N'IX_DropDescriptionTable_DropDescriptionIndex'
GO
EXEC sp_dropextendedproperty 
@name = N'MS_Description',
@level0type = N'SCHEMA', @level0name = N'DropDescriptionSchema', 
@level1type = N'TABLE',  @level1name = N'DropDescriptionTable',
@level2type = null, @level2name = null
GO

