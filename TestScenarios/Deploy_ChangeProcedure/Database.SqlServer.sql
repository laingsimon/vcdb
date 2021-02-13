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
