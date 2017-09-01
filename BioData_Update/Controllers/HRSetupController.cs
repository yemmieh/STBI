using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FastMember;
using BioData_Update.App_Code;
using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace BioData_Update.Controllers {
    public class HRSetupController : Controller {
        // GET: HRSetup
        private const string MARKETING  = "MARKETING";
        private const string HOBRCODE   = "001";
        private const string OTHERS     = "OTHERS";
        private const string NA         = "NA";

        private string UPLOADEDMSG  = "You have successfully uploaded the target setup.";

        //private string UserID    = "";
        private string _UserName = "";

        [HttpPost]
        public ActionResult GetStaffProfile( String StaffNumber ) {

            string errorResult = "{{\"employee_number\":\"{0}\",\"name\":\"{1}\"}}";
            if( string.IsNullOrEmpty( StaffNumber ) ) {
                errorResult = string.Format(errorResult , "Error" , "Invalid staff number");        
                return Content(errorResult, "application/json");
            }

            var profile = LINQCalls.getBranchStaffProfile(StaffNumber,1);    
            if( profile==null ){
                errorResult = string.Format(errorResult , "Error" , "No records found for the staff number");        
                return Content(errorResult, "application/json");
            } else {
                return Json( profile , JsonRequestBehavior.AllowGet );
            }
        }

        [HttpPost]
        [RBAC]
        public ActionResult FilterBranchInitiators( string FilterBy ) {
            return RedirectToAction( "ViewBranchInitiators" , new {FilterBy=FilterBy} );
        }

        private List<BHSingleSetupModel> FilterBranchInitiatorList( List<BHSingleSetupModel> bhList , string FilterBy ) {
                FilterBy = FilterBy.ToUpper();
                bhList = bhList.Where(  c => c.SetupBranch.ToUpper().Contains(FilterBy)   || 
                                             c.StaffNumber.ToUpper().Contains(FilterBy)     || 
                                             c.StaffName.ToUpper().Contains(FilterBy)          || 
                                             c.SetupAppPeriod.ToUpper().Contains(FilterBy)     || 
                                             c.SelectedAppraisalPeriod.ToUpper().Contains(FilterBy))
                                             .ToList();
            return bhList;
        }
        
        public static DataTable ToDataTable<T>( IEnumerable<T> data) {
            
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            
            foreach (PropertyDescriptor prop in properties) { 
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            
            foreach (T item in data) {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties) { 
                    Debug.WriteLine(prop.Name);   
                    Debug.WriteLine(prop.GetValue(item)); 
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    //Debug.WriteLine(row[prop.Name]);
                }
                table.Rows.Add(row);
            }
            return table;
        }
        
        public static List<SetupExcelModel> GetDataTableFromSpreadsheet( Stream MyExcelStream , bool ReadOnly , BHSingleSetupModel bHSingleSetupModel , HRProfile hRProfile) {
            
            List<SetupExcelModel> dt = new List<SetupExcelModel>();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly)) {
                
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0)) {
                    //dt.Add(GetCellValue(sDoc, cell));
                }

                SetupExcelModel setupExcelModel;
                Debug.WriteLine("rows length = "+ rows.Count());

                foreach (Row row in rows)  {

                    setupExcelModel = new SetupExcelModel();
                    setupExcelModel.Id = (int)row.RowIndex.Value;
                    
                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++) {           
                        Debug.WriteLine("i = "+i);
                        switch ( i ) {
                            case 0:      //StaffBranch
                                setupExcelModel.StaffBranch=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 1:     //StaffBranchCode
                                setupExcelModel.StaffBranchCode=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 2:     //StaffNumber
                                setupExcelModel.StaffNumber=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 3:     //StaffName
                                setupExcelModel.StaffName=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 4:     //StaffRole--SelectedAppraisalPeriod--SetupAppPeriod--HRProfile--Comments
                                setupExcelModel.StaffRole               = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                setupExcelModel.SelectedAppraisalPeriod = bHSingleSetupModel.SelectedAppraisalPeriod;
                                setupExcelModel.SetupAppPeriod          = bHSingleSetupModel.SetupAppPeriod;
                                setupExcelModel.HRProfile               = hRProfile;
                                setupExcelModel.Comments                = bHSingleSetupModel.Comments;

                                dt.Add( setupExcelModel );
                                
                                break;
                        }
                    }
                }
            }
            return dt;
        }
        
        public static string GetCellValue(SpreadsheetDocument document, Cell cell) {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString) {
                //Debug.WriteLine("stringTablePart = "+stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText);
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            } else {
                //Debug.WriteLine("value = "+value);
                return value;
            }
        }
        public static string ConvertDataTableToHTMLTable(DataTable dt) {
            string ret = "";
            ret = "<table id=" + (char)34 + "tblExcel" + (char)34 + ">";
            ret+= "<tr>";
            foreach (DataColumn col in dt.Columns) {
                ret += "<td class=" + (char)34 + "tdColumnHeader" + (char)34 + ">" + col.ColumnName + "</td>";
            }
            ret+= "</tr>";
            foreach (DataRow row in dt.Rows) {
                ret+="<tr>";
                for (int i = 0;i < dt.Columns.Count;i++) {
                    ret+= "<td class=" + (char)34 + "tdCellData" + (char)34 + ">" + row[i].ToString() + "</td>";
                }
                ret+= "</tr>";
            }
            ret+= "</table>";
            return ret;
        }

        [HttpGet]
        [RBAC]
        public ActionResult HRUpload() {
            /**First let's check if the PostBackMessage has something
             * Very important---DO NOT DELETE!!!!!!!!!!!!!!!!!!!!!**/
            string PostBackMessage  = TempData["PostBackMessage"] as string;
            string Approvers        = TempData["Approvers"] as string;
            if(!String.IsNullOrEmpty(PostBackMessage)){
                ViewBag.PostBackMessage = "<script type='text/javascript'>alert(\""+ PostBackMessage +"\\n\\n"+ Approvers +"\");</script>";
            }
    
            this._UserName = Session["UserName"] as string ?? "";
       
            //now get the pending items
            if( _UserName == null || _UserName.Equals(String.Empty) ){
                ViewBag.ErrorMessage="You must be logged in to continue.";
                return View();
            }            

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = _UserName;

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery( staffADProfile );
            staffADProfile = activeDirectoryQuery.GetStaffProfile();
            if( staffADProfile==null ){
                ViewBag.ErrorMessage="Your profile is not properly setup on the system. Please contact InfoTech.";
                return View();
            }

            //**Appraisal**Initiator**Setup**\\
            //Resolve the --branchname --branchcode --department --deptcode --appperiod from Tb_TargetInitiators table
            /*
          staffADProfile = new LINQCalls().setInitiatorFields( staffADProfile );
          
          if( staffADProfile.branch_code==null ){
              ViewBag.ErrorMessage="Your profile is not properly setup for Target. Please contact Human Resources.";
              return View();
          }*/
            ViewBag.AppID=DataHandlers.APP_ID;
         //   ViewBag.StaffBranch = staffADProfile.branch_name + ( ( staffADProfile.branch_code.Equals(HOBRCODE) ) ? " | " + staffADProfile.hodeptcode : String.Empty );
            
            //Check if the approver has an existing entry for the AppraisalPeriod from the Database
            List<EntriesModel> entryDetails =  new List<EntriesModel>();
            entryDetails = LINQCalls.getPendingHRUpload( staffADProfile );

            string filter = TempData["FilterBy"] as string;
            if (!String.IsNullOrEmpty(filter)) {
                entryDetails = FilterHRUploadList(entryDetails,filter);
            }
            Session["staffADProfile"] = staffADProfile;
            return View( entryDetails );
        }

        [HttpPost]
        [RBAC]
        public ActionResult FilterHRUpload( string FilterBy , FormCollection form , string[] WorkflowID , string TargetAction ) {
            
            switch ( TargetAction ){
                case "Search":
                    TempData["FilterBy"] = FilterBy;
                    return RedirectToAction( "HRUpload" ,new { UserName = Session["UserName"] as string } );
                
                case "Upload":

                    /*StaffADProfile staffADProfile = new StaffADProfile();
                    staffADProfile = Session["staffADProfile"] as StaffADProfile;


                    string _retVal = string.Empty;
                    foreach (string workflowid in WorkflowID) {
                        if ( workflowid.Length > 0 ) {
                            staffADProfile.user_logon_name = User.Identity.Name;
                            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
                            staffADProfile = activeDirectoryQuery.GetStaffProfile();

                            staffADProfile.branch_code = LINQCalls.getEntryProfile(workflowid).branch_code;
                            staffADProfile.branch_name = LINQCalls.getEntryProfile(workflowid).branch_name;
                            //staffADProfile.appperiod = "20150712";  
  
                            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                            System.Configuration.KeyValueConfigurationElement CurrentAppraisalPeriod = rootWebConfig.AppSettings.Settings["CurrentAppraisalPeriod"];            
                            staffADProfile.appperiod = CurrentAppraisalPeriod.Value.ToString();

                            _retVal = new AppDatabase().inputTargetEntriesHRUpload( workflowid , staffADProfile , "AppraisalDbConnectionString" , "Submitted" );
                            if( _retVal != null ){
                                TempData["UploadComplete"]  = "false";
                                TempData["PostBackMessage"] = _retVal;                   
                            } else {
                                TempData["PostBackMessage"] = UPLOADEDMSG;
                                TempData["Approvers"] = "";                        
                            }
                        }
                    }
                    Debug.WriteLine(_retVal);*/
                    break;
                    //return RedirectToAction( "AwaitingMyApproval","AwaitingApproval",new { UserName = Session["UserName"] as string } );
            }
            return RedirectToAction( "AwaitingMyApproval","AwaitingApproval",new { UserName = Session["UserName"] as string } );       
        }

        private List<EntriesModel> FilterHRUploadList( List<EntriesModel> bhList , string FilterBy ) {
                FilterBy = FilterBy.ToUpper();
                bhList = bhList.Where(  c => c.Branch.ToUpper().Contains(FilterBy)              || 
                                             c.StaffNumber.ToUpper().Contains(FilterBy)         || 
                                             c.StaffName.ToUpper().Contains(FilterBy)           || 
                                             c.AppraisalPeriodName.ToUpper().Contains(FilterBy) || 
                                             c.UnitName.ToUpper().Contains(FilterBy)            || 
                                             c.StaffName.ToUpper().Contains(FilterBy)           || 
                                             c.GroupName.ToUpper().Contains(FilterBy)           || 
                                             c.SuperGroupName.ToUpper().Contains(FilterBy))
                                             .ToList();
            return bhList;
        }
        public class SelectListItemHelper {
            public static SelectList GetBranches(){                
                return LINQCalls.getBranches();
            }
            public static SelectList GetDepts( string branchcode) {
                //branchcode  = "001";

                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement HeadOfficeDepartments = rootWebConfig.AppSettings.Settings["HeadOfficeDepartments"];            
                
                //string deptcodes   = "118,225,224,117,180,474,1473,166,658";
                string deptcodes   = HeadOfficeDepartments.Value.ToString();
                return LINQCalls.getHODepts( branchcode , deptcodes );
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class MultipleButtonAttribute : ActionNameSelectorAttribute {
            public string Name { get; set; }
            public string Argument { get; set; }

            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo) {
                var isValidName = false;
                var keyValue = string.Format("{0}:{1}", Name, Argument);
                var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

                if (value != null) {
                    controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                    isValidName = true;
                }
                return isValidName;
            }
        }
    }
}