This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a foreign key references columns that change name in the source and referenced tables.

This scenario requires the order of events to be scripted considering the dependency of objects, i.e.
- The referenced column (Id) cannot be renamed (to CarId) until the FOREIGN KEY is first dropped
- The FOREIGN KEY cannot be (re) created until the column has first been renamed

To make things simple the TableScript builder executes the following phases:
- Drop any constraints, primary and foreign keys
- Rename and retype any columns
- Apply any further changes, including re-creating required constraints, primary and foreign keys