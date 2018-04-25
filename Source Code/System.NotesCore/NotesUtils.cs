using System;
using System.Collections.Generic;
using System.Text;
using Domino;

using System.Global;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.DataPacket;
using System.Define;

namespace System.NotesCore
{
    /// <summary>
    /// Lotus Notes COM ���� : NotesDataBase�е�GetDocumentByKey�Ĳ���ΪNotesID
    /// </summary>
    public class NotesUtils : CustomClass
    {
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="pNotesSessionClass">Notes�Ự</param>
        /// <param name="strDomain">����</param>
        /// <param name="strDataBase">����</param>
        public NotesUtils(NotesSessionClass pNotesSessionClass, string strDomain, string strDataBase)
        {
            this._strDomain = strDomain;
            this._strDataBase = strDataBase;
            this._strUserName = String.Empty;

            this.strMessage = String.Empty;
            this.pInfoList = new ArrayList();

            this._pNotesSession = pNotesSessionClass;
        }

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        /// <param name="bDispodedByUser"></param>
        protected override void Free(bool bDispodedByUser)
        {
            if (bDispodedByUser)
            {
                this.pInfoList.Clear();

                if (this.pNotesDatabase != null)
                {
                    Marshal.ReleaseComObject(this.pNotesDatabase);
                }

                this.pNotesDatabase = null;
            }

            base.Free(bDispodedByUser);
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="strUserName">�û���</param>
        /// <param name="strPassword">����</param>
        /// <returns></returns>
        public bool OpenDataBase(string strUserName, string strPassword)
        {
            bool bResult = false;

            try
            {
                this._strUserName = strUserName;

                this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, this._strDataBase, false);

                if (this.pNotesDatabase == null)
                {
                    throw new Exception("���ܴ����ݿ⣺" + this._strDataBase);
                }

                bResult = true;
            }
            catch (Exception ex)
            {
                bResult = false;

                this.strMessage = ex.Message;
            }

            return bResult;
        }

        /// <summary>
        /// ��ȡ��ϵ��
        /// </summary>
        /// <param name="strCategory">��ϵ�˷���</param>
        /// <returns></returns>
        public bool GetLinkerInfo(string strCategory)
        {
            bool bResult = false;
            NotesView pLinkerView = null;
            NotesDocument pLinkerDocument = null;

            try
            {
                if (this._strDataBase != "names.nsf")
                {
                    this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, "names.nsf", false);
                }

                pLinkerView = this.pNotesDatabase.GetView(strCategory);

                pLinkerDocument = pLinkerView.GetFirstDocument();

                CustomDataCollection pLinkerStruct = new CustomDataCollection(StructType.CUSTOMDATA);

                while (pLinkerDocument != null)
                {
                    pLinkerStruct.Add(DataField.LINKER_NAME, DataFormat.STRING, pLinkerDocument.GetFirstItem("ListName").Text);
                    pLinkerStruct.Add(DataField.LINKER_CONTENT, DataFormat.STRING, (pLinkerDocument.GetFirstItem("Members") == null) ? "N/A" : pLinkerDocument.GetFirstItem("Members").Text);
                    pLinkerStruct.AddRows();                              

                    //pLinkerStruct[0].oFieldsName = "Linker_Name";
                    //pLinkerStruct[0].oFiledsTypes = "String";
                    //pLinkerStruct[0].oFieldValues = pLinkerDocument.GetFirstItem("ListName").Text;

                    //pLinkerStruct[1].oFieldsName = "Linker_Content";
                    //pLinkerStruct[1].oFiledsTypes = "String";
                    //pLinkerStruct[1].oFieldValues = (pLinkerDocument.GetFirstItem("Members") == null) ? "N/A" : pLinkerDocument.GetFirstItem("Members").Text;

                    //this.pInfoList.Add(pLinkerStruct);

                    pLinkerDocument = pLinkerView.GetNextDocument(pLinkerDocument);
                }
                this.pRecords = pLinkerStruct;
                bResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;

                bResult = false;
                //this.pInfoList.Clear();
            }
            finally
            {                
                if (pLinkerDocument != null)
                {
                    Marshal.ReleaseComObject(pLinkerDocument);
                }

                if (pLinkerView != null)
                {
                    Marshal.ReleaseComObject(pLinkerView);
                }

                pLinkerDocument = null;
                pLinkerView = null;
            }

