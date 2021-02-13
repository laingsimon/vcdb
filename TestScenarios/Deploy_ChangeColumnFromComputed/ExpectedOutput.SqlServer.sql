ALTER TABLE [dbo].[Car]
DROP COLUMN [PriceIncVat]
GO
ALTER TABLE [dbo].[Car]
ADD [PriceIncVat] decimal(8, 2)
GO
