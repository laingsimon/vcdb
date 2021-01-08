This scenario tests vcdb to confirm it can create the expected schema JSON from a given database.

This scenario tests that vcdb can create a ALTER table script for a table that has changed name. 
The table has a column with an unnamed default constraint.

This scenario uses the `--ignoreUnusedConstraints` switch to save needing to rename any unnamed constrants for tables and or columns that change name.
