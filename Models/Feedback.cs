using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project1_online_selling.Models
{
    public class Feedback
    {
        public int id {  get; set; }
        public int courseId { get; set; }
        public string fname { get; set; }
        public string email { get; set; }
        public string subject { get; set; }
        public string comment { get; set; }
    }
}