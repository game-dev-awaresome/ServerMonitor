using System;
using System.Collections.Generic;
using System.Text;

namespace System.DataCenter
{
    public static class SqlConstant
    {
        /// <summary>
        /// 修改数据表模板SQL-修改
        /// </summary>
        public const string Str_AlterTable_Alter = @"
                                                ALTER TABLE [ database_name ] table_name{ 
                                                    ALTER COLUMN column_name{ 
                                                        [ type_schema_name. ] type_name [ ( { precision [ , scale ] | max | xml_schema_collection } ) ] 
                                                        [ NULL | NOT NULL ] 
                                                        [ COLLATE collation_name ] 
                                                    | {ADD | DROP } { ROWGUIDCOL | PERSISTED }}}
                                                ";

        /// <summary>
        /// 修改数据表模板SQL-添加
        /// </summary>
        public const string Str_AlterTable_ADD = @"
                                                ALTER TABLE [ database_name ] table_name 
                                                { 
                                                    [ WITH { CHECK | NOCHECK } ] ADD 
                                                    { 
                                                        <column_definition> 
                                                        | <computed_column_definition> 
                                                        | <table_constraint> 
                                                    }
                                                }
                                                ";
        public const string Str_AlterTable_ADD_Key = @"
                                                    [ CONSTRAINT constraint_name ] 
                                                    { 
                                                        { PRIMARY KEY | UNIQUE } 
                                                            [ CLUSTERED | NONCLUSTERED ] 
                                                                    (column [ ASC | DESC ] [ ,...n ] )
                                                            [ WITH FILLFACTOR = fillfactor 
                                                            [ WITH ( <index_option>[ , ...n ] ) ]
                                                            [ ON { partition_scheme_name ( partition_column_name ... )
                                                              | filegroup | default } ] 
                                                        | FOREIGN KEY 
                                                                    ( column [ ,...n ] )
                                                            REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ] 
                                                            [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ] 
                                                            [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ] 
                                                            [ NOT FOR REPLICATION ] 
                                                        | DEFAULT constant_expression FOR column [ WITH VALUES ] 
                                                        | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
                                                    }
                                                    ";

        /// <summary>
        /// 修改数据表模板SQL-删除
        /// </summary>
        public const string Str_AlterTable_DROP = @"
                                                ALTER TABLE [ database_name ] table_name
                                                    DROP                                                      
                                                        [ CONSTRAINT ] constraint_name  [ WITH ( <drop_clustered_constraint_option> [ ,...n ] ) ]
                                                        | COLUMN column_name                                                     
                                                
                                                ";
    }

    public static class MSSQL_SqlConstant
    {
        /// <summary>
        /// MSSQL数据库列表
        /// </summary>
        public const string Str_GetDatabase = @"SELECT 
                                                    name AS DB_Name 
                                                FROM 
                                                    sys.sysdatabases 
                                                ORDER BY 
                                                    dbid
                                                ";

        public const string Str_2000_GetDatabase = @"SELECT 
                                                    name AS DB_Name 
                                                FROM 
                                                    sysdatabases 
                                                ORDER BY 
                                                    dbid
                                                ";

        #region 表
        public const string Str_GetTableList = @"--数据表
                                            SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'T' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS
                                            WHERE
	                                            xtype = 'U'";

        public const string Str_2000_GetTableList = @"SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'T' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast('N/A' as nvarchar(4000)) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS
                                            WHERE
	                                            xtype = 'U'";
        #endregion

        #region 视图
        public const string Str_GetViewList = @"--视图
                                            SELECT
	                                            VIEW_DEFINITION AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'V' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS,INFORMATION_SCHEMA.VIEWS
                                            WHERE
	                                            xtype = 'V' AND name = TABLE_NAME";

        public const string Str_2000_GetViewList = @"--视图
                                            SELECT
	                                            VIEW_DEFINITION AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'V' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast('N/A' as nvarchar(4000)) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS,INFORMATION_SCHEMA.VIEWS
                                            WHERE
	                                            xtype = 'V' AND name = TABLE_NAME";
        #endregion

        #region 存储过程
        public const string Str_GetProcList = @"--存储过程
                                            SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS TEXT,
	                                            'P' AS NAME_KIND,
	                                            SPECIFIC_NAME AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            ROUTINE_DEFINITION AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            CREATED AS CREATE_DATA,
	                                            LAST_ALTERED AS ALTER_DATA	
                                            FROM
	                                            INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
	                                            ROUTINE_TYPE = 'PROCEDURE'";

        public const string Str_2000_GetProcList = @"--存储过程
                                            SELECT
                                                'N/A' AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS TEXT,
                                                'P' AS NAME_KIND,
                                                SPECIFIC_NAME AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION, 
                                                cast(ROUTINE_DEFINITION as nvarchar(4000)) AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                CREATED AS CREATE_DATA,
                                                LAST_ALTERED AS ALTER_DATA	
                                            FROM
                                                INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
                                                ROUTINE_TYPE = 'PROCEDURE'";
        #endregion

        #region 函数
        public const string Str_GetFunctionList = @"SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'FC' AS NAME_KIND,
	                                            SPECIFIC_NAME AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            ROUTINE_DEFINITION AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            CREATED AS CREATE_DATA,
	                                            LAST_ALTERED AS ALTER_DATA	
                                            FROM
	                                            INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
	                                            ROUTINE_TYPE = 'FUNCTION'";

        public const string Str_2000_GetFunctionList = @"--函数
                                            SELECT
                                                'N/A' AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'FC' AS NAME_KIND,
                                                SPECIFIC_NAME AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                --ROUTINE_DEFINITION AS COLLATION_NAME,
                                                cast(ROUTINE_DEFINITION as nvarchar(4000)) AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                CREATED AS CREATE_DATA,
                                                LAST_ALTERED AS ALTER_DATA	
                                            FROM
                                                INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
                                                ROUTINE_TYPE = 'FUNCTION'";
        #endregion

