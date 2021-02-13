
ALTER PROCEDURE [dbo].[RegularProcedure]
(
    @name varchar(255)
)
AS
BEGIN
    SELECT @name
END
GO
EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = 'changed procedure description',
@level0type = N'SCHEMA', @level0name = N'dbo', 
@level1type = N'PROCEDURE',  @level1name = N'RegularProcedure',
@level2type = null, @level2name = null
GO
ALTER PROCEDURE [dbo].[EncryptedProcedure]
(
    @name varchar(255)
)
WITH ENCRYPTION
AS
BEGIN
    SELECT @name
END
GO
