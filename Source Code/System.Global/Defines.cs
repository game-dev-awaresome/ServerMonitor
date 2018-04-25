using System;
using System.Collections.Generic;
using System.Text;

namespace System.Global
{
    #region ����/ѹ������
    /// <summary>
    /// ���뷽ʽ����
    /// </summary>
    public enum EncryptType : sbyte
    {
        NONE        = 0,
        TRIPLEDES   = 1,
        RIJNDAEL    = 2
    }

    /// <summary>
    /// ѹ����ʽ����
    /// </summary>
    public enum CompressionType : sbyte
    {
        NONE        = 0,
        GZIP        = 1,
        DEFLATE     = 2
    }
    #endregion

    #region ���ݶ���
    /// <summary>
    /// ���ݿⶨ��
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
    /// ���ݸ�ʽ
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
    /// ��¼����ʽ
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
        ADDCOLUMN = 0,//�������
        DROPOBJECT = 1,//ɾ���С�Լ���ȶ���
        ALTERCOLUMN = 2,//�޸���
        ADDPRIMARYKEY = 3,//�������
        ADDFOREIGNKEY = 4,//������
        ADDDEFAULT = 5,//���Ĭ��ֵ
        ADDCHECK = 6,//���CheckԼ��
        ADDUNIQUE = 7,//���UNIQUEԼ��
        ADDEXTEND = 8,//�����չ����
        DELEXTEND = 9,//ɾ����չ����
        MODEXTEND = 10,//�޸���չ����
    }
    #endregion

    #region ϵͳ����
    /// <summary>
    /// ������
    /// </summary>
    public enum PlugIn : sbyte
    {
        UI          = 0,
        WEB         = 1,
        FUNCTION    = 2
    }
    #endregion

    #region �ļ����Ͷ���
    /// <summary>
    /// �Զ����ļ�����
    /// </summary>
    public enum FileType : sbyte
    {
        DAT         = 0,
        IDX         = 1,
        LOG         = 2
    }
    #endregion
}
