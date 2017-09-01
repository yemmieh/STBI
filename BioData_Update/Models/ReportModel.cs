using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BioData_Update.Models
{
    public class ReportModel {
        [Required]
        [Display(Name = "Search By")]
        public string QueryFieldID { get;set;}
        public SelectList QueryField { get;set;}
        public string QueryFieldTitle { get;set;}

        [Required]
        [Display(Name = "Entries containing")]
        public string QueryText { get;set;}

        public string ReportMode { get;set;}

        public IEnumerable<EntriesModel> EntriesModel{get;set;}
    }
}
