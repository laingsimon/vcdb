ALTER PROCEDURE [dbo].[RegularProcedure]
(
    @name varchar(255)
)
AS
BEGIN
    SELECT @name
END

GO
DENY EXECUTE ON [dbo].[RegularProcedure] TO [MyUser_Deny] CASCADE
GO
GRANT EXECUTE ON [dbo].[RegularProcedure] TO [MyUser_Grant]
GO
