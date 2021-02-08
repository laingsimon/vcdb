ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [FK_Person_FavouriteCar]
GO
ALTER TABLE [dbo].[Car]
DROP CONSTRAINT [PK_Car]
GO
ALTER TABLE [dbo].[Car]
ADD CONSTRAINT [PK_Car]
PRIMARY KEY ([PublicId])
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [FK_Person_TheFavouriteCar] 
FOREIGN KEY ([FavouriteCar]) 
REFERENCES [dbo].[Car] ([PublicId])
GO
