SELECT
    schema_name(tab.schema_id) + '.' + tab.name AS TableName, 
    col.column_id, 
    col.name AS ColumnName, 
    col_s.DATA_TYPE, col_s.ORDINAL_POSITION, 
    col_s.CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
    col_s.is_nullable,
    col.is_identity AS IsIdentity,
    ku.CONSTRAINT_NAME AS PrimaryKeyCol, 
    CASE WHEN fk.object_id IS NOT NULL THEN '>-' ELSE NULL END AS rel, 
    schema_name(pk_tab.schema_id) + '.' + pk_tab.name AS ReferenceTableName, 
    pk_col.name AS ReferenceColumnName, 
    fk.name AS fk_constraint_name 
FROM sys.tables tab 
    INNER JOIN INFORMATION_SCHEMA.COLUMNS col_s ON
        col_s.TABLE_NAME = tab.name
        AND col_s.TABLE_SCHEMA = schema_name(tab.schema_id) 
    LEFT OUTER JOIN sys.columns col ON
        col.object_id = tab.object_id
        AND col.Name = col_s.COLUMN_NAME 
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku ON
        ku.COLUMN_NAME = col_s.COLUMN_NAME
        AND ku.TABLE_NAME = col_s.TABLE_NAME
        AND ku.TABLE_SCHEMA = col_s.TABLE_SCHEMA 
        AND ku.CONSTRAINT_NAME like '%PK%' 
    LEFT OUTER JOIN sys.foreign_key_columns fk_cols ON
        fk_cols.parent_object_id = tab.object_id 
        AND fk_cols.parent_column_id = col.column_id 
    LEFT OUTER JOIN sys.foreign_keys fk ON
        fk.object_id = fk_cols.constraint_object_id 
    LEFT OUTER JOIN sys.tables pk_tab ON
        pk_tab.object_id = fk_cols.referenced_object_id 
    LEFT OUTER JOIN sys.columns pk_col ON
        pk_col.column_id = fk_cols.referenced_column_id 
        AND pk_col.object_id = fk_cols.referenced_object_id 
ORDER BY schema_name(tab.schema_id) + '.' + tab.name, col.column_id 
