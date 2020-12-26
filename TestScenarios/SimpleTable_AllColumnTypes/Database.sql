DROP DATABASE IF EXISTS [Scenario1]
GO

CREATE DATABASE [Scenario1]
GO

USE [Scenario1]
GO

CREATE TABLE S1_T1 (
	Id 			int	identity,
	Name 		nvarchar(255) not null,
	Title		varchar(10),
	Age			int,
	Price		decimal(18, 2),
	DoB			date,
	Deleted		bit not null default(0),
	UniqueId	uniqueidentifier
)