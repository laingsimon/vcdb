IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Drop')
BEGIN
    DROP LOGIN MyLogin_Drop
END    
GO
CREATE LOGIN MyLogin_Drop WITH PASSWORD = '123__aBC'
GO
CREATE USER [MyUser] FOR LOGIN [MyLogin_Drop]
GO