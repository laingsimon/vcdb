The following database entities are tested in the scenarios below.

# Reading
# 1 Tables
- [Read_AllColumnTypes]
- [Read_AllColumnTypes_CustomDefaultName]
# 2 Indexes
- [Read_CustomIndexes]
# 3 Columns
- [Read_AllColumnTypes]
- [Read_AllColumnTypes_CustomDefaultName]
# 4 Default Constraints
- [Read_AllColumnTypes_CustomDefaultName]
# 5 Check Constraints
- [Read_CheckConstraints]
- [Read_MultipleCheckConstraints]
# 6 Descriptions
- [Read_Descriptions]
# 7 Collations
- [Read_DifferentCollations]
# 8 Schemas
- [Read_CustomSchemas]

# Deployment
## 1 Tables
#### .1 Add
- [Deploy_AllColumnTypes]
#### .2 Rename
- [Deploy_RenameTable]
- [Deploy_RenameTable_WithUnnamedCheck]
- [Deploy_RenameTable_WithUnnamedDefault]
- [Deploy_RenameTable_WithUnnamedDefaultIgnoringUnnamedConstraints]
- [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck]
- [Deploy_MoveTableBetweenSchemas]
- [Deploy_MoveTableBetweenSchemasAndRenameTable]
#### .3 Change
- [Deploy_AddColumn]
- [Deploy_DropColumn]
- [Deploy_AddColumnCustomCollation]
#### .4 Drop
- **MISSING**

## 2 Indexes
#### .1 Add
- [Deploy_AddClusteredIndex]
#### .2 Rename
- **MISSING**
#### .3 Change
- [Deploy_AddColumnToIndex]
- [Deploy_RemoveColumnFromIndex]
#### .4 Drop
- [Deploy_DropIndex]

## 3 Columns
#### .1 Add
- [Deploy_AddColumn]
- [Deploy_AddColumnCustomCollation]
- [Deploy_AllColumnTypes]
#### .2 Rename
- Deploy_ColumnRename*
#### .3 Change
- [Deploy_ColumnRetype]
#### .4 Drop
- [Deploy_DropColumn]

## 4 Default Constraints
#### .1 Add
- [Deploy_AddColumn]
#### .2 Rename
- [Deploy_ColumnRenameWithCustomDefaultNameBeingRemoved]
- [Deploy_ColumnRenameWithUnnamedDefault]
- [Deploy_RenameTable_WithUnnamedDefault]
- [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck]
#### .3 Change
- **MISSING**
#### .4 Drop
- [Deploy_DropDefault]

## 5 Check Constraints
#### .1 Add
- [Deploy_AddColumn]
#### .2 Rename
- [Deploy_ColumnRenameWithCheckBeingRenamed]
- [Deploy_ColumnRenameWithCustomCheckNameBeingRemoved]
- [Deploy_ColumnRenameWithUnnamedCheck]
- [Deploy_RenameTable_WithUnnamedCheck]
- [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck]
#### .3 Change
- **MISSING**
#### .4 Drop
- [Deploy_DropCheck]

## 6 Descriptions
#### .1 Add
- [Deploy_ChangeDescriptions]
#### .2 Change
- [Deploy_ChangeDescriptions]
#### .3 Drop
- [Deploy_ChangeDescriptions]

## 7 Collations
#### .1 Override
- [Deploy_ColumnChangeCollation]
- [Deploy_ChangeDatabaseCollation]
- [Deploy_ChangeDatabaseCollationAndColumnCollation]
#### .2 Underride (reset)
- [Deploy_CollationsReturnToServerDefaults]
#### .3 Change
- [Deploy_ChangeDatabaseCollationAndColumnCollation]

## 8 Schemas
#### .1 Add
- [Deploy_AddSchema]
#### .2 Rename
- [Deploy_RenameSchema]
#### .3 Transfer-in
- [Deploy_MoveTableBetweenSchemas]
- [Deploy_MoveTableBetweenSchemasAndRenameTable]
- [Deploy_MoveTableBetweenSchemasAndRenameCheckConstraint]
#### .4 Drop
- [Deploy_DropSchema]