IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Rename_ReferencedFile')
BEGIN
    DROP LOGIN MyLogin_Rename_ReferencedFile
END    
GO
CREATE LOGIN MyLogin_Rename_ReferencedFile WITH PASSWORD = '123__aBC'
GO
CREATE USER [MyUser] FOR LOGIN [MyLogin_Rename_ReferencedFile]
GO