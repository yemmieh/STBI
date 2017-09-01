using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BioData_Update.Models {
    public class ViewStructure {

        [Required]
        [Display(Name = "Branch Name")]
        public string SelectedBranch { get; set; }
        public SelectList BranchName { get; set; }
        public String OrgBranch { get;set;}
        public IEnumerable<IEnumerable<AppraisalApproverModel>> AppraisalApproverModel { get;set;}

    }
}
