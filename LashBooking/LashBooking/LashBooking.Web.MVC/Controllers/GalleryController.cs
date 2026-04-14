using LashBooking.Domain.Entities;
using LashBooking.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LashBooking.Web.MVC.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IRepository<GalleryPhoto> _galleryPhotos;

        public GalleryController(IRepository<GalleryPhoto> galleryPhotos)
        {
            _galleryPhotos = galleryPhotos;
        }

        public async Task<IActionResult> Index()
        {
            var photos = await _galleryPhotos.GetAllAsync();
            var sorted = photos.OrderBy(p => p.SortOrder).ThenByDescending(p => p.UploadedAt).ToList();
            return View(sorted);
        }
    }
}
