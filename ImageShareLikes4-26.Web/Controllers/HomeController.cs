using ImageShareLikes.Data;
using ImageShareLikes4_26.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Text.Json;

namespace ImageShareLikes4_26.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString;
        private IWebHostEnvironment _webHostEnvironment;
        public HomeController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _connectionString = configuration.GetConnectionString("ConStr");
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var repo = new ImageRepository(_connectionString);
            var vm = new HomePageViewModel
            {
                Images = repo.GetAll()
            };
            return View(vm);
        }
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Upload(IFormFile image, string title)
        {
            var imageName = $"{Guid.NewGuid()}-{Path.GetExtension(image.FileName)}";
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", imageName);
            using var fs = new FileStream(imagePath, FileMode.CreateNew);
            image.CopyTo(fs);

            var repo = new ImageRepository(_connectionString);
            repo.Add(new()
            {
                ImageName = imageName,
                Title = title,
                DatePosted = DateTime.Now
            });
            return Redirect("/home/index");
        }
        public IActionResult ViewImage(int id)
        {
            var imageIdsAlreadyLiked = HttpContext.Session.Get<List<int>>("imageIdsAlreadyLiked"); 

            var repo = new ImageRepository(_connectionString);
            var vm = new ViewImageViewModel
            {
                Image = repo.GetImageById(id),
                AlreadyLiked = imageIdsAlreadyLiked != null && imageIdsAlreadyLiked.Contains(id)
            };
            return View(vm);
        }
        [HttpPost]
        public void LikeImage(int id)
        {
            var repo = new ImageRepository(_connectionString);
            repo.LikeImage(id);
            
            var imageIdsAlreadyLiked = HttpContext.Session.Get<List<int>>("imageIdsAlreadyLiked");
            if(imageIdsAlreadyLiked == null)
            {
                imageIdsAlreadyLiked = new List<int>();
            }
            imageIdsAlreadyLiked.Add(id);
            HttpContext.Session.Set<List<int>>("imageIdsAlreadyLiked", imageIdsAlreadyLiked);
        }
        public IActionResult GetLikes(int id)
        {
            var repo = new ImageRepository(_connectionString);
            return Json(new {Likes = repo.GetLikes(id) });
        }
        
    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }
}