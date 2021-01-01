CREATE TABLE dbo.Person (
	Id 			int	identity,
	Name 		nvarchar(255) not null,
	Title		varchar(10),
	Age			int,
	Price		decimal(18, 2),
	DoB			date,
	Deleted		bit not null,
	UniqueId	uniqueidentifier
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [DF_PersonActiveByDefault]
DEFAULT (0)
FOR [Deleted]
GO