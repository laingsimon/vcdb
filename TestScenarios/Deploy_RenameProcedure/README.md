This scenario tests `vcdb` to confirm it can create scripts to effect the appropriate database change.

In this scenario a procedure is renamed.

As one of the procedures is encrypted, it is not possible to detect if the definition has changed. To be on the safe-side the procedure is altered again after it has been renamed.