        #region 表信息
        public const string Str_GetTable = @"--字段及详细类型
                                                SELECT
                                                    DISTINCT TABLE_NAME AS NAMES_Layer,
                                                    (SELECT is_identity FROM sys.columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS IS_IDENTITY,
                                                    (SELECT CONVERT(VARCHAR,seed_value) FROM sys.identity_columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS SEED_VALUE,
                                                    (SELECT CONVERT(VARCHAR,increment_value) FROM sys.identity_columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS INCREMENT_VALUE,
                                                    'N/A' AS COLUMN_NAME,
                                                    'N/A' AS DEFAULT_VALUE,
                                                    'F' AS NAME_KIND,
                                                    COLUMN_NAME AS NAME_ID,
                                                    'N/A' AS CONSTRAINT_TYPE,
                                                    ORDINAL_POSITION,
                                                    COLUMN_DEFAULT,
                                                    IS_NULLABLE,
                                                    DATA_TYPE,
                                                    CHARACTER_MAXIMUM_LENGTH,
                                                    CHARACTER_OCTET_LENGTH,
                                                    NUMERIC_PRECISION,
                                                    NUMERIC_PRECISION_RADIX,
                                                    NUMERIC_SCALE,
                                                    DATETIME_PRECISION,
                                                    COLLATION_NAME,
                                                    (SELECT CONVERT(VARCHAR,value) FROM sys.extended_properties WHERE class_desc = 'OBJECT_OR_COLUMN' AND name = 'MS_Description'
                                                    AND major_id = (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) 
                                                    AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id=(SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME))
                                                    AS EXTEND_VALUE,
                                                    getdate() AS CREATE_DATA,
                                                    getdate() AS ALTER_DATA
                                                FROM
                                                    INFORMATION_SCHEMA.COLUMNS AS a
                                                WHERE
	                                                TABLE_NAME = '$table_name'
                                                UNION
                                                --主键、UNIQUE约束
                                                SELECT
                                                    DISTINCT TABLE_NAME AS NAMES_Layer,
                                                    0 AS IS_IDENTITY,
                                                    'N/A' AS SEED_VALUE,
                                                    'N/A' AS INCREMENT_VALUE,
                                                    (SELECT
                                                        TOP 1 c.name
                                                     FROM 
                                                        sys.index_columns k 
                                                        INNER JOIN sys.indexes i ON k.object_id = i.object_id and k.index_id = i.index_id and i.name = q.CONSTRAINT_NAME
                                                        INNER JOIN syscolumns c ON k.object_id = c.id AND k.column_id = c.colid
                                                    where
                                                        i.name <> 'pk_dtproperties'
                                                    ) AS COLUMN_NAME,
                                                    'N/A' AS DEFAULT_VALUE,
                                                    'K' AS NAME_KIND,
                                                    CONSTRAINT_NAME AS NAME_ID,
                                                    CONSTRAINT_TYPE,
                                                    0 AS ORDINAL_POSITION,
                                                    'N/A' AS COLUMN_DEFAULT,
                                                    'N/A' AS IS_NULLABLE,
                                                    'N/A' AS DATA_TYPE,
                                                    0 AS CHARACTER_MAXIMUM_LENGTH,
                                                    0 AS CHARACTER_OCTET_LENGTH,
                                                    0 AS NUMERIC_PRECISION,
                                                    0 AS NUMERIC_PRECISION_RADIX,
                                                    0 AS NUMERIC_SCALE,
                                                    0 AS DATETIME_PRECISION,
                                                    'N/A' AS COLLATION_NAME,
                                                    'N/A' AS EXTEND_VALUE,
                                                    getdate() AS CREATE_DATA,
                                                    getdate() AS ALTER_DATA
                                                FROM
                                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS as q
                                                WHERE 
                                                    (CONSTRAINT_TYPE = 'PRIMARY KEY' or CONSTRAINT_TYPE = 'UNIQUE') AND TABLE_NAME = '$table_name'
                                                UNION
                                                --外键
                                                SELECT
                                                    DISTINCT (select name from sysobjects where id = parent_object_id) AS NAMES_Layer,
                                                    0 AS IS_IDENTITY,
                                                    (select column_name from INFORMATION_SCHEMA.COLUMNS 
                                                    where table_name = (select name from sysobjects where id = parent_object_id)
                                                    and ordinal_position = (select parent_column_id from sys.foreign_key_columns where constraint_object_id = object_id))
                                                    AS SEED_VALUE,
                                                    (select name from sysobjects where id =parent_object_id) AS INCREMENT_VALUE,
                                                    (select column_name from INFORMATION_SCHEMA.COLUMNS 
                                                    where table_name = (select name from sysobjects where id = referenced_object_id)
                                                    and ordinal_position = (select referenced_column_id from sys.foreign_key_columns where constraint_object_id = object_id))
                                                    AS COLUMN_NAME,
                                                    (select name from sysobjects where id = referenced_object_id) AS DEFAULT_VALUE,
                                                    'FK' AS NAME_KIND,
                                                    name AS NAME_ID,
                                                    'N/A' AS CONSTRAINT_TYPE,
                                                    0 AS ORDINAL_POSITION,
                                                    'N/A' AS COLUMN_DEFAULT,
                                                    'N/A' AS IS_NULLABLE,
                                                    'N/A' AS DATA_TYPE,
                                                    0 AS CHARACTER_MAXIMUM_LENGTH,
                                                    0 AS CHARACTER_OCTET_LENGTH,
                                                    0 AS NUMERIC_PRECISION,
                                                    0 AS NUMERIC_PRECISION_RADIX,
                                                    0 AS NUMERIC_SCALE,
                                                    0 AS DATETIME_PRECISION,
                                                    'N/A' AS COLLATION_NAME,
                                                    'N/A' AS EXTEND_VALUE,
                                                    create_date AS CREATE_DATA,
                                                    modify_date AS ALTER_DATA
                                                FROM
                                                    sys.foreign_keys
                                                WHERE 
	                                                (SELECT object_id FROM sys.objects where name = '$table_name') = parent_object_id
                                                UNION
                                                --CHECK约束
                                                SELECT
                                                    DISTINCT TABLE_NAME AS NAMES_Layer,
                                                    0 AS IS_IDENTITY,
                                                    'N/A' AS SEED_VALUE,
                                                    'N/A' AS INCREMENT_VALUE,
                                                    'N/A' AS COLUMN_NAME,
                                                    (
                                                        SELECT
                                                        c.text
                                                        FROM 
                                                        sysobjects o 
                                                        INNER JOIN syscomments c ON o.id = c.id 
                                                        INNER JOIN sys.objects t ON t.object_id = o.parent_obj
                                                        INNER JOIN sys.schemas u ON u.schema_id = t.schema_id 
                                                        LEFT OUTER JOIN sys.extended_properties p ON p.major_id = o.id AND p.name = 'MS_Description' 
                                                        WHERE 
                                                        o.name = q.CONSTRAINT_NAME AND o.xtype = 'C'
                                                    ) AS DEFAULT_VALUE,
                                                    'K' AS NAME_KIND,
                                                    CONSTRAINT_NAME AS NAME_ID,
                                                    CONSTRAINT_TYPE,
                                                    0 AS ORDINAL_POSITION,
                                                    'N/A' AS COLUMN_DEFAULT,
                                                    'N/A' AS IS_NULLABLE,
                                                    'N/A' AS DATA_TYPE,
                                                    0 AS CHARACTER_MAXIMUM_LENGTH,
                                                    0 AS CHARACTER_OCTET_LENGTH,
                                                    0 AS NUMERIC_PRECISION,
                                                    0 AS NUMERIC_PRECISION_RADIX,
                                                    0 AS NUMERIC_SCALE,
                                                    0 AS DATETIME_PRECISION,
                                                    'N/A' AS COLLATION_NAME,
                                                    'N/A' AS EXTEND_VALUE,
                                                    getdate() AS CREATE_DATA,
                                                    getdate() AS ALTER_DATA
                                                FROM
                                                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS as q
                                                WHERE 
                                                    CONSTRAINT_TYPE = 'CHECK' AND TABLE_NAME = '$table_name'
                                                UNION
                                                --DEFAULT约束
                                                SELECT
                                                    (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                    0 AS IS_IDENTITY,
                                                    'N/A' AS SEED_VALUE,
                                                    'N/A' AS INCREMENT_VALUE,
                                                    (SELECT c.name
                                                    FROM 
                                                    sys.columns c 
                                                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id 
                                                    INNER JOIN sys.schemas tu ON tu.schema_id = t.schema_id 
                                                    INNER JOIN sys.types st ON c.system_type_id = st.user_type_id 
                                                    INNER JOIN sys.objects tb ON tb.object_id = c.object_id 
                                                    INNER JOIN sys.schemas u ON u.schema_id = tb.schema_id 
                                                    LEFT OUTER JOIN sys.objects d ON d.object_id = c.default_object_id 
                                                    LEFT OUTER JOIN sys.schemas du ON du.schema_id = d.schema_id 
                                                    LEFT OUTER JOIN sys.default_constraints cm ON cm.object_id = d.object_id 
                                                    LEFT OUTER JOIN sys.computed_columns fr ON fr.object_id = c.object_id AND fr.column_id = c.column_id 
                                                    LEFT OUTER JOIN sys.objects r ON r.object_id = c.rule_object_id LEFT OUTER JOIN sys.schemas ru ON ru.schema_id = r.schema_id 
                                                    LEFT OUTER JOIN sys.xml_schema_collections xc ON xc.xml_collection_id = c.xml_collection_id 
                                                    LEFT OUTER JOIN sys.schemas xu ON xu.schema_id = xc.schema_id 
                                                    LEFT OUTER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id AND p.class = 1 AND p.name = 'MS_Description'  
                                                    WHERE 
                                                    tb.name = (select name from sysobjects where id = a.parent_obj) AND d.name = a.name)  as COLUMN_NAME,
                                                    (SELECT cm.definition
                                                    FROM 
                                                    sys.columns c 
                                                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id 
                                                    INNER JOIN sys.schemas tu ON tu.schema_id = t.schema_id 
                                                    INNER JOIN sys.types st ON c.system_type_id = st.user_type_id 
                                                    INNER JOIN sys.objects tb ON tb.object_id = c.object_id 
                                                    INNER JOIN sys.schemas u ON u.schema_id = tb.schema_id 
                                                    LEFT OUTER JOIN sys.objects d ON d.object_id = c.default_object_id 
                                                    LEFT OUTER JOIN sys.schemas du ON du.schema_id = d.schema_id 
                                                    LEFT OUTER JOIN sys.default_constraints cm ON cm.object_id = d.object_id 
                                                    LEFT OUTER JOIN sys.computed_columns fr ON fr.object_id = c.object_id AND fr.column_id = c.column_id 
                                                    LEFT OUTER JOIN sys.objects r ON r.object_id = c.rule_object_id LEFT OUTER JOIN sys.schemas ru ON ru.schema_id = r.schema_id 
                                                    LEFT OUTER JOIN sys.xml_schema_collections xc ON xc.xml_collection_id = c.xml_collection_id 
                                                    LEFT OUTER JOIN sys.schemas xu ON xu.schema_id = xc.schema_id 
                                                    LEFT OUTER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id AND p.class = 1 AND p.name = 'MS_Description'  
                                                    WHERE 
                                                    tb.name = (select name from sysobjects where id = a.parent_obj) AND d.name = a.name) AS DEFAULT_VALUE,
                                                    'D' AS NAME_KIND,
                                                    name AS NAME_ID,
                                                    'DEFAULT' AS CONSTRAINT_TYPE,
                                                    0 AS ORDINAL_POSITION,
                                                    'N/A' AS COLUMN_DEFAULT,
                                                    'N/A' AS IS_NULLABLE,
                                                    'N/A' AS DATA_TYPE,
                                                    0 AS CHARACTER_MAXIMUM_LENGTH,
                                                    0 AS CHARACTER_OCTET_LENGTH,
                                                    0 AS NUMERIC_PRECISION,
                                                    0 AS NUMERIC_PRECISION_RADIX,
                                                    0 AS NUMERIC_SCALE,
                                                    0 AS DATETIME_PRECISION,
                                                    'N/A' AS COLLATION_NAME,
                                                    'N/A' AS EXTEND_VALUE,
                                                    crdate AS CREATE_DATA,
                                                    refdate AS ALTER_DATA
                                                FROM
                                                    sys.sysobjects as a 
                                                WHERE
                                                    xtype = 'D' and (SELECT object_id FROM sys.objects where name = '$table_name') = a.parent_obj
                                            union
                                            --触发器
                                            SELECT
	                                            (SELECT name FROM sysobjects WHERE ID = A.parent_obj) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'TR' AS NAME_KIND,
	                                            A.name AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS AS A
                                            WHERE
	                                            A.xtype = 'TR' and (SELECT object_id FROM sys.objects where name = '$table_name') = A.parent_obj
                                            union
                                            --触发器内容
                                            SELECT
	                                            (SELECT name FROM sys.objects WHERE  object_id = a.parent_object_id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'TR_TEXT' AS NAME_KIND,
	                                            a.name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            (SELECT text FROM sys.syscomments WHERE id = a.object_id) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            create_date AS CREATE_DATA,
	                                            modify_date AS ALTER_DATA	
                                            FROM
	                                            sys.objects AS a 
                                            WHERE 
	                                            type_desc = 'SQL_TRIGGER' and (SELECT object_id FROM sys.objects where name = '$table_name') = a.parent_object_id";

        public const string Str_2000_GetTable = @" --字段及详细类型
                                            SELECT 
	                                            (SELECT name FROM sysobjects WHERE ID = c.id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            CONVERT(VARCHAR,ident_seed(tb.name)) AS SEED_VALUE,
	                                            CONVERT(VARCHAR,ident_incr(tb.name)) AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'F' AS NAME_KIND,
	                                            c.name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            c.colid AS ORDINAL_POSITION,
	                                            CAST(p.[value] AS varchar(8000)) AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            t.name AS DATA_TYPE,
	                                            c.length AS CHARACTER_MAXIMUM_LENGTH,
	                                            c.length AS CHARACTER_OCTET_LENGTH,
	                                            c.prec AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            c.scale AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast(c.collation as nvarchar(4000)) AS COLLATION_NAME,
	                                            '' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA	
                                            FROM 
	                                            syscolumns c 
	                                            INNER JOIN systypes t ON c.xusertype = t.xusertype 
	                                            --INNER JOIN systypes st ON c.xtype = st.xusertype 
	                                            --LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            --LEFT OUTER JOIN sysusers du ON du.uid = d.uid 
	                                            --LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            --LEFT OUTER JOIN syscomments fr ON fr.id = c.id AND fr.number = c.colid 
	                                            --LEFT OUTER JOIN sysobjects r ON r.id = c.domain 
	                                            --LEFT OUTER JOIN sysusers ru ON ru.uid = r.uid 
	                                            INNER JOIN sysobjects tb ON tb.id = c.id 
	                                            INNER JOIN sysusers u ON u.uid = tb.uid 
	                                            LEFT OUTER JOIN sysproperties p ON p.id = c.id AND p.smallid = c.colid AND p.type = 4 AND p.name = 'MS_Description' 
                                            WHERE 
	                                            u.name = N'dbo' and tb.xtype = 'U' and (SELECT id FROM sysobjects WHERE name = '$table_name') = c.id
                                            UNION all
                                            --主键
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,

                                                (SELECT top 1 c.name
                                                FROM 
                                                sysindexkeys k 
                                                INNER JOIN sysindexes i ON k.id = i.id and k.indid = i.indid 
                                                INNER JOIN syscolumns c ON k.id = c.id AND k.colid = c.colid 
                                                where i.name = a.name) AS COLUMN_NAME,

                                                'N/A' AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'PRIMARY KEY' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'PK' and (SELECT id FROM sysobjects WHERE name = '$table_name') = a.parent_obj
                                            UNION ALL
                                            --唯一键
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,

                                                (SELECT top 1 c.name
                                                FROM 
                                                sysindexkeys k 
                                                INNER JOIN sysindexes i ON k.id = i.id and k.indid = i.indid 
                                                INNER JOIN syscolumns c ON k.id = c.id AND k.colid = c.colid 
                                                where i.name = a.name) AS COLUMN_NAME,

                                                'N/A' AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'UNIQUE' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'UQ' and (SELECT id FROM sysobjects WHERE name = '$table_name') = a.parent_obj
                                            UNION ALL
                                            --check约束
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                (SELECT TEXT FROM syscomments WHERE ID = a.id) AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'CHECK' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'C' and (SELECT id FROM sysobjects WHERE name = '$table_name') = a.parent_obj
                                            UNION ALL
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                ( SELECT top 1 c.name
	                                            FROM 
	                                            syscolumns c 
	                                            LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            WHERE d.parent_obj = a.parent_obj and d.name =  a.name)
                                                AS COLUMN_NAME,
                                                ( SELECT top 1 cm.text
	                                            FROM 
	                                            syscolumns c 
	                                            LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            WHERE d.parent_obj = a.parent_obj and d.name =  a.name)
                                                AS DEFAULT_VALUE,
                                                'D' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'DEFAULT' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'D' and (SELECT id FROM sysobjects WHERE name = '$table_name') = a.parent_obj
                                            UNION ALL
                                            --触发器
                                            SELECT
                                                (SELECT name FROM sysobjects WHERE ID = A.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'TR' AS NAME_KIND,
                                                A.name AS NAME_ID,
                                                'N/A' AS CONSTARINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA
                                            FROM
                                                SYSOBJECTS AS A 
                                            WHERE
                                                A.xtype = 'TR' and (SELECT id FROM sysobjects WHERE name = '$table_name') = a.parent_obj
                                            union all
                                            --触发器内容----------
                                            SELECT
                                                (SELECT name FROM sysobjects WHERE  id = o.id) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'TR_TEXT' AS NAME_KIND,
                                                o.name AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                CASE WHEN c.encrypted = '1' THEN '' ELSE c.text END AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA	
                                            FROM
                                                sysobjects AS o 
                                                LEFT OUTER JOIN syscomments c ON o.id = c.id AND c.colid = 1 
                                            WHERE 
                                                o.xtype = 'TR' AND (SELECT id FROM sysobjects WHERE name = '$table_name') = (SELECT parent_obj FROM sysobjects WHERE  id = o.id)
					    
                                            ";
        #endregion

        #region 视图信息
        public const string Str_GetView = @"--字段及详细类型
                                                SELECT
                                                    DISTINCT TABLE_NAME AS NAMES_Layer,
                                                    (SELECT is_identity FROM sys.columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS IS_IDENTITY,
                                                    (SELECT CONVERT(VARCHAR,seed_value) FROM sys.identity_columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS SEED_VALUE,
                                                    (SELECT CONVERT(VARCHAR,increment_value) FROM sys.identity_columns WHERE object_id = 
                                                        (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS INCREMENT_VALUE,
                                                    'N/A' AS COLUMN_NAME,
                                                    'N/A' AS DEFAULT_VALUE,
                                                    'F' AS NAME_KIND,
                                                    COLUMN_NAME AS NAME_ID,
                                                    'N/A' AS CONSTRAINT_TYPE,
                                                    ORDINAL_POSITION,
                                                    COLUMN_DEFAULT,
                                                    IS_NULLABLE,
                                                    DATA_TYPE,
                                                    CHARACTER_MAXIMUM_LENGTH,
                                                    CHARACTER_OCTET_LENGTH,
                                                    NUMERIC_PRECISION,
                                                    NUMERIC_PRECISION_RADIX,
                                                    NUMERIC_SCALE,
                                                    DATETIME_PRECISION,
                                                    COLLATION_NAME,
                                                    (SELECT CONVERT(VARCHAR,value) FROM sys.extended_properties WHERE class_desc = 'OBJECT_OR_COLUMN' AND name = 'MS_Description'
                                                    AND major_id = (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) 
                                                    AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id=(SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME))
                                                    AS EXTEND_VALUE,
                                                    getdate() AS CREATE_DATA,
                                                    getdate() AS ALTER_DATA
                                                FROM
                                                    INFORMATION_SCHEMA.COLUMNS AS a
                                                WHERE
	                                                TABLE_NAME = '$view_name'
                                                ";

        public const string Str_2000_GetView = @" --字段及详细类型
                                            SELECT 
	                                            (SELECT name FROM sysobjects WHERE ID = c.id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            CONVERT(VARCHAR,ident_seed(tb.name)) AS SEED_VALUE,
	                                            CONVERT(VARCHAR,ident_incr(tb.name)) AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'F' AS NAME_KIND,
	                                            c.name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            c.colid AS ORDINAL_POSITION,
	                                            CAST(p.[value] AS varchar(8000)) AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            t.name AS DATA_TYPE,
	                                            c.length AS CHARACTER_MAXIMUM_LENGTH,
	                                            c.length AS CHARACTER_OCTET_LENGTH,
	                                            c.prec AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            c.scale AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast(c.collation as nvarchar(4000)) AS COLLATION_NAME,
	                                            '' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA	
                                            FROM 
	                                            syscolumns c 
	                                            INNER JOIN systypes t ON c.xusertype = t.xusertype 
	                                            --INNER JOIN systypes st ON c.xtype = st.xusertype 
	                                            --LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            --LEFT OUTER JOIN sysusers du ON du.uid = d.uid 
	                                            --LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            --LEFT OUTER JOIN syscomments fr ON fr.id = c.id AND fr.number = c.colid 
	                                            --LEFT OUTER JOIN sysobjects r ON r.id = c.domain 
	                                            --LEFT OUTER JOIN sysusers ru ON ru.uid = r.uid 
	                                            INNER JOIN sysobjects tb ON tb.id = c.id 
	                                            INNER JOIN sysusers u ON u.uid = tb.uid 
	                                            LEFT OUTER JOIN sysproperties p ON p.id = c.id AND p.smallid = c.colid AND p.type = 4 AND p.name = 'MS_Description' 
                                            WHERE 
	                                            u.name = N'dbo' and tb.xtype = 'U' and (SELECT id FROM sysobjects WHERE name = '$view_name') = c.id
                                            ";
        #endregion

        #region 存储过程参数
        public const string Str_GetProc = @"
                                            --存储过程
                                            SELECT
	                                            DISTINCT SPECIFIC_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'PP' AS NAME_KIND,
	                                            PARAMETER_NAME AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            DATA_TYPE,
	                                            CHARACTER_MAXIMUM_LENGTH,
	                                            CHARACTER_OCTET_LENGTH,
	                                            NUMERIC_PRECISION,
	                                            NUMERIC_PRECISION_RADIX,
	                                            NUMERIC_SCALE,
	                                            DATETIME_PRECISION,
	                                            COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
	                                            PARAMETER_MODE = 'IN' and SPECIFIC_NAME = '$proc_name'";

        public const string Str_2000_GetProc = @"
                                            --存储过程
                                            SELECT
                                                DISTINCT SPECIFIC_NAME AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'PP' AS NAME_KIND,
                                                PARAMETER_NAME AS NAME_ID,
                                                'N/A' AS CONSTARINT_TYPE,
                                                ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                DATA_TYPE,
                                                CHARACTER_MAXIMUM_LENGTH,
                                                CHARACTER_OCTET_LENGTH,
                                                NUMERIC_PRECISION,
                                                NUMERIC_PRECISION_RADIX,
                                                NUMERIC_SCALE,
                                                DATETIME_PRECISION,
                                                COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA
                                            FROM
                                                INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
                                                PARAMETER_MODE = 'IN' and SPECIFIC_NAME = '$proc_name'";
        #endregion

        #region 函数参数
        public const string Str_GetFunction = @"--函数参数
                                            SELECT
	                                            DISTINCT SPECIFIC_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'PP' AS NAME_KIND,
	                                            PARAMETER_NAME AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            DATA_TYPE,
	                                            CHARACTER_MAXIMUM_LENGTH,
	                                            CHARACTER_OCTET_LENGTH,
	                                            NUMERIC_PRECISION,
	                                            NUMERIC_PRECISION_RADIX,
	                                            NUMERIC_SCALE,
	                                            DATETIME_PRECISION,
	                                            COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
	                                            PARAMETER_MODE = 'IN' and SPECIFIC_NAME = '$function_name'";

        public const string Str_2000_GetFunction = @"
                                            --函数参数
                                            SELECT
	                                            DISTINCT SPECIFIC_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'PP' AS NAME_KIND,
	                                            PARAMETER_NAME AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            DATA_TYPE,
	                                            CHARACTER_MAXIMUM_LENGTH,
	                                            CHARACTER_OCTET_LENGTH,
	                                            NUMERIC_PRECISION,
	                                            NUMERIC_PRECISION_RADIX,
	                                            NUMERIC_SCALE,
	                                            DATETIME_PRECISION,
	                                            COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
	                                            PARAMETER_MODE = 'IN' and SPECIFIC_NAME = '$function_name'
";
        #endregion

        #region 2005
        /// <summary>
        /// MSSQL数据库表、视图、存储过程、触发器、函数等结构
        /// </summary>
        public const string Str_Structure = @"
                                            --数据表
                                            SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'T' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS
                                            WHERE
	                                            xtype = 'U'
                                            UNION
                                            --视图
                                            SELECT
	                                            VIEW_DEFINITION AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'V' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS,INFORMATION_SCHEMA.VIEWS
                                            WHERE
	                                            xtype = 'V' AND name = TABLE_NAME
                                            UNION
                                            --字段及详细类型
                                            SELECT
	                                            DISTINCT TABLE_NAME AS NAMES_Layer,
	                                            (SELECT is_identity FROM sys.columns WHERE object_id = 
		                                            (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS IS_IDENTITY,
	                                            (SELECT CONVERT(VARCHAR,seed_value) FROM sys.identity_columns WHERE object_id = 
		                                            (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS SEED_VALUE,
	                                            (SELECT CONVERT(VARCHAR,increment_value) FROM sys.identity_columns WHERE object_id = 
		                                            (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME) AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'F' AS NAME_KIND,
	                                            COLUMN_NAME AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            ORDINAL_POSITION,
	                                            COLUMN_DEFAULT,
	                                            IS_NULLABLE,
	                                            DATA_TYPE,
	                                            CHARACTER_MAXIMUM_LENGTH,
	                                            CHARACTER_OCTET_LENGTH,
	                                            NUMERIC_PRECISION,
	                                            NUMERIC_PRECISION_RADIX,
	                                            NUMERIC_SCALE,
	                                            DATETIME_PRECISION,
	                                            COLLATION_NAME,
	                                            (SELECT CONVERT(VARCHAR,value) FROM sys.extended_properties WHERE class_desc = 'OBJECT_OR_COLUMN' AND name = 'MS_Description'
	                                            AND major_id = (SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) 
	                                            AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id=(SELECT object_id FROM sys.objects WHERE name = a.TABLE_NAME) AND name = a.COLUMN_NAME))
	                                            AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.COLUMNS AS a
                                            UNION
                                            --主键、UNIQUE约束
                                            SELECT
	                                            DISTINCT TABLE_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            (SELECT
		                                            TOP 1 c.name
	                                             FROM 
		                                            sys.index_columns k 
		                                            INNER JOIN sys.indexes i ON k.object_id = i.object_id and k.index_id = i.index_id and i.name = q.CONSTRAINT_NAME
		                                            INNER JOIN syscolumns c ON k.object_id = c.id AND k.column_id = c.colid
	                                            where
		                                            i.name <> 'pk_dtproperties'
	                                            ) AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'K' AS NAME_KIND,
	                                            CONSTRAINT_NAME AS NAME_ID,
	                                            CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS as q
                                            WHERE 
	                                            CONSTRAINT_TYPE = 'PRIMARY KEY' or CONSTRAINT_TYPE = 'UNIQUE'
                                            UNION
                                            --外键
                                            SELECT
	                                            DISTINCT (select name from sysobjects where id =parent_object_id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            (select column_name from INFORMATION_SCHEMA.COLUMNS 
	                                            where table_name = (select name from sysobjects where id = parent_object_id)
	                                            and ordinal_position = (select parent_column_id from sys.foreign_key_columns where constraint_object_id = object_id))
	                                            AS SEED_VALUE,
	                                            (select name from sysobjects where id =parent_object_id) AS INCREMENT_VALUE,
	                                            (select column_name from INFORMATION_SCHEMA.COLUMNS 
	                                            where table_name = (select name from sysobjects where id = referenced_object_id)
	                                            and ordinal_position = (select referenced_column_id from sys.foreign_key_columns where constraint_object_id = object_id))
	                                            AS COLUMN_NAME,
	                                            (select name from sysobjects where id = referenced_object_id) AS DEFAULT_VALUE,
	                                            'FK' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            create_date AS CREATE_DATA,
	                                            modify_date AS ALTER_DATA
                                            FROM
	                                            sys.foreign_keys
                                            UNION
                                            --CHECK约束
                                            SELECT
	                                            DISTINCT TABLE_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            (
		                                            SELECT
		                                            c.text
		                                            FROM 
		                                            sysobjects o 
		                                            INNER JOIN syscomments c ON o.id = c.id 
		                                            INNER JOIN sys.objects t ON t.object_id = o.parent_obj
		                                            INNER JOIN sys.schemas u ON u.schema_id = t.schema_id 
		                                            LEFT OUTER JOIN sys.extended_properties p ON p.major_id = o.id AND p.name = 'MS_Description' 
		                                            WHERE 
		                                            o.name = q.CONSTRAINT_NAME AND o.xtype = 'C'
	                                            ) AS DEFAULT_VALUE,
	                                            'K' AS NAME_KIND,
	                                            CONSTRAINT_NAME AS NAME_ID,
	                                            CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS as q
                                            WHERE 
	                                            CONSTRAINT_TYPE = 'CHECK'
                                            UNION
                                            --DEFAULT约束
                                            SELECT
	                                            (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            (SELECT c.name
	                                            FROM 
	                                            sys.columns c 
	                                            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id 
	                                            INNER JOIN sys.schemas tu ON tu.schema_id = t.schema_id 
	                                            INNER JOIN sys.types st ON c.system_type_id = st.user_type_id 
	                                            INNER JOIN sys.objects tb ON tb.object_id = c.object_id 
	                                            INNER JOIN sys.schemas u ON u.schema_id = tb.schema_id 
	                                            LEFT OUTER JOIN sys.objects d ON d.object_id = c.default_object_id 
	                                            LEFT OUTER JOIN sys.schemas du ON du.schema_id = d.schema_id 
	                                            LEFT OUTER JOIN sys.default_constraints cm ON cm.object_id = d.object_id 
	                                            LEFT OUTER JOIN sys.computed_columns fr ON fr.object_id = c.object_id AND fr.column_id = c.column_id 
	                                            LEFT OUTER JOIN sys.objects r ON r.object_id = c.rule_object_id LEFT OUTER JOIN sys.schemas ru ON ru.schema_id = r.schema_id 
	                                            LEFT OUTER JOIN sys.xml_schema_collections xc ON xc.xml_collection_id = c.xml_collection_id 
	                                            LEFT OUTER JOIN sys.schemas xu ON xu.schema_id = xc.schema_id 
	                                            LEFT OUTER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id AND p.class = 1 AND p.name = 'MS_Description'  
	                                            WHERE 
	                                            tb.name = (select name from sysobjects where id = a.parent_obj) AND d.name = a.name)  as COLUMN_NAME,
	                                            (SELECT cm.definition
	                                            FROM 
	                                            sys.columns c 
	                                            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id 
	                                            INNER JOIN sys.schemas tu ON tu.schema_id = t.schema_id 
	                                            INNER JOIN sys.types st ON c.system_type_id = st.user_type_id 
	                                            INNER JOIN sys.objects tb ON tb.object_id = c.object_id 
	                                            INNER JOIN sys.schemas u ON u.schema_id = tb.schema_id 
	                                            LEFT OUTER JOIN sys.objects d ON d.object_id = c.default_object_id 
	                                            LEFT OUTER JOIN sys.schemas du ON du.schema_id = d.schema_id 
	                                            LEFT OUTER JOIN sys.default_constraints cm ON cm.object_id = d.object_id 
	                                            LEFT OUTER JOIN sys.computed_columns fr ON fr.object_id = c.object_id AND fr.column_id = c.column_id 
	                                            LEFT OUTER JOIN sys.objects r ON r.object_id = c.rule_object_id LEFT OUTER JOIN sys.schemas ru ON ru.schema_id = r.schema_id 
	                                            LEFT OUTER JOIN sys.xml_schema_collections xc ON xc.xml_collection_id = c.xml_collection_id 
	                                            LEFT OUTER JOIN sys.schemas xu ON xu.schema_id = xc.schema_id 
	                                            LEFT OUTER JOIN sys.extended_properties p ON p.major_id = c.object_id AND p.minor_id = c.column_id AND p.class = 1 AND p.name = 'MS_Description'  
	                                            WHERE 
	                                            tb.name = (select name from sysobjects where id = a.parent_obj) AND d.name = a.name) AS DEFAULT_VALUE,
	                                            'D' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'DEFAULT' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            sys.sysobjects as a 
                                            WHERE
	                                            xtype = 'D'
                                            UNION
                                            --存储过程
                                            SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS TEXT,
	                                            'P' AS NAME_KIND,
	                                            SPECIFIC_NAME AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            ROUTINE_DEFINITION AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            CREATED AS CREATE_DATA,
	                                            LAST_ALTERED AS ALTER_DATA	
                                            FROM
	                                            INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
	                                            ROUTINE_TYPE = 'PROCEDURE'
                                            UNION
                                            --触发器
                                            SELECT
	                                            (SELECT name FROM sysobjects WHERE ID = A.parent_obj) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'TR' AS NAME_KIND,
	                                            A.name AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            'N/A' AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS AS A
                                            WHERE
	                                            A.xtype = 'TR'
                                            UNION
                                            --存储过程/函数参数
                                            SELECT
	                                            DISTINCT SPECIFIC_NAME AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'PP' AS NAME_KIND,
	                                            PARAMETER_NAME AS NAME_ID,
	                                            'N/A' AS CONSTARINT_TYPE,
	                                            ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            DATA_TYPE,
	                                            CHARACTER_MAXIMUM_LENGTH,
	                                            CHARACTER_OCTET_LENGTH,
	                                            NUMERIC_PRECISION,
	                                            NUMERIC_PRECISION_RADIX,
	                                            NUMERIC_SCALE,
	                                            DATETIME_PRECISION,
	                                            COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA
                                            FROM
	                                            INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
	                                            PARAMETER_MODE = 'IN'
                                            UNION
                                            --函数
                                            SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'FC' AS NAME_KIND,
	                                            SPECIFIC_NAME AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            ROUTINE_DEFINITION AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            CREATED AS CREATE_DATA,
	                                            LAST_ALTERED AS ALTER_DATA	
                                            FROM
	                                            INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
	                                            ROUTINE_TYPE = 'FUNCTION'
                                            UNION
                                            --触发器内容
                                            SELECT
	                                            (SELECT name FROM sys.objects WHERE  object_id = a.parent_object_id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'TR_TEXT' AS NAME_KIND,
	                                            a.name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            (SELECT text FROM sys.syscomments WHERE id = a.object_id) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            create_date AS CREATE_DATA,
	                                            modify_date AS ALTER_DATA	
                                            FROM
	                                            sys.objects AS a 
                                            WHERE 
	                                            type_desc = 'SQL_TRIGGER'
                                            ";
        #endregion

        #region 2000
        public const string Str_2000_Structure = @"SELECT
	                                            'N/A' AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'T' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast('N/A' as nvarchar(4000)) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS
                                            WHERE
	                                            xtype = 'U'
                                            UNION all
                                            --视图
                                            SELECT
	                                            VIEW_DEFINITION AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            'N/A' AS SEED_VALUE,
	                                            'N/A' AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'V' AS NAME_KIND,
	                                            name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            0 AS ORDINAL_POSITION,
	                                            'N/A' AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            'N/A' AS DATA_TYPE,
	                                            0 AS CHARACTER_MAXIMUM_LENGTH,
	                                            0 AS CHARACTER_OCTET_LENGTH,
	                                            0 AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            0 AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast('N/A' as nvarchar(4000)) AS COLLATION_NAME,
	                                            'N/A' AS EXTEND_VALUE,
	                                            crdate AS CREATE_DATA,
	                                            refdate AS ALTER_DATA
                                            FROM
	                                            SYSOBJECTS,INFORMATION_SCHEMA.VIEWS
                                            WHERE
	                                            xtype = 'V' AND name = TABLE_NAME
                                            UNION all
                                            --字段及详细类型
                                            SELECT 
	                                            (SELECT name FROM sysobjects WHERE ID = c.id) AS NAMES_Layer,
	                                            0 AS IS_IDENTITY,
	                                            CONVERT(VARCHAR,ident_seed(tb.name)) AS SEED_VALUE,
	                                            CONVERT(VARCHAR,ident_incr(tb.name)) AS INCREMENT_VALUE,
	                                            'N/A' AS COLUMN_NAME,
	                                            'N/A' AS DEFAULT_VALUE,
	                                            'F' AS NAME_KIND,
	                                            c.name AS NAME_ID,
	                                            'N/A' AS CONSTRAINT_TYPE,
	                                            c.colid AS ORDINAL_POSITION,
	                                            CAST(p.[value] AS varchar(8000)) AS COLUMN_DEFAULT,
	                                            'N/A' AS IS_NULLABLE,
	                                            t.name AS DATA_TYPE,
	                                            c.length AS CHARACTER_MAXIMUM_LENGTH,
	                                            c.length AS CHARACTER_OCTET_LENGTH,
	                                            c.prec AS NUMERIC_PRECISION,
	                                            0 AS NUMERIC_PRECISION_RADIX,
	                                            c.scale AS NUMERIC_SCALE,
	                                            0 AS DATETIME_PRECISION,
	                                            cast(c.collation as nvarchar(4000)) AS COLLATION_NAME,
	                                            '' AS EXTEND_VALUE,
	                                            getdate() AS CREATE_DATA,
	                                            getdate() AS ALTER_DATA	
                                            FROM 
	                                            syscolumns c 
	                                            INNER JOIN systypes t ON c.xusertype = t.xusertype 
	                                            --INNER JOIN systypes st ON c.xtype = st.xusertype 
	                                            --LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            --LEFT OUTER JOIN sysusers du ON du.uid = d.uid 
	                                            --LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            --LEFT OUTER JOIN syscomments fr ON fr.id = c.id AND fr.number = c.colid 
	                                            --LEFT OUTER JOIN sysobjects r ON r.id = c.domain 
	                                            --LEFT OUTER JOIN sysusers ru ON ru.uid = r.uid 
	                                            INNER JOIN sysobjects tb ON tb.id = c.id 
	                                            INNER JOIN sysusers u ON u.uid = tb.uid 
	                                            LEFT OUTER JOIN sysproperties p ON p.id = c.id AND p.smallid = c.colid AND p.type = 4 AND p.name = 'MS_Description' 
                                            WHERE 
	                                            u.name = N'dbo' and tb.xtype = 'U'
                                            UNION all
                                            --主键
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,

                                                (SELECT top 1 c.name
                                                FROM 
                                                sysindexkeys k 
                                                INNER JOIN sysindexes i ON k.id = i.id and k.indid = i.indid 
                                                INNER JOIN syscolumns c ON k.id = c.id AND k.colid = c.colid 
                                                where i.name = a.name) AS COLUMN_NAME,

                                                'N/A' AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'PRIMARY KEY' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'PK'
                                            UNION ALL
                                            --唯一键
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,

                                                (SELECT top 1 c.name
                                                FROM 
                                                sysindexkeys k 
                                                INNER JOIN sysindexes i ON k.id = i.id and k.indid = i.indid 
                                                INNER JOIN syscolumns c ON k.id = c.id AND k.colid = c.colid 
                                                where i.name = a.name) AS COLUMN_NAME,

                                                'N/A' AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'UNIQUE' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'UQ'
                                            UNION ALL
                                            --check约束
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                (SELECT TEXT FROM syscomments WHERE ID = a.id) AS DEFAULT_VALUE,
                                                'K' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'CHECK' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'C'
                                            UNION ALL
                                            SELECT
                                                (select name from sysobjects where id = a.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                ( SELECT top 1 c.name
	                                            FROM 
	                                            syscolumns c 
	                                            LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            WHERE d.parent_obj = a.parent_obj and d.name =  a.name)
                                                AS COLUMN_NAME,
                                                ( SELECT top 1 cm.text
	                                            FROM 
	                                            syscolumns c 
	                                            LEFT OUTER JOIN sysobjects d ON d.id = c.cdefault 
	                                            LEFT OUTER JOIN syscomments cm ON c.cdefault = cm.id 
	                                            WHERE d.parent_obj = a.parent_obj and d.name =  a.name)
                                                AS DEFAULT_VALUE,
                                                'D' AS NAME_KIND,
                                                a.name AS NAME_ID,
                                                'DEFAULT' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                crdate AS CREATE_DATA,
                                                GETDATE() AS ALTER_DATA	
                                            FROM
                                                sysobjects as a
                                            WHERE 
                                                xtype = 'D'
                                            UNION ALL
                                            --存储过程
                                            SELECT
                                                'N/A' AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS TEXT,
                                                'P' AS NAME_KIND,
                                                SPECIFIC_NAME AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION, 
                                                cast(ROUTINE_DEFINITION as nvarchar(4000)) AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                CREATED AS CREATE_DATA,
                                                LAST_ALTERED AS ALTER_DATA	
                                            FROM
                                                INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
                                                ROUTINE_TYPE = 'PROCEDURE'
                                            UNION ALL
                                            --触发器
                                            SELECT
                                                (SELECT name FROM sysobjects WHERE ID = A.parent_obj) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'TR' AS NAME_KIND,
                                                A.name AS NAME_ID,
                                                'N/A' AS CONSTARINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                'N/A' AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA
                                            FROM
                                                SYSOBJECTS AS A
                                            WHERE
                                                A.xtype = 'TR'
                                            UNION ALL
                                            --存储过程/函数参数
                                            SELECT
                                                DISTINCT SPECIFIC_NAME AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'PP' AS NAME_KIND,
                                                PARAMETER_NAME AS NAME_ID,
                                                'N/A' AS CONSTARINT_TYPE,
                                                ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                DATA_TYPE,
                                                CHARACTER_MAXIMUM_LENGTH,
                                                CHARACTER_OCTET_LENGTH,
                                                NUMERIC_PRECISION,
                                                NUMERIC_PRECISION_RADIX,
                                                NUMERIC_SCALE,
                                                DATETIME_PRECISION,
                                                COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA
                                            FROM
                                                INFORMATION_SCHEMA.PARAMETERS
                                            WHERE 
                                                PARAMETER_MODE = 'IN'
                                            UNION ALL
                                            --函数
                                            SELECT
                                                'N/A' AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'FC' AS NAME_KIND,
                                                SPECIFIC_NAME AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                --ROUTINE_DEFINITION AS COLLATION_NAME,
                                                cast(ROUTINE_DEFINITION as nvarchar(4000)) AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                CREATED AS CREATE_DATA,
                                                LAST_ALTERED AS ALTER_DATA	
                                            FROM
                                                INFORMATION_SCHEMA.ROUTINES
                                            WHERE 
                                                ROUTINE_TYPE = 'FUNCTION'
                                            union all
                                            --触发器内容----------
                                            SELECT
                                                (SELECT name FROM sysobjects WHERE  id = o.id) AS NAMES_Layer,
                                                0 AS IS_IDENTITY,
                                                'N/A' AS SEED_VALUE,
                                                'N/A' AS INCREMENT_VALUE,
                                                'N/A' AS COLUMN_NAME,
                                                'N/A' AS DEFAULT_VALUE,
                                                'TR_TEXT' AS NAME_KIND,
                                                o.name AS NAME_ID,
                                                'N/A' AS CONSTRAINT_TYPE,
                                                0 AS ORDINAL_POSITION,
                                                'N/A' AS COLUMN_DEFAULT,
                                                'N/A' AS IS_NULLABLE,
                                                'N/A' AS DATA_TYPE,
                                                0 AS CHARACTER_MAXIMUM_LENGTH,
                                                0 AS CHARACTER_OCTET_LENGTH,
                                                0 AS NUMERIC_PRECISION,
                                                0 AS NUMERIC_PRECISION_RADIX,
                                                0 AS NUMERIC_SCALE,
                                                0 AS DATETIME_PRECISION,
                                                CASE WHEN c.encrypted = '1' THEN '' ELSE c.text END AS COLLATION_NAME,
                                                'N/A' AS EXTEND_VALUE,
                                                getdate() AS CREATE_DATA,
                                                getdate() AS ALTER_DATA	
                                            FROM
                                                sysobjects AS o 
                                                LEFT OUTER JOIN syscomments c ON o.id = c.id AND c.colid = 1 
                                            WHERE 
                                                o.xtype = 'TR'
";
        #endregion

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public const string Str_GetDatatype = @"SELECT 
                                                    name AS DBTYPE_NAME
                                                FROM 
                                                    sys.types 
                                                ORDER BY 
                                                    name
                                                ";

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public const string Str_2000_GetDatatype = @"SELECT 
                                                    name AS DBTYPE_NAME
                                                FROM 
                                                    systypes 
                                                ORDER BY 
                                                    name
                                                ";    
    }

    public static class MYSQL_SqlConstant
    {
        /// <summary>
        /// MYSQL数据库列表
        /// </summary>
        public const string Str_GetDatabase = @"SELECT SCHEMA_NAME AS DB_Name FROM INFORMATION_SCHEMA.SCHEMATA";

        /// <summary>
        /// MYSQL数据库表
        /// </summary>
        public const string Str_GetTable = @"SELECT Table_Name,
                                                    'T' AS NAME_KIND,
                                                    CREATE_TIME,
                                                    UPDATE_TIME 
                                            FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '$TABLE_SCHEMA' AND TABLE_TYPE = 'BASE TABLE'";

        /// <summary>
        /// MYSQL数据库视图
        /// </summary>
        public const string Str_GetView = @"SELECT Table_Name,
                                                    'V' AS NAME_KIND,
                                                    CREATE_TIME,
                                                    UPDATE_TIME 
                                            FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '$TABLE_SCHEMA' AND TABLE_TYPE = 'VIEW'";

        /// <summary>
        /// 获取表字段
        /// </summary>
        public const string Str_GetColumn = @"SELECT TABLE_NAME,
                                                     COLUMN_NAME,
                                                     ORDINAL_POSITION,
                                                     COLUMN_DEFAULT,
                                                     IS_NULLABLE,
                                                     DATA_TYPE,
                                                     CHARACTER_MAXIMUM_LENGTH,
                                                     CHARACTER_OCTET_LENGTH,
                                                     NUMERIC_PRECISION,
                                                     NUMERIC_SCALE,
                                                     COLLATION_NAME
                                              FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '$TABLE_SCHEMA' AND TABLE_NAME = '$table_name'";

        /// <summary>
        /// 获取主键
        /// </summary>
        public const string Str_GetKey = @"SELECT                                                     
                                               TABLE_NAME,
                                               COLUMN_NAME,
                                               INDEX_NAME 
                                           FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '$TABLE_SCHEMA' AND INDEX_NAME = 'PRIMARY' AND TABLE_NAME = '$table_name'";

        /// <summary>
        /// 获取唯一性约束
        /// </summary>
        public const string Str_GetUQKey = @"SELECT 
                                                  a.TABLE_NAME,
                                                  b.COLUMN_NAME,
                                                  a.CONSTRAINT_NAME                                                  
                                             FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS as a,INFORMATION_SCHEMA.KEY_COLUMN_USAGE as b
                                             WHERE
                                                  a.CONSTRAINT_TYPE = 'UNIQUE' AND a.TABLE_SCHEMA = '$TABLE_SCHEMA' AND a.CONSTRAINT_NAME = b.CONSTRAINT_NAME AND a.TABLE_NAME = '$table_name'";

        /// <summary>
        /// 外键
        /// </summary>
        public const string Str_GetForeignKey = @"";

        /// <summary>
        /// Default约束
        /// </summary>
        public const string Str_GetDefault = @"SELECT 
                                                   (SELECT COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = a.COLUMN_NAME and TABLE_NAME = a.TABLE_NAME) AS DEFAULT_VALUE
                                                   TABLE_NAME,
                                                   COLUMN_NAME,
                                                   CONSTRAINT_NAME
                                               FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS a,INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS b
                                               WHERE
                                                   a.CONSTRAINT_NAME = b.CONSTRAINT_NAME AND b.CONSTRAINT_TYPE = '' AND TABLE_NAME = '$table_name'";

        /// <summary>
        /// Check约束
        /// </summary>
        public const string Str_GetCheck = @"SELECT 
                                                   TABLE_NAME,
                                                   CONSTRAINT_NAME
                                               FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                                               WHERE
                                                   CONSTRAINT_TYPE = 'CHECK' AND TABLE_SCHEMA = '$TABLE_SCHEMA' AND TABLE_NAME = '$table_name'";

        /// <summary>
        /// 存储过程
        /// </summary>
        public const string Str_GetProcedure = @"SELECT SPECIFIC_NAME,ROUTINE_DEFINITION,CREATED,LAST_ALTERED 
                                                 FROM INFORMATION_SCHEMA.ROUTINES 
                                                 WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_SCHEMA = '$DbName'";

        /// <summary>
        /// 函数
        /// </summary>
        public const string Str_GetFunction = @"SELECT SPECIFIC_NAME,ROUTINE_DEFINITION,CREATED,LAST_ALTERED 
                                                 FROM INFORMATION_SCHEMA.ROUTINES 
                                                 WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_SCHEMA = '$DbName'";

        /// <summary>
        /// 触发器
        /// </summary>
        public const string Str_GetTrigger = @"SELECT EVENT_OBJECT_TABLE,TRIGGER_NAME,ACTION_STATEMENT
                                               FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_SCHEMA = '$DbName'";

        /// <summary>
        /// 获取数据类型
        /// </summary>
        public const string Str_GetDatatype = @"";
    }
}