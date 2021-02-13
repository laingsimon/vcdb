ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [FK_Person_FavouriteCar] 
FOREIGN KEY ([FavouriteCar]) 
REFERENCES [dbo].[Car] ([Id])
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'the persons favourite car',
@level0type = N'SCHEMA', @level0name = N'dbo', 
@level1type = N'TABLE',  @level1name = N'Person',
@level2type = N'CONSTRAINT', @level2name = N'FK_Person_FavouriteCar'
GO
