DROP DATABASE IF EXISTS [Scenario1]
GO

CREATE DATABASE [Scenario1]
GO

USE [Scenario1]
GO

CREATE TABLE S1_T1 (
	Id 		int	identity,
	Name 	varchar(255) not null,
	Age		int,
	Price	decimal(18, 2),
	DoB		date,
	Deleted	bit not null default(0)
)