using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace System.DataPacket
{
    public class SwitchData
    {
        public static DataSet ToDataSet(object tRecord)
        {
            DataSet pSet = new DataSet();
            CustomData[,] pCustomData = (CustomData[,])tRecord;
            DataTable pTable = new DataTable();
            
            for (int i = 0; i < pCustomData.GetLength(1); i++)
            {
                pTable.Columns.Add(pCustomData[0, i].Field.ToString());
            }

            for (int i = 0; i < pCustomData.GetLength(0); i++)
            {
                DataRow newRow = pTable.NewRow();                

                for (int j = 0; j < pCustomData.GetLength(1); j++)
                {
                    newRow[j] = pCustomData[i, j].Content.ToString();
                }
                pTable.Rows.Add(newRow);
            }

            pSet.Tables.Add(pTable);
            return pSet;
        }
    }
}
