CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Title] varchar(10) COLLATE Latin1_General_CS_AI
)