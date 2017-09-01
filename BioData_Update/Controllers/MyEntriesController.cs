using BioData_Update.App_Code;
using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BioData_Update.Controllers
{
    public class MyEntriesController : Controller
    {
        // GET: MyEntries
        private string _UserName = "";

        public ActionResult MyEntries( ) {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? "";

            if( _UserName == null || _UserName.Equals(String.Empty)  ){
                ViewBag.ErrorMessage="You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval","AwaitingApproval");
            }

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = _UserName;

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery( staffADProfile );
            staffADProfile = activeDirectoryQuery.GetStaffProfile();
            if( staffADProfile==null ){
                ViewBag.ErrorMessage="Your profile is not properly setup on the system. Please contact InfoTech.";
                return RedirectToAction( "AwaitingMyApproval","AwaitingApproval");
            }
            
            //Check if the initiator/branch/has an existing entry for the AppraisalPeriod from the Database
            List<EntriesModel> entryDetails =  new List<EntriesModel>();
            entryDetails = LINQCalls.getMyPassportWorkflows( staffADProfile );

            return View( entryDetails );
        }

        public ActionResult OpenPassportEntry( string WorkflowID , int RequestStageID) {

            Session["UserName"] = @User.Identity.Name;
            this._UserName = Session["UserName"] as string ?? "";

            if( _UserName == null || _UserName.Equals(String.Empty) ){
                ViewBag.ErrorMessage="You must be logged in to continue.";
                return RedirectToAction("AwaitingMyApproval","AwaitingApproval");
            }

            //now resolve the user profile from AD and Xceed
            StaffADProfile staffADProfile = new StaffADProfile();
            staffADProfile.user_logon_name = _UserName;          

            //AD
            ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery( staffADProfile );
            staffADProfile = activeDirectoryQuery.GetStaffProfile();
            if( staffADProfile==null ){
                ViewBag.ErrorMessage="Your profile is not properly setup on the system. Please contact InfoTech.";
                return RedirectToAction( "AwaitingMyApproval","AwaitingApproval");
            }
            
            List<string> approvers = LINQCalls.getApproverIDs( WorkflowID,RequestStageID );
            bool isApprover = approvers.Contains( staffADProfile.employee_number );

            //get the request and setup the superPassportModel object

            string routeController = "PassportReviewForm";
            TempData["superPassportModel"] = null;

            if (isApprover) {
                routeController = "PassportReviewForm";
            }

            if ( !RequestStageID.Equals(-1) ) {
                return RedirectToAction( routeController,"PassportReview",new { WorkflowID=WorkflowID , editMode = isApprover , myEntries=false } );
            } else {
                return RedirectToAction( "EditPassport","PassportInput",new { WorkflowID=WorkflowID , editMode = isApprover , myEntries=true } );
            }
        }

        public ActionResult GetApprovers( string WorkflowID , int RequestStageID ) {

            string errorResult = "{{\"employee_number\":\"{0}\",\"name\":\"{1}\"}}";
            if( string.IsNullOrEmpty( WorkflowID ) ) {
                errorResult = string.Format(errorResult , "Error" , "Invalid entry detected");        
                return Content(errorResult, "application/json");
            }

            var profile = ( RequestStageID==20 ) ? LINQCalls.getHRApproverNames(WorkflowID,RequestStageID):LINQCalls.getApproverNumbersToNames(WorkflowID,RequestStageID);
            if( profile==null || profile.Count()<=0 ){
                errorResult = string.Format(errorResult , null , "No approvers found for the entry");        
                return Content(errorResult, "application/json");
            } else {
                return Json( profile , JsonRequestBehavior.AllowGet );
            }
        }
    }
}