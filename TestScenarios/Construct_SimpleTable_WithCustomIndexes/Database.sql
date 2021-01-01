CREATE TABLE dbo.Person (
	Id 			int	identity,
	Name 		nvarchar(255) not null,
	Age			int
)

GO

CREATE CLUSTERED INDEX IX_Person_Id ON dbo.Person (Id)
CREATE INDEX IX_Person_Name ON dbo.Person (Name)
CREATE INDEX IX_Person_Name_Age ON dbo.Person (Name ASC, Age DESC)
CREATE INDEX IX_Person_Name_IncAge ON dbo.Person (Name ASC) INCLUDE (Age)