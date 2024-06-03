using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project1_online_selling.Models
{
    public class CourseModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public HttpPostedFileBase cthumbnail { get; set; }
        public string thumbnail { get; set; }
        public int ccategory { get; set; }
        public string category {  get; set; }
        public int cinstructor { get; set; }
        public string instructor {  get; set; } 
        public int cfees { get; set; }
        public int cdiscount { get; set; }
        public DateTime cduration { get; set; }
        public string duration {  get; set; }
        [AllowHtml]
        public string editor { get; set; }
    }
}