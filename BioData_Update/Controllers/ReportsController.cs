using BioData_Update.App_Code;
using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BioDate_Update.Controllers {
    public class ReportsController : Controller {
        // GET: Reports

        //private string UserID = "";
        private string _UserName="";

        private const string ALLENTRIES= "ALLENTRIES";
        private const string ALLAPPRVED= "ALLAPPRVED";
        private const string ALLPENDING= "ALLPENDING";
        private const string ALLDENIALS= "ALLDENIALS";

        [HttpGet]
        public ActionResult Reports( string ReportMode ) {

            this._UserName = Session["UserName"] as string ?? "";

            string PostBackMessage  = TempData["PostBackMessage"] as string;
            string Approvers        = TempData["Approvers"] as string;
            if(!String.IsNullOrEmpty(PostBackMessage)){
                ViewBag.PostBackMessage = "<script type='text/javascript'>alert(\""+ PostBackMessage +"\\n\\n"+ Approvers +"\");</script>";
            }    
       
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

            //ReportMode = (String.IsNullOrEmpty(TempData["ReportMode"] as string) ) ? ReportMode : TempData["ReportMode"] as string ;
            
            //Now let's get all entries in the workflow, depending on what was passed in to ReportMode//
            List<EntriesModel> entryDetails =  new List<EntriesModel>();
            //entryDetails = new LINQCalls().getWorkflowReport( ReportMode );

            ReportModel reportModel = new ReportModel();

            if( TempData["reportModel"]!=null ){
                reportModel = TempData["reportModel"] as ReportModel;
                entryDetails = LINQCalls.getWorkflowQueryReport( reportModel);                
            } else {
                entryDetails = LINQCalls.getWorkflowReport( ReportMode );
                reportModel.ReportMode = ReportMode;
            }
            
            reportModel.QueryField = SelectListItemHelper.GetQueryFields();
            reportModel.EntriesModel = entryDetails;
            
            return View( reportModel );
        }

        [HttpPost]
        public ActionResult Filter( ReportModel reportModel ) {           

            TempData["Filter"]      = "Filter";
            TempData["reportModel"] = reportModel;
            return RedirectToAction("Reports" );

        }
        
        public class SelectListItemHelper {
            internal static SelectList GetQueryFields() {
                return new SelectList( new List<Object>
                                            {   new { value = "staffnumber" , text = "Staff Number" },
                                                new { value = "staffname" , text = "Staff Name"},
                                                new { value = "requeststage" , text = "Request Stage" },
                                                new { value = "appraisalperiod" , text = "Appraisal Period"  },
                                                new { value = "approverlist" , text = "Approver List" }
                                            }, "value", "text");
            }
        }
    }
}