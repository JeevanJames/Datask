 SELECT  OBJECT_SCHEMA_NAME (fkey.referenced_object_id) + '.' +  
         OBJECT_NAME (fkey.referenced_object_id) AS ReferenceTableName 
         ,COL_NAME(fcol.referenced_object_id, fcol.referenced_column_id) AS ReferenceColumnName 
         ,OBJECT_SCHEMA_NAME (fkey.parent_object_id) + '.' +  
         OBJECT_NAME(fkey.parent_object_id) AS TableName 
         ,COL_NAME(fcol.parent_object_id, fcol.parent_column_id) AS ColumnName 
         FROM sys.foreign_keys AS fkey 
         INNER JOIN sys.foreign_key_columns AS fcol ON fkey.OBJECT_ID = fcol.constraint_object_id 
         ORDER BY ReferenceTableName, ReferenceColumnName, TableName, ColumnName 