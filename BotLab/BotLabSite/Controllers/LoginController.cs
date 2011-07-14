using System;
using System.Web.Mvc;
using Compete.Site.Infrastructure;
using Compete.Site.Filters;

namespace Compete.Site.Controllers
{
    [RequireOneTimeSetupFilter]
    public class LoginController : Controller
    {
        readonly ISignin _signin;

        public LoginController(ISignin signin)
        {
            _signin = signin;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            if (_signin.IsSignedIn)
            {
                return Redirect("~/Home");
            }

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Index(string password)
        {
            if (String.IsNullOrEmpty(password))
                ModelState.AddModelError("password", "Entry required");
            else if (!_signin.Signin(password))
                ModelState.AddModelError("password", "Incorrect password; try again");

            if (!ModelState.IsValid)
                return View();
            else
                return Redirect("~/Home");
        }
    }
}