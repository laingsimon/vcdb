This scenario tests vcdb to confirm it can create the expected schema JSON from a given database.

This scenario tests that all built in SQL data types can be read correctly.

MySql generated columns are supported from mysql v8, however the build environment doesn't support mysql v8 & sqlserver at the same time, see issue #9.
Until this issue is resolved, all tests need to be paired back to what mysql v5.7.3 supports.