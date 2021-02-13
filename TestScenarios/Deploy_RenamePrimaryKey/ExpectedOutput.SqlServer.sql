EXEC sp_rename
    @objname = 'PK__Person__3214EC07/[A-F0-9]{8}/', 
    @newname = 'PK_Person_Id', 
    @objtype = 'OBJECT'
GO
EXEC sp_rename
    @objname = 'dbo.PK_Car', 
    @newname = 'PK__Car__3214EC0749AA6E8', 
    @objtype = 'OBJECT'
GO