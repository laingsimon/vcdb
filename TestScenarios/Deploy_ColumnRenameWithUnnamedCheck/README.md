This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a column is renamed. The column is bound to a check constraint with a system-generated name. As the system-generated name is calculated based on the name of the columns it is bound to, the system-generated is updated to keep it consistent.