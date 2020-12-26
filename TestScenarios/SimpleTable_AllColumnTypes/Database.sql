DROP DATABASE IF EXISTS [SimpleTable_AllColumnTypes]
GO

CREATE DATABASE [SimpleTable_AllColumnTypes]
GO

USE [SimpleTable_AllColumnTypes]
GO

CREATE TABLE dbo.Person (
	Id 			int	identity,
	Name 		nvarchar(255) not null,
	Title		varchar(10),
	Age			int,
	Price		decimal(18, 2),
	DoB			date,
	Deleted		bit not null default(0),
	UniqueId	uniqueidentifier
)