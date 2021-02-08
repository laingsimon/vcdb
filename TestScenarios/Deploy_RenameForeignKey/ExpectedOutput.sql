EXEC sp_rename 
    @objname = 'dbo.FK_Person_FavouriteCar',
    @newname = 'FK_Person_TheFavouriteCar', 
    @objtype = 'OBJECT'
GO