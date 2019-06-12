using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HRCentral.Services.Children;
using HRCentral.Services.Employees;
using HRCentral.Core.Data;
using System.Linq;

namespace HRCentral.Web.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ImageController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="childrenServices"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="logger"></param>
        public ImageController(ApplicationDbContext applicationDbContext, IChildrenServices childrenServices, IHostingEnvironment hostingEnvironment, ILogger<ImageController> logger)
        {
            _db = applicationDbContext;
            _env = hostingEnvironment;
            _logger = logger;


        }
        /// <summary>
        /// Loads the child birth certficates
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<FileStreamResult> Children(Guid id)
        {
            var q = from c in _db.EmployeeChildrens where c.Id == id select c;
            var k = q.ToList()[0].BirthCertificateImage.ToString();
           
                byte[] faceImageBytes = Convert.FromBase64String(k);

                Stream stream = new MemoryStream(faceImageBytes);
                var imageFileStreamResult = new FileStreamResult(stream, "application/pdf");

                return imageFileStreamResult;




            
        }
        /// <summary>
        /// Loads the Emplopee Image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<FileStreamResult> Employee(Guid id)
        {
            var q = from c in _db.Employees where c.Id == id select c;
            var k = q.ToList()[0].EmployeeImage.ToString();
           

                byte[] faceImageBytes = Convert.FromBase64String(k);

                Stream stream = new MemoryStream(faceImageBytes);
                var imageFileStreamResult = new FileStreamResult(stream, "image/jpeg");

                return imageFileStreamResult;
            



        }

        /// <summary>
        /// Loads the education certificates
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<FileStreamResult> Education(Guid id)
        {
            var q = from c in _db.EducationalDetails where c.Id == id select c;
            var k = q.ToList()[0].DocumentImage.ToString();

            byte[] pdfBytes = Convert.FromBase64String(k);

            Stream stream = new MemoryStream(pdfBytes);
            var pdfFileStreamResult = new FileStreamResult(stream, "application/pdf");
            return pdfFileStreamResult;




        }
    }
}