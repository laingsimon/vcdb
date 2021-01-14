CREATE TABLE [dbo].[Person] (
	[Id] int identity not null,
	[Name] nvarchar(255) not null,
    [Age] int not null check([Age]>0)
)
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_ValidName]
CHECK (LEN([Name])>3)
GO