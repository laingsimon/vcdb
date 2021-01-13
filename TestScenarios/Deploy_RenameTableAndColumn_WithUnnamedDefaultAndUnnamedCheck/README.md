This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a table is renamed. At the same time a column is renamed. The column has a check constraint and a default constraint bound to it. Both constraints have a system-generated name.
As their system-generated names contain the name of the table (and column), they're renamed to maintain consistency.