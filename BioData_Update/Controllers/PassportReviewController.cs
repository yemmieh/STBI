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
    public class PassportReviewController : Controller {
        // GET: PassportReview

        private const string INIT_STAGE     = "Passport Submission";
        private const string HR_UPLOAD      = "HR Approval";
        private const string DENIED         = "Denied";
        private const string APPROVED       = "Approved";
         
        private const string SUBMIT_STATUS  = "Submitted";
        private const string DENIED_STATUS  = DENIED;

        private const string SEX_MALE       = "Male";
        private const string SEX_FEMALE     = "Female";

        private string SUBMITTEDMSG         = "You have successfully approved the request";
        private string DENIEDMSG            = "You have successfully denied this entry for review by:";

        private string CANSAVE              = "true";

        private string _UserName            = "";

        //This shows the approver or other viewer the completed form for approval or viewing
        [Authorize]
        public ActionResult PassportReviewForm(string WorkflowID, bool editMode, bool? myEntries) {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? String.Empty;

            if (_UserName == null || _UserName.Equals(String.Empty)) {
                ViewBag.ErrorMessage = "You must be logged in to continue.";
                return RedirectToAction("Login", "Login");
            }

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile   = new StaffADProfile();
            staffADProfile.user_logon_name  = _UserName;

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
            staffADProfile              = activeDirectoryQuery.GetStaffProfile();

            if (staffADProfile == null) {
                ViewBag.ErrorMessage = "Your profile is not properly setup on the system. Please contact InfoTech.";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            //Get the request identified by the workflow id and be sure the user is an approver
            List<RequestDetails> requestDetails = new List<RequestDetails>();
            requestDetails = LINQCalls.getExistingPassportEntry(WorkflowID, staffADProfile.employee_number);

            if ( requestDetails == null ) {
                ViewBag.ErrorMessage = "You are not authorized to process this request";
                return RedirectToAction("AwaitingMyApproval", "AwaitingApproval");
            }

            List<PassportDetails> passportDetails = new List<PassportDetails>();
            PassportDetails pDetails = LINQCalls.getPassportDetails( WorkflowID );
            passportDetails.Add(pDetails);

            EntryModel entryModel   = new EntryModel();
            entryModel              = LINQCalls.getWorkflowEntry(WorkflowID);
            entryModel.DOE          = LINQCalls.getXceedProfile(entryModel.StaffNumber).doe;

            ViewBag.StaffBranch     = entryModel.Branch;
            int requeststageid      = entryModel.RequestStageId;
            string requeststage     = entryModel.RequestStage;
            DateTime requestdate    = entryModel.DateSubmitted;
            string initiatornumber  = entryModel.StaffNumber; //LINQCalls.getInitiatorNumber(WorkflowID) ?? staffADProfile.employee_number;

            string cansave          = ( !requeststage.Equals(INIT_STAGE) && !requeststage.Equals(DENIED) && !requeststage.Equals(APPROVED) 
                                        && !initiatornumber.Equals(staffADProfile.employee_number) ) ? "true" : "false";

            XElement ApprovalHistory = LINQCalls.getApprovalHistory(WorkflowID);
            XDocument xDocument = DataHandlers.ToXDocument(ApprovalHistory);

            List<ApprovalDetails> approvalHistory = xDocument.Descendants("Approvals").Select(det => new ApprovalDetails
                {
                    ApproverNames           = det.Element("ApproverName").Value,
                    ApproverStaffNumbers    = det.Element("ApproverStaffNumber").Value,
                    ApprovedStages          = det.Element("ApprovedStage").Value,
                    ApproverAction          = det.Element("ApproverAction").Value,
                    ApprovalDateTime        = det.Element("ApprovalDateTime").Value,
                    ApproverComments        = det.Element("ApproverComment").Value
                }).ToList();

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
                    RequestBranchCode = entryModel.BranchCode,
                    PassportDetails = passportDetails
                };
            }

            Session["requestDetails"] = superPassportModel.RequestDetails;
            TempData["editMode"] = (editMode == true) ? "true" : "false";

            if (TempData["ErrorMessage"] != null) {
                ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            }

            Session["superPassportModel"] = superPassportModel;

            TempData["superPassportModel"] = superPassportModel;
            return View( superPassportModel );
        }

        [HttpPost]
        [Authorize]
        public ActionResult ReviewPassportForm( SuperPassportModel superPassportModel, string ReviewAction ) {

            ActionResult actionResult = null;

            superPassportModel = Session["superPassportModel"] as SuperPassportModel;

            switch ( ReviewAction ) {

                case "View Passport":

                    PassportDetails pDet = superPassportModel.PassportDetails.First();
                    string filename      = pDet.passportFileName;
                    byte[] filedata      = pDet.passportFileBytes;
                    string contentType   = pDet.passportContentType;

                    var cd = new System.Net.Mime.ContentDisposition {
                        FileName    = filename,
                        Inline      = true,
                    };

                    Response.AppendHeader( "Content-Disposition", cd.ToString() );
                    Response.Write("<script>");
                    Response.Write("window.open('"+File( filedata, contentType )+"', '_newtab');");
                    Response.Write("</script>");

                    //actionResult = File( filedata, contentType );
                    break;

                case "Approve":
                    break;
                case "Deny":
                    break;
            }

            return actionResult;
        }

        [HttpGet]
        [Authorize]
        public ActionResult DownloadAttachment() {

            SuperPassportModel superPassportModel = Session["superPassportModel"] as SuperPassportModel;

            PassportDetails pDet = superPassportModel.PassportDetails.First();
            string filename      = pDet.passportFileName;
            byte[] filedata      = pDet.passportFileBytes;
            string contentType   = pDet.passportContentType;

            var cd = new System.Net.Mime.ContentDisposition {
                FileName    = filename,
                Inline      = true,
            };

            Response.AppendHeader( "Content-Disposition", cd.ToString() );
            return File( filedata, contentType );
        }

    }


}