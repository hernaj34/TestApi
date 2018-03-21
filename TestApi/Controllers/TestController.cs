using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using TestApi.FineUploader;

namespace TestApi.Controllers
{
    public class TestController : ApiController
    {
        public async Task<IHttpActionResult> Get()
        {
            return Ok(new { message = "Hello world" });
        }


        public async Task<IHttpActionResult> Upload(FineUpload upload)
        {
            string uploadedTemp =  HttpContext.Current.Server.MapPath("~/App_Data/" + "TEMP/");
            string uploadedLocation = HttpContext.Current.Server.MapPath("~/App_Data/");
            string filePath = System.IO.Path.Combine(uploadedTemp, upload.Uuid + "." + upload.Part.ToString().PadLeft(3,'0') + ".tmp");

            if (!System.IO.File.Exists(filePath))
            {

                using (System.IO.FileStream fileStream = System.IO.File.OpenWrite(filePath))
                {
                    upload.FileStream.CopyTo(fileStream);
                }
            }

            if (upload.Part == upload.TotalParts - 1) // all chunks have arrived
            {
                mergeTempFiles(uploadedTemp, upload.Uuid, uploadedLocation, upload.Filename);
            }
            return Ok(new { success = true });
        }
        [HttpDelete]
        public async Task<IHttpActionResult> Upload(string id)
        {
            return Ok(new { success = true });
        }

        public void mergeTempFiles(string pathOrigin, string file, string pathToSave, string filename)
        {
            string[] tmpfiles = System.IO.Directory.GetFiles(pathOrigin, file + ".*.tmp");

            if (!System.IO.Directory.Exists(pathToSave))
            {
                System.IO.Directory.CreateDirectory(pathToSave);
            }
            System.IO.FileStream outPutFile = new System.IO.FileStream(pathToSave + filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            foreach (string tempFile in tmpfiles)
            {
                int bytesRead = 0;
                byte[] buffer = new byte[1048576];
                System.IO.FileStream inputTempFile = new System.IO.FileStream(tempFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Read);
                while ((bytesRead = inputTempFile.Read(buffer, 0, 1048576)) > 0)
                { 
                    outPutFile.Write(buffer, 0, bytesRead);
                }
                inputTempFile.Close();
                System.IO.File.Delete(tempFile);
            }
            outPutFile.Close();
        }
    }
}
