IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Add')
BEGIN
    DROP LOGIN MyLogin_Add
END    
GO
CREATE LOGIN MyLogin_Add WITH PASSWORD = '123__aBC'
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyOtherLogin_Add')
BEGIN
    DROP LOGIN MyOtherLogin_Add
END
GO
CREATE LOGIN MyOtherLogin_Add WITH PASSWORD = '123__aBC'
GO