using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace project1_online_selling.Models
{
    public class OfferData
    {
        public int id { get; set; }

        public string email { get; set; }
        public string course { get; set; }
        public int totalfee { get; set; }
        public DateTime oldexpDate { get; set; }
        public int discountAmount { get; set; }
        public DateTime cexpDate { get; set;}
    }
}