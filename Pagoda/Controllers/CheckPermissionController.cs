using System.Web;
using System.Web.Mvc;

public class CheckPermission : AuthorizeAttribute
{
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        return httpContext.Session["MaNhomSession"].ToString() == "1";
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.Result = new RedirectResult("/");
    }
}