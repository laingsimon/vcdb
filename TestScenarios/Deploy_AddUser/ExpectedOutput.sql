CREATE USER [MyUser] FOR LOGIN [MyLogin_Add]
GO
CREATE USER [MyOtherUser] FOR LOGIN [MyOtherLogin_Add]
GO
ALTER LOGIN [MyOtherLogin_Add]
DISABLE
GO