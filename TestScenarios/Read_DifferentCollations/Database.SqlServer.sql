ALTER DATABASE [Read_DifferentCollations]
COLLATE Latin1_General_CI_AI
GO

CREATE TABLE dbo.Person (
	Name 			varchar(255) collate Latin1_General_CS_AI
)
GO
