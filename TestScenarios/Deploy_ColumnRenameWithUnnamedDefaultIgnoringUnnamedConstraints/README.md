This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a column is renamed. The `--ignoreUnusedConstraints` switch is provided, which means `vcdb` should ignore default constraints with system-generated names when their columns change name.