using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.DesignTime.Activities;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Microsoft.Web.Hosting.Administration;
using X5.SharePoint.Quality.Models;
using Function = X5.SharePoint.Quality.Workflows.Logic.Function;
using ManagerSQL = X5.SharePoint.Quality.Workflows.Logic.ManagerSQL;
using X5.SharePoint.Quality.Workflows.Logic;

namespace X5.SharePoint.Quality.Workflows.QualityDocumentWorkflow
{
    /// <summary>
    /// добавление записи по новым документам в БД
    /// </summary>
#pragma warning disable CS0618 // 'SequentialWorkflowActivity' is obsolete: 'The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*'
    public sealed partial class QualityDocumentWorkflow : SequentialWorkflowActivity
#pragma warning restore CS0618 // 'SequentialWorkflowActivity' is obsolete: 'The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*'
    {
        public QualityDocumentWorkflow()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        public string Market { get; set; }
        public string CodeSapMarket { get; set; }
        public string ProductionDate { get; set; }
        public string ExpirationDate { get; set; }
        public string ComplaintDescription { get; set; }
        public string ReclamationDate { get; set; }
        public string BuyerFio { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CorrectiveAction { get; set; }
        public string Rc { get; set; }
        public string ComplaintClosed { get; set; }
        public string NamePlu { get; set; }
        //public string Producer { get; set; }
        //public string SupplierName { get; set; }
        public string BarcodeBuf { get; set; }
        public string SupplierBuf { get; set; }
        public string ProducerBuf { get; set; }
        public string UnitOfMeasureBuf { get; set; }
        public string NamePluBuf { get; set; }
        public string CommertialNetwork { get; set; }
        public string AuditDate { get; set; }
        public string NextDateAudit { get; set; }
        public string AuditingCompany { get; set; }
        public string Auditor { get; set; }
        public string ManagerCurator { get; set; }
        public string ManagerTechnolog { get; set; }
        public string Rating { get; set; }
        public string Food { get; set; }
        public string NotFood { get; set; }
        public string Fresh { get; set; }
        public string LevelTwo { get; set; }
        public string Plan { get; set; }
        public string AuditScope { get; set; }
        public string Primary { get; set; }
        public string DateApplication { get; set; }
        public string Laboratory { get; set; }
        public string ContactPerson { get; set; }
        public string NotIdplu { get; set; }
        public string Ppk { get; set; }
        public string Nd { get; set; }
        public string ProducerNd { get; set; }
        public string NumberOfSamples { get; set; }
        public string Comments { get; set; }
        public string Paking { get; set; }
        public string Indicators { get; set; }
        public string Standarts { get; set; }
        public string Other { get; set; }
        public string ApplicationApproved { get; set; }
        public string Results { get; set; }
        //public string Source { get; set; }
        public string UserDisplayName { get; set; }
        //public string DocName { get; set; }
        //public string File { get; set; }
        public string FTPFiles { get; set; }
        //public Guid ListId { get; set; }
        //public Guid WebId { get; set; }
        //public Guid SiteId { get; set; }
        //public int ItemId { get; set; }
        public string DocTypeName { get; set; }
        public string CertifyNum { get; set; }
        public string[] Numbers { get; set; }
        public string[] Barcodes { get; set; }
        public string[] Suppliers { get; set; }
        public string[] Producers { get; set; }
        public string[] UnitOfMeasures { get; set; }
        public string[] NamePLUs { get; set; }
        //public string[] DeliveryDates { get; set; }
        public string Number { get; set; }
        public string PLUs { get; set; }
        public string NumbersInvoice { get; set; }
        public int DocTypeId { get; set; }
        public string Applicant { get; set; }
        public string Agency { get; set; }
        public string StartDate { get; set; }
        public string DurationTime { get; set; }
        public string DeliveryDate { get; set; }
        public string CurrentId { get; set; }
        public string DBName { get; set; }
        public ManagerSQL.DBSettings Settings { get; set; }
        public List<object> ResultNumber { get; set; }
        public string GroupName { get; set; }

       private void onWorkflowActivated1_Invoked(object sender, ExternalDataEventArgs e)
       {
            var fields = new SiteFieldsModel();
            var x5QualityFields = new X5QualitySpellabsFieldsModel();
            var contentTypes = new ContentTypesModel();
            var docflowLists = new Spellabs.Docflow.Models.ListsModel();

            #region Получение данных из списка для документов качества
            try
            {
                this.UserDisplayName = workflowProperties.OriginatorUser.Name;

                DocTypeName = workflowProperties.Item.ContentType.Name;
                Number = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NumberPLUBuf.StaticName], String.Empty);
                PLUs = Number;
                NumbersInvoice = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NumberInvoice.StaticName], String.Empty);
            }
            catch (Exception ex)
            {
                WriteToHistoryList("Can't get field values UserDisplayName, DocTypeName, PLUs, NumbersInvoice",
                    String.Format("UserDisplayName {0};DocTypeName {1};PLUs {2};NumbersInvoice {3};", UserDisplayName,
                        DocTypeName, PLUs, NumbersInvoice));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }

            try
            {
                CertifyNum = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NumberDeclaration.StaticName], String.Empty);
                if (CertifyNum.Length == 0)
                {
                    CertifyNum = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NumberCertificate.StaticName], String.Empty);
                }
                if (CertifyNum.Length == 0)
                {
                    CertifyNum =
                        Function.GetSPFieldValue<string>(
                            workflowProperties.Item[fields.NumberVoluntaryCertification.StaticName], String.Empty);
                }
                if (CertifyNum.Length == 0)
                {
                    CertifyNum = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NumberAttestation.StaticName], String.Empty);
                }
            }
            catch (Exception ex)
            {
                WriteToHistoryList("Can't get field values CertifyNum", String.Format("CertifyNum {0};", CertifyNum));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }

            try
            {
                Applicant = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Applicant.StaticName], String.Empty);
                Agency = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Agency.StaticName], String.Empty);
                StartDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.DataIssue.StaticName], String.Empty);
                DurationTime = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ValidityPeriod.StaticName], String.Empty);
                DeliveryDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.DeliveryDateBuf.StaticName], String.Empty);
                CurrentId = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.CurrentIDBuf1.StaticName], String.Empty);
            }
            catch (Exception ex)
            {
                WriteToHistoryList("Can't get field values Applicant, Agency, StartDate, DurationTime, DeliveryDate, CurrentId",
                    String.Format(
                        "Applicant {0};Agency {1};StartDate {2};DurationTime {3};DeliveryDate {4};CurrentId {5};",
                        Applicant, Agency, StartDate, DurationTime, DeliveryDate, CurrentId));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion

            #region Получение имени и ссылки прикрепляемого файла
            FTPFiles = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.DocumentFTPFiles.StaticName], String.Empty);
            //try
            //{

            //    var attachmentList = Function.GetList(workflowProperties.Web, docflowLists.AttachmentsLibrary.CustomUrl);

            //    var nameFileQuery = new SPQuery
            //    {
            //        Query =
            //            String.Format(@"<Where>
            //        <Eq>
            //            <FieldRef Name = 'ListItemId'/>
            //            <Value Type = 'Number'>{0}</Value>
            //        </Eq>
            //    </Where>", CurrentId)
            //    };

            //    var existAttachmentCollection = attachmentList.GetItems(nameFileQuery);
            //    if (existAttachmentCollection.Count > 0)
            //    {
            //        DocName = existAttachmentCollection[0].Title;
            //        File = existAttachmentCollection[0]["FileLeafRef"].ToString();
            //    }

            //    Source = SPUtility.ConcatUrls(workflowProperties.Web.ServerRelativeUrl,
            //        docflowLists.AttachmentsLibrary.CustomUrl + "/" + File);

            //    File = DocName;
            //}
            //catch (Exception ex)
            //{
            //    WriteToHistoryList(
            //        "Can't get field values CurrentId, DocName, File, Source",
            //        String.Format("CurrentId {0};DocName {1};File {2};Source {3};", CurrentId, DocName, File, Source));
            //    WriteToHistoryList(ex.Message, ex.StackTrace);
            //    throw;
            //}

            #endregion

            #region Получение номера типа документа
            try
            {
                if (DocTypeName == contentTypes.CertificateConformity.Name)
                {
                    DocTypeId = 0;
                }
                if (DocTypeName == contentTypes.DeclarationConformity.Name)
                {
                    DocTypeId = 5;
                }
                if (DocTypeName == contentTypes.CertificateStateRegistration.Name)
                {
                    DocTypeId = 6;
                }
                if (DocTypeName == contentTypes.CertificateConformityVoluntaryCertification.Name)
                {
                    DocTypeId = 7;
                }
                if (DocTypeName == contentTypes.RejectionLetter.Name)
                {
                    DocTypeId = 8;
                }
                if (DocTypeName == contentTypes.ReportAudit.Name)
                {
                    DocTypeId = 9;
                }
                if (DocTypeName == contentTypes.ConsumerComplaint.Name)
                {
                    DocTypeId = 10;
                }
                if (DocTypeName == contentTypes.ProductTestingApplication.Name)
                {
                    DocTypeId = 11;
                }


                if (!NumbersInvoice.Contains(';'))
                {
                    NumbersInvoice = NumbersInvoice + ';';
                }

                Numbers = NumbersInvoice.Split(';');

            }
            catch (Exception ex)
            {
                WriteToHistoryList("Can't get field values DocTypeName, DocTypeId, NumbersInvoice",
                    String.Format("DocTypeName {0};DocTypeId {1};NumbersInvoice {2};", DocTypeName, DocTypeId, NumbersInvoice));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion
            
            #region Удаление повторяющихся  значений в поставках
            try
            {
                var numberArray = new ArrayList();
                for (int i = 0; i < (Numbers.Length - 1); i++)
                {
                    numberArray.Add(Numbers[i]);
                    Function.AppendFileLog("x5.sharepoint.quality.EventReceiver", Numbers[i]);
                }
                numberArray.Sort();
                ResultNumber = numberArray.ToArray().Distinct().ToList();

                //DeliveryDates = DeliveryDate.Split(';');
            }
            catch (Exception ex)
            {
                WriteToHistoryList( "Can't get field values ResultNumber, Numbers", String.Format("ResultNumber {0};Numbers {1};", ResultNumber, Numbers));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion

            #region Получение данных из списка для рекламации потребителя
            try
            {
                Market = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Market.StaticName], String.Empty);
                CodeSapMarket = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.CodeSAPMarket.StaticName], String.Empty);
                ProductionDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ProductionDate.StaticName], String.Empty);
                ExpirationDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ExpirationDate.StaticName], String.Empty);
                ComplaintDescription = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ComplaintDescription.StaticName], String.Empty);
                ReclamationDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ComplaintDate.StaticName], String.Empty);
                BuyerFio = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.BuyerFIO.StaticName], String.Empty);
                Phone = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Phone.StaticName], String.Empty);
                Email = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Email.StaticName], String.Empty);
                CorrectiveAction = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.CorrectiveAction.StaticName], String.Empty);
                Rc = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.RC.StaticName], String.Empty);
                ComplaintClosed = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ComplaintClosed.StaticName], String.Empty);
                NamePlu = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.NamePLU.StaticName], String.Empty);
                BarcodeBuf = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.BarcodeBuf.StaticName], String.Empty);
                SupplierBuf = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.SupplierBuf.StaticName], String.Empty);
                ProducerBuf = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ProducerBuf.StaticName], String.Empty);
                UnitOfMeasureBuf = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.UnitOfMeasureBuf.StaticName], String.Empty);
                NamePluBuf = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NamePLUBuf.StaticName], String.Empty);

                Barcodes = BarcodeBuf.Split(';');
                Suppliers = SupplierBuf.Split(';');
                Producers = ProducerBuf.Split(';');
                UnitOfMeasures = UnitOfMeasureBuf.Split(';');
                NamePLUs = NamePluBuf.Split(';');
            }            
            catch (Exception ex)
            {
                WriteToHistoryList(
                    "Can't get field values Market, CodeSapMarket, ProductionDate, ExpirationDate, ComplaintDescription, ReclamationDate, BuyerFio, Phone,"
                    + "Email, CorrectiveAction, Rc, ComplaintClosed, NamePlu, BarcodeBuf, SupplierBuf, ProducerBuf, UnitOfMeasureBuf, NamePluBuf,"
                    + "Barcodes,Suppliers,Producers,UnitOfMeasures,NamePLUs",
                    String.Format("{0}{1}{2}",
                        String.Format(
                            "Market {0}, CodeSapMarket {1}, ProductionDate {2}, ExpirationDate {3}, ComplaintDescription {4}, ReclamationDate {5}, BuyerFio {6}, Phone {7}, ",
                            Market, CodeSapMarket, ProductionDate, ExpirationDate, ComplaintDescription, ReclamationDate,
                            BuyerFio, Phone),
                        String.Format(
                            "Email {0}, CorrectiveAction {1}, Rc {2}, ComplaintClosed {3}, NamePlu {4}, BarcodeBuf {5}, SupplierBuf {6}, ProducerBuf {7}, UnitOfMeasureBuf {8}, NamePluBuf {9},",
                            Email, CorrectiveAction, Rc, ComplaintClosed, NamePlu, BarcodeBuf, SupplierBuf, ProducerBuf,
                            UnitOfMeasureBuf, NamePluBuf),
                        String.Format("Barcodes {0},Suppliers {1},Producers {2},UnitOfMeasures {3},NamePLUs {4}",
                            Barcodes, Suppliers, Producers, UnitOfMeasures, NamePLUs)));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion

            #region Получение данных из списка для отчета по аудиту
            try
            {
                CommertialNetwork = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.CommertialNetwork.StaticName], String.Empty);
                AuditDate = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.AuditDate.StaticName], String.Empty);
                NextDateAudit = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NextDateAudit.StaticName], String.Empty);
                AuditingCompany = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.AuditingCompany.StaticName], String.Empty);
                Auditor = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Auditors.StaticName], String.Empty);
                ManagerCurator = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.ManagerCurators.StaticName], String.Empty);
                ManagerTechnolog = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.ManagerTechnologs.StaticName], String.Empty);
                Rating = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Evaluation.StaticName], String.Empty);
                Food = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Food.StaticName], String.Empty);
                NotFood = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.NotFood.StaticName], String.Empty);
                Fresh = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Fresh.StaticName], String.Empty);
                LevelTwo = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.LevelTwo.StaticName], String.Empty);
                Plan = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.HavingPlan.StaticName], String.Empty);
                AuditScope = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.AuditScope.StaticName], String.Empty);
                Primary = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Primary.StaticName], String.Empty);
            }
            catch (Exception ex)
            {
                WriteToHistoryList(
                    "Can't get field values CommertialNetwork, AuditDate, NextDateAudit, AuditingCompany, Auditor, ManagerCurator, ManagerTechnolog"
                    + "Rating, Food, NotFood, Fresh, LevelTwo, Plan, AuditScope, Primary",
                    String.Format(
                        "CommertialNetwork {0}, AuditDate {1}, NextDateAudit {2}, AuditingCompany {3}, Auditor {4}, ManagerCurator {5}, ManagerTechnolog {6},",
                        CommertialNetwork, AuditDate, NextDateAudit, AuditingCompany, Auditor, ManagerCurator,
                        ManagerTechnolog)
                    +
                    String.Format(
                        "Rating {0}, Food {1}, NotFood {2}, Fresh {3}, LevelTwo {4}, Plan {5}, AuditScope {6}, Primary {7}",
                        Rating, Food, NotFood, Fresh, LevelTwo, Plan, AuditScope, Primary));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion

            #region Получение данных из списка для заявки по проведению испытания продукции
            try
            {
                DateApplication = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ApplicationDate.StaticName], String.Empty);
                Laboratory = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Laboratory.StaticName], String.Empty);
                ContactPerson = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.ContactsPersons.StaticName], String.Empty);
                NotIdplu = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.WithoutPLU.StaticName], String.Empty);
                Ppk = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.PPK.StaticName], String.Empty);
                Nd = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ProductND.StaticName], String.Empty);
                ProducerNd = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ProducerProductND.StaticName], String.Empty);
                NumberOfSamples = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.SamplesNumber.StaticName], String.Empty);
                Comments = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Comments.StaticName], String.Empty);
                Paking = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Packaging.StaticName], String.Empty);
                Indicators = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Indicators.StaticName], String.Empty);
                Standarts = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Standards.StaticName], String.Empty);
                Other = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.Other.StaticName], String.Empty);
                ApplicationApproved = Function.GetSPFieldValue<string>(workflowProperties.Item[fields.ApplicationApproved.StaticName], String.Empty);
                Results = Function.GetSPFieldValue<string>(workflowProperties.Item[x5QualityFields.Results.StaticName], String.Empty);
            }
            catch (Exception ex)
            {
                WriteToHistoryList(
                    "Can't get field values DateApplication, Laboratory, ContactPerson, NotIdplu, Ppk, Nd, ProducerNd, NumberOfSamples"
                    + "Comments, Paking, Indicators, Standarts, Other, ApplicationApproved, Results",
                    String.Format("{0}{1}",
                        String.Format(
                            "DateApplication {0}, Laboratory {1}, ContactPerson {2}, NotIdplu {3}, Ppk {4}, Nd {5}, ProducerNd {6}, NumberOfSamples {7},",
                            DateApplication, Laboratory, ContactPerson, NotIdplu, Ppk, Nd, ProducerNd, NumberOfSamples),
                        String.Format(
                            "Comments {0}, Paking {1}, Indicators {2}, Standarts {3}, Other {4}, ApplicationApproved {5}, Results {6}",
                            Comments, Paking, Indicators, Standarts, Other, ApplicationApproved, Results)));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion

            #region получение имени группы
            var groupNameCo = String.Empty;
            var groupNameRc = String.Empty;
            try
            {
                try
                {
                    groupNameRc = workflowProperties.OriginatorUser.Groups.GetByName("Группа РЦ").ToString();

                }
                catch (Exception)
                {
                    // ignored
                }
                
                try
                {
                    groupNameCo = workflowProperties.OriginatorUser.Groups.GetByName("Группа ЦО").ToString();
                }
                catch (Exception)
                {
                    // ignored
                }
                GroupName = String.Empty;
                if (groupNameRc.Length > 0)
                {
                    if (GroupName.Length > 0)
                    {
                        GroupName += ",";
                    }
                    GroupName += workflowProperties.OriginatorUser.Groups.GetByName(groupNameRc);
                }
                if (groupNameCo.Length > 0)
                {
                    if (GroupName.Length > 0)
                    {
                        GroupName += ",";
                    }
                    GroupName += workflowProperties.OriginatorUser.Groups.GetByName(groupNameCo);
                }
                using (var manager = new ManagerSQL(workflowProperties.Web))
                {
                    DBName = manager.DBName;
                    Settings = new ManagerSQL.DBSettings(manager.DBLogin, manager.DBPassword, manager.DBServerName, manager.DBIntegratedSequrity);
                }
            }
            catch (Exception ex)
            {
                WriteToHistoryList(
                    "Can't get field values groupNameRc, groupNameCo",
                    String.Format("groupNameRc {0};groupNameCo {1};", groupNameRc, groupNameCo));
                WriteToHistoryList(ex.Message, ex.StackTrace);
                throw;
            }
            #endregion
        }

        private void GetData_ExecuteCode(object sender, EventArgs e)
        {
            var dbname = DBName;
            var dbsettings = Settings;
            using (var manager = new ManagerSQL(dbsettings, dbname))
            {
                switch (DocTypeId)
                {
                    case 10:
                        {
                            #region вставка в БД новой записи о рекламации потребителя
                            for (var i = 0; i < (Numbers.Length - 1); i++)
                            {
                                WriteToHistoryList("Command insertNewConsumerComplaint",
                                    manager.LogCommand("insertNewConsumerComplaint", false,
                                        //new SqlParameter("File", File),
                                        //new SqlParameter("Source", Source),,
                                        new SqlParameter("FTPFiles", FTPFiles),
                                        new SqlParameter("PLUID", Convert.ToInt64(Numbers[i])),
                                        new SqlParameter("MaterialShortText", NamePLUs[i]),
                                        new SqlParameter("Producer", Producers[i]),
                                        new SqlParameter("RC", Rc.Substring(Rc.IndexOf('#') + 1)), //todo
                                        new SqlParameter("Market", Market.Substring(Market.IndexOf('#') + 1)), //todo
                                        new SqlParameter("IDSAPMarket", CodeSapMarket.Substring(CodeSapMarket.IndexOf('#') + 1)),
                                        new SqlParameter("Barcode", Barcodes[i]),
                                        new SqlParameter("SupplierName", Suppliers[i]),
                                        new SqlParameter("ProductionDate", DateTime.Parse(ProductionDate)),
                                        new SqlParameter("ExpirationDate", DateTime.Parse(ExpirationDate)),
                                        new SqlParameter("DescrMarketing", ComplaintDescription),
                                        new SqlParameter("ReclamationDate", DateTime.Parse(ReclamationDate)),
                                        new SqlParameter("FIOBuyer", BuyerFio),
                                        new SqlParameter("PhoneBuyer", Phone),
                                        new SqlParameter("EmailBuyer", Email),
                                        new SqlParameter("CorrectiveAction", CorrectiveAction),
                                        new SqlParameter("StatusMarketing", ComplaintClosed)
                                        ));

                                manager.Command("insertNewConsumerComplaint", false,
                                        //new SqlParameter("File", File),
                                        //new SqlParameter("Source", Source),,
                                        new SqlParameter("FTPFiles", FTPFiles),
                                        new SqlParameter("PLUID", Convert.ToInt64(Numbers[i])),
                                        new SqlParameter("MaterialShortText", NamePLUs[i]),
                                        new SqlParameter("Producer", Producers[i]),
                                        new SqlParameter("RC", Rc.Substring(Rc.IndexOf('#') + 1)), //todo
                                        new SqlParameter("Market", Market.Substring(Market.IndexOf('#') + 1)), //todo
                                        new SqlParameter("IDSAPMarket", CodeSapMarket.Substring(CodeSapMarket.IndexOf('#') + 1)),
                                        new SqlParameter("Barcode", Barcodes[i]),
                                        new SqlParameter("SupplierName", Suppliers[i]),
                                        new SqlParameter("ProductionDate", DateTime.Parse(ProductionDate)),
                                        new SqlParameter("ExpirationDate", DateTime.Parse(ExpirationDate)),
                                        new SqlParameter("DescrMarketing", ComplaintDescription),
                                        new SqlParameter("ReclamationDate", DateTime.Parse(ReclamationDate)),
                                        new SqlParameter("FIOBuyer", BuyerFio),
                                        new SqlParameter("PhoneBuyer", Phone),
                                        new SqlParameter("EmailBuyer", Email),
                                        new SqlParameter("CorrectiveAction", CorrectiveAction),
                                        new SqlParameter("StatusMarketing", ComplaintClosed)
                                        );
                            }
                            #endregion
                            break;
                        }
                    case 9:
                        {
                            #region вставка в БД новой записи об отчете по аудиту
                            for (var i = 0; i < (Numbers.Length - 1); i++)
                            {
                                WriteToHistoryList("Command insertNewReportAudit",
                                    manager.LogCommand("insertNewReportAudit", false,
                                    //new SqlParameter("File", File),
                                    //new SqlParameter("Source", Source),
                                    new SqlParameter("FTPFiles", FTPFiles),
                                    new SqlParameter("CommertialNetwork", Convert.ToInt64(CommertialNetwork.Substring(0, CommertialNetwork.LastIndexOf(';')))),
                                    new SqlParameter("StartDate", DateTime.Parse(AuditDate)),
                                    new SqlParameter("NextDate", DateTime.Parse(NextDateAudit)),
                                    new SqlParameter("SupplierName", Suppliers[i]),
                                    new SqlParameter("Producer", Producers[i]),
                                    new SqlParameter("AuditingCompany", Convert.ToInt64(AuditingCompany.Substring(0, AuditingCompany.LastIndexOf(';')))),
                                    new SqlParameter("Auditor", Convert.ToInt64(Auditor.Substring(0, Auditor.LastIndexOf(';')))),
                                    new SqlParameter("ManagerCurator", Convert.ToInt64(ManagerCurator.Substring(0, ManagerCurator.LastIndexOf(';')))),
                                    new SqlParameter("ManagerTechnolog", Convert.ToInt64(ManagerTechnolog.Substring(0, ManagerTechnolog.LastIndexOf(';')))),
                                    new SqlParameter("Rating", Rating),
                                    new SqlParameter("Food", Food),
                                    new SqlParameter("NotFood", NotFood),
                                    new SqlParameter("Fresh", Fresh),
                                    new SqlParameter("LevelTwo", Convert.ToInt64(LevelTwo.Substring(0, LevelTwo.LastIndexOf(';')))),
                                    new SqlParameter("Category", null), //todo
                                    new SqlParameter("Plan", Plan),
                                    new SqlParameter("AuditScope", Convert.ToInt64(AuditScope.Substring(0, AuditScope.LastIndexOf(';')))), //todo
                                    new SqlParameter("Primary", Primary)
                                    ));

                                manager.Command("insertNewReportAudit", false,
                                    //new SqlParameter("File", File),
                                    //new SqlParameter("Source", Source),
                                    new SqlParameter("FTPFiles", FTPFiles),
                                    new SqlParameter("CommertialNetwork", Convert.ToInt64(CommertialNetwork.Substring(0, CommertialNetwork.LastIndexOf(';')))),
                                    new SqlParameter("StartDate", DateTime.Parse(AuditDate)),
                                    new SqlParameter("NextDate", DateTime.Parse(NextDateAudit)),
                                    new SqlParameter("SupplierName", Suppliers[i]),
                                    new SqlParameter("Producer", Producers[i]),
                                    new SqlParameter("AuditingCompany", Convert.ToInt64(AuditingCompany.Substring(0, AuditingCompany.LastIndexOf(';')))),
                                    new SqlParameter("Auditor", Convert.ToInt64(Auditor.Substring(0, Auditor.LastIndexOf(';')))),
                                    new SqlParameter("ManagerCurator", Convert.ToInt64(ManagerCurator.Substring(0, ManagerCurator.LastIndexOf(';')))),
                                    new SqlParameter("ManagerTechnolog", Convert.ToInt64(ManagerTechnolog.Substring(0, ManagerTechnolog.LastIndexOf(';')))),
                                    new SqlParameter("Rating", Rating),
                                    new SqlParameter("Food", Food),
                                    new SqlParameter("NotFood", NotFood),
                                    new SqlParameter("Fresh", Fresh),
                                    new SqlParameter("LevelTwo", Convert.ToInt64(LevelTwo.Substring(0, LevelTwo.LastIndexOf(';')))), //todo //new SqlParameter("Category", Convert.ToInt64(Logger.Category.Substring(0, Logger.Category.LastIndexOf(';')))),//todo
                                    new SqlParameter("Category", null), //todo
                                    new SqlParameter("Plan", Plan),
                                    new SqlParameter("AuditScope", Convert.ToInt64(AuditScope.Substring(0, AuditScope.LastIndexOf(';')))), //todo
                                    new SqlParameter("Primary", Primary)
                                    );
                            }
                            #endregion
                            break;
                        }
                    case 11:
                        {
                            #region вставка в БД новой записи для заявки по проведению испытания продукции
                            for (var i = 0; i < (Numbers.Length - 1); i++)
                            {
                                WriteToHistoryList("Command insertNewApplicationTesting",
                                    manager.LogCommand("insertNewApplicationTesting", false,
                                    //new SqlParameter("File", File),
                                    //new SqlParameter("Source", Source),
                                    new SqlParameter("FTPFiles", FTPFiles),
                                    new SqlParameter("Producer", Producers[i]),
                                    new SqlParameter("DateApplication", DateTime.Parse(DateApplication)),
                                    new SqlParameter("Laboratory", Convert.ToInt64(Laboratory.Substring(0, Laboratory.LastIndexOf(';')))), //todo
                                    new SqlParameter("ContactPerson", Convert.ToInt64(ContactPerson.Substring(0, ContactPerson.LastIndexOf(';')))),
                                    new SqlParameter("SeatCelection", Rc.Substring(Rc.IndexOf('#') + 1)), //todo
                                    new SqlParameter("PLUID", Convert.ToInt64(Numbers[i])),
                                    new SqlParameter("Barcode", Barcodes[i]),
                                    new SqlParameter("NotIDPLU", NotIdplu),
                                    new SqlParameter("PPK", Ppk),
                                    new SqlParameter("ND", Nd),
                                    new SqlParameter("ProducerND", ProducerNd),
                                    new SqlParameter("ProductionDate", DateTime.Parse(ProductionDate)),
                                    new SqlParameter("EndDate", DateTime.Parse(ExpirationDate)),
                                    new SqlParameter("NumberOfSamples", Convert.ToInt64(NumberOfSamples)),
                                    new SqlParameter("UnitOfMeasure", UnitOfMeasures[i]),
                                    new SqlParameter("Comments", Comments),
                                    new SqlParameter("Paking", Paking),
                                    new SqlParameter("Indicators", Convert.ToInt64(Indicators.Substring(0, Indicators.LastIndexOf(';')))), //todo
                                    new SqlParameter("Standarts", Standarts),
                                    new SqlParameter("Other", Other),
                                    new SqlParameter("ApplicationApproved", ApplicationApproved),
                                    new SqlParameter("Results", Convert.ToInt64(Results.Substring(0, Results.LastIndexOf(';'))))
                                    ));
                                manager.Command("insertNewApplicationTesting", false,
                                    //new SqlParameter("File", File),
                                    //new SqlParameter("Source", Source),
                                    new SqlParameter("FTPFiles", FTPFiles),
                                    new SqlParameter("Producer", Producers[i]),
                                    new SqlParameter("DateApplication", DateTime.Parse(DateApplication)),
                                    new SqlParameter("Laboratory", Convert.ToInt64(Laboratory.Substring(0, Laboratory.LastIndexOf(';')))), //todo
                                    new SqlParameter("ContactPerson", Convert.ToInt64(ContactPerson.Substring(0, ContactPerson.LastIndexOf(';')))),
                                    new SqlParameter("SeatCelection", Rc.Substring(Rc.IndexOf('#') + 1)), //todo
                                    new SqlParameter("PLUID", Convert.ToInt64(Numbers[i])),
                                    new SqlParameter("Barcode", Barcodes[i]),
                                    new SqlParameter("NotIDPLU", NotIdplu),
                                    new SqlParameter("PPK", Ppk),
                                    new SqlParameter("ND", Nd),
                                    new SqlParameter("ProducerND", ProducerNd),
                                    new SqlParameter("ProductionDate", DateTime.Parse(ProductionDate)),
                                    new SqlParameter("EndDate", DateTime.Parse(ExpirationDate)),
                                    new SqlParameter("NumberOfSamples", Convert.ToInt64(NumberOfSamples)),
                                    new SqlParameter("UnitOfMeasure", UnitOfMeasures[i]),
                                    new SqlParameter("Comments", Comments),
                                    new SqlParameter("Paking", Paking),
                                    new SqlParameter("Indicators", Convert.ToInt64(Indicators.Substring(0, Indicators.LastIndexOf(';')))), //todo
                                    new SqlParameter("Standarts", Standarts),
                                    new SqlParameter("Other", Other),
                                    new SqlParameter("ApplicationApproved", ApplicationApproved),
                                    new SqlParameter("Results", Convert.ToInt64(Results.Substring(0, Results.LastIndexOf(';'))))
                                );
                            }
                            #endregion
                            break;
                        }
                    default:
                        {
                            #region проверка Agency
                            try
                            {
                                if (!String.IsNullOrEmpty(Agency))
                                {
                                    if (!Agency.Contains('#'))
                                    {
                                        WriteToHistoryList("Select getCertifyAgencyByName",
                                            manager.LogSelect("getCertifyAgencyByName", new SqlParameter("Name", Agency)));
                                        var result = manager.Select("getCertifyAgencyByName", new SqlParameter("Name", Agency));
                                        Agency = result["ID"].ToString() + ";#" + Agency;

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteToHistoryList(
                                    "Can't get field values Agency",
                                    String.Format("Agency {0};", Agency));
                                WriteToHistoryList(ex.Message, ex.StackTrace);
                                throw;
                            }
                            #endregion
                            #region вставка в БД новой записи о документе
                            try
                            {
                                WriteToHistoryList("Command insertNewQualityDocumentNew",
                                    manager.LogCommand("insertNewQualityDocumentNew", false,
                                        new SqlParameter("CertifyNum", CertifyNum),
                                        new SqlParameter("DocTypeID", DocTypeId),
                                        new SqlParameter("StartDate", String.IsNullOrEmpty(StartDate) ? (object)DBNull.Value : DateTime.Parse(StartDate)),
                                        new SqlParameter("DurationTime", String.IsNullOrEmpty(DurationTime) ? (object)DBNull.Value : DateTime.Parse(DurationTime)),
                                        new SqlParameter("AgencyID", String.IsNullOrEmpty(Agency) ? (object)DBNull.Value : (new SPFieldLookupValue(Agency)).LookupId),
                                        //new SqlParameter("File", File),
                                        //new SqlParameter("Source", Source),
                                        new SqlParameter("Applicant", Applicant),
                                        // new SqlParameter("Number", number),
                                        // new SqlParameter("DeliveryDate", String.Empty),
                                        new SqlParameter("CreationDate", DateTime.Now),
                                        new SqlParameter("Inspection", bool.FalseString),
                                        new SqlParameter("PLUIDs", PLUs),
                                        new SqlParameter("CurrentID", Convert.ToInt64(CurrentId)),
                                        new SqlParameter("CurrentUser", UserDisplayName),
                                        new SqlParameter("CurrentGroup", GroupName),
                                        new SqlParameter("FTPFiles", FTPFiles)
                                        ));

                                manager.Command("insertNewQualityDocumentNew", false,
                                        new SqlParameter("CertifyNum", CertifyNum),
                                        new SqlParameter("DocTypeID", DocTypeId),
                                        new SqlParameter("StartDate", String.IsNullOrEmpty(StartDate) ? (object)DBNull.Value : DateTime.Parse(StartDate)),
                                        new SqlParameter("DurationTime", String.IsNullOrEmpty(DurationTime) ? (object)DBNull.Value : DateTime.Parse(DurationTime)),
                                        new SqlParameter("AgencyID", String.IsNullOrEmpty(Agency) ? (object)DBNull.Value : (new SPFieldLookupValue(Agency)).LookupId),
                                        //new SqlParameter("File", File),
                                        //new SqlParameter("Source", Source),
                                        new SqlParameter("Applicant", Applicant),
                                        //new SqlParameter("Number", number),
                                        //new SqlParameter("DeliveryDate", String.Empty),
                                        new SqlParameter("CreationDate", DateTime.Now),
                                        new SqlParameter("Inspection", bool.FalseString),
                                        new SqlParameter("PLUIDs", PLUs),
                                        new SqlParameter("CurrentID", Convert.ToInt64(CurrentId)),
                                        new SqlParameter("CurrentUser", UserDisplayName),
                                        new SqlParameter("CurrentGroup", GroupName),
                                        new SqlParameter("FTPFiles", FTPFiles)
                                    );

                                //DateTime DataIssue = Function.GetSPFieldValue<DateTime>(properties.AfterProperties[_fields.DataIssue.StaticName], new DateTime(1980, 1, 1));
                                //new SqlParameter("DataIssue", DataIssue)
                                manager.Command("MarkDocumentForRemove", false, new SqlParameter("DocID", Convert.ToInt32(CurrentId)));
                            }
                            catch (Exception ex)
                            {
                                WriteToHistoryList(ex.Message, ex.StackTrace);
                            }
                            #endregion
                            break;
                        }
                }
            }
        }

        private void WriteToHistoryList(string comment, string outcome)
        {
            string[] aMessagePath = SplitEx(outcome, 255);

            foreach(string msg in aMessagePath)
            {
                SPWorkflow.CreateHistoryEvent(workflowProperties.Web, this.WorkflowInstanceId, 0, workflowProperties.Web.CurrentUser, new TimeSpan(), msg, comment, comment);
            }
        }

        public static string[] SplitEx(string InputString, int Size)
        {
            return Enumerable.Range(0, (int)Math.Ceiling((double)InputString.Length / (double)Size)).Select(i => InputString.Length > (i * Size + Size) ? InputString.Substring(i * Size, Size) : InputString.Substring(i * Size, InputString.Length - i * Size)).ToArray();
        }
    }
}
