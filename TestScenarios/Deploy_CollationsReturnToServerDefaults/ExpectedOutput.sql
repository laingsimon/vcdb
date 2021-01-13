ALTER DATABASE [Deploy_CollationsReturnToServerDefaults]
COLLATE SQL_Latin1_General_CP1_CI_AS
GO
ALTER TABLE [dbo].[Person]
ALTER COLUMN [Name] nvarchar(1024) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
GO