IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeSchemaPermissions_Grant')
BEGIN
    DROP LOGIN MyLogin_ChangeSchemaPermissions_Grant
END    
GO
CREATE LOGIN MyLogin_ChangeSchemaPermissions_Grant WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Grant FOR LOGIN MyLogin_ChangeSchemaPermissions_Grant
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeSchemaPermissions_Deny')
BEGIN
    DROP LOGIN MyLogin_ChangeSchemaPermissions_Deny
END    
GO
CREATE LOGIN MyLogin_ChangeSchemaPermissions_Deny WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Deny FOR LOGIN MyLogin_ChangeSchemaPermissions_Deny
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeSchemaPermissions_Change')
BEGIN
    DROP LOGIN MyLogin_ChangeSchemaPermissions_Change
END    
GO
CREATE LOGIN MyLogin_ChangeSchemaPermissions_Change WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Change FOR LOGIN MyLogin_ChangeSchemaPermissions_Change
GO
CREATE SCHEMA MySchema
GO
GRANT CONTROL ON SCHEMA :: MySchema TO MyUser_Change WITH GRANT OPTION
GO