using System.IO;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace TestApi.FineUploader
{
    [ModelBinder(typeof(ModelBinder))]
    public class FineUpload
    {
        public int Part { get; set; }
        public int TotalParts { get; set; }
        public string Filename { get; set; }
        public string Uuid { get; set; }
        public Stream FileStream { get; set; }

        public void SaveAs(string destination, bool overwrite = false, bool autoCreateDirectory = true)
        {
            if (autoCreateDirectory)
            {
                var directory = new FileInfo(destination).Directory;
                if (directory != null) directory.Create();
            }

            using (var file = new FileStream(destination, overwrite ? FileMode.Create : FileMode.CreateNew))
                FileStream.CopyTo(file);
        }

        public class ModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext controllerContext, ModelBindingContext bindingContext)
            {
                var request = HttpContext.Current.Request;
                var formUpload = request.Files.Count > 0;

                // find filename
                var fileName = request["qqfilename"];
                var part = int.Parse(request["qqpartindex"]);
                var totalParts = int.Parse(request["qqtotalparts"]);
                var uuid = request["qquuid"];

                bindingContext.Model = new FineUpload
                {
                    Filename = fileName,
                    Part = part,
                    TotalParts = totalParts,
                    Uuid = uuid,
                    FileStream = formUpload ? request.Files[0].InputStream : request.InputStream
                };

                return true;
            }

        }

    }
}