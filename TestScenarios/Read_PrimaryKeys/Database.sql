CREATE TABLE dbo.Person (
	Id 			int not null PRIMARY KEY NONCLUSTERED,
    Name        varchar(255)
)
GO
CREATE TABLE dbo.Car (
	Id 			int not null,
    Name        varchar(255) not null
)
GO
ALTER TABLE dbo.Car
ADD CONSTRAINT PK_Car_IdAndName
PRIMARY KEY (Id, Name)
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'primary key description',
@level0type = 'SCHEMA', @level0name = 'dbo', 
@level1type = 'TABLE',  @level1name = 'Car',
@level2type = 'CONSTRAINT', @level2name = 'PK_Car_IdAndName'
GO