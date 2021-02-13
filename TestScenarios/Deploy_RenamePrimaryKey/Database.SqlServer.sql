CREATE TABLE [dbo].[Person] (
	[Id] int identity not null PRIMARY KEY,
	[Name] nvarchar(255) not null
)
GO
CREATE TABLE [dbo].[Car] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    CONSTRAINT PK_Car PRIMARY KEY (Id)
)
GO