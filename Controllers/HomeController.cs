using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Security;
using project1_online_selling.Models;
using GoogleAuthentication.Services;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stripe;
using System.Configuration;
using Razorpay.Api;



namespace project1_online_selling.Controllers
{
    [HandleError] // show custom error page when error comes
    public class HomeController : Controller
    {
        //create DBLayer Object for user their methods
        DBLayer db = new DBLayer();
        // Home Page----------------------------------------
        public ActionResult Index()
        {
            DataTable dt = db.ExecuteSelect("sp_selectCourseWiseCategory", new SqlParameter[] { new SqlParameter("@action", 1) });
            DataTable dtcourse = db.ExecuteSelect("sp_selectCourseInfo");
            DataSet set = new DataSet();
            set.Tables.Add(dt);
            set.Tables.Add(dtcourse);


            return View(set);
        }

        //google auth------------------------------------------------------------
public async Task<ActionResult> googleauth(string code)
        {
            var clientId = "97828072451-3dkr915etbadurqlself655pfqp28feg.apps.googleusercontent.com";
            var url = "https://localhost:44341/home/googleauth";
            var clientSecret = "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";
            var token = await GoogleAuth.GetAuthAccessToken(code, clientId, clientSecret, url);
            var userProfile = await GoogleAuth.GetProfileResponseAsync(token.AccessToken.ToString());
            var jsonObject = JObject.Parse(userProfile);
            string output = jsonObject["email"].ToString();

            if (!string.IsNullOrEmpty(output)) {
                FormsAuthentication.SetAuthCookie(output, false);
            }
            return RedirectToAction("Index", "Home"); 
        }
        //login with linkedin----------------------------------------------
      
        //public ActionResult RedirectToLinkedin(string code,string state)
        //{
        //    try
        //    {
        //        var client = new RestClient("https://www.linkedin.com/oauth/v2/accessToken");
        //        var request = new RestRequest("accessToken", Method.Post);
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        //        request.AddParameter("grant_type", "authorization_code");
        //        request.AddParameter("code", Request.QueryString["code"].ToString());
        //        request.AddParameter("redirect_uri", "https://localhost:44341/home/RedirectToLinkedin");
        //        request.AddParameter("client_id", "865krf196n177r");
        //        request.AddParameter("client_secret", "gIJ6CSW1LXr5XJWI");
        //        request.AddHeader("Content-Type", "application/json; charset=utf-8");
        //        RestResponse response = client.Execute(request);
        //        var content = response.Content;
        //        var res = (JObject)JsonConvert.DeserializeObject(content);
        //        //var client2 = new RestClient("https://api.linkedin.com/v2/userinfo");
        //        //request = new RestRequest(Method.Get);
        //        return RedirectToAction("index");

