using BioData_Update.App_Code;
using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace BioData_Update.Controllers {
    public class PassportInputController : Controller {

        private const string INIT_STAGE = "Passport Submission";
        private const string HR_UPLOAD = "HR Approval";
        private const string DENIED = "Denied";
        private const string APPROVED = "Approved";
         
        private const string SUBMIT_STATUS = "Submitted";
        private const string DENIED_STATUS = DENIED;

        private const string SEX_MALE   = "Male";
        private const string SEX_FEMALE = "Female";

        private string SUMBMITTEDMSG = "You have successfully submitted your request for possible approval by:";
        private string DENIEDMSG = "You have successfully denied this entry for review by:";

        private string CANSAVE = "true";

        private string _UserName = "";


        // GET: PassportInput
        [HttpGet]
        [Authorize]
        public ActionResult PassportInputForm()
        {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? "";

            if (_UserName == null || _UserName.Equals(String.Empty)) {
                ViewBag.ErrorMessage = "You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            SuperPassportModel superPassportModel = new SuperPassportModel();

            StaffADProfile staffADProfile = new StaffADProfile();
            if ( TempData["superPassportModel"] != null ) {

                superPassportModel = TempData["superPassportModel"] as SuperPassportModel;
                staffADProfile = superPassportModel.StaffADProfile;
                ViewBag.hasdata="true";

                //superPassportModel.PassportDetails.First().Nationality = SelectListItemHelper.GetCountries();
            } else {

                //now resolve the user profile from AD and Xceed
                staffADProfile.user_logon_name = _UserName;

                //AD
                ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
                staffADProfile = activeDirectoryQuery.GetStaffProfile();
                if (staffADProfile == null) {
                    ViewBag.ErrorMessage = "Your profile is not properly setup on the system. Please contact InfoTech.";
                    return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
                }

                //EXCEED
                staffADProfile = LINQCalls.getXceedProfile( staffADProfile.employee_number );
                staffADProfile.doe = staffADProfile.doe;
                staffADProfile.dob = staffADProfile.dob;

                string sex = (staffADProfile.gender.Equals(1)) ? SEX_FEMALE: SEX_MALE;
                
                List<RequestDetails> requestDetails = new List<RequestDetails>();
                List<PassportDetails> passportDetails = new List<PassportDetails>();

                requestDetails = LINQCalls.getExistingPassportEntry(staffADProfile);

                string workflowid = Guid.NewGuid().ToString().ToUpper() ;
                int requeststageid = 0;
                string requeststage = INIT_STAGE;
                DateTime requestdate = new DateTime();
                string cansave = this.CANSAVE;

                superPassportModel = new SuperPassportModel {
                    WorkflowID = workflowid,
                    RequestStageID = requeststageid,
                    RequestStage = requeststage,
                    RequestDate = requestdate,
                    CanSave = cansave,
                    StaffADProfile = staffADProfile,
                    RequestDetails = requestDetails,
                    EntryModel = null,
                    RequestBranch = staffADProfile.branch_name,
                    RequestBranchCode = staffADProfile.branch_code,
                    PassportDetails = passportDetails
                };
            }

            staffADProfile.NumberOfCountries    = SelectListItemHelper.GetNumberCountries();
            superPassportModel.StaffADProfile   = staffADProfile;

            if (TempData["ErrorMessage"] != null) {
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            return View( superPassportModel );
        }

        [HttpPost]
        [Authorize]
        public ActionResult PassportInputForm( SuperPassportModel superPassportModel, string PassportAction ) {

            List<RequestDetails> requestDetails = superPassportModel.RequestDetails;

            IEnumerable<RequestDetails> requestdetails = superPassportModel.RequestDetails;
            string retVal = "";

            switch (PassportAction ) {

                case "CountryCountRefresh":
                    
                    //Form is submitted when the user selects the number of countries
                    List<PassportDetails> passportDetails =  new List<PassportDetails>();
                    string sex = (superPassportModel.StaffADProfile.gender.Equals(1)) ? SEX_FEMALE: SEX_MALE;

                    for ( int i=0; i<superPassportModel.StaffADProfile.countryCount;i++ ) {

                        PassportDetails pDetails = new PassportDetails();
                        pDetails.entry_key = "";
                        pDetails.passportAuthority="";
                        pDetails.passportDateOfExpiry="";
                        pDetails.passportDateOfIssue ="";
                        pDetails.passportNationality = "";
                        pDetails.passportNumber ="";
                        pDetails.passportOtherNames="";
                        pDetails.passportPlaceOfBirth="";
                        pDetails.passportSex = sex;
                        pDetails.passportSurname="";
                        pDetails.passportType="P";
                        pDetails.passportDateOfBirth = superPassportModel.StaffADProfile.dob;
                        pDetails.Nationality = SelectListItemHelper.GetCountries();
                        passportDetails.Add(pDetails);
                    }

                    superPassportModel.PassportDetails = passportDetails;
                    break;
                case "Deny":
                    // Good. let's send this f@*ker back
                    //requestDetails.Select(c => { c.entry_key = c.employee_number + "_" + superPassportModel.StaffADProfile.appperiod + "_" + superPassportModel.StaffADProfile.branch_code; return c; }).ToList();
                    requestDetails.Select(c => { c.workflowid = superPassportModel.WorkflowID; return c; }).ToList();

                    // SAVE the value to the DATABASE
                    requestdetails = superPassportModel.RequestDetails;
                    //dataTable = DataHandlers.ToDataTable(requestdetails);

                    //retVal = new AppDatabase().inputPassportEntries(dataTable, superPassportModel, "AppraisalDbConnectionString", DENIED_STATUS);

                    if (retVal != null) {
                        TempData["UploadComplete"] = "false";
                        TempData["ErrorMessage"] = retVal;
                        TempData["superPassportModel"] = superPassportModel;
                    } else {
                        //String.format(SUMBMITTEDMSG)--add the approvers
                        var approvers = LINQCalls.getApproverNames(superPassportModel.WorkflowID, -1);
                        TempData["PostBackMessage"] = DENIEDMSG;
                        TempData["Approvers"] = string.Join("\\n", approvers.ToArray());
                        return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
                    }
                    break;
                case "Submit":
                    if( ModelState.IsValid )  {

                        if (!checkVlaidFileTypes(superPassportModel.PassportDetails) ) {
                            ViewBag.ErrorMessage = "Invalid file type. Only \"pdf\" files are supported.";
                            //TempData["superPassportModel"] = superPassportModel;
                        } else {

                            IEnumerable<PassportDetails> _passportDetails           = superPassportModel.PassportDetails;
                            _passportDetails.Select(c => { c.entry_key              = superPassportModel.StaffADProfile.employee_number + "_" + c.passportNumber.ToUpper(); return c; }).ToList();
                            _passportDetails.Select(c => { c.passportFileName       = c.passportUpload.FileName; return c; }).ToList();

                            _passportDetails.Select(c => { c.passportNumber         = c.passportNumber.ToUpper();       return c; }).ToList();
                            _passportDetails.Select(c => { c.passportSurname        = c.passportSurname.ToUpper();      return c; }).ToList();
                            _passportDetails.Select(c => { c.passportOtherNames     = c.passportOtherNames.ToUpper();   return c; }).ToList();
                            _passportDetails.Select(c => { c.passportPlaceOfBirth   = c.passportPlaceOfBirth.ToUpper(); return c; }).ToList();
                            _passportDetails.Select(c => { c.passportAuthority      = c.passportAuthority.ToUpper();    return c; }).ToList();

                            _passportDetails.Select(c => { c.passportContentType    = c.passportUpload.ContentType; return c; }).ToList();
                            _passportDetails.Select(c => {
                                                            Stream fs               = c.passportUpload.InputStream;
                                                            BinaryReader br         = new BinaryReader(fs);
                                                            byte[] bytes            = br.ReadBytes((Int32)fs.Length);
                                                            c.passportFileBytes     = bytes;
                                                            return c;
                            }).ToList();

                            DataTable dataTable = new DataTable();
                            dataTable = DataHandlers.ToDataTable( _passportDetails );
                        
                            string _retVal = new AppDatabase().inputPassportEntries(dataTable, superPassportModel, "AppraisalDbConnectionString", SUBMIT_STATUS);

                            if (_retVal != null) {
                                TempData["UploadComplete"] = "false";
                                TempData["ErrorMessage"] = _retVal;
                                TempData["superPassportModel"] = superPassportModel;
                            } else {
                                int newstageid = 0;
                                switch (superPassportModel.RequestStageID)
                                {
                                    case 0:
                                        newstageid = 3;
                                        break;
                                    case -1:
                                        newstageid = 3;
                                        break;
                                    case 3:
                                        newstageid = 20;
                                        break;
                                    case 20:
                                        newstageid = 100;
                                        break;
                                }

                                var approvers = LINQCalls.getHRApproverNames(superPassportModel.WorkflowID, newstageid);
                                TempData["PostBackMessage"] = SUMBMITTEDMSG;
                                TempData["Approvers"] = string.Join("\\n", approvers.ToArray());
                                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
                            }
                        }
                    }
                    break;
                }

            //superPassportModel.RequestDetails = requestDetails;
            TempData["superPassportModel"] = superPassportModel;
            return RedirectToAction("PassportInputForm");
        }

        private bool checkVlaidFileTypes(List<PassportDetails> passportDetails) {
            
            foreach (var pDetail in passportDetails) {
                if (pDetail.passportUpload.ContentType != "application/pdf") {
                    return false;
                }
            } 

            return true;
        }

        //This handles when the user has to edit after request is denied
        [Authorize]
        public ActionResult EditPassport(string WorkflowID, bool editMode, bool? myEntries) {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? "";

            if (_UserName == null || _UserName.Equals(String.Empty)) {
                ViewBag.ErrorMessage = "You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = _UserName;

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
            staffADProfile              = activeDirectoryQuery.GetStaffProfile();
            staffADProfile.branch_code  = LINQCalls.getEntryProfile( WorkflowID ).branch_code;
            staffADProfile.branch_name  = LINQCalls.getEntryProfile( WorkflowID ).branch_name;

            if (staffADProfile == null) {
                ViewBag.ErrorMessage = "Your profile is not properly setup on the system. Please contact InfoTech.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            //Get the request identified by the workflow id
            List<RequestDetails> requestDetails = new List<RequestDetails>();
            if (myEntries != null && myEntries == true) {
                string entrykey = staffADProfile.employee_number + "_" + staffADProfile.branch_code;
                requestDetails = LINQCalls.getExistingPassportEntry(WorkflowID, staffADProfile.employee_number, entrykey);
            } else {
                requestDetails = LINQCalls.getExistingPassportEntry(WorkflowID, staffADProfile.employee_number);
            }

            EntryModel entryModel   = new EntryModel();
            entryModel              = LINQCalls.getWorkflowEntry(WorkflowID);
            ViewBag.StaffBranch     = entryModel.Branch;
            int requeststageid      = entryModel.RequestStageId;
            string requeststage     = entryModel.RequestStage;
            DateTime requestdate    = entryModel.DateSubmitted;
            string initiatornumber  = LINQCalls.getInitiatorNumber(WorkflowID) ?? staffADProfile.employee_number;

            string cansave = (requeststage.Equals(INIT_STAGE) || requeststage.Equals(DENIED)) && initiatornumber.Equals(staffADProfile.employee_number)
                                            ? "true" : "false";

            staffADProfile.appperiod = entryModel.AppraisalPeriod;

            XElement ApprovalHistory = LINQCalls.getApprovalHistory(WorkflowID);
            XDocument xDocument = DataHandlers.ToXDocument(ApprovalHistory);

            List<ApprovalDetails> approvalHistory = xDocument.Descendants("Approvals").Select(det => new ApprovalDetails
                {
                    ApproverNames = det.Element("ApproverName").Value,
                    ApproverStaffNumbers = det.Element("ApproverStaffNumber").Value,
                    ApprovedStages = det.Element("ApprovedStage").Value,
                    ApproverAction = det.Element("ApproverAction").Value,
                    ApprovalDateTime = det.Element("ApprovalDateTime").Value
                })
                .ToList();

            SuperPassportModel superPassportModel = new SuperPassportModel();

            if (TempData["superPassportModel"] != null) {
                superPassportModel = TempData["superPassportModel"] as SuperPassportModel;
            } else {
                superPassportModel = new SuperPassportModel {
                    WorkflowID = WorkflowID,
                    RequestStageID = entryModel.RequestStageId,
                    RequestStage = entryModel.RequestStage,
                    RequestDate = entryModel.DateSubmitted,
                    StaffADProfile = staffADProfile,
                    RequestDetails = requestDetails,
                    CanSave = cansave,
                    ApprovalDetails = approvalHistory,
                    EntryModel = entryModel,
                    RequestBranch = entryModel.Branch,
                    RequestBranchCode = entryModel.BranchCode
                };
            }

            Session["requestDetails"] = superPassportModel.RequestDetails;
            TempData["editMode"] = (editMode == true) ? null : "false";

            if (TempData["ErrorMessage"] != null) {
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            TempData["superPassportModel"] = superPassportModel;
            return RedirectToAction("PassportInputForm");
        }

        //This handles normal vieweing of the request//the view defines methods for HR to apporve using @param: editMode
        [Authorize]
        public ActionResult ViewPassport(string WorkflowID, bool editMode, bool? myEntries, bool? viewMode) {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? "";

            if (_UserName == null || _UserName.Equals(String.Empty)) {
                ViewBag.ErrorMessage = "You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = _UserName;

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
            staffADProfile              = activeDirectoryQuery.GetStaffProfile();
            staffADProfile.branch_code  = LINQCalls.getEntryProfile( WorkflowID ).branch_code;
            staffADProfile.branch_name  = LINQCalls.getEntryProfile( WorkflowID ).branch_name;

            if (staffADProfile == null) {
                ViewBag.ErrorMessage = "Your profile is not properly setup on the system. Please contact InfoTech.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            //Get the request identified by the workflow id
            List<RequestDetails> requestDetails = new List<RequestDetails>();
            if (myEntries != null && myEntries == true) {
                string entrykey = staffADProfile.employee_number + "_" + staffADProfile.branch_code;
                requestDetails = LINQCalls.getExistingPassportEntry(WorkflowID, staffADProfile.employee_number, entrykey);
            } else {
                requestDetails = LINQCalls.getExistingPassportEntry(WorkflowID, staffADProfile.employee_number);
            }

            EntryModel entryModel   = new EntryModel();
            entryModel              = LINQCalls.getWorkflowEntry(WorkflowID);
            ViewBag.StaffBranch     = entryModel.Branch;
            int requeststageid      = entryModel.RequestStageId;
            string requeststage     = entryModel.RequestStage;
            DateTime requestdate    = entryModel.DateSubmitted;
            string initiatornumber  = LINQCalls.getInitiatorNumber(WorkflowID) ?? staffADProfile.employee_number;

            string cansave          = (requeststage.Equals(INIT_STAGE) || requeststage.Equals(DENIED)) && initiatornumber.Equals(staffADProfile.employee_number)
                                            ? "true" : "false";

            staffADProfile.appperiod = entryModel.AppraisalPeriod;

            XElement ApprovalHistory = LINQCalls.getApprovalHistory(WorkflowID);
            XDocument xDocument = DataHandlers.ToXDocument(ApprovalHistory);

            List<ApprovalDetails> approvalHistory = xDocument.Descendants("Approvals").Select(det => new ApprovalDetails
                {
                    ApproverNames = det.Element("ApproverName").Value,
                    ApproverStaffNumbers = det.Element("ApproverStaffNumber").Value,
                    ApprovedStages = det.Element("ApprovedStage").Value,
                    ApproverAction = det.Element("ApproverAction").Value,
                    ApprovalDateTime = det.Element("ApprovalDateTime").Value
                })
                .ToList();

            SuperPassportModel superPassportModel = new SuperPassportModel();

            if (TempData["superPassportModel"] != null) {
                superPassportModel = TempData["superPassportModel"] as SuperPassportModel;
            } else {
                superPassportModel = new SuperPassportModel {
                    WorkflowID = WorkflowID,
                    RequestStageID = entryModel.RequestStageId,
                    RequestStage = entryModel.RequestStage,
                    RequestDate = entryModel.DateSubmitted,
                    StaffADProfile = staffADProfile,
                    RequestDetails = requestDetails,
                    CanSave = cansave,
                    ApprovalDetails = approvalHistory,
                    EntryModel = entryModel,
                    RequestBranch = entryModel.Branch,
                    RequestBranchCode = entryModel.BranchCode
                };
            }

            Session["requestDetails"] = superPassportModel.RequestDetails;
            TempData["editMode"] = (editMode == true) ? null : "false";

            if (TempData["ErrorMessage"] != null) {
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            TempData["superPassportModel"] = superPassportModel;
            return RedirectToAction("PassportInputForm");
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class MultipleButtonAttribute : ActionNameSelectorAttribute {
            public string Name { get; set; }
            public string Argument { get; set; }

            public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
            {
                var isValidName = false;
                var keyValue = string.Format("{0}:{1}", Name, Argument);
                var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

                if (value != null)
                {
                    controllerContext.Controller.ControllerContext.RouteData.Values[Name] = Argument;
                    isValidName = true;
                }
                return isValidName;
            }


        }

        public class SelectListItemHelper {
            public static SelectList GetCountries() {
                return LINQCalls.getCountries();
            }
            public static SelectList GetNumberCountries() {

                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement maxCountries = rootWebConfig.AppSettings.Settings["maxCountries"];

                var list = new List<SelectListItem>();
                for (var i = 1; i < int.Parse(maxCountries.Value.ToString()); i++) {
                    list.Add(new SelectListItem { Text = i.ToString(), Value =    i.ToString() });
                }

                return new SelectList(list, "Text", "Value");
            }
            public static SelectList GetSexes() {

                System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
                System.Configuration.KeyValueConfigurationElement sexes = rootWebConfig.AppSettings.Settings["sexes"];

                var sexList = sexes.Value.ToString().Split(',').ToList();

                SelectList list = new SelectList(sexList);

                /*var list = new List<SelectListItem>();
                for (var i = 1; i <= sexList.Count; i++) {
                    list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }

                list = sexList.Select((r,index) => new SelectListItem{Text = r, Value = index.ToString()}).ToList<SelectList>();*/
                return list;
            }
        }
    }
}