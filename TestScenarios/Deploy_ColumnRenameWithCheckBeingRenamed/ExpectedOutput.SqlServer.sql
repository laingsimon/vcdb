ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK_Person_ValidName]
GO
EXEC sp_rename 
    @objname = 'dbo.Person.FullName', 
    @newname = 'Name',
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_ValidFullName]
CHECK (len([Name])>3)
GO
