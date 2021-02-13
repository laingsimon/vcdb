ALTER TABLE [dbo].[Car]
ADD [PriceIncVat] AS ([Price]*1.2)
GO