            return bResult;
        }

        /// <summary>
        /// ��ȡ���ż�
        /// </summary>
        /// <returns></returns>
        public bool GetMailInfo()
        {
            bool bResult = false;
            NotesView pMailView = null;
            NotesDocument pMailDocument = null;
            int iCount = 0;

            try
            {
                if (this._strDataBase == "names.nsf")
                {
                    this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, this._strDataBase, false);
                }

                if (this.pNotesDatabase == null)
                {
                    throw new Exception("���ܴ����ݿ⣺" + this._strDataBase);
                }

                pMailView = this.pNotesDatabase.GetView("($inbox)");

                pMailDocument = pMailView.GetFirstDocument();
                Console.WriteLine("���ƣ�" + pMailView.EntryCount.ToString());

                CustomDataCollection pMailStruct = new CustomDataCollection(StructType.CUSTOMDATA);
                
                while (pMailDocument != null)
                {
                    if (((object[])pMailDocument.GetItemValue("Reader"))[0].ToString() != "YES")
                    {
                        string strPUID = string.Empty;
                        string strSubject = (((object[])pMailDocument.ColumnValues)[5] == null) ? "N/A" : ((object[])pMailDocument.ColumnValues)[5].ToString();
                        string strSupervisors = (((object[])pMailDocument.GetItemValue("CopyTo")).Length == 0) ? "N/A" : ((object[])pMailDocument.GetItemValue("CopyTo"))[0].ToString();
                        string strSendTo = (((object[])pMailDocument.GetItemValue("SendTo")).Length == 0) ? "N/A" : ((object[])pMailDocument.GetItemValue("SendTo"))[0].ToString();

                        if (strSendTo != "N/A")
                        {
                            for (int i = 0; i < ((object[])pMailDocument.GetItemValue("SendTo")).Length; i++)
                            {
                                strSupervisors = strSupervisors + ";" + ((object[])pMailDocument.GetItemValue("SendTo"))[i].ToString();
                            }
                        }

                        if (strSupervisors != "N/A")
                        {
                            for (int i = 1; i < ((object[])pMailDocument.GetItemValue("CopyTo")).Length; i++)
                            {
                                strSupervisors = strSupervisors + ";" + ((object[])pMailDocument.GetItemValue("CopyTo"))[i].ToString();
                            }
                        }

                        string strBody = ((object[])pMailDocument.GetItemValue("Body"))[0].ToString();

                        if (((object[])pMailDocument.GetItemValue("SSM_Agent"))[0].ToString().Length != 0)
                        {
                            strPUID = ((object[])pMailDocument.GetItemValue("SSM_Agent"))[0].ToString();
                        }
                        else if (((object[])pMailDocument.GetItemValue("SSM_Agent"))[0].ToString().Length == 0 && pMailDocument.ParentDocumentUNID == null)
                        {
                            strPUID = "N/A";
                        }
                        else
                        {
                            strPUID = pMailDocument.ParentDocumentUNID;
                        }
                        /*
                        if (pMailDocument.HasItem("SMS Agent"))
                        {
                            strPUID = ((object[])pMailDocument.GetItemValue("SMS Agent"))[0].ToString();
                        }
                        else if (!pMailDocument.HasItem("SMS Agent") && pMailDocument.ParentDocumentUNID == null)
                        {
                            strPUID = "N/A";
                        }
                        else
                        {
                            strPUID = pMailDocument.ParentDocumentUNID;
                        }
                        */

                        //�޳�--�����������ͷ�����һ������
                        if (strSubject.IndexOf("����������", StringComparison.CurrentCulture) > 0 || strSubject.IndexOf("������һ������", StringComparison.CurrentCulture) > 0)
                        {
                            if (pMailDocument.HasItem("Reader"))
                            {
                                pMailDocument.ReplaceItemValue("Reader", "YES");
                            }
                            else
                            {
                                pMailDocument.AppendItemValue("Reader", "YES");
                            }

                            pMailDocument.Save(true, true, true);
                        }
                        else
                        {
                            pMailStruct.Add(DataField.NOTES_UID, DataFormat.STRING, pMailDocument.UniversalID);
                            pMailStruct.Add(DataField.NOTES_UID, DataFormat.STRING, strPUID);
                            pMailStruct.Add(DataField.NOTES_SUBJECT, DataFormat.STRING, strSubject);
                            pMailStruct.Add(DataField.NOTES_FROM, DataFormat.STRING, ((object[])pMailDocument.GetItemValue("Principal"))[0].ToString());
                            pMailStruct.Add(DataField.NOTES_DATE, DataFormat.STRING, ((object[])pMailDocument.ColumnValues)[2].ToString());
                            pMailStruct.Add(DataField.NOTES_SUPERVISORS, DataFormat.STRING, (strSupervisors == "") ? "N/A" : strSupervisors);
                            pMailStruct.Add(DataField.NOTES_CONTENT, DataFormat.STRING, (strBody == "") ? "N/A" : strBody);
                            try
                            {
                                pMailStruct.Add(DataField.NOTES_ATTACHMENTCOUNT, DataFormat.STRING, (((NotesRichTextItem)pMailDocument.GetFirstItem("Body")).EmbeddedObjects == null) ? "0" : ((object[])((NotesRichTextItem)pMailDocument.GetFirstItem("Body")).EmbeddedObjects).Length.ToString());
                            }
                            catch
                            {
                                pMailStruct.Add(DataField.NOTES_ATTACHMENTCOUNT, DataFormat.STRING, "�ż��ڰ�������ż����壬������Լ������䣡");
                            }
                            pMailStruct.AddRows();

                            //GlobalStruct[] pMailStruct = new GlobalStruct[8];

                            //pMailStruct[0].oFieldsName = "Notes_UID";
                            //pMailStruct[0].oFiledsTypes = "String";
                            //pMailStruct[0].oFieldValues = pMailDocument.UniversalID;

                            //pMailStruct[1].oFieldsName = "Notes_PUID";
                            //pMailStruct[1].oFiledsTypes = "String";
                            //pMailStruct[1].oFieldValues = strPUID; // (pMailDocument.ParentDocumentUNID == null) ? "N/A" : pMailDocument.ParentDocumentUNID;

                            //pMailStruct[2].oFieldsName = "Notes_Subject";
                            //pMailStruct[2].oFiledsTypes = "String";
                            //pMailStruct[2].oFieldValues = strSubject;

                            //pMailStruct[3].oFieldsName = "Notes_From";
                            //pMailStruct[3].oFiledsTypes = "String";
                            //pMailStruct[3].oFieldValues = ((object[])pMailDocument.GetItemValue("Principal"))[0].ToString(); //((object[])pMailDocument.ColumnValues)[1].ToString();

                            //pMailStruct[4].oFieldsName = "Notes_Date";
                            //pMailStruct[4].oFiledsTypes = "String";
                            //pMailStruct[4].oFieldValues = ((object[])pMailDocument.ColumnValues)[2].ToString();

                            //pMailStruct[5].oFieldsName = "Notes_Supervisors";
                            //pMailStruct[5].oFiledsTypes = "String";
                            //pMailStruct[5].oFieldValues = (strSupervisors == "") ? "N/A" : strSupervisors;

                            //pMailStruct[6].oFieldsName = "Notes_Content";
                            //pMailStruct[6].oFiledsTypes = "String";
                            //pMailStruct[6].oFieldValues = (strBody == "") ? "N/A" : strBody;

                            //pMailStruct[7].oFieldsName = "Notes_AttachmentCount";
                            //pMailStruct[7].oFiledsTypes = "String";
                            //try
                            //{
                            //    pMailStruct[7].oFieldValues = (((NotesRichTextItem)pMailDocument.GetFirstItem("Body")).EmbeddedObjects == null) ? "0" : ((object[])((NotesRichTextItem)pMailDocument.GetFirstItem("Body")).EmbeddedObjects).Length.ToString();
                            //}
                            //catch
                            //{
                            //    pMailStruct[7].oFieldValues = "�ż��ڰ�������ż����壬������Լ������䣡";
                            //}

                            this.pInfoList.Add(pMailStruct);

                            if (pMailDocument.HasItem("Reader"))
                            {
                                pMailDocument.ReplaceItemValue("Reader", "YES");
                            }
                            else
                            {
                                pMailDocument.AppendItemValue("Reader", "YES");
                            }

                            //pMailStruct = null;

                            pMailDocument.Save(true, true, true);
                            iCount++;
                        }
                    }

                    //pMailDocument.PutInFolder("��ʷ��¼", false);

                    pMailDocument = pMailView.GetNextDocument(pMailDocument);

                    //if (pMailDocument != null)
                    //{
                    //    pMailView.GetPrevDocument(pMailDocument).Remove(false);
                    //}
                }

                this.pRecords = pMailStruct;
                bResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;
                Console.WriteLine("��ʧNotes����" + iCount.ToString());
                bResult = false;
                //this.pInfoList.Clear();
            }
            finally
            {
                if (pMailDocument != null)
                {
                    Marshal.ReleaseComObject(pMailDocument);
                }

                if (pMailView != null)
                {
                    Marshal.ReleaseComObject(pMailView);
                }

                pMailDocument = null;
                pMailView = null;
            }

