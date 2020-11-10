using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private HttpClient _client = new HttpClient 
        { 
            BaseAddress = new Uri(ConfigurationManager.AppSettings["api-url"] ?? "http://localhost/WebApplication2/", UriKind.Absolute) 
        };

        public HomeController()
        {
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ActionResult> Index(bool hasUploaded = false)
        {
            ViewBag.hasUploaded = hasUploaded;
            try
            {
                using (var res = await _client.GetAsync("api/files"))
                {
                    var str = res.IsSuccessStatusCode ? (await res.Content.ReadAsStringAsync() ?? "[]") : "[]";
                    var t = JToken.Parse(str);
                    return View(t);
                }
            }
            catch (Exception exc)
            {
                ViewBag.Exception = exc;
                System.Diagnostics.Trace.TraceError($"Exception encountered: {exc}");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(file.InputStream), Path.GetFileNameWithoutExtension(file.FileName), file.FileName);
                    using (var message = await _client.PostAsync("api/files", content))
                    {
                    }
                }

            }
            else
                throw new HttpRequestException("No file!");
            return RedirectToAction("Index", new { hasUploaded = true });
        }

        public async Task<ActionResult> Download(string id)
        {
            using (var res = await _client.GetAsync($"api/files?id={id}"))
            {
                return File(await res.Content.ReadAsByteArrayAsync(), "application/octet-stream", id);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}