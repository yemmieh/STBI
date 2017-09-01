using BioData_Update.Models;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace BioData_Update.Controllers
{
    public class LoginController : Controller
    {

        //[HttpGet]
        public ActionResult Login(string returnUrl)
        {
            //So that the user can be referred back to where they were when they click logon
            if (string.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null)
                returnUrl = Server.UrlEncode(Request.UrlReferrer.PathAndQuery);

            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnURL = returnUrl;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            /*string decodedUrl = "";
            if (!string.IsNullOrEmpty(returnUrl)) {
                decodedUrl = Server.UrlDecode(returnUrl);
            }*/

            try
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (this.Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return this.Redirect(returnUrl);
                    }

                    Session["UserName"] = @User.Identity.Name;

                    return this.RedirectToAction("AwaitingMyApproval", "AwaitingApproval");

                }
                this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }
            return this.View(model);
        }

        public ActionResult LogOff() {
            FormsAuthentication.SignOut();
            bool checkApproverUser = false;
            ViewData["checkApproverUser"] = checkApproverUser;

            return this.RedirectToAction("Index", "Home");
        }
    }
}