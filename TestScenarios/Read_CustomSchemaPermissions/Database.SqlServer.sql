IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_SchemaPermissions_Grant')
BEGIN
    DROP LOGIN MyLogin_SchemaPermissions_Grant
END    
GO
CREATE LOGIN MyLogin_SchemaPermissions_Grant WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Grant FOR LOGIN MyLogin_SchemaPermissions_Grant
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_SchemaPermissions_Deny')
BEGIN
    DROP LOGIN MyLogin_SchemaPermissions_Deny
END    
GO
CREATE LOGIN MyLogin_SchemaPermissions_Deny WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Deny FOR LOGIN MyLogin_SchemaPermissions_Deny
GO
CREATE SCHEMA MySchema
GO
REVOKE CONNECT TO MyUser_Grant AS dbo
GO
GRANT CONTROL ON SCHEMA :: MySchema TO MyUser_Grant WITH GRANT OPTION
GO
DENY CONTROL ON SCHEMA :: MySchema TO MyUser_Deny
GO