using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    #region 加密/压缩定义
    /// <summary>
    /// 编码方式定义
    /// </summary>
    public enum EncryptType : sbyte
    {
        NONE        = 0,
        TRIPLEDES   = 1,
        RIJNDAEL    = 2
    }

    /// <summary>
    /// 压缩方式定义
    /// </summary>
    public enum CompressionType : sbyte
    {
        NONE        = 0,
        GZIP        = 1,
        DEFLATE     = 2
    }
    #endregion

    #region 数据定义
    /// <summary>
    /// 数据库定义
    /// </summary>
    public enum DataBase : sbyte
    {
        ODBC        = 0,
        OLEDB       = 1,
        MSSQL       = 2,
        MYSQL       = 3,
        POSTGRESQL  = 4,
        ORACLE      = 4,
        SYBASE      = 5,
        DB2         = 6,
        INFORMIX    = 7,
        CHANNEL     = 8,
        FILE        = 127
    }

    /// <summary>
    /// 数据格式
    /// </summary>
    public enum DataFormat : sbyte
    {
        NONE        = 0,
        BOOLEAN     = 1,
        BYTE        = 2,
        UNSIGNED    = 3,
        SIGNED      = 4,
        PRECISION   = 5,
        DATETIME    = 6,
        STRING      = 7,
        DECIMAL     = 8,
        GUID        = 9,
        VARIANT     = 10
    }

    /// <summary>
    /// 记录集样式
    /// </summary>
    public enum RecordStyle : sbyte
    {
        NONE        = 0,
        STRUCT      = 1,
        DATASET     = 2,
        DATATABLE   = 3,
        XML         = 4,
        DATAFILE    = 5,
        REALTIME    = 6
    }

    public enum AlterTableStyle : sbyte
    {
        ADDCOLUMN = 0,//添加新列
        DROPOBJECT = 1,//删除列、约束等对象
        ALTERCOLUMN = 2,//修改列
        ADDPRIMARYKEY = 3,//添加主键
        ADDFOREIGNKEY = 4,//添加外键
        ADDDEFAULT = 5,//添加默认值
        ADDCHECK = 6,//添加Check约束
        ADDUNIQUE = 7,//添加UNIQUE约束
        ADDEXTEND = 8,//添加扩展属性
        DELEXTEND = 9,//删除扩展属性
        MODEXTEND = 10,//修改扩展属性
    }
    #endregion

    #region 系统定义
    /// <summary>
    /// 插件类别
    /// </summary>
    public enum PlugIn : sbyte
    {
        UI          = 0,
        WEB         = 1,
        FUNCTION    = 2
    }
    #endregion

    #region 文件类型定义
    /// <summary>
    /// 自定义文件类型
    /// </summary>
    public enum FileType : sbyte
    {
        DAT         = 0,
        IDX         = 1,
        LOG         = 2
    }
    #endregion
}
