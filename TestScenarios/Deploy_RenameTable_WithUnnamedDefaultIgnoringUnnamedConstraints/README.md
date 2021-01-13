This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a table is renamed. The table contains a system-named default constraint. As the default constraint will contain the table name in it's name it is renamed to be consistent.
This scenario uses the `--ignoreUnusedConstraints` switch to tell `vcdb` to ignore system-named default constraints when their table/column change name.
