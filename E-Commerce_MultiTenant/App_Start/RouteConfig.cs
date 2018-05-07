using E_Commerce_MultiTenant.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace E_Commerce_MultiTenant
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // This will add the parameter "subdomain" to the route parameters
            routes.Add(new SubdomainRoute());

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Homeku",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Indexku", id = UrlParameter.Optional }
           );
           // routes.MapRoute(
           //    name: "Login",
           //    url: "{controller}/{action}/{id}",
           //    defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
           //);

        }
    }
}
