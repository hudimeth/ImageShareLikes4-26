using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageShareLikes.Data
{
    public class ImageRepository
    {
        private string _connectionString;
        public ImageRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<Image> GetAll()
        {
            using var context = new ImageDbContext(_connectionString);
            return context.Images.OrderByDescending(image => image.DatePosted).ToList();
        }
        public void Add(Image image)
        {
            using var context = new ImageDbContext(_connectionString);
            context.Images.Add(image);
            context.SaveChanges();
        }
        public Image GetImageById(int id)
        {
            using var context = new ImageDbContext(_connectionString);
            return context.Images.FirstOrDefault(image => image.Id == id);
        }
        public void LikeImage(int id)
        {
            using var context = new ImageDbContext(_connectionString);
            var image = context.Images.FirstOrDefault(image => image.Id == id);
            image.Likes++;
            context.SaveChanges();
        }
        public int GetLikes(int id)
        {
            using var context = new ImageDbContext(_connectionString);
            return context.Images.Where(image => image.Id == id).Select(image => image.Likes).FirstOrDefault();
        }
    }
}
