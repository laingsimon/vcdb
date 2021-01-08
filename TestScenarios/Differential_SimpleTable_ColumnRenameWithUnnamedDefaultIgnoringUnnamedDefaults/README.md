This scenario tests vcdb to confirm it can create the expected schema JSON from a given database.

This scenario tests that vcdb can create script to rename a column with an unnamed default constraint. The vcdb option to ignore (i.e. dont rename) unnamed default constraints is provided.

This scenario uses the `--ignoreUnusedConstraints` switch to save needing to rename any unnamed constrants for tables and or columns that change name.
