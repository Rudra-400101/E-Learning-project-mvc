using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project1_online_selling.Models
{
    public class VideoData
    {
        public int id { get; set; }
        public int course { get; set; }
        public string pic {  get; set; }
        public string coursetitle { get; set; }
        public string name { get; set; }
        public string video { get; set; }
        public string chapter {  get; set; }
        public int ispaid { get; set; }   
        public float duration {  get; set; }
        public float totalduration { get; set; }
        public int totalcourse { get; set; }
        [AllowHtml]
        public string description { get; set; }


    }
}