EXEC sp_rename 
    @old_name = 'dbo.Person', 
    @new_name = 'dbo.People', 
    @object_type = 'TABLE'
GO