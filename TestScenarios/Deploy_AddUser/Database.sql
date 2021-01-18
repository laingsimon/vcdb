IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin')
BEGIN
    DROP LOGIN MyLogin
END    
GO
CREATE LOGIN MyLogin WITH PASSWORD = '123__aBC'
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyOtherLogin')
BEGIN
    DROP LOGIN MyOtherLogin
END
GO
CREATE LOGIN MyOtherLogin WITH PASSWORD = '123__aBC'
GO