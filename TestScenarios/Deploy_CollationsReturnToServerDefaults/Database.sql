/*Server collation is SQL_Latin1_General_CP1_CI_AS*/
ALTER DATABASE [Deploy_CollationsReturnToServerDefaults]
COLLATE Latin1_General_CI_AS
GO
CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(1024) COLLATE Latin1_General_CS_AS not null
)