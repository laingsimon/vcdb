ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [FK_Person_FavouriteCar]
GO
ALTER TABLE [dbo].[Car]
DROP CONSTRAINT [PK__Car__3214EC07/[A-Z0-9]{8}/]
GO
EXEC sp_rename
    @objname = 'dbo.Person.FavouriteCar',
    @newname = 'FavouriteCarId',
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = 'dbo.Car.Id',
    @newname = 'CarId',
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[Car]
ADD CONSTRAINT [PK__Car__3214EC071872DE6A]
PRIMARY KEY ([CarId])
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [FK_Person_FavouriteCar] 
FOREIGN KEY ([FavouriteCarId]) 
REFERENCES [dbo].[Car] ([CarId])
GO
