using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sozif.Attributes
{
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Cookies.ContainsKey("AUTH") || context.HttpContext.Request.Cookies["AUTH"] != "OK" || context.HttpContext.Session.GetString("user") != "OK")
            {
                context.HttpContext.Response.Redirect("Login/Index");
            }
            base.OnActionExecuting(context);
        }
    }
}
