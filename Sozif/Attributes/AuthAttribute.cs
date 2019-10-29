using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sozif.Attributes
{
    public class AuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Cookies.ContainsKey("AUTH") || context.HttpContext.Request.Cookies["AUTH"] != "OK")
            {
                context.HttpContext.Response.Redirect("Login/Index");
            } else
            {
                context.HttpContext.Session.SetString("user", "OK");
            }
            base.OnActionExecuting(context);
        }
    }
}
