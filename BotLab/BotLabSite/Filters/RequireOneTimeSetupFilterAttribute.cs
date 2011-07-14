using System;
using System.Web.Mvc;
using Compete.Core.Infrastructure;
using Microsoft.Practices.ServiceLocation;

namespace Compete.Site.Filters
{
    public class RequireOneTimeSetupFilterAttribute : ActionFilterAttribute
    {
        readonly IInitialSetup _setup;

        public RequireOneTimeSetupFilterAttribute()
        {
            _setup = ServiceLocator.Current.GetInstance<IInitialSetup>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Boolean isOneTimeSetupPage = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "InitialSetup";
            if (!_setup.IsDone)
            {
                if (!isOneTimeSetupPage)
                    filterContext.Result = new RedirectResult("~/InitialSetup");
            }
            else
            {
                if (isOneTimeSetupPage)
                    filterContext.Result = new RedirectResult("~/Login");
            }
        }
    }
}