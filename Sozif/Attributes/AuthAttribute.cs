using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sozif.Attributes
{
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string CookieToken = context.HttpContext.Request.Cookies["AUTH"];
            string SessionToken = context.HttpContext.Session.GetString("AUTH");

            if (CookieToken == null || SessionToken == null || CookieToken != SessionToken)
            {
                context.HttpContext.Response.Redirect("Login/Index");
            }
            base.OnActionExecuting(context);
        }
    }
}
