CREATE PROCEDURE [dbo].[EncryptedProcedure]
(
    @id int
)
WITH ENCRYPTION 
AS
BEGIN
    SELECT @id * 2
END
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
exec sp_addextendedproperty 
		@name = N'MS_Description', @value = 'procedure description',
		@level0type = N'SCHEMA', @level0name = 'dbo', 
		@level1type = N'PROCEDURE',  @level1name = 'RegularProcedure'
GO
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'MyLogin_Procedure')
BEGIN
    DROP LOGIN MyLogin_Procedure
END    
GO
CREATE LOGIN MyLogin_Procedure WITH PASSWORD = '123__aBC'
GO
CREATE USER MyUser FOR LOGIN MyLogin_Procedure
GO
REVOKE CONNECT TO MyUser AS dbo
GO
GRANT EXECUTE ON [dbo].[RegularProcedure] TO [MyUser]
GO