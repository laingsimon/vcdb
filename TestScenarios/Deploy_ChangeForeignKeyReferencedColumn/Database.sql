CREATE TABLE [dbo].[Car] (
	[Id] int identity not null,
    [PublicId]  int not null,
	[Name] nvarchar(255),
    CONSTRAINT PK_Car PRIMARY KEY (Id)
)
GO
CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [FavouriteCar] int,
    CONSTRAINT FK_Person_FavouriteCar FOREIGN KEY (FavouriteCar) REFERENCES dbo.Car (Id)
)
GO
