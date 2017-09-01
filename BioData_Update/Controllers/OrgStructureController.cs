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
    public class OrgStructureController : Controller {
        // GET: OrgStructure

        private static string HOBCODE = "001";
        private static string ABJCODE = "013";
        private static string BRACODE = "000";

        private string _UserName ="";

        [Authorize]
        public ActionResult ViewStructure() {

            Session["UserName"] = @User.Identity.Name;
            //Session["UserName"] = "wunmi.ogunbiyi";
            this._UserName = Session["UserName"] as string ?? "";

            if( _UserName == null || _UserName.Equals(String.Empty)  ){
                ViewBag.ErrorMessage="You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval","AwaitingApproval");
            }

            ViewStructure viewStructure = new ViewStructure{ 
                BranchName      = SelectListItemHelper.GetBranches()
            };

            if( TempData["viewStructure"]!=null ){
                viewStructure = TempData["viewStructure"] as ViewStructure; 
                viewStructure.BranchName = SelectListItemHelper.GetBranches();
            }
            
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as String ?? String.Empty;
            
            return View( viewStructure );
        }

        [HttpPost]
        [Authorize]
        public ActionResult ViewStructure( ViewStructure viewStructure ) {

            //Now let's get the Approvers for the Selected branch
            IEnumerable<IEnumerable<AppraisalApproverModel>> branchStructure = LINQCalls.getOrgStructure( viewStructure.SelectedBranch ); 
            
            branchStructure = branchStructure.OrderByDescending(c => c.First().RoleID);

            viewStructure.AppraisalApproverModel = branchStructure;
            TempData["viewStructure"] = viewStructure;
            return RedirectToAction( "ViewStructure","OrgStructure" );
        }


        [HttpGet]
        [RBAC]
        public ActionResult SetupAppraisalApprovers() {  
    
            AppraisalApproverModel appraisalApproverModel = new AppraisalApproverModel();
            string deptcode = BRACODE;
            string origdeptcode="";
            ViewBag.hasdata="false";
            
            if (TempData["appraisalApproverModel"] as AppraisalApproverModel  != null) {
                appraisalApproverModel = TempData["appraisalApproverModel"] as AppraisalApproverModel;
                deptcode = String.IsNullOrEmpty(appraisalApproverModel.DeptCode) ? deptcode : appraisalApproverModel.DeptCode ;
                origdeptcode = appraisalApproverModel.DeptCode;
                int j;
                if ( !Int32.TryParse(deptcode, out j) ){
                    deptcode = HOBCODE;
                } else {
                    deptcode = ( deptcode.Equals(ABJCODE) ) ? ABJCODE : BRACODE;
                }
                ViewBag.hasdata="true";
            }

            //appraisalApproverModel.DeptName = SelectListItemHelper.GetDepts();
            //appraisalApproverModel.UnitName = SelectListItemHelper.GetUnits( deptcode );
            /*int k;
            if ( Int32.TryParse(deptcode, out k) ){
                deptcode = ( deptcode.Equals(ABJCODE) ) ? ABJCODE : BRACODE;
            }else {
                deptcode = HOBCODE;
            }*/
            origdeptcode=String.IsNullOrEmpty(origdeptcode)?deptcode:origdeptcode;
            appraisalApproverModel.Role     = SelectListItemHelper.GetRoles( deptcode );     
            
            if( !String.IsNullOrEmpty(TempData["ErrorMessage"] as string) )  {  
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }
            return View( appraisalApproverModel );
        }

        [HttpPost]
        [RBAC]
        [ValidateAntiForgeryToken]
        public ActionResult SetupAppraisalApprovers( AppraisalApproverModel appraisalApproverModel ) {  

            appraisalApproverModel.EntryKey = getEntryKey(appraisalApproverModel);

            /*bool duplicateEntry = LINQCalls.checkDupApproverSetup( appraisalApproverModel.EntryKey.ToUpper() );
            if (!duplicateEntry) {
                TempData["ErrorMessage"] = "Error : The staff : "+ appraisalApproverModel.StaffName +" has an existing setup with the same identity";
            }*/

            HRProfile hrprofile = LINQCalls.hrprofile(appraisalApproverModel.HRStaffName,1);   
            if( hrprofile==null ){
                TempData["ErrorMessage"] = "Error : You staff profile is not properly setup";
            }
            
            //Setup the branch
            int inputMode = 0;
            string retVal = new AppDatabase().insertApproverSetup( appraisalApproverModel , hrprofile , inputMode  ,"AppraisalDbConnectionString" );
            if( !String.IsNullOrEmpty(retVal) && !retVal.Split('|')[0].Equals("0")){
                TempData["ErrorMessage"] = "Error :"+retVal.Split('|')[1];
            } else {
                appraisalApproverModel = null;
            }

            TempData["appraisalApproverModel"] = appraisalApproverModel;
            return RedirectToAction( "SetupAppraisalApprovers" );
        }
        private string getEntryKey(AppraisalApproverModel ap) {
            return ap.StaffNumber+"_"+ap.RoleID+"_"+ap.UnitCode+"_"+ap.DeptCode+"_"+ap.GroupCode+"_"+ap.SuperGroupCode;
        }


        /***THIS IS FOR BULK APPROVER SETUP***/
        [HttpGet]
        [RBAC]
        public ActionResult AppraisalApproverBulkSetupForm( int? ActionState ) {

            SuperApproverBulkSetupModel superApproverBulkSetupModel;

            if( ActionState!=null && ActionState==0 ){
                AppraisalApproverModel aam      = new AppraisalApproverModel(); 
                AppraisalApproverBulkModel aabm = new AppraisalApproverBulkModel();
                List<ApproverExcelModel> sem    = new List<ApproverExcelModel>();
                
                superApproverBulkSetupModel                             = new SuperApproverBulkSetupModel();
                superApproverBulkSetupModel.AppraisalApproverBulkModel  = aabm;
                superApproverBulkSetupModel.ApproverExcelModel          = sem;
                superApproverBulkSetupModel.AppraisalApproverModel      = aam;

                return View( superApproverBulkSetupModel );
            } 

            if ( TempData[ "superApproverBulkSetupModel" ]!=null ){   
                superApproverBulkSetupModel = TempData["superApproverBulkSetupModel"] as SuperApproverBulkSetupModel;
            } else {

                if( ViewBag.HasGrid!=null ){

                    superApproverBulkSetupModel = TempData["superApproverBulkSetupModel"] as SuperApproverBulkSetupModel;

                } else {
                    AppraisalApproverBulkModel appraisalApproverBulkModel = new AppraisalApproverBulkModel();
                    List<ApproverExcelModel> approverExcelModel = new List<ApproverExcelModel>();

                    superApproverBulkSetupModel = new SuperApproverBulkSetupModel();
                    superApproverBulkSetupModel.AppraisalApproverBulkModel  = appraisalApproverBulkModel;
                    superApproverBulkSetupModel.ApproverExcelModel          = approverExcelModel;
                }               
            }

            String ErrorMessage =  TempData["ErrorMessage"] as String;
            if ( ErrorMessage != null ) {
                ViewBag.ErrorMessage = ErrorMessage;
            }
            
            //superApproverBulkSetupModel.AppraisalApproverModel  = aam;
            return View( superApproverBulkSetupModel );
        }

        [HttpPost]
        [RBAC]
        [MultipleButton(Name = "action", Argument = "Upload")]
        public ActionResult AppraisalApproverBulkSetupForm( SuperApproverBulkSetupModel superApproverBulkSetupModel ) {

            HRProfile hrprofile = LINQCalls.hrprofile( Session["UserName"] as String , 1 );   
            if( hrprofile==null ){
                TempData["ErrorMessage"] = "Error : You staff profile is not properly setup";
                TempData["superApproverBulkSetupModel"] = superApproverBulkSetupModel;
                return RedirectToAction("AppraisalApproverBulkSetupForm");  
            }

            //string periodSelectedValue = Request.Form["BHSingleSetupModel.SelectedAppraisalPeriod"];
            HttpPostedFileBase uploadedExcelFile = superApproverBulkSetupModel.AppraisalApproverBulkModel.UploadedExcelFile;

            AppraisalApproverBulkModel sppraisalApproverBulkModel = superApproverBulkSetupModel.AppraisalApproverBulkModel;
            List<ApproverExcelModel> approverExcelModel   = GetDataTableFromSpreadsheet(superApproverBulkSetupModel.AppraisalApproverBulkModel.UploadedExcelFile.InputStream,false,hrprofile);

            superApproverBulkSetupModel.ApproverExcelModel     = approverExcelModel;            
            //TempData[ "periodSelectedValue" ] = periodSelectedValue;
            TempData[ "superApproverBulkSetupModel" ] = superApproverBulkSetupModel;
            return RedirectToAction( "AppraisalApproverBulkSetupForm" );
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
                    Debug.WriteLine(row[prop.Name]);
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public static List<ApproverExcelModel> GetDataTableFromSpreadsheet( Stream MyExcelStream , bool ReadOnly , HRProfile hRProfile) {
            
            List<ApproverExcelModel> dt = new List<ApproverExcelModel>();
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

                ApproverExcelModel approverExcelModel;
                Debug.WriteLine("rows length = "+ rows.Count());

                foreach (Row row in rows)  {

                    approverExcelModel = new ApproverExcelModel();
                    //approverExcelModel.entrykey = ( int )row.RowIndex.Value;
                    
                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++) {           
                        Debug.WriteLine( "i = " + i );
                        //approverExcelModel = new ApproverExcelModel();
                        switch ( i ) {
                            case 0:     //approvername
                                approverExcelModel.approvername=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 1:     //approverid
                                approverExcelModel.approverid=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 2:     //role
                                approverExcelModel.role=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 3:     //roleid
                                approverExcelModel.roleid = int.Parse( GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i)) );
                                break;
                            case 4:     //unitcode
                                approverExcelModel.unitcode=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 5:     //unitname
                                approverExcelModel.unitname=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 6:     //deptcode
                                approverExcelModel.deptcode=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 7:     //deptname
                                approverExcelModel.deptname = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 8:     //groupcode
                                approverExcelModel.groupcode=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 9:     //groupname
                                approverExcelModel.groupname=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 10:    //supergroupcode
                                approverExcelModel.supergroupcode=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 11:    //supergroupname
                                approverExcelModel.supergroupname=GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                break;
                            case 12:     //comments
                                approverExcelModel.comments = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                                //approverExcelModel.HRProfile = hRProfile;
                                approverExcelModel.hrstaffname      = hRProfile.name;
                                approverExcelModel.hrstaffnumber    = hRProfile.employee_number;
                                approverExcelModel.entrykey         = approverExcelModel.approverid+"_"+approverExcelModel.roleid.ToString()+"_"+approverExcelModel.unitcode+approverExcelModel.deptcode+"_"+approverExcelModel.groupcode+"_"+approverExcelModel.supergroupcode;

                                dt.Add( approverExcelModel );
                                
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

        /***END THIS IS FOR BULK APPROVER SETUP***/


        [RBAC]
        public ActionResult ViewAppraisalApprovers( string FilterBy="" ) {

            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
            System.Configuration.KeyValueConfigurationElement CurrentAppraisalPeriod = rootWebConfig.AppSettings.Settings["CurrentAppraisalPeriod"];

            List<AppraisalApproverModel> appraisalApproverModel = LINQCalls.getApproverSetupList();
            if( !String.IsNullOrEmpty(FilterBy) ){
                appraisalApproverModel = FilterAppraisalApproverList( appraisalApproverModel , FilterBy.ToUpper() );
            }
            return View( appraisalApproverModel );
        }

        [RBAC]
        [HttpPost]
        public ActionResult FilterApprovers( string FilterBy = "" ) {
            return RedirectToAction( "ViewAppraisalApprovers" , new {FilterBy=FilterBy} );
        }

        private List<AppraisalApproverModel> FilterAppraisalApproverList( List<AppraisalApproverModel> bhList , string FilterBy ) {
                FilterBy = FilterBy.ToUpper();
                bhList = bhList.Where(  c => c.DeptTitle.ToUpper().Contains(FilterBy)     || 
                                             c.GroupTitle.ToUpper().Contains(FilterBy)    ||
                                             c.StaffName.ToUpper().Contains(FilterBy)     || 
                                             c.StaffNumber.ToUpper().Contains(FilterBy)   ||
                                             c.RoleTitle.ToUpper().Contains(FilterBy)     || 
                                             c.UnitTitle.ToUpper().Contains(FilterBy))
                                             .ToList();
            return bhList;
        }

        [HttpGet]
        [RBAC]
        public ActionResult ApproverSetupEdit( string EntryKey, string Func ) {  

            AppraisalApproverModel appraisalApproverModel = new AppraisalApproverModel();

            if (String.IsNullOrEmpty(EntryKey) || String.IsNullOrEmpty(Func) ) {
                TempData["ErrorMessage"] = "Error : Please access the page properly.";
            } else {
                if( Func.Equals("Edit") ){
                    appraisalApproverModel = LINQCalls.getApproverSetupEntry(EntryKey);
                } else {
                    string retVal = new AppDatabase().deleteApproverSetup( EntryKey ,"AppraisalDbConnectionString" );
                    if( !String.IsNullOrEmpty(retVal) && !retVal.Split('|')[0].Equals("0")){
                        TempData["ErrorMessage"] = "Error : "+retVal.Split('|')[1];
                    } else {
                        return RedirectToAction( "ViewAppraisalApprovers" );
                    }
                }                
            }
            TempData["appraisalApproverModel"] = appraisalApproverModel;
            return RedirectToAction( "SetupAppraisalApprovers" );
        }
        
        //Setup HR Roles
        [HttpGet]
        [RBAC]
        public ActionResult HRRoleSetup() {  
    
            AppraisalApproverModel appraisalApproverModel = new AppraisalApproverModel();
            ViewBag.hasdata = "false";
            
            if (TempData["appraisalApproverModel"] as AppraisalApproverModel  != null) {
                appraisalApproverModel = TempData["appraisalApproverModel"] as AppraisalApproverModel;
                ViewBag.hasdata = "true";
            }
            
            appraisalApproverModel.Role = SelectListItemHelper.GetHRRoles(); 
            appraisalApproverModel.StatusName = SelectListItemHelper.GetHRStatus();

            if( !String.IsNullOrEmpty(TempData["ErrorMessage"] as string) )  {  
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            return View( appraisalApproverModel );
        }
        
        [HttpPost]
        [RBAC]
        public ActionResult HRRoleSetup( AppraisalApproverModel appraisalApproverModel ) {  
    
            appraisalApproverModel.EntryKey = (String.IsNullOrEmpty(appraisalApproverModel.EntryKey)) ? getHREntryKey(appraisalApproverModel) : appraisalApproverModel.EntryKey ;

            HRProfile hrprofile = LINQCalls.hrprofile(appraisalApproverModel.HRStaffName,1);   
            if( hrprofile==null ){
                TempData["ErrorMessage"] = "Error : You staff profile is not properly setup";
            }

            //Get the staff's username from the staff number
            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(appraisalApproverModel.StaffNumber );
            string _username = activeDirectoryQuery.GetUserName();
            if( _username==null ){
                ViewBag.ErrorMessage="The user's profile is not properly setup on the system. Please contact InfoTech.";
                return View();
            }

            appraisalApproverModel.UserName = _username;
            
            //Setup the staff
            string retVal = new AppDatabase().insertRoleSetup( appraisalApproverModel , hrprofile  ,"AppraisalDbConnectionString" );
            if( !String.IsNullOrEmpty(retVal) && !retVal.Split('|')[0].Equals("0")){
                TempData["ErrorMessage"] = "Error :"+retVal.Split('|')[1];
            } else {
                appraisalApproverModel = null;
            }

            TempData["appraisalApproverModel"] = appraisalApproverModel;
            return RedirectToAction( "HRRoleSetup" );
        }
        private string getHREntryKey(AppraisalApproverModel ap) {
            return ap.StaffNumber+"_"+ap.RoleID;
        }

        [HttpGet]
        [RBAC]
        public ActionResult ViewHRUsers() {
            List<AppraisalApproverModel> appraisalApproverModel = LINQCalls.getHRUsers();
            return View( appraisalApproverModel );
        }
        
        [HttpGet]
        [RBAC]
        public ActionResult RoleEdit( string EntryKey, string Func ) {  

            AppraisalApproverModel appraisalApproverModel = new AppraisalApproverModel();

            if (String.IsNullOrEmpty(EntryKey) || String.IsNullOrEmpty(Func) ) {
                TempData["ErrorMessage"] = "Error : Please access the page properly.";
            } else {
                if( Func.Equals("Edit") ){
                    appraisalApproverModel = LINQCalls.getRoleSetupEntry(EntryKey);
                } else {
                    string retVal = new AppDatabase().deleteRoleSetup( EntryKey ,"AppraisalDbConnectionString" );
                    if( !String.IsNullOrEmpty(retVal) && !retVal.Split('|')[0].Equals("0")){
                        TempData["ErrorMessage"] = "Error : "+retVal.Split('|')[1];
                    } else {
                        return RedirectToAction( "ViewHRUsers" );
                    }
                }                
            }
            TempData["appraisalApproverModel"] = appraisalApproverModel;
            return RedirectToAction( "HRRoleSetup" );
        }
        /**

        [HttpGet]
        public ActionResult BHSingleSetupFormEdit( string StaffNumber , string SelectedAppraisalPeriod ) {
            
            if (String.IsNullOrEmpty(StaffNumber) || String.IsNullOrEmpty(SelectedAppraisalPeriod) ) {
                return RedirectToAction( "BHSingleSetupForm" );
            }
            BHSingleSetupEditModel bHSingleSetupModel = LINQCalls.bHSingleSetupModel(StaffNumber,SelectedAppraisalPeriod);
            return View( bHSingleSetupModel );
        }

        [HttpPost]
        public ActionResult BHSingleSetupFormEdit( BHSingleSetupEditModel bHSingleSetupEditModel ) {
            
            if (!ModelState.IsValid) {
                TempData["ErrorMessage"] = "Invalid model ";
                TempData["bHSingleSetupEditModel"] = bHSingleSetupEditModel;
                return RedirectToAction("BHSingleSetupFormEdit");  
            }

            HRProfile hrprofile = LINQCalls.hrprofile(bHSingleSetupEditModel.InitiatorLoginName,1);   
            if( hrprofile==null ){
                TempData["ErrorMessage"] = "Error : You staff profile is not properly setup";
                TempData["bHSingleSetupEditModel"] = bHSingleSetupEditModel;
                return RedirectToAction("BHSingleSetupFormEdit");  
            }

            int inputMode = 1;
            int retVal = new AppDatabase().insertSingleSetup( bHSingleSetupEditModel , hrprofile , inputMode  ,"AppraisalDbConnectionString" );

            return RedirectToAction("ViewBranchInitiators");  
        }

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

        public ActionResult ViewAppriasalApprovers() {
            return View();
        }
        */
        
        [RBAC]
        public ActionResult GetUnitsAndRoles( string deptcode,string _type ) {
            
            string _deptcode = deptcode;
            int j;
            if ( Int32.TryParse(deptcode, out j) ){
                deptcode = ( deptcode.Equals(ABJCODE) ) ? ABJCODE : BRACODE;
            }else {
                deptcode = HOBCODE;
            }
            if( _type.Equals("unit") ){
                //return GetUnitsForDept( deptcode , _deptcode );
                return GetRolesForDept(deptcode);
            }else{
                return GetRolesForDept(deptcode);
            }
        }
        
        /*[RBAC]
        public ActionResult GetUnitsForDept( string deptcode , string _deptcode ) {
            var units_ = LINQCalls.getUnitsAsJSON( deptcode , _deptcode );    
            return Json( units_ , JsonRequestBehavior.AllowGet );
        }*/

        [RBAC]
        public ActionResult GetRolesForDept( string deptcode) {
            var units_ = LINQCalls.getRolesAsJSON(deptcode );    
            return Json( units_ , JsonRequestBehavior.AllowGet );
        }
        public class SelectListItemHelper {
            public static SelectList GetRoles( string deptcode ) {
                return LINQCalls.getRoles( deptcode );
            }
            /*public static SelectList GetUnits( string deptcode ) {
                return LINQCalls.getUnits( deptcode );
            }
            public static SelectList GetDepts() {
                return LINQCalls.getDepts();
            }*/

            internal static SelectList GetHRRoles() {
                return LINQCalls.getHRRoles();
            }

            internal static SelectList GetHRStatus() {
                return new SelectList(new[] { "Enabled", "Disabled" }
                .Select(x => new {value = x, text = x}),"value", "text");
            }

            public static SelectList GetBranches(){                
                return LINQCalls.getBranches();
            }
            public static SelectList GetDepts( string branchcode) {
                //branchcode  = "001";
                string deptcodes   = "118,225,224,117,180,474,1473,166";
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

