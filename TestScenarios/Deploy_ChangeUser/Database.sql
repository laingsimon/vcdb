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
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyDisabledLogin')
BEGIN
    DROP LOGIN MyDisabledLogin
END
GO
CREATE LOGIN MyDisabledLogin WITH PASSWORD = '123__aBC'
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyEnabledLogin')
BEGIN
    DROP LOGIN MyEnabledLogin
END
GO
CREATE LOGIN MyEnabledLogin WITH PASSWORD = '123__aBC'
GO
ALTER LOGIN [MyDisabledLogin]
DISABLE
GO
CREATE USER [MyUser] FOR LOGIN [MyLogin]
GO
CREATE USER [MyDisabledUser] FOR LOGIN [MyDisabledLogin]
GO
CREATE USER [MyEnabledUser] FOR LOGIN [MyEnabledLogin]
GO
CREATE SCHEMA [MySchema]
GO
ALTER USER [MyEnabledUser]
WITH DEFAULT_SCHEMA = [MySchema]
GO