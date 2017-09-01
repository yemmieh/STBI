using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BioData_Update.Models
{
    public class StaffADProfile {

        [Display(Name = "Staff Number")]
        public string employee_number { get; set; }

        [Display(Name = "Name")]
        public string name { get; set; }

        [Display(Name = "Grade")]
        public string grade { get; set; }

        [Display(Name = "Date of Employment")]
        public string doe { get; set; }

        public string dob { get; set; }

        [Display(Name = "Branch")]
        public string branch_name { get;set;}

        [Required]
        [Display(Name = "No of Countries For Which You Hold A Passport")]
        public string noOfCountries { get;set;}
        public SelectList NumberOfCountries { get;set;}

        public string RoleTitle { get;set;}
        public int countryCount { get;set;}

        public string branch_code { get; set; }
        public string branch_address { get; set; }
        public string mobile_phone { get; set; }
        public string gsm { get; set; }
        public string jobtitle { get; set; }
        public string office_ext { get; set; }        
        public string department { get;set;}
        public string user_logon_name { get;set;}
        public string email { get;set;}
        public List<string> membership { get;set;}
        public string hodeptcode { get;set;}
        public string hodeptname { get;set;}
        public string appperiod { get;set;}  
        public DateTime? lastpromotiondate {get;set; }
        public int? department_id {get;set; }
        public string unit {get;set; }
        public byte? confirm {get;set; }
        public byte? gender {get;set;}
        public string imagelink { get;set;}
    }

    public class PassportDetails {

        /*Passport Information**/
        
        [Display(Name = "Type")]
        public string passportType {get;set; }

        public string entry_key { get;set;}
        
        [Required]
        [Display(Name = "Passport No")]
        public string passportNumber {get;set; }

        [Required]
        [Display(Name = "Surname")]
        public string passportSurname {get;set; }

        [Required]
        [Display(Name = "Given Names")]
        public string passportOtherNames {get;set; }

        [Required]
        [Display(Name = "Sex")]
        public string passportSex {get;set; }

        [Required]
        [Display(Name = "Nationality")]
        public string passportNationality {get;set; }
        public SelectList Nationality { get;set;}
        [Required]
        public string passportCountry {get;set; } 
        [Required]
        public string passportCountryCode {get;set; }       

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public string passportDateOfBirth {get;set; }

        [Required]
        [Display(Name = "Place of Birth")]
        public string passportPlaceOfBirth {get;set; }

        [Required]
        [Display(Name = "Authority (Issue Place)")]
        public string passportAuthority {get;set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Issue")]
        public string passportDateOfIssue {get;set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Expiry")]
        public string passportDateOfExpiry {get;set; }

        /*[Display(Name = "Upload Passport")]
        public string passportSignature {get;set; }*/
        
        [Required]
        [FileTypes("pdf")]
        [Display(Name = "Upload Passport")]
        public HttpPostedFileBase  passportUpload { get; set; }

        public string passportFileName {get;set;}
        public string passportContentType {get;set; }
        public byte[] passportFileBytes {get;set; }


    }

    public class staffprofile
    {
        public string employee_number { get; set; }
        public string branch_name { get; set; }
        
        public string branch_code { get; set; }
        public string branch_address { get; set; }
        public string mobile_phone { get; set; }
        public string gsm { get; set; }
        public string jobtitle { get; set; }
        public string office_ext { get; set; }
        public string department { get; set; }
        public string user_logon_name { get; set; }
        public string email { get; set; }
        public List<string> membership { get; set; }
        public string hodeptcode { get; set; }
        public string hodeptname { get; set; }
        public string appperiod { get; set; }

        /***New Staff Input Fields***/
        [Display(Name = "Staff Number")]
        public string in_StaffNumber { get; set; }

        [Display(Name = "Staff Name")]
        public string in_StaffName { get; set; }

        [Display(Name = "Staff Grade")]
        public string in_StaffGrade { get; set; }
    }
    public class RequestDetails {
        public RequestDetails() { 
            workflowid      = String.Empty;
            requeststageid = 0;            
            requeststage    = INIT_STAGE;
            requestdate     = DateTime.Now;
        }
        private const string INIT_STAGE = "Passport Submission";
        public string workflowid { get;set;}
        public int requeststageid { get;set;}
        public string requeststage { get;set;}
        public DateTime requestdate { get;set;}
        public string employee_number { get;set;}
        public string name { get;set;}
        public string grade { get; set; }
    }
    public class ApprovalDetails {
        //public string Approver{ get;set;}
        public string ApproverNames{ get;set;}
        public string ApproverStaffNumbers{ get;set;}
        public string ApprovedStages{ get;set;}
        public string ApproverAction{ get;set;}
        public string ApproverComments{ get;set;}
        public string ApprovalDateTime{ get;set;}
    }
    public class AuditDetails {
        public string approver{ get;set;}
        public string stageprocessed{ get;set;}
    }
    public class EntryModel {
        public string WorkflowID { get;set;}
        public string StaffNumber { get;set;}
        public string StaffName { get;set;}
        public string Branch { get;set;}
        public string BranchCode { get;set;}
        public string DeptName { get;set;}
        public string DeptCode { get;set;}
        public string AppraisalPeriod { get;set;}
        public string AppraisalPeriodName { get;set;}
        public string RequestStage { get;set;}
        public int RequestStageId { get;set;}
        public string UploadStatus { get;set;}
        public DateTime DateSubmitted { get;set;}
        public string Approvers { get;set;}
        public string Action { get;set;}
        public string Audit { get; set; }
        public string StaffGrade {get;set; }
        public string DOE {get;set; }
    }
    public class SuperPassportModel {
        public string WorkflowID { get;set; }
        public int RequestStageID { get;set; }
        public string RequestStage { get;set; }
        public string RequestBranch { get;set; }
        public string CanSave { get;set; }
        public string RequestBranchCode { get;set; }
        public DateTime RequestDate { get;set; }
        public StaffADProfile StaffADProfile{get; set;}
        public List<RequestDetails> RequestDetails{get; set;}
        public List<PassportDetails> PassportDetails{get; set;}
        //public List<StaffTargetProfile> StaffTargetProfiles{get;set;}
        public List<ApprovalDetails> ApprovalDetails{get;set;}
        public EntryModel EntryModel { get;set; }
    }
}
