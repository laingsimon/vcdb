CREATE PROCEDURE [dbo].[EncryptedProcedure]
(
    @name varchar(255)
)
WITH ENCRYPTION 
AS
BEGIN
    SELECT @name
END
GO
CREATE PROCEDURE [dbo].[RegularProcedure]
(
    @name varchar(255)
)
AS
BEGIN
    SELECT @name
END
GO
