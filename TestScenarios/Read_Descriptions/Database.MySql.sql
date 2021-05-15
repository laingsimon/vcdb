CREATE TABLE `Person` (
	`Id` 			int not null COMMENT 'column description'
) COMMENT 'table description';
CREATE INDEX `IX_Person_Id`
ON `Person` (`Id`)
COMMENT 'index description';
CREATE PROCEDURE `MyProcedure` () COMMENT 'procedure description'
    SELECT 1 as `Nothing`
;