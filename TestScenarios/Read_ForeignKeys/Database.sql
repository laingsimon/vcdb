CREATE TABLE dbo.Car (
	Id 			int not null PRIMARY KEY,
    Name        varchar(255) not null
)
GO
CREATE TABLE dbo.Person (
	Id 			    int not null PRIMARY KEY,
    Name            varchar(255),
    FavouriteCar    int,
    ParentId        int not null,
    CONSTRAINT FK_Person_FavouriteCar FOREIGN KEY ([FavouriteCar]) REFERENCES [dbo].[Car] ([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_Person_Parent FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Person] ([Id])
)
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'the persons favourite car',
@level0type = 'SCHEMA', @level0name = 'dbo', 
@level1type = 'TABLE',  @level1name = 'Person',
@level2type = 'CONSTRAINT', @level2name = 'FK_Person_FavouriteCar'
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'foreign key description',
@level0type = 'SCHEMA', @level0name = 'dbo', 
@level1type = 'TABLE',  @level1name = 'Person',
@level2type = 'CONSTRAINT', @level2name = 'FK_Person_Parent'
GO