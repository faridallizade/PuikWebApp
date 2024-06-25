using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Areas.Admin.DTOs;
using WebUI.Data;
using WebUI.Helpers;
using WebUI.Models;

namespace WebUI.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
	[Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDTO product)
        {
            if (ModelState.IsValid)
            {
                var prod = new Product
                {
                    Name = product.Name,
                    Price = product.Price,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                };

                if (FileManager.CheckLength(product.File, 3) && FileManager.CheckType(product.File, "image/"))
                {
                    prod.ImgUrl = FileManager.Upload(product.File, _env.WebRootPath, @"\Upload\ProductImages\");
                }
                else
                {
                    ModelState.AddModelError("File", "Only photo files below 3 mb allowed");

                    return View(product);
                }

                _context.Products.Add(prod);
                await _context.SaveChangesAsync();


                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var prod = new UpdateProductDTO
            {
                ID = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImgUrl
            };

            return View(prod);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateProductDTO product)
        {
            if (product.ID <= 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var prod = await _context.Products.FindAsync(product.ID);
                if (prod == null)
                {
                    return NotFound();
                }

                prod.Name = product.Name;
                prod.Price = product.Price;
                prod.UpdateDate = DateTime.Now;


                if (FileManager.CheckLength(product.File, 3) && FileManager.CheckType(product.File, "image/"))
                {
                    FileManager.Delete(prod.ImgUrl, _env.WebRootPath, @"\Upload\ProductImages\");

                    prod.ImgUrl = FileManager.Upload(product.File, _env.WebRootPath, @"\Upload\ProductImages\");
                }
                else
                {

                    ModelState.AddModelError("File", "Only photo files below 3 mb allowed");
                    return View(product);
                }

                _context.Products.Update(prod);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
