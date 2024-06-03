using Antlr.Runtime.Misc;
using Microsoft.Ajax.Utilities;
using project1_online_selling.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace project1_online_selling.Controllers

{
    [HandleError] // show custom error page when error comes
    [Authorize(Roles="admin")]
    public class AdminController : Controller
    {
        //Create Object for DBLayer------------------------------------------
        DBLayer db = new DBLayer();
        // Admin Dashboard----------------------------
        public ActionResult Dashboard()
        {
            DataTable dt = db.ExecuteSelect("sp_selectTotalRows");
            return View(dt.Rows[0]);
        }

        //fetch data from enroll table for graphs using ajax
        [HttpPost]
        public JsonResult DashboardGraph()
        {
            DataTable dt = db.ExecuteSelect("sp_enroll", new SqlParameter[]{new SqlParameter("@action", 8)});

            List<enrollstudentDashboard> list = new List<enrollstudentDashboard>();
            if (dt.Rows.Count > 0)
            {
                foreach(DataRow dr in dt.Rows)
                {
                    list.Add(new enrollstudentDashboard()
                    {
                        month = Convert.ToInt32(dr["dates"]),
                        monthsRecords = Convert.ToInt32(dr["totalEnrollByMonth"])
                });
                  
                                    }
            }

            return Json(list);
        }

        //------------------------------Course Category--------------------------------------------------------------
        public ActionResult AddCourseCategory()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AddCourseCategory(string categoryName,HttpPostedFileBase pic) {
            SqlParameter[] prms = new SqlParameter[]
            {
                new SqlParameter("@cname", categoryName),
                new SqlParameter("@cpic",pic.FileName)
            };
            object res = db.ExecuteScalar("sp_category",prms);
            if (res.ToString().Equals("Records Added"))
            {
                pic.SaveAs(Server.MapPath("/Content/categoryPic/") + pic.FileName);
            }
            return Json(res);
        }
        // Category Management-----------------------------------------------------------------------------------------
        public ActionResult categoryManagement()
        {
            DataTable dt = db.ExecuteSelect("sp_selectcategory");

            return View(dt);
        }

        //Delete category-----------------------------------------------------------------------------------
        public ActionResult deletecategory(int? id, string pic)
        {
                SqlParameter[] prms = new SqlParameter[]
                {
                new SqlParameter("@id",id)
                };
                object res = db.ExecuteScalar("sp_deletecategory", prms);
                if (res.ToString().Equals("Record Deleted"))
                {
                    string path = Server.MapPath("/Content/categoryPic/") + pic;
                    System.IO.File.Delete(path);
                }
            
            
            return RedirectToAction("categoryManagement");
        }

        //update category-------------------------------------------------------------------------------------
        public ActionResult updateCategory(string categoryName,HttpPostedFileBase CategoryIcon,int? id,string oldpic)
        {
            if (Request.QueryString["status"] != null) //update category status
            {
               db.ExecuteScalar("sp_manupulatecategory", new SqlParameter[] {new SqlParameter("@id", Request.QueryString["status"])});

            }
            else
            {
                if (CategoryIcon != null) //update with image
                {
                    SqlParameter[] prms = new SqlParameter[]
                     {
                new SqlParameter("@id",id),
                new SqlParameter("@cname",categoryName),
                new SqlParameter("@cpic",CategoryIcon.FileName)
                     };
                    object res = db.ExecuteScalar("sp_manupulatecategory", prms);
                    if (res.ToString().Equals("Record Updated"))
                    {
                        CategoryIcon.SaveAs(Server.MapPath("/Content/categoryPic/") + CategoryIcon.FileName);
                        string path = Server.MapPath("/Content/categoryPic/") + oldpic;
                        System.IO.File.Delete(path);
                    }
                }
                else   // uppdate without image
                {
                    SqlParameter[] prms = new SqlParameter[]
                                     {
                new SqlParameter("@id",id),
                new SqlParameter("@cname",categoryName)
                                     };
                    db.ExecuteScalar("sp_manupulatecategory", prms);

                }
            }
            return RedirectToAction("categoryManagement");
        }

        //-------------------------------------------------- Instructor--------------------------------------------
        public ActionResult AddInstructor()
        {
            return View();
        }
        [HttpPost]
        public JsonResult AddInstructor(InstructorData d)
        {
            SqlParameter[] prms = new SqlParameter[]
            {
                new SqlParameter("@name",d.name),
                new SqlParameter("@email",d.email),
                new SqlParameter("@mobno",d.mobno),
                new SqlParameter("@gender",d.gender),
                new SqlParameter("@profilePic",d.profilePic.FileName),
                new SqlParameter("@summary",d.summary),
                new SqlParameter("@about",d.about),
                new SqlParameter("@qualification",d.qualification)
            };
            object res = db.ExecuteScalar("sp_instructor", prms);
            if(res.ToString().Equals("Records Added"))
            {
                d.profilePic.SaveAs(Server.MapPath("/Content/instructorPic/") + d.profilePic.FileName);
            }
            return Json(res.ToString());
        }

        //Instructor management-----------------------------------------------------------------------------------
        public ActionResult InstructorManagement()
        {
            DataTable dt = db.ExecuteSelect("sp_selectInstructor",new SqlParameter[] {new SqlParameter("@action",1)});

            return View(dt);
        }

        //delete instructor-----------------------------------------------------------------------------------------
        public ActionResult deleteInstructor()
        {            
            int res = db.ExecuteDML("sp_deleteInstructor", new SqlParameter[] {
                new SqlParameter("@id",Request.QueryString["id"])
            });
            if(res>0)
            {
                string path = Server.MapPath("/Content/instructorPic/") + Request.QueryString["pic"];
                    System.IO.File.Delete(path);
            }
            return RedirectToAction("InstructorManagement");
        }

        //update instructor-----------------------------------------------------------------------------------
        public ActionResult updateInstructor(InstructorData d)
        {
            if (Request.QueryString["status"] != null)  //update instructor status
            {
                db.ExecuteDML("sp_selectInstructor", new SqlParameter[] { new SqlParameter("@action", 2), new SqlParameter("@id", Request.QueryString["status"]) });
            }
            else
            {
                if (d.profilePic != null) //update with image
                {
                    SqlParameter[] prms = new SqlParameter[]
                    {
                new SqlParameter("@id",d.id),
                new SqlParameter("@name",d.name),
                new SqlParameter("@email",d.email),
                new SqlParameter("@mobno",d.mobno),
                new SqlParameter("@gender",d.gender),
                new SqlParameter("@pic",d.profilePic.FileName),
                new SqlParameter("@summary",d.summary),
                new SqlParameter("@about",d.about),
                new SqlParameter("@qualification",d.qualification)
                    };
                    object res = db.ExecuteScalar("sp_manupulateInstructor", prms);
                    if (res.ToString().Equals("Record Updated Successfully"))
                    {
                        d.profilePic.SaveAs(Server.MapPath("/Content/instructorPic/") + d.profilePic.FileName);
                        string path = Server.MapPath("/Content/instructorPic") + d.oldpic;
                        System.IO.File.Delete(path);
                    }
                }
                else  //update without image
                {
                    SqlParameter[] prms = new SqlParameter[]
                   {
                new SqlParameter("@id",d.id),
                new SqlParameter("@name",d.name),
                new SqlParameter("@email",d.email),
                new SqlParameter("@mobno",d.mobno),
                new SqlParameter("@gender",d.gender),
                new SqlParameter("@summary",d.summary),
                new SqlParameter("@about",d.about),
                new SqlParameter("@qualification",d.qualification)
                   };
                    db.ExecuteScalar("sp_manupulateInstructor", prms);

                }
            }
            return Content("<script>alert('Instructor Updated Successfully');location.href='/admin/InstructorManagement'</script>");
        }
        //------------------------------------------course------------------------------------------
        public ActionResult addCourse()
        {
            DataTable category  = db.ExecuteSelect("sp_selectcategory",new SqlParameter[] {new SqlParameter("@action",1)});
            DataTable instructor = db.ExecuteSelect("sp_selectInstructor",new SqlParameter[] {new SqlParameter("@action",3)});


            DataSet dt = new DataSet();
            dt.Tables.Add(category);
            dt.Tables.Add(instructor);

            return View(dt);
        }
        [HttpPost]
        public ActionResult addCourse(CourseModel d)
        {
            if (d.title != null && d.cthumbnail != null && d.ccategory != 0 && d.cfees != 0 && d.cduration != null && d.editor != null)
            {
                SqlParameter[] prms = new SqlParameter[]
                {
                new SqlParameter("@title",d.title),
                new SqlParameter("@thumbnail",d.cthumbnail.FileName),
                new SqlParameter("@instructor",d.cinstructor),
                new SqlParameter("@category",d.ccategory),
                new SqlParameter("@fees",d.cfees),
                new SqlParameter("@discount",d.cdiscount),
                new SqlParameter("@duration",d.cduration),
                new SqlParameter("@details",d.editor),
                new SqlParameter("@action",1)
                };
                object res = db.ExecuteScalar("sp_course", prms);
                if (res.ToString().Equals("Record Added Successfully"))
                {
                    d.cthumbnail.SaveAs(Server.MapPath("/Content/coursePic/") + d.cthumbnail.FileName);
                }
                return Content($"<script>alert('{res}');location.href='/admin/addCourse'</script>");

            }
            else
            {
                return Content("<script>alert('Al Fields Are Required!');location.href='/admin/addCourse';</script>");

            }
        }
        //course Management---------------------------------------------------------------------------------
        public ActionResult CourseManagement()
        {
            if (Request.QueryString["status"] != null) //update course status
            {
                db.ExecuteScalar("sp_course", new SqlParameter[] { new SqlParameter("@action", 4), new SqlParameter("@courseId", Request.QueryString["status"]) });
            }
            else { 
                if (Request.QueryString["id"] != null) //delete records
                {
                    object res = db.ExecuteScalar("sp_course", new SqlParameter[] { new SqlParameter("@action", 2), new SqlParameter("@courseId", Request.QueryString["id"]) });
                    if (res.ToString().Equals("Records Deleted Succesfully"))
                    {
                        string path = Server.MapPath("/Content/coursePic/") + Request.QueryString["pic"];
                        System.IO.File.Delete(path);
                    }
                }
                else //select Records
                {
                    DataTable dt = db.ExecuteSelect("sp_course", new SqlParameter[] { new SqlParameter("@action", 3) });
                    DataTable category = db.ExecuteSelect("sp_selectcategory", new SqlParameter[] { new SqlParameter("@action", 1) });
                    DataTable instructor = db.ExecuteSelect("sp_selectInstructor", new SqlParameter[] { new SqlParameter("@action", 3) });


                    DataSet data = new DataSet();
                    data.Tables.Add(category);
                    data.Tables.Add(instructor);
                    data.Tables.Add(dt);
                    return View(data);

                }
            }
            
            return RedirectToAction("CourseManagement");
        }
        //update course---------------------------------------------------------------------------------------
        public ActionResult updateCourse(CourseModel d)
        {
            if (d.cthumbnail != null) //update with image
            {
                SqlParameter[] prms = new SqlParameter[]
                {
                new SqlParameter("@courseid",d.id),
                  new SqlParameter("@title",d.title),
                new SqlParameter("@thumbnail",d.cthumbnail.FileName),
                new SqlParameter("@instructor",d.cinstructor),
                new SqlParameter("@category",d.ccategory),
                new SqlParameter("@fees",d.cfees),
                new SqlParameter("@discount",d.cdiscount),
                new SqlParameter("@duration",d.cduration),
                new SqlParameter("@details",d.editor),
                new SqlParameter("@action",5)
                };
            int res=db.ExecuteDML("sp_course",prms);
                if (res>0)
                {
                    d.cthumbnail.SaveAs(Server.MapPath("/Content/coursePic/") + d.cthumbnail.FileName);
                    string path = Server.MapPath("/Content/coursePic") + d.thumbnail;
                    System.IO.File.Delete(path);
                }
            }
            else //update without image
            {
                SqlParameter[] prms = new SqlParameter[]
               {
                new SqlParameter("@courseId",d.id),
                new SqlParameter("@title",d.title),
                new SqlParameter("@instructor",d.cinstructor),
                new SqlParameter("@category",d.ccategory),
                new SqlParameter("@fees",d.cfees),
                new SqlParameter("@discount",d.cdiscount),
                new SqlParameter("@duration",d.cduration),
                new SqlParameter("@details",d.editor),
                new SqlParameter("@action",5)
               };
                db.ExecuteScalar("sp_course", prms);

            }
            return RedirectToAction("CourseManagement");
        }
        //----------------------------------------Register Management--------------------------------------
       public ActionResult RegisterManagement(string status)
        {
            if (status != null) //update status
            {
                db.ExecuteDML("sp_Registeration",new SqlParameter[] {new SqlParameter("@action",11),new SqlParameter("@email",status)});
            }
            else if (Request.QueryString["email"] != null & Request.QueryString["pic"] != null) //delete records
            {
                object res=db.ExecuteScalar("sp_Registeration",new SqlParameter[] {new SqlParameter("@email", Request.QueryString["email"]),new SqlParameter("@action",3)});
                if(res.ToString().Equals("Record Deleted"))
                {
                    string path = Server.MapPath("/Content/RegisterPic") + Request.QueryString["pic"];
                    System.IO.File.Delete(path);
                    return RedirectToAction("registerManagement");
                }
            }
            DataTable dt = db.ExecuteSelect("sp_Registeration",new SqlParameter[] {new SqlParameter("@action",2)}); //select Records
            return View(dt);
        }
        //----------------------------------------------Video--------------------------------------------------------
        public ActionResult AddVideo()
        {
            DataTable dt = db.ExecuteSelect("sp_course", new SqlParameter[] { new SqlParameter("@action", 6) });
            return View(dt);
        }
        [HttpPost]
        public ActionResult AddVideo(VideoData d)
        {
            if (d.video != null&&d.course!=0&&d.duration!=0&&d.description!=null&&d.chapter!=null)
            {
                SqlParameter[] prms = new SqlParameter[]
                {
                new SqlParameter("@title",d.name),
                new SqlParameter("@course",d.course),
                new SqlParameter("@link",d.video),
                new SqlParameter("@duration",d.duration),
                new SqlParameter("@details",d.description),
                new SqlParameter("@chapter",d.chapter),
                new SqlParameter("@ispaid",d.ispaid),
                new SqlParameter("@action",1)
                };
                int res = db.ExecuteDML("sp_Video", prms);
                if (res > 0)
                {
                    return Content("<script>alert('Record Added Successfully');location.href='/admin/addvideo'</script>");

                }
                else
                {
                    return Content("<script>alert('Error While Adding Records');location.href='/admin/addvideo'</script>");
                }
            }
            else
            {
                return Content("<script>alert('Al Fields Are Required!');location.href='/admin/addVideo';</script>");

            }
        }
        //Select Video with ajax-----------------------------------------------------------------------------------
        [HttpPost]
        public JsonResult selectVideo(string category)
        {
            DataTable dt = db.ExecuteSelect("sp_video", new SqlParameter[] { new SqlParameter("@course", category),new SqlParameter("@action",2) });
            List<VideoData> list = new List<VideoData>();
            foreach (DataRow dr in dt.Rows) {
                list.Add(new VideoData()
                {
                    id = Convert.ToInt32(dr["id"]),
                    name = dr["title"].ToString(),
                    coursetitle = dr["Coursetitle"].ToString(),
                    video = dr["link"].ToString(),
                    duration = float.Parse((dr["duration"]).ToString()),
                    description = dr["details"].ToString(),
                    chapter = dr["chapter"].ToString(),
                    ispaid = Convert.ToInt32(dr["ispaid"])
                });
            }
            return Json(list);
        }

     //video mangement----------------------------------------------------------------------------------------
        public ActionResult VideoManagement()
        {

            if (Request.QueryString["status"] != null) //update Status
            {
                db.ExecuteDML("sp_video", new SqlParameter[] { new SqlParameter("@action", 6), new SqlParameter("@id", Request.QueryString["status"]) });
                return RedirectToAction("VideoManagement");
            }
            DataTable dt = db.ExecuteSelect("sp_video", new SqlParameter[] { new SqlParameter("@action", 3) }); //select Records
            return View(dt);
        }

        //update Video---------------------------------------------------------------------

        [HttpPost]
        public ActionResult updateVideo(VideoData d)
        {
            SqlParameter[] prms = new SqlParameter[]
               {
                new SqlParameter("@id",d.id),
                new SqlParameter("@title",d.name),
                new SqlParameter("@course",d.course),
                new SqlParameter("@link",d.video),
                new SqlParameter("@duration",d.duration),
                new SqlParameter("@details",d.description),
                new SqlParameter("@chapter",d.chapter),
                new SqlParameter("@ispaid",d.ispaid),
                new SqlParameter("@action",7)
               };
            int res = db.ExecuteDML("sp_Video", prms);
            if (res > 0)
            {
                return Content("<script>alert('Record updated Successfully');location.href='/admin/VideoManagement'</script>");

            }
            else
            {
                return Content("<script>alert('Error While Updating Records');location.href='/admin/VideoManagement'</script>");
            }
    }
        //Delete Video--------------------------------------------------------------------------------------------
        public ActionResult deleteVideo(int? id)
        {
            if (id.HasValue)
            {
                int res = db.ExecuteDML("sp_video", new SqlParameter[] { new SqlParameter("@action", 4),new SqlParameter("@id",id) });
                if (res > 0)
                {
                    return Content("<script>alert('Video Deleted Successfully');location.href='/admin/VideoManagement'</script>");

                }
                else
                {
                    return Content("<script>alert('Video Not Deleted');location.href='/admin/VideoManagement'</script>");

                }
            }

            return RedirectToAction("VideoManagement");
        }


        //-----------------------------Offer Management--------------------------------------------------------

        [Authorize]
        public ActionResult OfferManagement()
        {
            DataTable dt = db.ExecuteSelect("sp_course", new SqlParameter[] { new SqlParameter("@action", 3) });

            return View(dt);
        }

        //find and match user for provide offers using ajax------------------------------------------------------------
        public JsonResult matchUser(string userId)
        {
            DataTable dt = db.ExecuteSelect("sp_Registeration",new SqlParameter[] {new SqlParameter("@action",10),new SqlParameter("@email",userId) });

            List<Register> list = new List<Register>();

            foreach(DataRow dr in dt.Rows)
            {
                list.Add(new Register()
                {
                    email = dr["email"].ToString(),
                    name = dr["name"].ToString(),
                    mob = Convert.ToInt64(dr["mobno"]),
                    college = dr["college"].ToString(),
                    gender = dr["gender"].ToString()

                }) ;
                                
            };

            return Json(list,JsonRequestBehavior.AllowGet);
        }

        //Select course Wise data for provide offer to User on a choosed course with ajax---------------------------------------
        public JsonResult selectCourseWiseData(int? course)
        {
           
            DataTable dt = db.ExecuteSelect("sp_selectOneCourseDetail", new SqlParameter[] { new SqlParameter("@courseId", course) });

            List<CourseModel> list = new List<CourseModel>();
            foreach(DataRow dr in dt.Rows)
            {
                list.Add(new CourseModel()
                {
                    title = dr["title"].ToString(),
                    cfees = Convert.ToInt32(dr["fees"]) - Convert.ToInt32(dr["discount"]),
                    duration = Convert.ToDateTime(dr["duration"]).ToString("yyyy-MM-dd")
                }) ;
            };

            return Json(list,JsonRequestBehavior.AllowGet);
        }
        //provide offer to a choosed user--------------------------------------------------------------------------
        [HttpPost]
        public ActionResult AddOffer(OfferData d)
        {
            SqlParameter[] prms = new SqlParameter[]
            {
                new SqlParameter("@email",d.email),
                new SqlParameter("@course",d.course),
                new SqlParameter("@finalfee",d.totalfee),
                new SqlParameter("@oldExpiry",d.oldexpDate),
                new SqlParameter("@DiscountAmount",d.discountAmount),
                new SqlParameter("@newExpiry",d.cexpDate),
                new SqlParameter("@action",1)
            };
            
            object res = db.ExecuteScalar("sp_Offers", prms);
            if (res.ToString().Equals("Offer Apply Successfully"))
            {
                //send email when admin give offer to a particular user
                try
                {
                    MailMessage message = new MailMessage("rudrasingh400101@gmail.com", d.email);
                    message.Subject = "Special Offer for You!";
                    message.Body = $"Dear {d.email}<br> Admin Provide You a Special Offer for Rs.{d.discountAmount} and Expiry Date For This Offer is {d.cexpDate},<br>So Hurry! Get it<br>ThankYou";
                    message.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("rudrasingh400101@gmail.com", "brxt jeel gasr qoxe");
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }
                catch
                {
                    Console.Write("Internet Not Connected");
                }
                return Content("<script>alert('Offer Added Successfully');location.href='/admin/OfferManagement'</script>");
            }
            else
            {
                return Content($"<script>alert('{res.ToString()}');location.href='/admin/OfferManagement'</script>");
            }

        }

        //check Offers----------------------------------------------------------------------------------

        public ActionResult CheckOffers(int? id, int? status)
        {
            if (id.HasValue) //Delete Offers
            {
                int res = db.ExecuteDML("sp_offers", new SqlParameter[] { new SqlParameter("@action", 4), new SqlParameter("@id", id) });
                if (res > 0)
                {
                    return Content("<script>alert('Record Deleted Successfully');location.href='/admin/CheckOffers'</script>");
                }
                else
                {
                    return Content("<script>alert('Error Occured While Deleting Records');location.href='/admin/CheckOffers'</script>");

                }
            }
            else if (status.HasValue)  //update Status
            {
                db.ExecuteDML("sp_offers", new SqlParameter[] { new SqlParameter("@action", 5), new SqlParameter("@id", status) });

            }

            DataTable dt = db.ExecuteSelect("sp_offers", new SqlParameter[] { new SqlParameter("@action", 3) }); //select offers records

            return View(dt);
        }

        //----------------------------FeedBack Management--------------------------------------------------------

        public ActionResult FeedbackManagement(int? id,int? status)
        {
            if (id.HasValue) //delete Comments
            {
                int res = db.ExecuteDML("sp_CourseFeedback", new SqlParameter[] { new SqlParameter("@action", 3),new SqlParameter("@id",id) });
                if(res > 0)
                {
                    return Content("<script>alert('Comment Deleted Successfully');location.href='/admin/feedbackmanagement'</script>");
                }
            }else if (status.HasValue) //update status
            {
              db.ExecuteDML("sp_CourseFeedback", new SqlParameter[] { new SqlParameter("@action", 4),new SqlParameter("@id",status) });
                return RedirectToAction("feedbackmanagement");
            }

            DataTable dt = db.ExecuteSelect("sp_CourseFeedback", new SqlParameter[] { new SqlParameter("@action", 2) }); //select comments

            return View(dt);  

        }

        //-------------------------------check enrollment----------------------------------------------------------
        public ActionResult Enrollmentmanagment(int? id,int? status)
        {
            if(id.HasValue) // delete Enroll Students
            { 
                int res = db.ExecuteDML("sp_enroll", new SqlParameter[] { new SqlParameter("@action", 3),new SqlParameter("@id",id) });
                if (res > 0)
                {
                    return Content("<script>alert('Record Deleted Successfully');location.href='/admin/enrollmentmanagment'</script>");
                }
                else
                {
                    return Content("<script>alert('Error Occured While Deleting Records');location.href='/admin/enrollmentmanagment'</script>");

                }
            }else if(status.HasValue) //update status
            {
                 db.ExecuteDML("sp_enroll", new SqlParameter[] { new SqlParameter("@action", 7), new SqlParameter("@id", status) });

            }

            DataTable dt = db.ExecuteSelect("sp_Enroll",new SqlParameter[] {new SqlParameter("@action",5)}); //select enroll Students

            return View(dt);
        }

        //Logout for admin--------------------------------------------------------------------------------------
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("login", "home");
        }
    }
}