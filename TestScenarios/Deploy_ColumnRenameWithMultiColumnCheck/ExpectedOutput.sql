ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK_Person_Valid]
GO
EXEC sp_rename 
    @objname = 'dbo.Person.FullName', 
    @newname = 'Name',
    @objtype = 'COLUMN'
GO
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [CK_Person_StillValid]
CHECK (len([Name])>3 AND [Id]>0)
GO
