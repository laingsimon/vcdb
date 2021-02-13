IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_TablePermissions_Grant')
BEGIN
    DROP LOGIN MyLogin_TablePermissions_Grant
END    
GO
CREATE LOGIN MyLogin_TablePermissions_Grant WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Grant FOR LOGIN MyLogin_TablePermissions_Grant
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_TablePermissions_Deny')
BEGIN
    DROP LOGIN MyLogin_TablePermissions_Deny
END    
GO
CREATE LOGIN MyLogin_TablePermissions_Deny WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Deny FOR LOGIN MyLogin_TablePermissions_Deny
GO
CREATE TABLE Person
(
    [Id]    int,
    [Name]  varchar(255)
)
GO
REVOKE CONNECT TO MyUser_Grant AS dbo
GO
GRANT SELECT ON dbo.Person TO MyUser_Grant
GO
REVOKE SELECT ON dbo.Person (Name) TO MyUser_Grant
GO
DENY SELECT ON dbo.Person TO MyUser_Deny
GO
