CREATE TABLE `Person` (
	Id 			int CHECK (Id > 1),
	Name 		nvarchar(255) not null
);
ALTER TABLE `Person`
ADD CONSTRAINT `CK_Name_LongerThan3`
CHECK (char_length(`Name`) > 3);