CREATE OR ALTER PROCEDURE [dbo].[CC_OuterProcedure]
AS
BEGIN
    EXEC dbo.CA_InnerProcedure
END
