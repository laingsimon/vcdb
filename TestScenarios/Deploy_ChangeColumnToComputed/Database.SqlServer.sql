CREATE TABLE [dbo].[Car] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Price] decimal(8, 2),
    [PriceIncVat] decimal(8, 2)
)
GO