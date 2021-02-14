/*
Docker server collation is SQL_Latin1_General_CP1_CI_AS
This scenario expects that the database collation is NOT the collation below.
*/
ALTER DATABASE [Deploy_CollationsReturnToServerDefaults]
COLLATE Latin1_General_CI_AI
GO
CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(1024) COLLATE Latin1_General_CS_AI not null
)