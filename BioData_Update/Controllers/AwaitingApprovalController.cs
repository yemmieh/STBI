using BioData_Update.App_Code;
using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MarketersTarget_MVC.Controllers {
    public class AwaitingApprovalController : Controller {
        // GET: AwaitingApproval
        private const string MARKETING  = "MARKETING";
        private const string HOBRCODE   = "001";
        private const string OTHERS     = "OTHERS";
        private const string NA         = "NA";

        //private string UserID = "";
        private string UserName = "";
        private LogWriter logWriter;

        [Authorize]
        public ActionResult AwaitingMyApproval()
        {
            /**First let's check if the PostBackMessage has something
             * Very important---DO NOT DELETE!!!!!!!!!!!!!!!!!!!!!**/

            Session["UserName"]     = @User.Identity.Name;
            this.UserName           = Session["UserName"] as string ?? "";

            string PostBackMessage  = TempData["PostBackMessage"] as string;
            string Approvers        = TempData["Approvers"] as string;
            this.UserName           = Session["UserName"] as String ?? "";
            
            this.logWriter          = new LogWriter();

            try {
                logWriter.WriteErrorLog(string.Format("about to PostBackMessage : Exception!!! / {0}", "Posted back")); 

                if (!String.IsNullOrEmpty(PostBackMessage))
                {
                    logWriter.WriteErrorLog(string.Format("PostBackMessage Status : Exception!!! / {0}", "Posted back")); 

                    ViewBag.PostBackMessage = string.Format("<script type='text/javascript'>alert(\"" + PostBackMessage + "\\n\\n{0}\");</script>", Approvers);
                }
                logWriter.WriteErrorLog(string.Format("After post back : Exception!!! / {0}", "Posted back")); 

                //now get the pending items
                if ( UserName == null || UserName.Equals(String.Empty))
                {
                    ViewBag.ErrorMessage = "You must be logged in to continue.";
                    return View();
                }               

                //now resolve the user profile from AD and Xceed
                StaffADProfile staffADProfile = new StaffADProfile();              
                staffADProfile.user_logon_name = UserName;

                //AD
                ActiveDirectoryQuery activeDirectoryQuery = new ActiveDirectoryQuery(staffADProfile);
                staffADProfile = activeDirectoryQuery.GetStaffProfile();
                if (staffADProfile == null) {
                    ViewBag.ErrorMessage = "Your profile is not properly setup on the system. Please contact InfoTech.";
                    return View();
                }

                //Check if the approver has an existing entry in the Database
                logWriter.WriteErrorLog(string.Format("get Awaiting : about firing getMyPendingPassportWorkflows!!! / {0}", "")); 

                List<EntriesModel> entryDetails = new List<EntriesModel>();
                entryDetails = LINQCalls.getMyPendingPassportWorkflows(staffADProfile);
                logWriter.WriteErrorLog(string.Format("Entry List : Staff Name!!! / {0}", entryDetails.First().StaffName)); 

                return View(entryDetails);
            } catch(Exception ex) {
                logWriter.WriteErrorLog(string.Format("AwaitingMyApproval : Exception!!! / {0}", ex.Message)); 
                return View();
            }
        }
    
    }
}