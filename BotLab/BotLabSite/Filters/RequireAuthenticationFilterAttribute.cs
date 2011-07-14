using System.Web.Mvc;
using Compete.Core.Infrastructure;
using Microsoft.Practices.ServiceLocation;

namespace Compete.Site.Filters
{
    public class RequireAuthenticationFilterAttribute : RequireOneTimeSetupFilterAttribute
    {
        readonly IFormsAuthentication _formsAuthentication;

        public RequireAuthenticationFilterAttribute()
        {
            _formsAuthentication = ServiceLocator.Current.GetInstance<IFormsAuthentication>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result == null)
            {
                if (!_formsAuthentication.IsCurrentlySignedIn)
                {
                    filterContext.Result = new RedirectResult("~/Login");
                }
            }
        }
    }
}