        //    }catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //--------------------------USer Register Page-------------------------------------------------------------
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(Register r)
        {
            if (r.name != null && r.email != null && r.password != null && r.course != null && r.mob != 0 && r.college != null && r.year != null && r.gender != null && r.picture != null)
            {
                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                string encpassword=BCrypt.Net.BCrypt.HashPassword(r.password,salt);
                SqlParameter[] parameters = new SqlParameter[] {
     new SqlParameter("@action",1),
     new SqlParameter("@name",r.name),
     new SqlParameter("@email",r.email),
     new SqlParameter("@password",encpassword),
     new SqlParameter("@course",r.course),
     new SqlParameter("@mobno",r.mob),
     new SqlParameter("@college",r.college),
     new SqlParameter("@year",r.year),
     new SqlParameter("@pic",r.picture.FileName),
     new SqlParameter("@gender",r.gender)
     };
                int res = db.ExecuteDML("sp_Registeration", parameters);
                if (res > 0)
                {
                    //Send Mail on Registered User
                    try
                    {
                        MailMessage message = new MailMessage("rudrasingh400101@gmail.com", r.email);
                        message.Subject = "UserId And Password From CodeHelper";
                        message.Body = $"Dear {r.name},\n\nThankyou for Being Registered With Codehelper\n\nYour Login Id is <b>{r.email}</b> And Password is <b>{r.password}</b>\n\nThankyou ";
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

                    r.picture.SaveAs(Server.MapPath("/Content/RegisterPic/") + r.picture.FileName);
                }
            }
            else
            {
                return Content("<script>alert('Al Fields Are Required!');location.href='/home/Register';</script>");

            }
            return Content("<script>alert('You Are Succesfully Registered');location.href='/home/login';</script>");
        }

        //-------------------------------Login Page For User----------------------------------------------------------
        public ActionResult Login()
        {
            var clientId = "97828072451-3dkr915etbadurqlself655pfqp28feg.apps.googleusercontent.com";
            var url = "https://localhost:44341/home/googleauth";
            var response = GoogleAuth.GetAuthUrl(clientId, url);
            ViewBag.Response = response;


            if (Request.QueryString["ReturnUrl"] != null)
            {
                Session["ReturnUrl"] = Request.QueryString["ReturnUrl"]; //get url and after login open previews page
            }
            
            return View();
        }
        [HttpPost]
        public ActionResult Login(string userId, string password)
        {
            
            if (userId != null & password != null)
            {
                SqlParameter[] prms = new SqlParameter[]
                {
                    new SqlParameter("@email",userId),
                };
                DataTable dt = db.ExecuteSelect("sp_login", prms);
                var decpassword =dt.Rows[0]["password"].ToString();
                if (dt.Rows.Count > 0)
                {
                    bool verifypassword = BCrypt.Net.BCrypt.Verify(password, decpassword);
                    if (verifypassword)
                    {
                        FormsAuthentication.SetAuthCookie(userId, false);
                    }
                    else
                    {
                        return Content("<script>alert('Invalid Password');location.href='/home/login'</script>");

                    }

                    if (Session["ReturnUrl"] != null)
                    {
                        return Content($"<script>alert('Login Successfully');location.href='{Session["ReturnUrl"]}'</script>");

                    }
                    else
                    {
                        return Content("<script>alert('Login Successfully');location.href='/home/myCourse'</script>");
                    }
                }
                else
                {
                    return Content("<script>alert('Invalid Email Id');location.href='/home/login'</script>");

                }
            }
            return RedirectToAction("login");
        }

        //---------------------------------Logout user-------------------------------------------------------
        public ActionResult Logout()
        {
            Session.Abandon(); //remove Session permanently
            FormsAuthentication.SignOut();
            return RedirectToAction("login");
        }

        //---------------------------Email Verification Page for Records Changes-------------------------------
        public ActionResult EmailVerify()
        {
            return View();
        }
        [HttpPost]
        public JsonResult EmailVerify(string email)
        {

            object res = db.ExecuteScalar("sp_registeration", new SqlParameter[] { new SqlParameter("@action", 4), new SqlParameter("@email", email) });

            if (res.ToString().Equals("Email Matched"))
            {
                //send otp
                try
                {
                    Random r = new Random();
                    int random = r.Next(00001, 99999); //generate random number for otp
                    Session["otp"] = random;
                    Session["email"] = email;
                    MailMessage message = new MailMessage("rudrasingh400101@gmail.com", email);
                    message.Subject = "OTP For Password Reset";
                    message.Body = $"Dear User Your OTP For Password Reset is {random} \n\nThankyou ";
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
                    Response.Write("<script>alert('Internet Not Connected')</script>");
                }
            }

            return Json(res.ToString());
        }

        //--------------------OTP Verification Page-------------------------------------------------------------------
        public ActionResult verifyOtp()
        {
            if (Session["email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("EmailVerify");
            }
        }

        [HttpPost]
        public ActionResult verifyOtp(string otp)
        {
            if (Session["otp"] != null)
            {
                if (Session["otp"].ToString() == otp)
                {

                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    return RedirectToAction("verifyOtp");
                }
            }
            else
            {
                return RedirectToAction("EmailVerify");
            }
        }

        //----------------------Reset Password Page---------------------------------------------------------
        public ActionResult ResetPassword()
        {
            if (Session["email"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("EmailVerify");
            }
        }
        [HttpPost]
        public ActionResult ResetPassword(string password, string cpassword)
        {
            if (Session["email"] != null)
            {
                if (password.Equals(cpassword))
                {
                    if (Session["admin"] != null) // for change admin password
                    {
                        var salt = BCrypt.Net.BCrypt.GenerateSalt();
                        var encpassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
                        int res = db.ExecuteDML("sp_admin", new SqlParameter[] { new SqlParameter("@email", Session["email"]), new SqlParameter("@action", 3), new SqlParameter("@password", encpassword) });
                        if (res > 0)
                        {
                            Session.Clear();
                            return RedirectToAction("adminlogin");
                        }
                    }
                    else //for change user password
                    {
                        var salt = BCrypt.Net.BCrypt.GenerateSalt();
                        var encpassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
                        int res = db.ExecuteDML("sp_Registeration", new SqlParameter[] { new SqlParameter("@email", Session["email"]), new SqlParameter("@action", 5), new SqlParameter("@password", encpassword) });
                        if (res > 0)
                        {
                            Session.Clear();
                            return RedirectToAction("login");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("ResetPassword");
                }
            }

            return RedirectToAction("EmailVerify");

        }

        [Authorize(Roles = "student")]

        //--------------User enrolled Course Page------------------------------------------------------------
        public ActionResult myCourse()
        {

            DataTable dt = db.ExecuteSelect("sp_enroll", new SqlParameter[] { new SqlParameter("@action", 2), new SqlParameter("@email", User.Identity.Name) });
            return View(dt);
        }

        //--------------------------------Admin Login page---------------------------------
        public ActionResult adminlogin()
        {

            return View();
        }

        [HttpPost]
        public ActionResult adminlogin(string userId, string password)
        {
            if (userId != "" && password != "")
            {
                DataTable dt = db.ExecuteSelect("sp_admin", new SqlParameter[] { new SqlParameter("@action", 1), new SqlParameter("@email", userId)});
                if (dt.Rows.Count > 0)
                {
                    var decpassword = dt.Rows[0]["password"].ToString();
                    bool verifypassword = BCrypt.Net.BCrypt.Verify(password, decpassword);
                    if (verifypassword)
                    {
                        FormsAuthentication.SetAuthCookie("admin", false);
                    }
                    else
                    {
                        return Content("<script>alert('Invalid Password');location.href='/home/adminlogin'</script>");

                    }
                    return Content("<script>alert('Login Successfully');location.href='/admin/dashboard'</script>");
                }
                else
                {
                    return Content("<script>alert('Error While Login');location.href='/home/adminlogin'</script>");

                }
            }
            else
            {
                return Content("<script>alert('All Fiels are Required');location.href='/home/adminlogin'</script>");


            }
        }

        // Verify Email for Reset Admin Password--------------------------------------------
        public ActionResult verifyAdminEmail()
        {
            return View();
        }
        [HttpPost]
        public JsonResult verifyAdminEmail(string email)
        {
            object res = db.ExecuteScalar("sp_admin", new SqlParameter[] { new SqlParameter("@action", 2), new SqlParameter("@email", email) });
            if (res.ToString().Equals("Email Matched"))
            {
                //send otp
                try
                {
                    Random r = new Random();
                    int random = r.Next(00001, 99999); //generate otp for admin
                    Session["otp"] = random;
                    Session["email"] = email;
                    Session["admin"] = "admin";

                    MailMessage message = new MailMessage("rudrasingh400101@gmail.com", email);
                    message.Subject = "OTP For Password Reset";
                    message.Body = $"Dear User Your OTP For Password Reset is {random} \n\nThankyou ";
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
                    Response.Write("<script>alert('Internet Not Connected')</script>");
                }
            }

            return Json(res.ToString());
        }

        [Authorize(Roles = "student")]

        //User Profile Page------------------------------------------------------------------------
        public ActionResult UserDetail()
        {
            DataTable dt = db.ExecuteSelect("sp_Registeration", new SqlParameter[] { new SqlParameter("@action", 6), new SqlParameter("@email", User.Identity.Name) });
            return View(dt);
        }
        [HttpPost]
        //Update User Profile---------------------
        public ActionResult updateProfile(Register r)
        {
            if (r.picture != null && r.pic != "") //update prfile image for user
            {
                int res = db.ExecuteDML("sp_Registeration", new SqlParameter[] {new SqlParameter("@action",8),
     new SqlParameter("@email",User.Identity.Name),new SqlParameter("@pic",r.picture.FileName)});
                if (res > 0)
                {
                    string path = Server.MapPath("/Content/RegisterPic/") + r.pic;
                    System.IO.File.Delete(path);
                    r.picture.SaveAs(Server.MapPath("/Content/RegisterPic/") + r.picture.FileName);

                    return Content("<script>alert('Profile Picture Updated Successfully');location.href='/home/userdetail'</script>");
                }

            }
            else //update user details except profile picture-------------------------------------------
            {
                SqlParameter[] prms = new SqlParameter[] {

     new SqlParameter("@action",7),
     new SqlParameter("@name",r.name),
     new SqlParameter("@email",r.email),
     new SqlParameter("@changeEmail",User.Identity.Name),
     new SqlParameter("@password",r.password),
     new SqlParameter("@course",r.course),
     new SqlParameter("@mobno",r.mob),
     new SqlParameter("@college",r.college),
     new SqlParameter("@year",r.year),
     new SqlParameter("@gender",r.gender)
     };
                int res = db.ExecuteDML("sp_Registeration", prms);
                if (res > 0)
                {
                    FormsAuthentication.SetAuthCookie(r.email, false); //set auth cookie when user update his email
                    //send success messege for updating user details
                    try
                    {
                        MailMessage message = new MailMessage("rudrasingh400101@gmail.com", r.email);
                        message.Subject = "Your UserId Are Successfuly Updated";
                        message.Body = $"Dear {r.name},\n\nThankyou for Being Registered With Codehelper\n\nYour Updated Login Id is <b>{r.email}</b>\n\nThankyou ";
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
                        Response.Write("<script>alert('Internet Not Connected')</script>");
                    }
                    return Content("<script>alert('Record Updated Successfully');location.href='/home/userdetail'</script>");
                }
            }
            return RedirectToAction("UserDetail");
        }

        //--------------------Change Password with old Password-----------------------------------------------------
        [Authorize(Roles = "student")]
        public ActionResult changePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult changePassword(string oldPassword, string newPassword, string conPassword)
        {
            if (oldPassword != "" && newPassword != "" && conPassword != "")
            {
                if (newPassword.Equals(conPassword))
                {
                    int res = db.ExecuteDML("sp_Registeration", new SqlParameter[] {
     new SqlParameter("@email",User.Identity.Name),new SqlParameter("@password",newPassword),new SqlParameter("@changePassword",oldPassword),new SqlParameter("@action",9)});

                    if (res > 0)
                    {
                        try
                        {
                            MailMessage message = new MailMessage("rudrasingh400101@gmail.com", User.Identity.Name);
                            message.Subject = "Password Updated Successfully";
                            message.Body = $"Dear User Your UserId is {User.Identity.Name} And Password is <b><u>{newPassword}</u></b>,\n\nThankyou for Being Registered With Codehelper</b>\n\nThankyou ";
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
                            Response.Write("<script>alert('Internet Not Connected')</script>");
                        }
                        return Content("<script>alert('Password Updated Successfully');location.href='/home/changePassword'</script>");
                    }
                    else
                    {
                        return Content("<script>alert('Old Password did not match');location.href='/home/changePassword'</script>");
                    }
                }
                else
                {
                    return Content("<script>alert('Confirm Password Does Not Match');location.href='/home/changePassword'</script>");
                }

            }
            else
            {
                return Content("<script>alert('All Fields Are Required');location.href='/home/changePassword'</script>");
            }

        }

        //Singal Course Detail Page ----------------------------------------------------------------------
        public ActionResult CourseDetail(int? course_id)
        {

            if (course_id.HasValue)
            {
                //courseDetail by Course Id
                DataTable dt = db.ExecuteSelect("sp_selectOneCourseDetail", new SqlParameter[] { new SqlParameter("@courseId", course_id) });
                // user comments ->only 3
                DataTable dtcomment = db.ExecuteSelect("sp_CourseFeedback", new SqlParameter[] { new SqlParameter("@action", 5), new SqlParameter("@id", course_id) });
                // chapter Details according to course id
                DataTable dtchapter = db.ExecuteSelect("sp_selectCourseWiseChapter", new SqlParameter[] { new SqlParameter("@courseId", course_id) });
                //total enroll Students
                DataTable dt2 = db.ExecuteSelect("sp_enroll", new SqlParameter[] { new SqlParameter("@action", 4), new SqlParameter("@courseId", course_id) });


                DataSet ds = new DataSet();
                ds.Tables.Add(dt);  //courseDetail by Course Id
                ds.Tables.Add(dtcomment); // user comments ->only 3
                ds.Tables.Add(dtchapter); // chapter Details according to course id
                ds.Tables.Add(dt2); // select Total Enroll Student according to course
                if (dt.Rows.Count > 0)
                {
                    return View(ds);
                }
                return Content("<script>alert('Relatable Course not found');location.href='/home/index#courseDiv'</script>");

            }
            else
            {
                return Content("<script>alert('Relatable Course not found');location.href='/home/index#courseDiv'</script>");
            }
        }

        //select Course By category Wise--------------------------------------------------------------------------------
        public ActionResult CategoryWiseCourse(int? catId)
        {
            DataTable dt = db.ExecuteSelect("sp_selectCourseWiseCategory", new SqlParameter[] { new SqlParameter("@action", 1) }); //select all courses

            DataTable dtCourse;
            if (catId.HasValue)// category Wise
            {
                dtCourse = db.ExecuteSelect("sp_selectCourseWiseCategory", new SqlParameter[] { new SqlParameter("@action", 3), new SqlParameter("@catId", catId) });
            }
            else //without category wise
            {
                dtCourse = db.ExecuteSelect("sp_selectCourseWiseCategory", new SqlParameter[] { new SqlParameter("@action", 2) });

            }
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.Tables.Add(dtCourse);
            return View(ds);
        }

        //get data from video table according to course id and chapter name using ajax-------------------------------------

        public JsonResult selectVideoChapterWise(string chapter, int? id)
        {
            SqlParameter[] prms = new SqlParameter[]
            {
                new SqlParameter("@chapter",chapter),
                new SqlParameter("@course",id),
                new SqlParameter("@action",5)
            };
            DataTable dt = db.ExecuteSelect("sp_video", prms);
            List<VideoData> list = new List<VideoData>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new VideoData()
                {
                    id = Convert.ToInt32(dr["course"]),
                    pic = dr["thumbnail"].ToString(),
                    coursetitle = dr["title"].ToString(),
                    video = dr["link"].ToString(),
                    duration = Convert.ToInt32(dr["duration"]),
                    totalcourse = Convert.ToInt32(dr["totalcourse"]),
                    totalduration = Convert.ToInt32(dr["totalduration"]),
                    ispaid = Convert.ToInt32(dr["ispaid"])
                }); ;
            };
            return Json(list);
        }

        //Get Feedback From User for saperate course------------------------------------------------------------------------
        [HttpPost]
        public JsonResult feedback(Feedback d)
        {
            SqlParameter[] prms = new SqlParameter[]
            {
                new SqlParameter("@fullname",d.fname),
                new SqlParameter("@email",d.email),
                new SqlParameter("@courseId",d.courseId),
                new SqlParameter("@subject",d.subject),
                new SqlParameter("@comment",d.comment),
                new SqlParameter("@action",1)
            };

            object res = db.ExecuteScalar("sp_courseFeedback", prms);

            return Json(res.ToString());
        }

        //My Comment Page where all user Comments Are Shown--------------------------------------------
        public ActionResult myComments(int? id)
        {
            if (id.HasValue)
            {
                int res = db.ExecuteDML("sp_courseFeedback", new SqlParameter[] { new SqlParameter("@action", 3), new SqlParameter("@id", id) });
                if (res > 0)
                {
                    return Content("<script>alert('Comment Deleted Successfully');location.href='/home/myComments'</script>");
                }
                else
                {
                    return Content("<script>alert('Error While Deleting Comment');location.href='/home/myComments'</script>");

                }
            }

            DataTable dt = db.ExecuteSelect("sp_courseFeedback", new SqlParameter[] { new SqlParameter("@action", 6), new SqlParameter("@email", User.Identity.Name) });
            return View(dt);
        }

        //Invoice---------------------------------------------------------------------------
        [Authorize(Roles = "student")]
        public ActionResult Invoice(int? course_id,string getReceipt)
        {

            if (course_id.HasValue)
            {
                DataTable dt = db.ExecuteSelect("sp_Offers", new SqlParameter[] { new SqlParameter("@course", course_id), new SqlParameter("@email", User.Identity.Name), new SqlParameter("@action", 2) });
                if (dt.Rows.Count > 0)
                {
                    int totalamount = 0;
                    if (!dt.Rows[0]["email1"].Equals(DBNull.Value))
                    {
                        totalamount = Convert.ToInt32(dt.Rows[0]["finalfee"]) - Convert.ToInt32(dt.Rows[0]["DiscountAmount"]);
                    }
                    else
                    {
                     totalamount = Convert.ToInt32(dt.Rows[0]["fees"]) - Convert.ToInt32(dt.Rows[0]["discount"]);

                    }
                    var key = ConfigurationManager.AppSettings["KeyId"].ToString();
                    var secret = ConfigurationManager.AppSettings["SecretId"].ToString();
                    RazorpayClient client = new RazorpayClient(key, secret);
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    options.Add("amount", Convert.ToDecimal(totalamount) * 100);
                    options.Add("currency", "INR");
                    Order order = client.Order.Create(options);
                    ViewBag.OrderId = order["id"].ToString();
                    return View(dt.Rows[0]);
                }
                return Content("<script>alert('Relatable Course not found');location.href='/home/index#courseDiv'</script>");

            }
            else
            {
                return RedirectToAction("index");
            }


        }

        //manage payment gateway-------------------------------------------------------------------

        public ActionResult ManagePayment(string paymentId, string orderId, string signature)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            attributes.Add("razorpay_payment_id", paymentId);
            attributes.Add("razorpay_order_id", orderId);
            attributes.Add("razorpay_signature", signature);
            try
            {
                Utils.verifyPaymentSignature(attributes);
                return Json("Payment Successfull.", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Content("<script>alert('Payment not successfull.'); location.href='#';</script>");
            }
        }


        // Enrolled Student using ajax----------------------------------------------------------------
        public JsonResult AddEnrollStudent(string orderid,int? courseId, string email, int? finalFee, string expiryDate)
        {
            SqlParameter[] prms = new SqlParameter[]
            {
new SqlParameter("@orderid",orderid),
                new SqlParameter("@courseId",courseId),
                new SqlParameter("@email",email),
                new SqlParameter("@finalFee",finalFee),
                new SqlParameter("@expiryDate",expiryDate),
                new SqlParameter("@action",1)
            };

            object res = db.ExecuteScalar("sp_enroll", prms);
            return Json(res.ToString());
        }

        //Remove Enrolled Courses according to user---------------------------------------------------------
        public ActionResult removeEnroll(int? enrollId)
        {
            if (enrollId.HasValue)
            {
                int res = db.ExecuteDML("sp_enroll", new SqlParameter[] { new SqlParameter("@action", 3), new SqlParameter("@id", enrollId) });
                return RedirectToAction("myCourse");

            }
            else
            {
                return RedirectToAction("myCourse");
            }
        }

        //----------------------Handle Bad Request----------------------------------------------------------------------

        public ActionResult DefaultPage()
        {
            return View();
        }

        //---------------------------------------------------instructor details Page-------------------------------------------------
        public ActionResult InstrutorDetails(int? instId)
        {
            DataTable dt = db.ExecuteSelect("sp_selectInstructor", new SqlParameter[] { new SqlParameter("@action", 1) });
            DataSet ds = new DataSet();
            if (dt.Rows.Count > 0)
            {
                int instructorId;
                if (instId.HasValue)
                {
                    instructorId = Convert.ToInt32(instId);
                }
                else
                {
                    instructorId = Convert.ToInt32(dt.Rows[0]["instructorId"]);
                }
                DataTable dt2 = db.ExecuteSelect("sp_selectInstructor", new SqlParameter[] { new SqlParameter("@action", 4), new SqlParameter("id", instructorId) });
                ds.Tables.Add(dt2);

            }
            ds.Tables.Add(dt);
            return View(ds);
        }
     

    //write csv file------------------------------------------------------------------------------
    //public ActionResult Writecsv()
    //{
    //    DataTable dt = db.ExecuteSelect("sp_selectInstructor", new SqlParameter[] { new SqlParameter("@action", 1) });
    //    List<InstructorData> list = new List<InstructorData>();


    //    foreach (DataRow data in dt.Rows)
    //    {
    //        list.Add(new InstructorData()
    //        {
    //            id = Convert.ToInt32(data["instructorid"]),
    //            name = data["name"].ToString(),
    //            email = data["email"].ToString(),
    //            mobno = Convert.ToInt64(data["mobno"]),
    //            gender = data["gender"].ToString(),
    //            summary = data["profilesummary"].ToString(),
    //            about = data["aboutme"].ToString(),
    //            qualification = data["qualification"].ToString(),
    //            oldpic = data["profilepic"].ToString(),
    //        }) ;

    //    }
    //    var dlist = list.Select(d => new
    //    {
    //        id = d.id,
    //        name = d.name,
    //        email = d.email,
    //        mobno = d.mobno,
    //        gender = d.gender,
    //        summary = d.summary,
    //        about = d.about,
    //        qualification = d.qualification,
    //        oldpic = d.oldpic
    //    }).ToList();

    //    try
    //    {
    //        string path = Server.MapPath("~/Content/csv/");
    //       string[] files= Directory.GetFiles(path);
    //        foreach(var file in files)
    //        {
    //            System.IO.File.Delete(file);
    //        }
    //        Random r = new Random();
    //        string filename =$"instructor{string.Concat(r.Next(100)+"-",DateTime.Today.ToString("dd-MM-yyyy"))}.csv";
    //         path = Server.MapPath("~/Content/csv/") + filename;
    //        var configPersons = new CsvConfiguration(CultureInfo.InvariantCulture)
    //        {
    //            HasHeaderRecord = true
    //        };
    //        using (StreamWriter writer = new StreamWriter(path))
    //        using (CsvWriter csv = new CsvWriter(writer, configPersons))
    //        {
    //            csv.WriteRecords(dlist); 
    //        };
    //        Session["fname"] = filename;

    //    }
    //    catch (Exception exp)
    //    {
    //        throw exp;
    //    }

    //    return RedirectToAction("index");
    //}
}
}
   

