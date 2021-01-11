EXEC sp_rename
    @objname = 'dbo.Person.Name',
    @newname = 'FullName',
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = 'dbo.Person.Age',
    @newname = 'TheirAge',
    @objtype = 'COLUMN'
GO
EXEC sp_rename
    @objname = 'dbo.Person.IX_Person_NameIncAge',
    @newname = 'IX_Person_FullNameIncTheirAge',
    @objtype = 'INDEX'
GO