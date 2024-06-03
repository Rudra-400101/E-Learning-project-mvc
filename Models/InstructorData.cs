using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project1_online_selling.Models
{
    public class InstructorData
    {
        public int id {  get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public long mobno { get; set; }
        public HttpPostedFileBase profilePic { get; set; }
        public string gender { get; set; }
        public string summary { get; set; }
        public string about { get; set; }
        public string qualification { get; set; }
        public string oldpic { get; set; }
    }
}