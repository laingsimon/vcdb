ALTER TABLE [dbo].[Person]
ADD [NewColumn] bit NOT NULL
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_Person_NewColumn]
DEFAULT (0)
FOR [NewColumn]
GO