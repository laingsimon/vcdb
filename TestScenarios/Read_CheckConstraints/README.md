This scenario tests vcdb to confirm it can create the expected schema JSON from a given database.

This scenario tests the creation of named and unnamed check constraints.

The expected output/check is : `len([Name])>(3)`
However the check is created with the following statement: `CHECK (LEN([Name]) > 3)`

Note the difference in case, `len` is recorded in lowercase in the database, this is an outstanding discrepancy/issue.
