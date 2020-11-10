﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication2
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var uploadDir = Server.MapPath("~/App_Data/Upload");
            System.Diagnostics.Trace.TraceInformation($"Probing '{uploadDir}' for uploads...");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
                System.Diagnostics.Trace.TraceInformation($"Created '{uploadDir}' for uploads!");
            }                
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            System.Diagnostics.Trace.TraceError($"Unhandled exception: {ex}");
        }

    }
}
