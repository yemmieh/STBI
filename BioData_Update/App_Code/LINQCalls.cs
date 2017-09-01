using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using MoreLinq;
using System.Diagnostics;
using System.Data.Entity;
using System.Globalization;

namespace BioData_Update.App_Code
{
    class LINQCalls {

        public LINQCalls() {
            logWriter = new LogWriter();
        }

        //private static string HOBCODE = "001";
        //private static string ABJCODE = "013";
        //private static string BRACODE = "000";

        private const string ALLENTRIES= "ALLENTRIlogWriter = new LogWriter();ES";
        private const string ALLAPPRVED= "ALLAPPRVED";
        private const string ALLPENDING= "ALLPENDING";
        private const string ALLDENIALS= "ALLDENIALS";
        private const string ALLHRUPLOAD="ALLHRUPLOAD";
        static private LogWriter logWriter = new LogWriter();



        /*public staffprofile getProfile( string employee_number )
        {
            staffprofile profile = new staffprofile();

            ExceedConnectionDataContext xceed = new ExceedConnectionDataContext();
            var Profileinfo = (from distinct in xceed.vw_employeeinfos
                               where (distinct.employee_number == employee_number )
                               select
                               new
                               {
                                   BranchName = distinct.Branch,
                                   BranchCode = distinct.Branch_code,
                                   StaffNumber = distinct.employee_number,
                                   StaffName = distinct.name,
                                   DateOfEmployment = distinct.employment_date,
                                   LastPromotionDate = distinct.last_promo_date,
                                   Level = distinct.grade_code,
                                   Email = distinct.email,
                                   Dept = distinct.dept,

                                   Dept_id = distinct.department_id,
                                   Unit = distinct.unit,
                                   unitCode = distinct.unit,
                                   jobtitle = distinct.jobtitle,
                                   confirm = distinct.emp_confirm,

                               }).Distinct();

            foreach (var Profiles in Profileinfo)
            {
                profile.branch_name = Profiles.BranchName;
                profile.branch_code = Profiles.BranchCode.ToString();
                profile.hodeptname = Profiles.Dept;
                profile.hodeptcode = Profiles.Dept_id.ToString() ;
        
            }
            return profile;

        }*/

        static public StaffADProfile getXceedProfile( string employee_number ) {
            ExceedConnectionDataContext xceed = new ExceedConnectionDataContext();
            var profile = ( from distinct in xceed.vw_employeeinfos
                               where (distinct.employee_number == employee_number )
                               select
                               new StaffADProfile {
                                   branch_name = distinct.Branch,
                                   branch_code = distinct.Branch_code,
                                   employee_number = distinct.employee_number,
                                   name = distinct.name,
                                   doe = distinct.employment_date.Value.ToString( "dddd MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US") ),
                                   dob = distinct.date_of_birth.Value.ToString( "dddd MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US") ),
                                   lastpromotiondate = distinct.last_promo_date,
                                   grade = distinct.grade_code,
                                   email = distinct.email,
                                   department = distinct.dept,
                                   department_id = distinct.department_id,
                                   unit = distinct.unit,
                                   jobtitle = distinct.jobtitle,
                                   confirm = distinct.emp_confirm,
                                   gender = distinct.gender,
                                   imagelink   = "url(http://xceedservermain/EmployeePassport/"+distinct.employee_number+".jpg)"

                               }).Distinct();

