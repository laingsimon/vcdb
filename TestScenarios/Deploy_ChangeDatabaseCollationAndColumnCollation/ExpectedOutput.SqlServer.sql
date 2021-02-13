ALTER DATABASE [Deploy_ChangeDatabaseCollationAndColumnCollation]
COLLATE Latin1_General_CS_AI
GO
ALTER TABLE [dbo].[Person]
ALTER COLUMN [Name] nvarchar(1024) COLLATE Latin1_General_CI_AS NOT NULL
GO