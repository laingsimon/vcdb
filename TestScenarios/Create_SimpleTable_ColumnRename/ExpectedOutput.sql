EXEC sp_rename 
    @old_name = 'dbo.Person.Name', 
    @new_name = 'FullName',
    @object_type = 'COLUMN'
GO