IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeProcedurePermissions_Grant')
BEGIN
    DROP LOGIN MyLogin_ChangeProcedurePermissions_Grant
END    
GO
CREATE LOGIN MyLogin_ChangeProcedurePermissions_Grant WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Grant FOR LOGIN MyLogin_ChangeProcedurePermissions_Grant
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_ChangeProcedurePermissions_Deny')
BEGIN
    DROP LOGIN MyLogin_ChangeProcedurePermissions_Deny
END    
GO
CREATE LOGIN MyLogin_ChangeProcedurePermissions_Deny WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser_Deny FOR LOGIN MyLogin_ChangeProcedurePermissions_Deny
GO
CREATE PROCEDURE [dbo].[RegularProcedure]
(
    @id int
)
AS
BEGIN
    SELECT @id * 3
END
GO
