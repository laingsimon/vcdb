CREATE TABLE [dbo].[Car] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Price] decimal(8, 2),
    [PriceIncVat] as ([Price]*1.175)
)
GO