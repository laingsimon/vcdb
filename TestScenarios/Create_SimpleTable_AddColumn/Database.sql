DROP DATABASE IF EXISTS [Create_SimpleTable_AddColumn]
GO

CREATE DATABASE [Create_SimpleTable_AddColumn]
GO

USE [Create_SimpleTable_AddColumn]
GO

CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null
)