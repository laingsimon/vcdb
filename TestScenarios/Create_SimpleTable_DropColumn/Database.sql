DROP DATABASE IF EXISTS [Create_SimpleTable_DropColumn]
GO

CREATE DATABASE [Create_SimpleTable_DropColumn]
GO

USE [Create_SimpleTable_DropColumn]
GO

CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [OldColumn] bit
)