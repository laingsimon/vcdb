IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Rename')
BEGIN
    DROP LOGIN MyLogin_Rename
END    
GO
CREATE LOGIN MyLogin_Rename WITH PASSWORD = '123__aBC'
GO
CREATE USER [MyUser] FOR LOGIN [MyLogin_Rename]
GO