            return profile.First();
        }

        static public StaffADProfile getEntryProfile(string workflowid)
        {
            StaffADProfile profile = new StaffADProfile();

            AppraisalConnectionDataContext Ent = new AppraisalConnectionDataContext();
            var Profileinfo = (from distinct in Ent.zib_workflow_masters
                               where ( distinct.workflowid == workflowid )
                               select
                               new 
                               {
                                   branch_name = distinct.deptname,
                                   branch_code = distinct.deptcode,
                                   hodeptname = distinct.unitname,
                                   hodeptcode = distinct.unitcode
                                  

                               }).Distinct();

            foreach (var Profiles in Profileinfo)
            {
                profile.branch_name = Profiles.branch_name;
                profile.branch_code = Profiles.branch_code.ToString();
                profile.hodeptname = Profiles.hodeptname;
                profile.hodeptcode = Profiles.hodeptcode.ToString();
               //profile.in_StaffNumber = Profiles.StaffNumber;

            }
            return profile;

        }

        static internal SelectList getCountries() {
            LocationConnectionDataContext locationCtxt = new LocationConnectionDataContext();
            var countries   =   from c in locationCtxt.COUNTRY_LISTINGs
                                orderby c.COUNTRY_CODE_ISO3166_1_Alpha_3 ascending
                                select c;
            return new SelectList( countries.Distinct() , "COUNTRY_CODE_ISO3166_1_Alpha_3" , "COUNTRY_NAME" );
        }


        

        static internal HRProfile hrprofile( string initiatorLoginName , int org_id ) {
            HRProfile hr       = new HRProfile();
            try { 
                ExceedConnectionDataContext dbXceed = new ExceedConnectionDataContext();
                var hrprofiles =     from v in dbXceed.vw_employeeinfos
                                     where v.logon_name.Equals( "africa\\"+initiatorLoginName )
                                     && v.org_id.Equals( org_id )
                                     select new {    
                                                    name            = v.name ,
                                                    employee_number = v.employee_number
                                                }; 
                foreach (var staff in hrprofiles) {
                    hr.name              = staff.name;
                    hr.employee_number   = staff.employee_number;                    
                }    
                return hr;                
            } catch (Exception ex) {
                logWriter.WriteErrorLog(string.Format( "hrprofile : Exception!!! / {0}",ex.Message ));
                hr=null;
            }
            return hr;
        }

        

        static internal object getBranchStaffProfile( string staffNumber , int org_id ) {

            try {
                ExceedConnectionDataContext dbXceed = new ExceedConnectionDataContext();
                var profile =   from v in dbXceed.vw_employeeinfos
                                where v.employee_number.Equals( staffNumber )
                                && ( v.org_id.Equals(1) || v.org_id.Equals(4) )
                                select new {
                                                name            = v.name ,
                                                grade_code      = v.grade_code ,
                                                branch_code     = v.Branch_code,
                                                
                                                branch          = v.Branch,
                                                employee_number = v.employee_number ?? "" ,
                                                hodept          = (v.Branch_code == "001" ? "Yes" : "No")
                                            };      
                if( profile.Any() ){
                    return profile.ToArray();
                }
            } catch ( Exception ex ) {
                logWriter.WriteErrorLog(string.Format( "hrprofile : Exception!!! / {0}",ex.Message ));
                return null;
            }
            return null;
        }

        
        static internal SelectList getBranches() {
            ExceedConnectionDataContext exceedcnxn = new ExceedConnectionDataContext();
            var branches =  from s in exceedcnxn.vw_branch_analysis
                            where s.org_id.Equals(1)
                            orderby s.description ascending
                            select s;
            return new SelectList( branches , "analysis_det_code" , "Description" );
        }

        static internal SelectList getHODepts(string brcode, string depts) {
            ExceedConnectionDataContext exceedcnxn = new ExceedConnectionDataContext();
            int[] _depts =  depts.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            var depts_   =  from c in exceedcnxn.cm_departments
                            where c.org_id.Equals("001") &&  _depts.Contains(c.department_id)
                            orderby c.description ascending
                            select c;
            return new SelectList( depts_ , "department_id" , "description" );
        }
        static internal SelectList getRoles( string deptcode ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var roles   =  (from c in periodcnxn.zib_workflow_approver_roles
                            where c.deptcode.Equals(deptcode)
                           orderby c.role ascending
                           select c).Distinct();
            return new SelectList( roles , "roleid" , "role" );
        }
        static internal SelectList getRequestStages() {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var stages_ =   from c in periodcnxn.zib_workflow_stages
                            where c.appid.ToUpper().Equals( DataHandlers.APP_ID.ToUpper() )
                            orderby c.requeststage ascending
                            select c;
            return new SelectList( stages_ , "requeststageid" , "requeststage" );
        }
        static internal object getRolesAsJSON( string deptcode ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var roles   =   (from c in periodcnxn.zib_workflow_approver_roles
                            orderby c.role ascending
                            where c.deptcode.Equals(deptcode)
                            select new{roleid=c.roleid,role=c.role}).OrderBy(t=>t.role).ToList();
            if( roles.Any() ){
                return roles.ToArray();
            } else {
                return null;
            }
        }
        
        
        static internal StaffADProfile setApproverFields(StaffADProfile staffADProfile) {
            ExceedConnectionDataContext exceedcnxn = new ExceedConnectionDataContext();
            //AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var setup = (from x in exceedcnxn.vw_employeeinfos
                         where x.employee_number.Equals( staffADProfile.employee_number )
                         select new 
                            {                                                
                                branchname  = x.Branch,
                                branchcode  = x.Branch_code,
                                department  = x.dept,
                                deptcode    = x.department_code
                            }).FirstOrDefault();
            if( setup!=null ){
                staffADProfile.branch_name  = setup.branchname;
                staffADProfile.branch_code  = setup.branchcode;
                staffADProfile.hodeptname   = setup.department;
                staffADProfile.hodeptcode   = setup.deptcode;
            } else {
                staffADProfile.branch_name  = null;
                staffADProfile.branch_code  = null;
                staffADProfile.hodeptname   = null;
                staffADProfile.hodeptcode   = null;
                staffADProfile.appperiod    = null;
            }
            return staffADProfile;            
        }
        
        static internal List<RequestDetails> getMarketingStaff_Branch(StaffADProfile staffADProfile) {            
            ExceedConnectionDataContext exceedcnxn = new ExceedConnectionDataContext();
            var marketers   =   from v in exceedcnxn.vw_employeeinfos
                                where v.Category.Equals("marketing")
                                && v.Branch_code.Equals(staffADProfile.branch_code) 
                                && !v.jobtitle.Contains("BRANCH HEAD")
                                && !v.jobtitle.Contains("BRANCH HEAD/ZONAL HEAD")
                                && !v.jobtitle.Contains("ZONAL HEAD")
                                && !v.jobtitle.Contains("GROUP ZONAL HEAD")
                                && !v.jobtitle.Contains("GROUP HEAD")
                                && !v.jobtitle.Contains("ACTING BRANCH HEAD")
                                && ( v.org_id.Equals(1) )
                                orderby v.grade_id ascending
                                select new RequestDetails {
                                    employee_number = v.employee_number,
                                    name            = v.name,
                                    grade           = v.grade_code,
                                    //entry_key       = v.employee_number+"_"+staffADProfile.appperiod

                                };
            return  marketers.ToList(); 
        }
        static internal List<RequestDetails> getMarketingStaff_HO(StaffADProfile staffADProfile) {            
            ExceedConnectionDataContext exceedcnxn = new ExceedConnectionDataContext();
            var marketers   =   from v in exceedcnxn.vw_employeeinfos
                                where v.Branch_code.Equals("001")
                                //&& v.Branch_code.Equals(staffADProfile.branch_code) 
                                && v.department_id.Equals(staffADProfile.hodeptcode)
                                && !v.jobtitle.Contains("BRANCH HEAD")
                                && !v.jobtitle.Contains("BRANCH HEAD/ZONAL HEAD")
                                && !v.jobtitle.Contains("ZONAL HEAD")
                                && !v.jobtitle.Contains("GROUP ZONAL HEAD")
                                && !v.jobtitle.Contains("GROUP HEAD")
                                && !v.jobtitle.Contains("ACTING BRANCH HEAD")
                                && !v.jobtitle.Contains("BRANCH HEAD/DEPUTY ZONAL HEAD")
                                && ( v.org_id.Equals(1) )
                                orderby v.name ascending
                                select new RequestDetails {
                                    employee_number = v.employee_number,
                                    name            = v.name,
                                    grade           = v.grade_code,
                                    //entry_key       = v.employee_number+"_"+staffADProfile.appperiod
                                };
            return  marketers.ToList(); 
        }

        static internal List<RequestDetails> getExistingPassportEntry(StaffADProfile staffADProfile) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var entries =   from t in periodcnxn.zib_passportentries
                            where t.deptcode.Equals(staffADProfile.branch_code)
                            orderby t.name ascending
                            select new RequestDetails
                                {    
                                    workflowid      = t.workflowid,
                                    requeststageid  = t.requeststageid,
                                    requeststage    = t.requeststage,
                                    requestdate     = t.requestdate,
                                    employee_number = t.employee_number,
                                    name            = t.name,
                                    //entry_key       = t.entry_key
                                };
            return entries.ToList();
        }

        static internal List<RequestDetails> getExistingPassportEntry(string workflowid,string staffnumber) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var entries =   from t in periodcnxn.zib_passportentries
                            join m in periodcnxn.zib_workflow_masters on t.workflowid equals m.workflowid
                            from a in periodcnxn.zib_passport_approvers
                                .Where(a => a.approverid.Equals(staffnumber) && a.roleid.Equals(m.requeststageid))
                                .DefaultIfEmpty()
                            where m.workflowid.Equals(workflowid)
                            orderby t.name ascending
                            select new RequestDetails
                                {    
                                    workflowid      = m.workflowid,
                                    requeststageid  = m.requeststageid,
                                    requeststage    = m.requeststage,
                                    requestdate     = m.createdt,
                                    employee_number = t.employee_number,
                                    name            = t.name,
                                    grade           = t.grade
                                };
            return entries.DistinctBy(c => c.employee_number).ToList();
        }

        static internal List<RequestDetails> getExistingPassportEntry(string workflowid,string staffnumber,string entry_key) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var entries =   from t in periodcnxn.zib_passportentries
                            join m in periodcnxn.zib_workflow_masters on t.workflowid equals m.workflowid
                            //join m in periodcnxn.zib_workflow_masters.Where( x => x.workflowid == t.workflowid && x.Completed == true )
                            from a in periodcnxn.zib_passport_approvers
                                .Where(a => a.approverid.Equals(staffnumber) && a.roleid.Equals(m.requeststageid))
                                .DefaultIfEmpty()
                            where m.workflowid.Equals(workflowid)
                            //&& t.entry_key==entry_key
                            orderby t.name ascending
                            select new RequestDetails
                                {    
                                    workflowid      = m.workflowid,
                                    requeststageid  = m.requeststageid,
                                    requeststage    = m.requeststage,
                                    requestdate     = m.createdt,
                                    employee_number = t.employee_number,
                                    name            = t.name,
                                    grade           = t.grade,
                                    /*cabal           = t.cabal,
                                    cabal_l         = t.cabal_l,
                                    sabal           = t.sabal,
                                    sabal_l         = t.sabal_l,
                                    fx              = t.fx,
                                    rv              = t.rv,
                                    fd              = t.fd,
                                    inc             = t.inc,
                                    inc_l           = t.inc_l,*/
                                    //entry_key       = t.entry_key
                                };
            return entries.DistinctBy(c=>c.employee_number).ToList();
        }

        static internal PassportDetails getPassportDetails( string workflowid ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var passportEntry =     (from p in periodcnxn.zib_passportentries
                                    where p.workflowid.Equals(workflowid)
                                    select new {
                                        passportType            = p.passportType,
                                        entry_key               = p.entry_key,
                                        passportNumber          = p.passportNumber,
                                        passportSurname         = p.passportSurname,
                                        passportOtherNames      = p.passportOtherNames,
                                        passportSex             = p.passportSex,
                                        passportNationality   = p.passportCountry,
                                        passportCountryCode     = p.passportCountryCode,
                                        passportCountry         = p.passportCountry,
                                        passportDateOfBirth     = p.passportDateOfBirth.Date,
                                        passportPlaceOfBirth    = p.passportPlaceOfBirth,
                                        passportAuthority       = p.passportAuthority,
                                        passportDateOfIssue     = p.passportDateOfIssue.Date,
                                        passportDateOfExpiry    = p.passportDateOfExpiry.Date,
                                        passportFileName        = p.passportFileName,
                                        passportContentType     = p.passportContentType,
                                        passportBytes           = p.passportBytes 
                                    })
                                    .ToList()
                                    .Select( _p =>  new PassportDetails {    
                                        passportType            = _p.passportType,
                                        entry_key               = _p.entry_key,
                                        passportNumber          = _p.passportNumber,
                                        passportSurname         = _p.passportSurname,
                                        passportOtherNames      = _p.passportOtherNames,
                                        passportSex             = _p.passportSex,
                                        passportNationality   = _p.passportCountry,
                                        passportCountryCode     = _p.passportCountryCode,
                                        passportCountry         = _p.passportCountry,
                                        passportDateOfBirth     = _p.passportDateOfBirth.ToString(),
                                        passportPlaceOfBirth    = _p.passportPlaceOfBirth,
                                        passportAuthority       = _p.passportAuthority,
                                        passportDateOfIssue     = _p.passportDateOfIssue.ToString(),
                                        passportDateOfExpiry    = _p.passportDateOfExpiry.ToString(),
                                        passportFileName        = _p.passportFileName,
                                        passportContentType     = _p.passportContentType,
                                        passportFileBytes       = _p.passportBytes.ToArray()
                                    });
            Console.WriteLine(passportEntry);
            return passportEntry.FirstOrDefault();
        }

        static internal List<EntriesModel> getMyPassportWorkflows(StaffADProfile staffADProfile) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   from w in workflowcnxn.zib_workflow_masters
                            from t in workflowcnxn.zib_passportentries
                                    .Where( tt => tt.hr_uploader_id.Equals(staffADProfile.employee_number) )
                                    .Take( 1 )
                            where w.appid.Equals(DataHandlers.APP_ID)
                            && w.initiatornumber.Equals(staffADProfile.employee_number)
                            orderby w.createdt descending
                            select new EntriesModel
                                {    
                                    WorkflowID     = w.workflowid,
                                    StaffNumber     = w.initiatornumber,
                                    StaffName       = w.initiatorname,
                                    Branch          = w.deptname,
                                    BranchCode      = w.deptcode,
                                    DeptName        = (w.deptcode.Equals("001"))? w.unitname: w.deptname,
                                    DeptCode        = (w.deptcode.Equals("001")) ? w.unitname : w.deptcode,
                                    RequestStage    = w.requeststage,
                                    RequestStageId  = w.requeststageid,
                                    UploadStatus    = t.passport_status,
                                    DateSubmitted   = w.createdt,
                                    Approvers       = w.approvalhistory.ToString(),
                                    Audit           = w.audithistory.ToString(),
                                    Action          = "View"
                                };
            return entries.ToList();
        }        
        static internal List<EntriesModel> getMyPendingPassportWorkflows(StaffADProfile staffADProfile) {

            logWriter.WriteErrorLog(string.Format(" getMyPendingPassportWorkflows :inside getMyPendingPassportWorkflows!!! / {0}", staffADProfile.user_logon_name));
            List<EntriesModel> entrym = new List<EntriesModel>();

            try {
                AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
                var entries =   from w in workflowcnxn.zib_workflow_masters
                                from a in workflowcnxn.zib_passport_approvers
                                from t in workflowcnxn.zib_passportentries
                                        .Where( tt => tt.workflowid.Equals(w.workflowid) )
                                        .Take( 1 )
                                where w.appid.Equals( DataHandlers.APP_ID )
                                && w.requeststageid.Equals(a.roleid)
                                && (a.approverid.Equals(staffADProfile.employee_number) && (a.deptcode.Equals(w.deptcode)|| w.unitname.Equals(a.deptname) ))
                                orderby w.createdt descending
                                select new EntriesModel
                                    {    
                                        WorkflowID      = w.workflowid,
                                        StaffNumber     = w.initiatornumber,
                                        StaffName       = w.initiatorname,
                                        Branch          = w.deptname,
                                        BranchCode      = w.deptcode,
                                        DeptName        = (w.deptcode.Equals("001")) ? w.unitname : w.deptname,
                                        DeptCode        = (w.deptcode.Equals("001")) ? w.unitname : w.deptcode,
                                        RequestStage    = w.requeststage,
                                        RequestStageId  = w.requeststageid,
                                        UploadStatus    = t.passport_status,
                                        DateSubmitted   = w.createdt,
                                        Approvers       = w.approvalhistory.ToString(),
                                        Audit           = w.audithistory.ToString(),
                                        Action          = "View"
                                    };
                logWriter.WriteErrorLog(string.Format("get Entry List Count : Exception!!! / {0}", entries.Count()));

                return entries.ToList();
            } catch (Exception ex) {
                logWriter.WriteErrorLog(string.Format("getMyPendingPassportWorkflows : Exception!!! / {0}", ex.Message));
                return entrym.ToList();
            }
        }

        static internal List<EntriesModel> getMyApprovedPassportWorkflows(StaffADProfile staffADProfile) {
            logWriter.WriteErrorLog(string.Format(" getMyApprovedTargetWorkflows :inside getMyApprovedTargetWorkflows!!! / {0}", staffADProfile.user_logon_name));
            
            //staffADProfile.employee_number ="2002205";

            List<EntriesModel> entrym = new List<EntriesModel>();
            try {
                AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
                var entries =   from w in workflowcnxn.zib_workflow_masters
                                from a in workflowcnxn.zib_passport_approvers
                                from t in workflowcnxn.zib_passportentries
                                        .Where( tt => tt.workflowid.Equals(w.workflowid) )
                                        .Take( 1 )
                                /*from el in w.approvalhistory.Descendants("Approvals")
                                        .Where(dd => dd.Element("ApproverStaffNumber").Value.Equals(staffADProfile.employee_number))*/
                                where w.appid.Equals( DataHandlers.APP_ID )
                                //&& w.requeststageid.Equals(a.roleid)
                                //&& (a.approverid.Equals(staffADProfile.employee_number) && (a.deptcode.Equals(w.deptcode)|| w.unitname.Equals(a.deptname) ))
                                && w.approvalhistory.ToString().Contains(staffADProfile.employee_number)
                                orderby w.createdt descending
                                select new EntriesModel
                                    {    
                                        WorkflowID      = w.workflowid,
                                        StaffNumber     = w.initiatornumber,
                                        StaffName       = w.initiatorname,
                                        Branch          = w.deptname,
                                        BranchCode      = w.deptcode,
                                        DeptName        = (w.deptcode.Equals("001")) ? w.unitname : w.deptname,
                                        DeptCode        = (w.deptcode.Equals("001")) ? w.unitname : w.deptcode,
                                        RequestStage    = w.requeststage,
                                        RequestStageId  = w.requeststageid,
                                        UploadStatus    = t.passport_status,
                                        DateSubmitted   = w.createdt,
                                        Approvers       = w.approvalhistory.ToString(),
                                        Audit           = w.audithistory.ToString(),
                                        Action          = "View"
                                    };
                logWriter.WriteErrorLog(string.Format("get Entry List Count : Exception!!! / {0}", entries.Count()));

                return entries.ToList();
            } catch (Exception ex) {
                logWriter.WriteErrorLog(string.Format("getMyPendingTargetWorkflows : Exception!!! / {0}", ex.Message));
                return entrym.ToList();
            }
        }
        static internal EntryModel getWorkflowEntry( string workflowid ) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   from w in workflowcnxn.zib_workflow_masters
                            from t in workflowcnxn.zib_passportentries
                                    .Where( tt => tt.workflowid.Equals(workflowid ))
                                    .Take( 1 )
                            where w.workflowid.Equals(workflowid)
                            orderby w.createdt descending
                            select new EntryModel
                                {    
                                    WorkflowID      = w.workflowid,
                                    StaffNumber     = w.initiatornumber,
                                    StaffName       = w.initiatorname,
                                    Branch          = w.deptname,
                                    BranchCode      = w.deptcode,
                                    DeptName        = (w.deptcode.Equals("001")) ? w.unitname : w.deptname,
                                    DeptCode        = (w.deptcode.Equals("001")) ? w.unitname : w.deptcode,
                                    RequestStage    = w.requeststage,
                                    RequestStageId  = w.requeststageid,
                                    UploadStatus    = t.passport_status,
                                    DateSubmitted   = w.createdt,
                                    Approvers       = w.approvalhistory.ToString(),
                                    Audit           = w.audithistory.ToString(),
                                    Action          = "View",
                                    StaffGrade      = t.grade

                                };
            return entries.FirstOrDefault();
           
        }

        static internal string getInitiatorNumber(string workflowid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.initiatornumber;
            if( entries.Count()>0 ){
                return entries.First().ToString() ?? null;
            } else {
                return null;
            }
        }
        static internal List<string> getApproverNames(string workflowid,int requeststageid) {

            if(requeststageid==20){
                return getHRApproverNames(workflowid,requeststageid);
            }

            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   ( requeststageid.Equals(-1) || requeststageid.Equals(0) ) ?                
                            from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.initiatorname
                            :
                            from w in workflowcnxn.zib_workflow_masters
                            join a in workflowcnxn.zib_passport_approvers on w.requeststageid equals a.roleid
                            where w.workflowid.Equals(workflowid)
                            && (w.deptcode.Equals(a.deptcode) || w.unitname.Equals(a.deptname) 
                                /*&&  w.unitcode.Equals(a.unitcode) 
                                 && w.groupcode.Equals(a.groupcode)
                                 && w.supergroupcode.Equals(a.supergroupcode)*/
                                )
                            select a.approvername;
            return entries.ToList();
        }
        static internal List<string> getApproverIDs(string workflowid,int requeststageid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   ( requeststageid.Equals(-1) || requeststageid.Equals(0) ) ? 
                            from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.initiatornumber
                            :
                            from w in workflowcnxn.zib_workflow_masters
                            join a in workflowcnxn.zib_passport_approvers on w.requeststageid equals a.roleid
                            where w.workflowid.Equals(workflowid)
                            && (w.deptcode.Equals(a.deptcode) || w.unitname.Equals(a.deptname) 
                                 /*&& w.unitcode.Equals(a.unitcode) 
                                 && w.groupcode.Equals(a.groupcode)
                                 && w.supergroupcode.Equals(a.supergroupcode)*/
                                )
                            select a.approverid;
            return entries.ToList();
        }
        static internal List<string> getHRApproverNames(string workflowid,int requeststageid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   from w in workflowcnxn.zib_workflow_masters
                            join a in workflowcnxn.zib_workflow_user_roles on w.requeststageid equals a.roleid
                            where w.workflowid.Equals(workflowid)
                            && ( a.status.Equals("Enabled") )
                            && w.appid.Equals(DataHandlers.APP_ID)
                            orderby a.name ascending
                            select a.name;
            return entries.ToList();
        }
        static internal XElement getApprovalHistory(string workflowid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   (from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.approvalhistory).First();
            return entries;
        }
       static internal XElement getAuditHistory(string workflowid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   (from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.audithistory).First();
            return entries;
        }

        static internal List<string> getApproverNumbersToNames(string workflowid,int requeststageid) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   ( requeststageid.Equals(-1) || requeststageid.Equals(0) ) ?                
                            from w in workflowcnxn.zib_workflow_masters
                            where w.workflowid.Equals(workflowid)
                            select w.initiatornumber
                            :
                            from w in workflowcnxn.zib_workflow_masters
                            join a in workflowcnxn.zib_passport_approvers on w.requeststageid equals a.roleid
                            where w.workflowid.Equals(workflowid)
                            && (w.deptcode.Equals(a.deptcode) || w.unitname.Equals(a.deptname) 
                                 /*&& w.unitcode.Equals(a.unitcode) 
                                 && w.groupcode.Equals(a.groupcode)
                                 && w.supergroupcode.Equals(a.supergroupcode)*/
                                )
                            select a.approverid;
            Debug.WriteLine(getStaffNames(entries.ToList()));
            return getStaffNames(entries.ToList());
        }

        private static List<string> getStaffNames( List<string> empnos ) {
            ExceedConnectionDataContext exceedcnxn      = new ExceedConnectionDataContext(); 
            return (from e in exceedcnxn.vw_employeeinfos where empnos.Contains(e.employee_number) select e.name).ToList();
        }

        static internal bool checkDupApproverSetup( string entrykey ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var exists   =  from c in periodcnxn.zib_passport_approvers
                            where c.entrykey.ToUpper().Equals(entrykey)
                            select c;
            if( exists.Any() ){
                return false;
            } else {
                return true;
            }
        }

        static internal AppraisalApproverModel getApproverSetupEntry(string entrykey) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entry =   (from w in workflowcnxn.zib_passport_approvers
                            from t in workflowcnxn.zib_workflow_approver_roles
                            where w.entrykey.Equals(entrykey)
                            orderby w.createdt
                            select new AppraisalApproverModel
                                {    
                                    EntryKey    = w.entrykey,
                                    StaffNumber = w.approverid,
                                    StaffName   = w.approvername,
                                    UnitCode    = w.unitcode,
                                    UnitTitle   = w.unitname,
                                    DeptCode    = w.deptcode,
                                    DeptTitle   = w.deptname,
                                    GroupCode   = w.groupcode,
                                    GroupTitle  = w.groupname,
                                    SuperGroupCode = w.supergroupcode,
                                    SuperGroupTitle= w.supergroupname,
                                    RoleTitle   = w.role,
                                    RoleID      = w.roleid
                                }).First();
            return entry;
        }

        static internal List<AppraisalApproverModel> getApproverSetupList() {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   (from w in workflowcnxn.zib_passport_approvers
                            from t in workflowcnxn.zib_workflow_approver_roles
                            where w.roleid.Equals( t.roleid )
                            orderby w.createdt
                            select new AppraisalApproverModel
                                {    
                                    EntryKey    = w.entrykey,
                                    StaffNumber = w.approverid,
                                    StaffName   = w.approvername,
                                    UnitCode    = w.unitcode,
                                    UnitTitle   = w.unitname,
                                    DeptCode    = w.deptcode,
                                    DeptTitle   = w.deptname,
                                    GroupCode   = w.groupcode,
                                    GroupTitle  = w.groupname,
                                    SuperGroupCode = w.supergroupcode,
                                    SuperGroupTitle= w.supergroupname,
                                    RoleTitle   = w.role,
                                    RoleID      = w.roleid
                                }).DistinctBy(c=>c.EntryKey).ToList();
            return entries;
        }

        static internal SelectList getHRRoles() {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var hrroles =   from s in workflowcnxn.zib_workflow_approver_roles
                            where s.deptcode.Equals("020")
                            orderby s.role ascending
                            select s;
            return new SelectList( hrroles , "roleid" , "role" );
        }

        static internal List<AppraisalApproverModel> getHRUsers() {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   (from w in workflowcnxn.zib_workflow_user_roles
                            from t in workflowcnxn.zib_workflow_approver_roles
                            where w.roleid.Equals( t.roleid )
                            && t.deptcode.Equals("020")
                            orderby w.roleid, w.name, w.createdt
                            select new AppraisalApproverModel
                                {    
                                    EntryKey    = w.entrykey,
                                    StaffName   = w.name,
                                    StaffNumber = w.employee_number,                                    
                                    RoleTitle   = w.role,
                                    RoleID      = w.roleid,
                                    StatusCode  = w.status,
                                    StatusTitle = w.status,
                                    HRStaffNumber = w.editedbyid,
                                    CreateDate  = w.createdt
                                }).DistinctBy(c=>c.EntryKey).ToList();
            return entries;
        }

        static internal AppraisalApproverModel getRoleSetupEntry( string entrykey ) {
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entry =   (from w in workflowcnxn.zib_workflow_user_roles
                            from t in workflowcnxn.zib_workflow_approver_roles
                            where w.entrykey.Equals(entrykey)
                            orderby w.createdt
                            select new AppraisalApproverModel
                                {    
                                    EntryKey    = w.entrykey,
                                    StaffName   = w.name,
                                    StaffNumber = w.employee_number,                                    
                                    RoleTitle   = w.role,
                                    RoleID      = w.roleid,
                                    StatusCode  = w.status,
                                    StatusTitle = w.status,
                                    HRStaffNumber = w.editedbyid,
                                    CreateDate  = w.createdt
                                }).First();
            return entry;
        }

        static internal List<EntriesModel> getWorkflowReport( string ReportMode ) {

            //System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
            //System.Configuration.KeyValueConfigurationElement CurrentAppraisalPeriod = rootWebConfig.AppSettings.Settings["CurrentAppraisalPeriod"];
            //string APP_PERIOD = CurrentAppraisalPeriod.Value.ToString();

            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            var entries =   from w in workflowcnxn.zib_workflow_masters
                            //from a in workflowcnxn.zib_appraisal_approvers
                            from t in workflowcnxn.zib_passportentries
                                    .Where( tt => tt.workflowid.Equals( w.workflowid ) )
                                    .Take( 1 )
                            where w.appid.Equals(DataHandlers.APP_ID)
                            orderby w.createdt descending
                            select new EntriesModel
                                {    
                                    WorkflowID      = w.workflowid,
                                    StaffNumber     = w.initiatornumber,
                                    StaffName       = w.initiatorname,
                                    Branch          = w.deptname,
                                    BranchCode      = w.deptcode,
                                    UnitName        = w.unitname,
                                    DeptName = (w.deptcode.Equals("001")) ? w.unitname : w.deptname,
                                    DeptCode = (w.deptcode.Equals("001")) ? w.unitname : w.deptcode,
                                    GroupName       = w.groupname,
                                    SuperGroupName  = w.supergroupname,
                                    RequestStage    = w.requeststage,
                                    RequestStageId  = w.requeststageid,
                                    UploadStatus    = t.passport_status,
                                    DateSubmitted   = w.createdt,
                                    Approvers       = w.approvalhistory.ToString(),
                                    Audit           = w.audithistory.ToString(),
                                    Action          = "View",
                                    EntryKey        = t.entry_key
                                };

            if( ReportMode.Equals(ALLAPPRVED) ){
                entries = entries.Where(r=>r.RequestStageId.Equals(100));
            }else if( ReportMode.Equals(ALLDENIALS) ){
                entries = entries.Where(r=>r.RequestStageId.Equals(-1));
            }else if( ReportMode.Equals(ALLHRUPLOAD) ){
                entries = entries.Where(r=>r.RequestStageId.Equals(20));
            }else if( ReportMode.Equals(ALLPENDING) ){
                entries = entries.Where( r=>!r.RequestStageId.Equals(100) && !r.RequestStageId.Equals(-1)  );
            }
            return entries.ToList();
        }

        static internal List<EntriesModel> getWorkflowQueryReport(ReportModel reportModel) {
            
            List<EntriesModel> workflowReport = getWorkflowReport( reportModel.ReportMode );            
            switch (reportModel.QueryFieldTitle) {
                case "deptname":
                    workflowReport = workflowReport.Where(s=>s.DeptName.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "groupname":
                    workflowReport = workflowReport.Where(s=>s.GroupName.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "supergroupname":
                    workflowReport = workflowReport.Where(s=>s.SuperGroupName.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "staffnumber":
                    workflowReport = workflowReport.Where(s=>s.StaffNumber.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "staffname":
                    workflowReport = workflowReport.Where(s=>s.StaffName.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "requeststage":
                    workflowReport = workflowReport.Where(s=>s.RequestStage.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "appraisalperiod":
                    workflowReport = workflowReport.Where(s=>s.AppraisalPeriod.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
                case "approverlist":
                    workflowReport = workflowReport.Where(s=>s.Approvers.ToLower().Contains(reportModel.QueryText.ToLower())).ToList();
                    break;
            }
            return workflowReport;
        }
        
        static internal List<EntriesModel> getPendingHRUpload(StaffADProfile staffADProfile) {
            List<EntriesModel> workflowReport = getWorkflowReport( ALLHRUPLOAD );  
            return workflowReport;
        }
        
        private static List<string> getStaffGrades( List<string> staffnumbers ) {
            ExceedConnectionDataContext exceedcnxn      = new ExceedConnectionDataContext(); 
            return (from e in exceedcnxn.vw_employeeinfos where staffnumbers.Contains(e.employee_number) select e.grade_code).ToList();
        }

        /*private static List<string> getStaffNames( List<string> empnos ) {
            ExceedConnectionDataContext exceedcnxn      = new ExceedConnectionDataContext(); 
            return (from e in exceedcnxn.vw_employeeinfos where empnos.Contains(e.employee_number) select e.name).ToList();
        }
        */

        static internal IEnumerable<IEnumerable<AppraisalApproverModel>> getOrgStructure( string BranchCode ) {
            
            AppraisalConnectionDataContext workflowcnxn = new AppraisalConnectionDataContext();
            //ExceedConnectionDataContext exceedcnxn      = new ExceedConnectionDataContext(); 

            var entries =   from w in workflowcnxn.zib_passport_approvers
                            where w.deptcode.Equals(BranchCode)
                            orderby w.approverid descending
                            select new AppraisalApproverModel
                                {    
                                    EntryKey    = w.entrykey,
                                    StaffNumber = w.approverid,
                                    StaffName   = w.approvername,
                                    UnitCode    = w.unitcode,
                                    UnitTitle   = w.unitname,
                                    DeptCode    = w.deptcode,
                                    DeptTitle   = w.deptname,
                                    GroupCode   = w.groupcode,
                                    GroupTitle  = w.groupname,
                                    SuperGroupCode = w.supergroupcode,
                                    SuperGroupTitle= w.supergroupname,
                                    RoleTitle   = w.role,
                                    RoleID      = w.roleid,
                                    CreateDate  = w.createdt,
                                    EditDate    = w.editdate,
                                    HRStaffName = w.edittedbyid,
                                    ImageLink   = "url(http://xceedservermain/EmployeePassport/"+w.approverid+".jpg)"
                                };

            return entries.GroupBy(x => x.RoleID).OrderByDescending(c => c.First().RoleID);
        }  
        
        /*static internal object getUnitsAsJSON( string deptcode , string _deptcode ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var units_   =  (from c in periodcnxn.zib_appraisal_dept_structures
                            where (!deptcode.Equals( HOBCODE ) ? c.deptcode.Equals(deptcode) : c.groupcode.Equals(deptcode) && c.deptcode.Equals(_deptcode) )
                            select new{unitcode=c.unitcode,unitname=c.unitname})
                            .DistinctBy(m=>m.unitname).OrderBy(t=>t.unitname).ToList();
            if( units_.Any() ){
                return units_.ToArray();
            } else {
                return null;
            }
        }*/

        /*static internal SelectList getUnits( string deptcode ) {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var units_   =  (from c in periodcnxn.zib_appraisal_dept_structures
                            where c.deptcode.Equals(deptcode)
                            select c).DistinctBy(m=>m.unitname).OrderBy(t=>t.unitname);
            return new SelectList( units_ , "unitcode" , "unitname" );
        } */

        /*static internal SelectList getDepts() {
            AppraisalConnectionDataContext periodcnxn = new AppraisalConnectionDataContext();
            var depts_   =  (from c in periodcnxn.zib_appraisal_dept_structures
                             where !c.deptcode.Equals("000")
                            orderby c.deptname ascending
                            select c).DistinctBy(m=>m.deptname).OrderBy(t=>t.deptname);
            return new SelectList( depts_ , "deptcode" , "deptname" );
        }*/
    }



    

    

    /**
    public static class MyExtensions {
        public static IEnumerable<IGrouping<TKey, TSource>> ChunkBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return source.ChunkBy(keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> ChunkBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            // Flag to signal end of source sequence.
            const bool noMoreSourceElements = true;

            // Auto-generated iterator for the source array.       
            var enumerator = source.GetEnumerator();

            // Move to the first element in the source sequence.
            if (!enumerator.MoveNext()) yield break;

            // Iterate through source sequence and create a copy of each Chunk.
            // On each pass, the iterator advances to the first element of the next "Chunk"
            // in the source sequence. This loop corresponds to the outer foreach loop that
            // executes the query.
            Chunk<TKey, TSource> current = null;
            while (true)
            {
                // Get the key for the current Chunk. The source iterator will churn through
                // the source sequence until it finds an element with a key that doesn't match.
                var key = keySelector(enumerator.Current);

                // Make a new Chunk (group) object that initially has one GroupItem, which is a copy of the current source element.
                current = new Chunk<TKey, TSource>(key, enumerator, value => comparer.Equals(key, keySelector(value)));

                // Return the Chunk. A Chunk is an IGrouping<TKey,TSource>, which is the return value of the ChunkBy method.
                // At this point the Chunk only has the first element in its source sequence. The remaining elements will be
                // returned only when the client code foreach's over this chunk. See Chunk.GetEnumerator for more info.
                yield return current;

                // Check to see whether (a) the chunk has made a copy of all its source elements or 
                // (b) the iterator has reached the end of the source sequence. If the caller uses an inner
                // foreach loop to iterate the chunk items, and that loop ran to completion,
                // then the Chunk.GetEnumerator method will already have made
                // copies of all chunk items before we get here. If the Chunk.GetEnumerator loop did not
                // enumerate all elements in the chunk, we need to do it here to avoid corrupting the iterator
                // for clients that may be calling us on a separate thread.
                if (current.CopyAllChunkElements() == noMoreSourceElements)
                {
                    yield break;
                }
            }
        }

        // A Chunk is a contiguous group of one or more source elements that have the same key. A Chunk 
        // has a key and a list of ChunkItem objects, which are copies of the elements in the source sequence.
        class Chunk<TKey, TSource> : IGrouping<TKey, TSource>
        {
            // INVARIANT: DoneCopyingChunk == true || 
            //   (predicate != null && predicate(enumerator.Current) && current.Value == enumerator.Current)

            // A Chunk has a linked list of ChunkItems, which represent the elements in the current chunk. Each ChunkItem
            // has a reference to the next ChunkItem in the list.
            class ChunkItem
            {
                public ChunkItem(TSource value)
                {
                    Value = value;
                }
                public readonly TSource Value;
                public ChunkItem Next = null;
            }
            // The value that is used to determine matching elements
            private readonly TKey key;

            // Stores a reference to the enumerator for the source sequence
            private IEnumerator<TSource> enumerator;

            // A reference to the predicate that is used to compare keys.
            private Func<TSource, bool> predicate;

            // Stores the contents of the first source element that
            // belongs with this chunk.
            private readonly ChunkItem head;

            // End of the list. It is repositioned each time a new
            // ChunkItem is added.
            private ChunkItem tail;

            // Flag to indicate the source iterator has reached the end of the source sequence.
            internal bool isLastSourceElement = false;

            // Private object for thread syncronization
            private object m_Lock;

            // REQUIRES: enumerator != null && predicate != null
            public Chunk(TKey key, IEnumerator<TSource> enumerator, Func<TSource, bool> predicate)
            {
                this.key = key;
                this.enumerator = enumerator;
                this.predicate = predicate;

                // A Chunk always contains at least one element.
                head = new ChunkItem(enumerator.Current);

                // The end and beginning are the same until the list contains > 1 elements.
                tail = head;

                m_Lock = new object();
            }

            // Indicates that all chunk elements have been copied to the list of ChunkItems, 
            // and the source enumerator is either at the end, or else on an element with a new key.
            // the tail of the linked list is set to null in the CopyNextChunkElement method if the
            // key of the next element does not match the current chunk's key, or there are no more elements in the source.
            private bool DoneCopyingChunk { get { return tail == null; } }

            // Adds one ChunkItem to the current group
            // REQUIRES: !DoneCopyingChunk && lock(this)
            private void CopyNextChunkElement()
            {
                // Try to advance the iterator on the source sequence.
                // If MoveNext returns false we are at the end, and isLastSourceElement is set to true
                isLastSourceElement = !enumerator.MoveNext();

                // If we are (a) at the end of the source, or (b) at the end of the current chunk
                // then null out the enumerator and predicate for reuse with the next chunk.
                if (isLastSourceElement || !predicate(enumerator.Current))
                {
                    enumerator = null;
                    predicate = null;
                }
                else
                {
                    tail.Next = new ChunkItem(enumerator.Current);
                }

                // tail will be null if we are at the end of the chunk elements
                // This check is made in DoneCopyingChunk.
                tail = tail.Next;
            }

            // Called after the end of the last chunk was reached. It first checks whether
            // there are more elements in the source sequence. If there are, it 
            // Returns true if enumerator for this chunk was exhausted.
            internal bool CopyAllChunkElements()
            {
                while (true)
                {
                    lock (m_Lock)
                    {
                        if (DoneCopyingChunk)
                        {
                            // If isLastSourceElement is false,
                            // it signals to the outer iterator
                            // to continue iterating.
                            return isLastSourceElement;
                        }
                        else
                        {
                            CopyNextChunkElement();
                        }
                    }
                }
            }

            public TKey Key { get { return key; } }

            // Invoked by the inner foreach loop. This method stays just one step ahead
            // of the client requests. It adds the next element of the chunk only after
            // the clients requests the last element in the list so far.
            public IEnumerator<TSource> GetEnumerator()
            {
                //Specify the initial element to enumerate.
                ChunkItem current = head;

                // There should always be at least one ChunkItem in a Chunk.
                while (current != null)
                {
                    // Yield the current item in the list.
                    yield return current.Value;

                    // Copy the next item from the source sequence, 
                    // if we are at the end of our local list.
                    lock (m_Lock)
                    {
                        if (current == tail)
                        {
                            CopyNextChunkElement();
                        }
                    }

                    // Move to the next ChunkItem in the list.
                    current = current.Next;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }*/

    /**Org Structure Listing of All Approvers**/
        
}