using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    public class FilesController : ApiController
    {
        
        // GET api/files
        public IHttpActionResult Get()
        {
            var files = Directory.GetFiles(WebApiApplication.UploadDir, "*.zip", SearchOption.TopDirectoryOnly);
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

        // GET api/files/see.zip
        public HttpResponseMessage Get(string id)
        {
            // Create HTTP Response.
            var res = Request.CreateResponse(HttpStatusCode.OK);
            // Check file id parameter.
            if (string.IsNullOrWhiteSpace(id))
            {
                res.StatusCode = HttpStatusCode.BadRequest;
                res.ReasonPhrase = "No file ID provided.";
                throw new HttpResponseException(res);
            }
            var fp = Path.Combine(WebApiApplication.UploadDir, id);
            if (!File.Exists(fp))
            {
                res.StatusCode = HttpStatusCode.NotFound;
                res.ReasonPhrase = $"File id not found!";
                throw new HttpResponseException(res);
            }
            // Read the File into a Byte Array.
            var bytes = File.ReadAllBytes(fp);
            // Set the Response Content.
            res.Content = new ByteArrayContent(bytes);
            // Set the Response Content Length.
            res.Content.Headers.ContentLength = bytes.LongLength;
            // Set the Content Disposition Header Value and FileName.
            res.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            res.Content.Headers.ContentDisposition.FileName = id;
            // Set the File Content Type as byte stream (browsers will download instead of display).
            res.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return res;
        }

        // POST api/files
        public Task<HttpResponseMessage> Post()
        {
            var request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var provider = new MultipartFormDataStreamProvider(WebApiApplication.UploadDir);
            var task = request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(_ =>
                {
                    return new HttpResponseMessage
                    {
                        Content = new StringContent("File uploaded.")
                    };
                }
            );

            // Create/insert SQL row here

            return task;
        }

        // PUT api/files/5
        public void Put(int id, [FromBody] string value)
        {
            // Create HTTP Response.
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.StatusCode = HttpStatusCode.NotImplemented;
            res.ReasonPhrase = "Not implemented.";
            throw new HttpResponseException(res);
        }

        // DELETE api/files/5
        public void Delete(int id)
        {
            // Create HTTP Response.
            var res = Request.CreateResponse(HttpStatusCode.OK);
            res.StatusCode = HttpStatusCode.NotImplemented;
            res.ReasonPhrase = "Not implemented.";
            throw new HttpResponseException(res);
        }
    }
}
