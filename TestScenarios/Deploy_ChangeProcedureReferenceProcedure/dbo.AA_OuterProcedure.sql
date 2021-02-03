CREATE OR ALTER PROCEDURE [dbo].[AA_OuterProcedure]
AS
BEGIN
    EXEC dbo.AB_InnerProcedure
END
