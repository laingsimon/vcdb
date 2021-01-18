The following database entities are tested in the scenarios below.

|Object type|Operation|Scenario|
|----|----|----|
|Table|Read|[Read_AllColumnTypes](Read_AllColumnTypes)<br />[Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)|
|Table|Add|[Deploy_AllColumnTypes](Deploy_AllColumnTypes)|
|Table|Rename|Deploy_RenameTable*<br />[Deploy_MoveTableBetweenSchemasAndRenameTable](Deploy_MoveTableBetweenSchemasAndRenameTable)|
|Table|Change|[Deploy_AddColumn](Deploy_AddColumn)<br />[Deploy_DropColumn](Deploy_DropColumn)<br />[Deploy_AddColumnCustomCollation](Deploy_AddColumnCustomCollation)|
|Table|Drop|[Deploy_DropTable](Deploy_DropTable)|
|Index|Read|[Read_CustomIndexes](Read_CustomIndexes)|
|Index|Add|[Deploy_AddIndex](Deploy_AddIndex)|
|Index|Rename|[Deploy_RenameIndex](Deploy_RenameIndex)|
|Index|Change|[Deploy_AddColumnToIndex](Deploy_AddColumnToIndex)<br />[Deploy_RemoveColumnFromIndex](Deploy_RemoveColumnFromIndex)|
|Index|Drop|[Deploy_DropIndex](Deploy_DropIndex)|
|Column|Read|[Read_AllColumnTypes](Read_AllColumnTypes)<br />[Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)|
|Column|Add|[Deploy_AddColumn](Deploy_AddColumn)<br />[Deploy_AddColumnCustomCollation](Deploy_AddColumnCustomCollation)<br />[Deploy_AllColumnTypes](Deploy_AllColumnTypes)|
|Column|Rename|Deploy_ColumnRename*|
|Column|Change|[Deploy_ColumnRetype](Deploy_ColumnRetype)<br />[Deploy_ColumnChangeCollation](Deploy_ColumnChangeCollation)|
|Column|Drop|[Deploy_DropColumn](Deploy_DropColumn)|
|Default constraint|Read|[Read_AllColumnTypes](Read_AllColumnTypes)<br />[Read_AllColumnTypes_CustomDefaultName](Read_AllColumnTypes_CustomDefaultName)|
|Default constraint|Add|[Deploy_AddColumn](Deploy_AddColumn)|
|Default constraint|Rename|[Deploy_ColumnRenameWithCustomDefaultNameBeingRemoved](Deploy_ColumnRenameWithCustomDefaultNameBeingRemoved)<br />[Deploy_ColumnRenameWithUnnamedDefault](Deploy_ColumnRenameWithUnnamedDefault)<br />[Deploy_RenameTable_WithUnnamedDefault](Deploy_RenameTable_WithUnnamedDefault)<br />[Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck](Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck)|
|Default constraint|Change|[Deploy_ChangeDefault](Deploy_ChangeDefault)|
|Default constraint|Drop|[Deploy_DropDefault](Deploy_DropDefault)|
|Check constraint|Read|[Read_CheckConstraints](Read_CheckConstraints)<br />[Read_MultipleCheckConstraints](Read_MultipleCheckConstraints)|
|Check constraint|Add|[Deploy_AddColumn](Deploy_AddColumn)|
|Check constraint|Rename|[Deploy_ColumnRenameWithCheckBeingRenamed](Deploy_ColumnRenameWithCheckBeingRenamed)<br />[Deploy_ColumnRenameWithCustomCheckNameBeingRemoved](Deploy_ColumnRenameWithCustomCheckNameBeingRemoved)<br />[Deploy_ColumnRenameWithUnnamedCheck](Deploy_ColumnRenameWithUnnamedCheck)<br />[Deploy_RenameTable_WithUnnamedCheck](Deploy_RenameTable_WithUnnamedCheck)<br /> [Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck](Deploy_RenameTableAndColumn_WithUnnamedDefaultAndUnnamedCheck) |
|Check constraint|Change|[Deploy_ChangeCheck](Deploy_ChangeCheck)|
|Check constraint|Drop|[Deploy_DropCheck](Deploy_DropCheck)|
|Description|Read|[Read_Descriptions](Read_Descriptions)|
|Description|Add|[Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)|
|Description|Change|[Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)|
|Description|Drop|[Deploy_ChangeDescriptions](Deploy_ChangeDescriptions)|
|Default collation|Read|[Read_DifferentCollations](Read_DifferentCollations)|
|Default collation|Override|[Deploy_ColumnChangeCollation](Deploy_ColumnChangeCollation)<br />[Deploy_ChangeDatabaseCollation](Deploy_ChangeDatabaseCollation)<br />[Deploy_ChangeDatabaseCollationAndColumnCollation](Deploy_ChangeDatabaseCollationAndColumnCollation)|
|Default collation|Underride|[Deploy_CollationsReturnToServerDefaults](Deploy_CollationsReturnToServerDefaults)|
|Default collation|Change|[Deploy_ChangeDatabaseCollationAndColumnCollation](Deploy_ChangeDatabaseCollationAndColumnCollation)|
|Schema|Read|[Read_CustomSchemas](Read_CustomSchemas)|
|Schema|Add|[Deploy_AddSchema](Deploy_AddSchema)|
|Schema|Rename|[Deploy_RenameSchema](Deploy_RenameSchema)|
|Schema|Transfer-in|[Deploy_MoveTableBetweenSchemas](Deploy_MoveTableBetweenSchemas)<br />[Deploy_MoveTableBetweenSchemasAndRenameTable](Deploy_MoveTableBetweenSchemasAndRenameTable)<br />[Deploy_MoveTableBetweenSchemasAndRenameCheckConstraint](Deploy_MoveTableBetweenSchemasAndRenameCheckConstraint)|
|Schema|Drop|[Deploy_DropSchema](Deploy_DropSchema)|
|Primary key|Read|[Read_PrimaryKeys](Read_PrimaryKeys)|
|Primary key|Add|[Deploy_AddPrimaryKey](Deploy_AddPrimaryKey)|
|Primary key|Change|[Deploy_ChangePrimaryKey](Deploy_ChangePrimaryKey)|
|Primary key|Rename|TBD|
|Primary key|Drop|[Deploy_DropPrimaryKey](Deploy_DropPrimaryKey)<br />[Deploy_ChangePrimaryKeyClustering](Deploy_ChangePrimaryKeyClustering)|