DROP DATABASE IF EXISTS [Create_SimpleTableRename]
GO

CREATE DATABASE [Create_SimpleTableRename]
GO

USE [Create_SimpleTableRename]
GO

CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null
)