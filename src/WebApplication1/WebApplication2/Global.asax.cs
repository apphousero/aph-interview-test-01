using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication2.Tasks;

namespace WebApplication2
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        /// <summary>
        /// Gets upload dir.
        /// </summary>
        public static string UploadDir { get; private set; } = null;

        /// <summary>
        /// Gets zip processing task.
        /// </summary>
        public ZipProcessingTask ZipProcessingTask { get; private set; } = null;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Get upload dir path.
            var uploadDir = Server.MapPath("~/App_Data/Upload");
            // Set it as globally available in this static property.
            UploadDir = uploadDir;
            // Trace something about it.
            System.Diagnostics.Trace.TraceInformation($"Probing '{uploadDir}' for uploads...");
            // Create folder if it does not exist.
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
                System.Diagnostics.Trace.TraceInformation($"Created '{uploadDir}' for uploads!");
            }
            // Start processing task.
            ZipProcessingTask = new ZipProcessingTask();
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            System.Diagnostics.Trace.TraceError($"Unhandled exception: {ex}");
        }

    }
}