            return bResult;
        }

        /// <summary>
        /// ������ָ��ʱ����ʼ��ƶ���ָ��Ŀ¼��
        /// </summary>
        /// <returns></returns>
        public bool MoveMailInfo()
        {
            bool bResult = false;
            NotesView pMailView = null;
            NotesDocument pMailDocument = null;

            try
            {
                if (this._strDataBase == "names.nsf")
                {
                    this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, this._strDataBase, false);
                }

                if (null == this.pNotesDatabase)
                {
                    throw new Exception("���ܴ����ݿ⣺" + this._strDataBase);
                }

                pMailView = this.pNotesDatabase.GetView("($inbox)");

                pMailDocument = pMailView.GetFirstDocument();

                DateTime NowTime = DateTime.Now.AddDays(-2);

                int MailCount = 0;

                for (int i = 0; i < pMailView.EntryCount; i++)
                {
                    if (null != pMailDocument)
                    {
                        DateTime MailTime = Convert.ToDateTime(((object[])pMailDocument.GetItemValue("DeliveredDate"))[0].ToString());
                        string str_subject = ((object[])pMailDocument.ColumnValues)[5].ToString();

                        if (NowTime > MailTime || 0 < str_subject.IndexOf("����������", 0) || 0 < str_subject.IndexOf("������һ������", 0))
                        {
                            pMailDocument.PutInFolder("��ʱ�ʼ�", false);

                            NotesDocument oldMailDocument = pMailDocument;
                            if (null != oldMailDocument)
                            {
                                oldMailDocument.RemoveFromFolder("($inbox)");
                            }
                            pMailDocument = pMailView.GetFirstDocument();
                            i = -1;
                            MailCount++;
                        }
                        else
                        {
                            pMailDocument = pMailView.GetNextDocument(pMailDocument);
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("���ƶ��ˣ�" + MailCount);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");

                bResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;
                Console.WriteLine(ex.Message);

                bResult = false;
            }
            finally
            {
                if (pMailDocument != null)
                {
                    Marshal.ReleaseComObject(pMailDocument);
                }

                if (pMailView != null)
                {
                    Marshal.ReleaseComObject(pMailView);
                }

                pMailDocument = null;
                pMailView = null;
            }

            return bResult;
        }

        /// <summary>
        /// ��ȡ�ż��еĸ�������
        /// </summary>
        /// <param name="strNotesUID"></param>
        /// <returns></returns>
        public bool GetMailAttachment(string strNotesUID)
        {
            bool bResult = false;
            NotesView pMailView = null;
            NotesDocument pMailDocument = null;
            int iCount = 0;

            try
            {
                if (this._strDataBase == "names.nsf")
                {
                    this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, this._strDataBase, false);
                }

                if (this.pNotesDatabase == null)
                {
                    throw new Exception("���ܴ����ݿ⣺" + this._strDataBase);
                }

                pMailView = this.pNotesDatabase.GetView("($inbox)");

                pMailDocument = pMailView.GetFirstDocument();

                CustomDataCollection pAttachmentStruct = new CustomDataCollection(StructType.CUSTOMDATA);
                
                while (pMailDocument != null)
                {
                    if (pMailDocument.UniversalID == strNotesUID)
                    {
                        object[] arrItemObject = (object[])((NotesRichTextItem)pMailDocument.GetFirstItem("Body")).EmbeddedObjects;

                        for (int i = 0; i < arrItemObject.Length; i++)
                        {
                            NotesEmbeddedObject pEmbeddedObject = (NotesEmbeddedObject)arrItemObject[i];

                            pEmbeddedObject.ExtractFile(System.AppDomain.CurrentDomain.BaseDirectory + "\\Attachment\\" + pEmbeddedObject.Source);

                            FileStream pAttachmentStream = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "\\Attachment\\" + pEmbeddedObject.Source, FileMode.Open, FileAccess.Read);
                            byte[] pAttachmentContent = new byte[pAttachmentStream.Length];
                            pAttachmentStream.Read(pAttachmentContent, 0, pAttachmentContent.Length);
                            pAttachmentStream.Close();
                            pAttachmentStream.Dispose();

                            File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "\\Attachment\\" + pEmbeddedObject.Source);
                            
                            pAttachmentStruct.Add(DataField.NOTES_ATTACHMENTNAME, DataFormat.STRING, pEmbeddedObject.Source);
                            pAttachmentStruct.Add(DataField.NOTES_ATTACHMENTCOUNT, DataFormat.NONE, pAttachmentContent);
                            pAttachmentStruct.AddRows();

                            //GlobalStruct[] pAttachmentStruct = new GlobalStruct[2];

                            //pAttachmentStruct[0].oFieldsName = "Notes_AttachmentName";
                            //pAttachmentStruct[0].oFiledsTypes = "String";
                            //pAttachmentStruct[0].oFieldValues = pEmbeddedObject.Source;

                            //pAttachmentStruct[1].oFieldsName = "Notes_AttachmentCount";
                            //pAttachmentStruct[1].oFiledsTypes = "Byte[]";
                            //pAttachmentStruct[1].oFieldValues = pAttachmentContent;

                            pAttachmentContent = null;

                            this.pInfoList.Add(pAttachmentStruct);
                        }
                    }

                    pMailDocument = pMailView.GetNextDocument(pMailDocument);
                    iCount++;
                }

                this.pRecords = pAttachmentStruct;
                bResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;

                bResult = false;
                //this.pInfoList.Clear();
            }
            finally
            {
                if (pMailDocument != null)
                {
                    Marshal.ReleaseComObject(pMailDocument);
                }

                if (pMailView != null)
                {
                    Marshal.ReleaseComObject(pMailView);
                }

                pMailDocument = null;
                pMailView = null;
            }

            if (iCount == 0)
            {
                return false;
            }

            return bResult;
        }

        /// <summary>
        /// �������ż�
        /// </summary>
        /// <param name="pSupervisors">������</param>
        /// <param name="pSendSecret">������</param>
        /// <param name="pSubject">����</param>
        /// <param name="strMailContent">����</param>
        /// <returns></returns>
        public bool SendMailInfo(object pSupervisors, object pSendSecret, object pSubject, string strMailContent)
        {
            bool bResult = false;
            NotesDocument pMailDocument = null;
            NotesRichTextItem pContentItem = null;

            try
            {
                pMailDocument = this.pNotesDatabase.CreateDocument();

                pMailDocument.ReplaceItemValue("Form", "Memo");
                pMailDocument.ReplaceItemValue("CopyTo", pSupervisors);//����
                pMailDocument.ReplaceItemValue("BlindCopyTo", pSendSecret); //����
                pMailDocument.ReplaceItemValue("Subject", pSubject);
                pMailDocument.ReplaceItemValue("PostedDate", DateTime.Now.ToString());

                pContentItem = pMailDocument.CreateRichTextItem("Body");
                pContentItem.AppendText(strMailContent);

                object pSendOwner = this._strUserName;
                pMailDocument.Send(false, ref pSendOwner);

                bResult = true;
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;

                bResult = false;
            }
            finally
            {
                if (pContentItem != null)
                {
                    Marshal.ReleaseComObject(pContentItem);
                }

                if (pMailDocument != null)
                {
                    Marshal.ReleaseComObject(pMailDocument);
                }

                pContentItem = null;
                pMailDocument = null;
            }

            return bResult;
        }

        /// <summary>
        /// ת��/�ظ��ż�
        /// </summary>
        /// <param name="pSupervisors">������</param>
        /// <param name="pSendSecret">������</param>
        /// <param name="strNotesUID">ԭNotesID</param>
        /// <param name="strMailContent">����</param>
        /// <returns></returns>
        public bool RelayMailInfo(object pSupervisors, object pSendSecret, string strNotesUID, string strMailContent)
        {
            bool bResult = false;
            NotesView pParentView = null;
            NotesDocument pParentDocument = null;

            try
            {
                if (this._strDataBase == "names.nsf")
                {
                    this.pNotesDatabase = this._pNotesSession.GetDatabase(this._strDomain, this._strDataBase, false);
                }

                if (this.pNotesDatabase == null)
                {
                    throw new Exception("���ܴ����ݿ⣺" + this._strDataBase);
                }

                pParentView = this.pNotesDatabase.GetView("($inbox)");
                pParentDocument = pParentView.GetFirstDocument();

                while (pParentDocument != null)
                {
                    if (pParentDocument.UniversalID == strNotesUID)
                    {
                        NotesDocument pRelayDocument = pParentDocument.CreateReplyMessage(false);
                        string strPrincipal = (((object[])pParentDocument.GetItemValue("Principal"))[0] == null) ? "N/A" : ((object[])pParentDocument.GetItemValue("Principal"))[0].ToString();
                        string strRelaySubject = (((object[])pParentDocument.GetItemValue("Subject"))[0] == null) ? "N/A" : ((object[])pParentDocument.GetItemValue("Subject"))[0].ToString();

                        pParentDocument.ReplaceItemValue("Form", "Reply");
                        pParentDocument.ReplaceItemValue("CopyTo", pSupervisors);//����
                        pParentDocument.ReplaceItemValue("BlindCopyTo", pSendSecret); //����
                        pParentDocument.ReplaceItemValue("Subject", "�ظ�:" + strRelaySubject);
                        pParentDocument.ReplaceItemValue("PostedDate", DateTime.Now.ToString());
                        pParentDocument.ReplaceItemValue("Principal", "CN=netadmin/OU=���ܲ�/OU=��Ʒ��Ӫ����/O=runstar");
                        pParentDocument.ReplaceItemValue("Body", "");
                        pParentDocument.ReplaceItemValue("SSM_Agent", strNotesUID);

                        if (pParentDocument.HasItem("Reader"))
                        {
                            pParentDocument.ReplaceItemValue("Reader", "NO");
                        }

                        NotesRichTextItem pOldItem = (NotesRichTextItem)pParentDocument.GetFirstItem("Body");
                        pOldItem.AppendText(strMailContent);
                        pOldItem.AddNewLine(5, false);
                        pOldItem.AppendRTItem((NotesRichTextItem)pRelayDocument.GetFirstItem("Body"));

                        object pSendOwner = strPrincipal;//"��¶"
                        pParentDocument.Send(false, ref pSendOwner);

                        bResult = true;

                        Marshal.ReleaseComObject(pOldItem);
                        Marshal.ReleaseComObject(pRelayDocument);

                        pOldItem = null;
                        pRelayDocument = null;
                        break;
                    }
                    else
                    {
                        bResult = false;

                        this.strMessage = "�����ҵ�ԭʼ�ż���ԭ�ż�������ɾ�������½�һ���ŵ��ż��������ˣ�";
                    }

                    pParentDocument = pParentView.GetNextDocument(pParentDocument);
                }
            }
            catch (Exception ex)
            {
                this.strMessage = ex.Message;

                bResult = false;
            }
            finally
            {
                if (pParentDocument != null)
                {
                    Marshal.ReleaseComObject(pParentDocument);
                }

                if (pParentView != null)
                {
                    Marshal.ReleaseComObject(pParentView);
                }

                pParentDocument = null;
                pParentView = null;
            }

            return bResult;
        }

        /// <summary>
        /// ϵͳ��Ϣ
        /// </summary>
        public string Message
        {
            get
            {
                return this.strMessage;
            }
        }

        /// <summary>
        /// ��¼�����
        /// </summary>
        public object Records
        {
            get
            {
                return pRecords;
            }
        }

        private string _strDomain;
        private string _strDataBase;
        private string _strUserName;
        private string strMessage;
        private ArrayList pInfoList;
        private CustomDataCollection pRecords;

        private NotesSessionClass _pNotesSession;
        private NotesDatabase pNotesDatabase;
    }
}
