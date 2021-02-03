CREATE OR ALTER PROCEDURE [dbo].[RegularProcedure]
(
    @name varchar(255)
)
AS
BEGIN
    SELECT @name
END

GO
CREATE SCHEMA [NewSchema]