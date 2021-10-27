 SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS TableName
            FROM INFORMATION_SCHEMA.Tables
            WHERE TABLE_TYPE='BASE TABLE'
            ORDER BY TableName