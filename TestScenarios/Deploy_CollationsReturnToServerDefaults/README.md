This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario the collation of the database is returned to the same as the server default collation. At the same time the custom collation of a column is removed and reset to the database (now the server) default collation.

This scenario sets up a database with the `Latin1_General_CI_AI`, if the database server has this as the default collation then the scenario will fail, as it is already set to the database default collation.