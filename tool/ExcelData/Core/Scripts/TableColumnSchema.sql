select schema_name(tab.schema_id) + '.' + tab.name as TableName, 
            col.column_id, 
            col.name as ColumnName, 
            col_s.DATA_TYPE, col_s.ORDINAL_POSITION, 
            col_s.CHARACTER_MAXIMUM_LENGTH as MaxLength, 
            col_s.is_nullable,
            col.is_identity as IsIdentity,
            ku.CONSTRAINT_NAME as PrimaryKeyCol, 
            case when fk.object_id is not null then '>-' else null end as rel, 
            schema_name(pk_tab.schema_id) + '.' + pk_tab.name as ReferenceTableName, 
            pk_col.name as ReferenceColumnName, 
            fk.name as fk_constraint_name 
            from sys.tables tab 
            inner join INFORMATION_SCHEMA.COLUMNS col_s on col_s.TABLE_NAME = tab.name and col_s.TABLE_SCHEMA = schema_name(tab.schema_id) 
            left outer join sys.columns col 
            on col.object_id = tab.object_id and col.Name = col_s.COLUMN_NAME 
            left join INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku on ku.COLUMN_NAME = col_s.COLUMN_NAME and ku.TABLE_NAME = col_s.TABLE_NAME and ku.TABLE_SCHEMA = col_s.TABLE_SCHEMA 
            and ku.CONSTRAINT_NAME like '%PK%' 
            left outer join sys.foreign_key_columns fk_cols 
            on fk_cols.parent_object_id = tab.object_id 
            and fk_cols.parent_column_id = col.column_id 
            left outer join sys.foreign_keys fk 
            on fk.object_id = fk_cols.constraint_object_id 
            left outer join sys.tables pk_tab 
            on pk_tab.object_id = fk_cols.referenced_object_id 
            left outer join sys.columns pk_col 
            on pk_col.column_id = fk_cols.referenced_column_id 
            and pk_col.object_id = fk_cols.referenced_object_id 
            order by schema_name(tab.schema_id) + '.' + tab.name, col.column_id 
