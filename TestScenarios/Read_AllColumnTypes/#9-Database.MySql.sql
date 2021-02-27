CREATE TABLE `Person` (
	Id 			int not null auto_increment primary key,
	Name 		nvarchar(255) not null,
	Title		varchar(10),
	Age			int,
	Price		decimal(18, 2),
	DoB			date,
	Deleted		bit(1) not null default 0,
    PriceIncVat decimal(18, 2) AS (`Price` * 1.2)
);