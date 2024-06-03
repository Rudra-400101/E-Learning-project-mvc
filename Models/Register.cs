using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project1_online_selling.Models
{
    public class Register
    {
       
            public string name { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public string course { get; set; }
            public long mob { get; set; }
            public string college { get; set; }
            public string year { get; set; }
            public HttpPostedFileBase picture { get; set; }
        public string pic {  get; set; }
            public string gender { get; set; }
        
    
}
}