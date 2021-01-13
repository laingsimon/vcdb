This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario the collation of the database is returned to the same as the server default collation. At the same time the custom collation of a column is removed and reset to the database (now the server) default collation.