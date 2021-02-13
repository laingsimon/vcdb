IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Change')
BEGIN
    DROP LOGIN MyLogin_Change
END    
GO
CREATE LOGIN MyLogin_Change WITH PASSWORD = '123__aBC'
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyOtherLogin_Change')
BEGIN
    DROP LOGIN MyOtherLogin_Change
END
GO
CREATE LOGIN MyOtherLogin_Change WITH PASSWORD = '123__aBC'
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
CREATE USER [MyUser] FOR LOGIN [MyLogin_Change]
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