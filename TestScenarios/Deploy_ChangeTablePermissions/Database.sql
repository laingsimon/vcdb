IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeTablePermissions_Grant')
BEGIN
    DROP LOGIN MyLogin_ChangeTablePermissions_Grant
END    
GO
CREATE LOGIN MyLogin_ChangeTablePermissions_Grant WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Grant FOR LOGIN MyLogin_ChangeTablePermissions_Grant
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeTablePermissions_Deny')
BEGIN
    DROP LOGIN MyLogin_ChangeTablePermissions_Deny
END    
GO
CREATE LOGIN MyLogin_ChangeTablePermissions_Deny WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Deny FOR LOGIN MyLogin_ChangeTablePermissions_Deny
GO
CREATE TABLE Person
(
    [Id]    int,
    [Name]  varchar(255)
)
GO