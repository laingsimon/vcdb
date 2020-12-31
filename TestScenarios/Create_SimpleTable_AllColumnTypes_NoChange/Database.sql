DROP DATABASE IF EXISTS [Create_SimpleTable_AllColumnTypes_NoChange]
GO

CREATE DATABASE [Create_SimpleTable_AllColumnTypes_NoChange]
GO

USE [Create_SimpleTable_AllColumnTypes_NoChange]
GO

CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
	[Title] varchar(10),
	[Age] int,
	[Price] decimal(18, 2),
	[DoB] date,
	[Deleted] bit not null default(0),
	[UniqueId] uniqueidentifier
)