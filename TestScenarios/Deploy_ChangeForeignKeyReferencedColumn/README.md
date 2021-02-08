This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a foreign key is changed to reference a different column on the same referenced table.

This scenario requires the order of events to be scripted considering the dependency of objects, i.e.
- The PublicId column cannot be promoted to a PRIMARY KEY until the FOREIGN KEY is first dropped
- The current PRIMARY KEY must be dropped before the PublicId column can be added to an exclusive PRIMARY KEY
- The FOREIGN KEY cannot be (re) created until the PRIMARY KEY (containing PublicId) is created

