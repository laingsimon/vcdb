This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a table is renamed. The table contains a system-named check constraint. As the check constraint will contain the table name in it's name it is renamed to be consistent.