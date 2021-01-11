This scenario tests vcdb to confirm it can create the expected schema JSON from a given database.

This scenario tests that vcdb can create scripts to rename a column and its check constraint. As the check constraint isn't named in the JSON the name in the database should be set back to a automatically generated name.