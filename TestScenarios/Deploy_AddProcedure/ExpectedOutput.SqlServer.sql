CREATE PROCEDURE [dbo].[RegularProcedure]
(
    @id int
)
AS
BEGIN
    SELECT @id * 3
END
GO
EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = 'procedure description',
@level0type = N'SCHEMA', @level0name = N'dbo', 
@level1type = N'PROCEDURE',  @level1name = N'RegularProcedure',
@level2type = null, @level2name = null
GO
CREATE PROCEDURE [dbo].[EncryptedProcedure]
(
    @id int
)
WITH ENCRYPTION
AS
BEGIN
    SELECT @id * 3
END
GO
