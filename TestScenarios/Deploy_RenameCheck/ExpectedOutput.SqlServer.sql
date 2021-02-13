EXEC sp_rename 
    @objname = 'dbo.CK_Person_ValidName', 
    @newname = 'CK_Person_StillValid',
    @objtype = 'OBJECT'
GO
