REVOKE SELECT ON [dbo].[Person] ([Name]) TO [MyUser_Grant]
GO
DENY SELECT ON [dbo].[Person] TO [MyUser_Deny] CASCADE
GO
GRANT SELECT ON [dbo].[Person] TO [MyUser_Grant]
GO
