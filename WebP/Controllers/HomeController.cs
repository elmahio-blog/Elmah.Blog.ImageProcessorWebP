using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageProcessor;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebP.Models;

namespace WebP.Controllers
{
    public class HomeController : Controller
    {
        IHostingEnvironment hostingEnvironment;

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
            if (image == null) return View();
            if (image.Length < 1) return View();
            string[] allowedImageTypes = new string[] { "image/jpeg", "image/png" };
            if (!allowedImageTypes.Contains(image.ContentType.ToLower())) return View();

            // Prepare paths for saving images
            string imagesPath = Path.Combine(hostingEnvironment.WebRootPath, "images");
            string webPFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".webp";
            string normalImagePath = Path.Combine(imagesPath, image.FileName);
            string webPImagePath = Path.Combine(imagesPath, webPFileName);

            // Save the image in its original format for fallback
            using (var normalFileStream = new FileStream(normalImagePath, FileMode.Create))
            {
                image.CopyTo(normalFileStream);
            }

            // Then save in WebP format
            using (var webPFileStream = new FileStream(webPImagePath, FileMode.Create))
            {
                using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                {
                    imageFactory.Load(image.OpenReadStream())
                                .Format(new WebPFormat())
                                .Quality(50)
                                .Save(webPFileStream);
                }
            }

            Images viewModel = new Images();
            viewModel.NormalImage = "/images/" + image.FileName;
            viewModel.WebPImage = "/images/" + webPFileName;

            return View(viewModel);
        }
    }
}
