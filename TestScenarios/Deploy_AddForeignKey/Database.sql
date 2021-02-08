CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [FavouriteCar] int
)
GO
CREATE TABLE [dbo].[Car] (
	[Id] int identity not null PRIMARY KEY,
	[Name] nvarchar(255)
)
GO