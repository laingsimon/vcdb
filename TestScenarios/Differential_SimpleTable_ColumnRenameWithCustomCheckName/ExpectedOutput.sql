EXEC sp_rename 
    @objname = 'dbo.Person.Name', 
    @newname = 'FullName',
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK_Person_ValidName]
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_ValidName]
CHECK (len([FullName])>3)
GO