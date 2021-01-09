CREATE TABLE dbo.Person (
	Id 			int	identity CHECK (Id > 1),
	Name 		nvarchar(255) not null
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Name_LongerThan3]
CHECK (LEN([Name]) > 3)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Name_ShorterThan100]
CHECK (LEN([Name]) < 100)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_ComplicatedCheck]
CHECK (LEN([Name]) < 100 AND [Id] < 10)
GO