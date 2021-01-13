The following database entities are tested in the scenarios below.

# Reading
## 1 Tables
- [Read_AllColumnTypes](Read_AllColumnTypes)
- [Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)
## 2 Indexes
- [Read_CustomIndexes](Read_CustomIndexes)
## 3 Columns
- [Read_AllColumnTypes](Read_AllColumnTypes)
- [Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)
## 4 Default Constraints
- [Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)
## 5 Check Constraints
- [Read_CheckConstraints](Read_CheckConstraints)
- [Read_MultipleCheckConstraints](Read_MultipleCheckConstraints)
## 6 Descriptions
- [Read_Descriptions](Read_Descriptions)
## 7 Collations
- [Read_DifferentCollations](Read_DifferentCollations)
## 8 Schemas
- [Read_CustomSchemas](Read_CustomSchemas)

# Deployment
## 1 Tables
#### .1 Add
- [Deploy_AllColumnTypes](Deploy_AllColumnTypes)
#### .2 Rename
- Deploy_RenameTable*
- [Deploy_MoveTableBetweenSchemas](Deploy_MoveTableBetweenSchemas)
- [Deploy_MoveTableBetweenSchemasAndRenameTable](Deploy_MoveTableBetweenSchemasAndRenameTable)
#### .3 Change
- [Deploy_AddColumn](Deploy_AddColumn)
- [Deploy_DropColumn](Deploy_DropColumn)
- [Deploy_AddColumnCustomCollation](Deploy_AddColumnCustomCollation)
#### .4 Drop
- **MISSING**

## 2 Indexes
#### .1 Add
- [Deploy_AddClusteredIndex](Deploy_AddClusteredIndex)
#### .2 Rename
- **MISSING**
#### .3 Change
- [Deploy_AddColumnToIndex](Deploy_AddColumnToIndex)
- [Deploy_RemoveColumnFromIndex](Deploy_RemoveColumnFromIndex)
#### .4 Drop
- [Deploy_DropIndex](Deploy_DropIndex)

## 3 Columns
#### .1 Add
- [Deploy_AddColumn](Deploy_AddColumn)
- [Deploy_AddColumnCustomCollation](Deploy_AddColumnCustomCollation)
- [Deploy_AllColumnTypes](Deploy_AllColumnTypes)
#### .2 Rename
- Deploy_ColumnRename*
#### .3 Change
- [Deploy_ColumnRetype](Deploy_ColumnRetype)
#### .4 Drop
- [Deploy_DropColumn](Deploy_DropColumn)

## 4 Default Constraints
#### .1 Add
- [Deploy_AddColumn](Deploy_AddColumn)
#### .2 Rename
- [Deploy_ColumnRenameWithCustomDefaultNameBeingRemoved](Deploy_ColumnRenameWithCustomDefaultNameBeingRemoved)
- [Deploy_ColumnRenameWithUnnamedDefault](Deploy_ColumnRenameWithUnnamedDefault)
- [Deploy_RenameTable_WithUnnamedDefault](Deploy_RenameTable_WithUnnamedDefault)
- [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck](Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck)
#### .3 Change
- **MISSING**
#### .4 Drop
- [Deploy_DropDefault](Deploy_DropDefault)

## 5 Check Constraints
#### .1 Add
- [Deploy_AddColumn](Deploy_AddColumn)
#### .2 Rename
- [Deploy_ColumnRenameWithCheckBeingRenamed](Deploy_ColumnRenameWithCheckBeingRenamed)
- [Deploy_ColumnRenameWithCustomCheckNameBeingRemoved](Deploy_ColumnRenameWithCustomCheckNameBeingRemoved)
- [Deploy_ColumnRenameWithUnnamedCheck](Deploy_ColumnRenameWithUnnamedCheck)
- [Deploy_RenameTable_WithUnnamedCheck](Deploy_RenameTable_WithUnnamedCheck)
- [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck](Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck)
#### .3 Change
- **MISSING**
#### .4 Drop
- [Deploy_DropCheck](Deploy_DropCheck)

## 6 Descriptions
#### .1 Add
- [Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)
#### .2 Change
- [Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)
#### .3 Drop
- [Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)

## 7 Collations
#### .1 Override
- [Deploy_ColumnChangeCollation](Deploy_ColumnChangeCollation)
- [Deploy_ChangeDatabaseCollation](Deploy_ChangeDatabaseCollation)
- [Deploy_ChangeDatabaseCollationAndColumnCollation](Deploy_ChangeDatabaseCollationAndColumnCollation)
#### .2 Underride (reset)
- [Deploy_CollationsReturnToServerDefaults](Deploy_CollationsReturnToServerDefaults)
#### .3 Change
- [Deploy_ChangeDatabaseCollationAndColumnCollation](Deploy_ChangeDatabaseCollationAndColumnCollation)

## 8 Schemas
#### .1 Add
- [Deploy_AddSchema](Deploy_AddSchema)
#### .2 Rename
- [Deploy_RenameSchema](Deploy_RenameSchema)
#### .3 Transfer-in
- [Deploy_MoveTableBetweenSchemas](Deploy_MoveTableBetweenSchemas)
- [Deploy_MoveTableBetweenSchemasAndRenameTable](Deploy_MoveTableBetweenSchemasAndRenameTable)
- [Deploy_MoveTableBetweenSchemasAndRenameCheckConstraint](Deploy_MoveTableBetweenSchemasAndRenameCheckConstraint)
#### .4 Drop
- [Deploy_DropSchema](Deploy_DropSchema)
