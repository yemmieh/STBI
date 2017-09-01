using BioData_Update.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioData_Update.App_Code {
    class AppDatabase {

        private LogWriter logWriter;
        public AppDatabase() {
            this.logWriter = new LogWriter();
        }
        internal string getConnectionString( string serverName ) {
            string connectionString = "";
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[serverName];
            if (settings != null) {
                connectionString = settings.ConnectionString;
            }
            return connectionString;
        }

        internal string inputPassportEntries( DataTable dataTable , SuperPassportModel superPassportModel, string ConnString , string Status) {
            
            string retVal       = null;          
            string connString   = getConnectionString(ConnString);
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_insert_passport_entries";

            dataTable.Columns.Remove("Nationality");
            dataTable.Columns.Remove("passportNationality");
            dataTable.Columns.Remove("passportUpload");

            DataTable dataTableCloned = dataTable.Clone();
            dataTableCloned.Columns["passportDateOfBirth"].DataType = typeof(DateTime);
            dataTableCloned.Columns["passportDateOfIssue"].DataType = typeof(DateTime);
            dataTableCloned.Columns["passportDateOfExpiry"].DataType = typeof(DateTime);

             dataTableCloned.Columns["passportFileBytes"].DataType = typeof(byte[]);

            foreach (DataRow row in dataTable.Rows) {

                row.SetField("passportDateOfBirth", Convert.ToDateTime(row["passportDateOfBirth"].ToString()).ToString("yyyy-MM-dd hh:mm:ss"));
                row.SetField("passportDateOfIssue", Convert.ToDateTime(row["passportDateOfIssue"].ToString()).ToString("yyyy-MM-dd hh:mm:ss"));
                row.SetField("passportDateOfExpiry", Convert.ToDateTime(row["passportDateOfExpiry"].ToString()).ToString("yyyy-MM-dd hh:mm:ss"));

                dataTableCloned.ImportRow(row);
            }

            SqlParameter parameter  = cmnd.CreateParameter();
            parameter.ParameterName = "@tvpPassportEntries";
            parameter.Value         = dataTableCloned;
            parameter.SqlDbType     = SqlDbType.Structured;
            parameter.TypeName      = "dbo.PassportEntriesType";
            cmnd.Parameters.Add( parameter );
            
            cmnd.Parameters.Add("@workflowid"       , SqlDbType.VarChar).Value  = superPassportModel.WorkflowID;
            cmnd.Parameters.Add("@requeststageid"   , SqlDbType.Int).Value      = superPassportModel.RequestStageID;
            cmnd.Parameters.Add("@requeststage"     , SqlDbType.VarChar).Value  = superPassportModel.RequestStage;
            cmnd.Parameters.Add("@employee_number"  , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.employee_number;
            cmnd.Parameters.Add("@name"             , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.name;
            cmnd.Parameters.Add("@grade"            , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.grade;
            
            cmnd.Parameters.Add("@branchname"       , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.branch_name;
            cmnd.Parameters.Add("@branchcode"       , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.branch_code;
            cmnd.Parameters.Add("@deptname"         , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.department;
            cmnd.Parameters.Add("@deptcode"         , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.department_id;
            cmnd.Parameters.Add("@passport_status"  , SqlDbType.VarChar).Value  = Status;
            cmnd.Parameters.Add("@hr_uploader_name" , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.name;
            cmnd.Parameters.Add("@hr_uploader_id"   , SqlDbType.VarChar).Value  = superPassportModel.StaffADProfile.employee_number;
            cmnd.Parameters.Add("@appid"            , SqlDbType.VarChar).Value  = DataHandlers.APP_ID;
            
            cmnd.Parameters.Add("@rErrorCode"       , SqlDbType.Int,2).Direction= ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"        , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "inpuPassportEntries : Exception!!! / {0}",retVal));
                }
            } finally {
                conn.Close();
                cmnd.Dispose();
                dr=null;
            }
            return retVal;
        }

        internal string inputPassportEntriesHRUpload( string workflowid , StaffADProfile staffADProfile , string ConnString , string Status) {
            
            string retVal       = null;          
            string connString   = getConnectionString(ConnString);
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_insert_passport_entries_hrupload";

            cmnd.Parameters.Add("@workflowids"      , SqlDbType.VarChar).Value  = workflowid;
            cmnd.Parameters.Add("@deptname"         , SqlDbType.VarChar).Value  = staffADProfile.branch_name;
            cmnd.Parameters.Add("@deptcode"         , SqlDbType.VarChar).Value  = staffADProfile.branch_code;
            cmnd.Parameters.Add("@passport_status"    , SqlDbType.VarChar).Value  = Status;
            cmnd.Parameters.Add("@hr_uploader_name" , SqlDbType.VarChar).Value  = staffADProfile.user_logon_name;
            cmnd.Parameters.Add("@hr_uploader_id"   , SqlDbType.VarChar).Value  = staffADProfile.employee_number;
            cmnd.Parameters.Add("@appid"            , SqlDbType.VarChar).Value  = DataHandlers.APP_ID;
            
            cmnd.Parameters.Add("@rErrorCode"       , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"        , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "inpuPassportEntries : Exception!!! / {0}",retVal));
                }
            } finally {
                conn.Close();
                cmnd.Dispose();
                dr=null;
            }
            return retVal;
        }

        internal string routePassportEntries(RerouteModel rerouteModel, StaffADProfile staffADProfile , string ConnString ) {
            string retVal       = null;          
            string connString   = getConnectionString(ConnString);
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_reroute_passport_entries";

            cmnd.Parameters.Add("@workflowid"       , SqlDbType.VarChar).Value  = rerouteModel.WorkflowID;
            cmnd.Parameters.Add("@newrequeststageid", SqlDbType.Int).Value      = Int32.Parse(rerouteModel.NewRequestStageCode);
            cmnd.Parameters.Add("@comments"         , SqlDbType.VarChar).Value  = rerouteModel.Comments;
            cmnd.Parameters.Add("@passport_status"    , SqlDbType.VarChar).Value  = "Rerouted";
            cmnd.Parameters.Add("@hr_uploader_name" , SqlDbType.VarChar).Value  = staffADProfile.user_logon_name;
            cmnd.Parameters.Add("@hr_uploader_id"   , SqlDbType.VarChar).Value  = staffADProfile.employee_number;
            cmnd.Parameters.Add("@appid"            , SqlDbType.VarChar).Value  = DataHandlers.APP_ID;
            
            cmnd.Parameters.Add("@rErrorCode"       , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"        , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "routePassportEntries : Exception!!! / {0}",retVal));
                }
            } finally {
                conn.Close();
                cmnd.Dispose();
                dr=null;
            }
            return retVal;
        }
        internal string insertApproverSetup(AppraisalApproverModel appr, HRProfile hrprofile, int inputMode, string ConnString ) {
            string retVal       = "";            
            string connString   = getConnectionString( ConnString );
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_insert_approver_setup";

            cmnd.Parameters.Add("@entrykey"     , SqlDbType.VarChar).Value  = appr.EntryKey;
            cmnd.Parameters.Add("@unitcode"     , SqlDbType.VarChar).Value  = appr.UnitCode;
            cmnd.Parameters.Add("@unitname"     , SqlDbType.VarChar).Value  = appr.UnitTitle;
            cmnd.Parameters.Add("@deptcode"     , SqlDbType.VarChar).Value  = appr.DeptCode;
            cmnd.Parameters.Add("@deptname"     , SqlDbType.VarChar).Value  = appr.DeptTitle;
            cmnd.Parameters.Add("@roleid"       , SqlDbType.VarChar).Value  = appr.RoleID;
            cmnd.Parameters.Add("@role"         , SqlDbType.VarChar).Value  = appr.RoleTitle;
            cmnd.Parameters.Add("@approverid"   , SqlDbType.VarChar).Value  = appr.StaffNumber;
            cmnd.Parameters.Add("@approvername" , SqlDbType.VarChar).Value  = appr.StaffName;
            cmnd.Parameters.Add("@createdbyid"  , SqlDbType.VarChar).Value  = hrprofile.employee_number;
            cmnd.Parameters.Add("@edittedbyid"  , SqlDbType.VarChar).Value  = hrprofile.name;
            cmnd.Parameters.Add("@comments"     , SqlDbType.VarChar).Value  = "";
            
            cmnd.Parameters.Add("@rErrorCode"   , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"    , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;
            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "insertApproverSetup : Exception!!! / {0}",retVal));
                }
            }
            return retVal;
        }
        internal string deleteApproverSetup(string entrykey , string ConnString ) {
            
            string retVal       = "";            
            string connString   = getConnectionString( ConnString );
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_delete_approver_setup";

            cmnd.Parameters.Add("@entrykey"   , SqlDbType.VarChar).Value  = entrykey;                        
            cmnd.Parameters.Add("@rErrorCode" , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"  , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {

                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "deleteApproverSetup : Exception!!! / {0}",retVal));
                }

            }

            return retVal;
        }

        internal string insertRoleSetup(AppraisalApproverModel appr, HRProfile hrprofile, string ConnString ) {
            
            string retVal       = "";            
            string connString   = getConnectionString( ConnString );
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_insert_workflow_user_roles";

            cmnd.Parameters.Add("@entrykey"         , SqlDbType.VarChar).Value  = appr.EntryKey;
            cmnd.Parameters.Add("@roleid"           , SqlDbType.Int).Value      = appr.RoleID;
            cmnd.Parameters.Add("@role"             , SqlDbType.VarChar).Value  = appr.RoleTitle;
            cmnd.Parameters.Add("@username"         , SqlDbType.VarChar).Value  = appr.UserName;
            cmnd.Parameters.Add("@employee_number"  , SqlDbType.VarChar).Value  = appr.StaffNumber;
            cmnd.Parameters.Add("@name"             , SqlDbType.VarChar).Value  = appr.StaffName;
            cmnd.Parameters.Add("@appid"            , SqlDbType.VarChar).Value  = DataHandlers.APP_ID;
            cmnd.Parameters.Add("@status"           , SqlDbType.VarChar).Value  = appr.StatusCode;
            cmnd.Parameters.Add("@createdbyid"      , SqlDbType.VarChar).Value  = hrprofile.employee_number;
            
            cmnd.Parameters.Add("@rErrorCode"       , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"        , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "insertRoleSetup : Exception!!! / {0}",retVal));
                }
            }
            return retVal;
        }

        internal string deleteRoleSetup(string entrykey, string ConnString ) {
            string retVal       = "";            
            string connString   = getConnectionString( ConnString );
            
            SqlConnection conn  = new SqlConnection(connString);
            SqlCommand cmnd     = new SqlCommand();
            
            cmnd.Connection     = conn;
            cmnd.CommandType    = CommandType.StoredProcedure;
            cmnd.CommandText    = "zsp_delete_role_setup";

            cmnd.Parameters.Add("@entrykey"   , SqlDbType.VarChar).Value  = entrykey;                        
            cmnd.Parameters.Add("@rErrorCode" , SqlDbType.Int,2).Direction=ParameterDirection.Output;
            cmnd.Parameters.Add("@rErrorMsg"  , SqlDbType.VarChar,255).Direction=ParameterDirection.Output;            

            SqlDataReader dr;
               
            try {
                // Open the data connection
                cmnd.Connection = conn;
                conn.Open();

                dr = cmnd.ExecuteReader(); 

                int retCode = int.Parse(cmnd.Parameters["@rErrorCode"].Value.ToString());
                if ( retCode!=0 ) {
                    retVal = retCode+"|"+cmnd.Parameters["@rErrorMsg"].Value.ToString();
                }
            
            } catch (SqlException ex) {
                if ( ex.Number!=0 ) {
                    retVal = ex.Number+"|"+ex.Message;
                    logWriter.WriteErrorLog(string.Format( "deleteRoleSetup : Exception!!! / {0}",retVal));
                }
            }

            return retVal;
        }
    }
}
