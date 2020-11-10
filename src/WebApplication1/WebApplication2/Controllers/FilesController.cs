using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    public class FilesController : ApiController
    {

        /// <summary>
        /// Gets upload dir.
        /// </summary>
        public static string UploadDir
        {
            get { return HttpContext.Current.Server.MapPath("~/App_Data/Upload"); }
        }

        // GET api/files
        public IHttpActionResult Get()
        {
            var files = Directory.GetFiles(UploadDir, "*.zip");
            // Get file system info for each file.
            var res = from _ in files
                      select new FileInfo(_);
            // Return file info as serializable DTO.
            return this.Ok(from _ in res
                    select new 
                    {
                        _.Name,
                        _.FullName,
                        _.Extension,
                        _.Length,
                        _.CreationTime,
                        _.LastWriteTime,                    
                    });
        }

        // GET api/files/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/files
        public void Post([FromBody] string value)
        {
        }

        // PUT api/files/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/files/5
        public void Delete(int id)
        {
        }
    }
}
