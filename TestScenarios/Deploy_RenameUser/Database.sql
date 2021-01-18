IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin')
BEGIN
    DROP LOGIN MyLogin
END    
GO
CREATE LOGIN MyLogin WITH PASSWORD = '123__aBC'
GO
CREATE USER [MyUser] FOR LOGIN [MyLogin]
GO