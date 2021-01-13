This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a schema is renamed. At the same time the table, and one of its columns, in the schema is renamed.
As schemas cannot be renamed, a new schema is created and all bound items are transfered to the new schema. The old schema is then dropped.