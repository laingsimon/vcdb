ALTER DATABASE [Deploy_CollationsReturnToServerDefaults]
COLLATE /[A-Za-z0-9_]+/
GO
ALTER TABLE [dbo].[Person]
ALTER COLUMN [Name] nvarchar(1024) COLLATE /[A-Za-z0-9_]+/ NOT NULL
GO