ALTER TABLE [dbo].[Car]
DROP COLUMN [PriceIncVat]
GO
ALTER TABLE [dbo].[Car]
ADD [PriceIncVat] AS ([Price]*1.2)
GO
