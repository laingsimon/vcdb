EXEC sp_rename
    @objname = 'dbo.Person.IX_Person_Id',
    @newname = 'IX_Person_Id_New',
    @objtype = 'INDEX'
GO
EXEC sp_rename
    @objname = 'dbo.Person.IX_Person_IdIncName',
    @newname = 'IX_Person_IdIncName_New',
    @objtype = 'INDEX'
GO