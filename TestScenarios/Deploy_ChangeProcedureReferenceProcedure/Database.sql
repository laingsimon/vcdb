CREATE PROCEDURE [dbo].[AA_OuterProcedure]
AS
BEGIN
    EXEC dbo.AB_InnerProcedure
END
GO
CREATE PROCEDURE [dbo].[AB_InnerProcedure]
AS
BEGIN
    SELECT 'Old Inner'
END
GO
CREATE PROCEDURE [dbo].[CA_InnerProcedure]
AS
BEGIN
    SELECT 'Old Inner'
END
GO
CREATE PROCEDURE [dbo].[CC_OuterProcedure]
AS
BEGIN
    EXEC dbo.CA_InnerProcedure
END
GO