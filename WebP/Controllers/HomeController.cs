using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using WebP.Models;

namespace WebP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile image)
        {
            // Check if valid image type (can be extended with more rigorous checks)
            if (image == null)
            {
                return View();
            }

            if (image.Length < 1)
            {
                return View();
            }

            string[] allowedImageTypes = new string[] { "image/jpeg", "image/png" };
            if (!allowedImageTypes.Contains(image.ContentType.ToLower()))
            {
                return View();
            }

            // Prepare paths for saving images
            string imagesPath = Path.Combine(hostingEnvironment.WebRootPath, "images");
            string webPFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".webp";
            string normalImagePath = Path.Combine(imagesPath, image.FileName);
            string webPImagePath = Path.Combine(imagesPath, webPFileName);

            // Save the image in its original format for fallback
            using (FileStream normalFileStream = new FileStream(normalImagePath, FileMode.Create))
            {
                image.CopyTo(normalFileStream);
            }

            // Then save in WebP format
            using (FileStream webPFileStream = new FileStream(webPImagePath, FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(image.OpenReadStream())
                                .Format(new WebPFormat())
                                .Quality(50)
                                .Save(webPFileStream);
                }
            }

            Images viewModel = new Images
            {
                NormalImage = "/images/" + image.FileName,
                WebPImage = "/images/" + webPFileName
            };

            return View(viewModel);
        }
    }
}
