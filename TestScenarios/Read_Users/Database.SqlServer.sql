IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin')
BEGIN
    DROP LOGIN MyLogin
END    
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyDisabledLogin')
BEGIN
    DROP LOGIN MyDisabledLogin
END    
GO
CREATE LOGIN MyLogin WITH PASSWORD = '123__aBC'
GO
CREATE LOGIN MyDisabledLogin WITH PASSWORD = '123__aBC'
GO
ALTER LOGIN [MyDisabledLogin]
DISABLE
GO
CREATE SCHEMA [MySchema]
GO
CREATE USER MyUser FOR LOGIN MyLogin
GO
CREATE USER MyDisabledUser FOR LOGIN MyDisabledLogin
GO
ALTER USER [MyDisabledUser]
WITH DEFAULT_SCHEMA = MySchema
GO