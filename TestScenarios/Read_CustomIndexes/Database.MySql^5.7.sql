CREATE TABLE Person (
	Id 			int not null PRIMARY KEY,
	Name 		nvarchar(255) not null,
	Age			int
);

CREATE UNIQUE /*CLUSTERED*/ INDEX `IX_Person_Id` ON `Person` (`Id`);
CREATE UNIQUE INDEX `IX_Person_Name` ON `Person` (`Name`);
CREATE INDEX `IX_Person_Name_Age` ON `Person` (`Name` ASC, `Age`);
CREATE INDEX `IX_Person_Name_IncAge` ON `Person` (`Name` ASC) /*INCLUDE(`Age`)*/;