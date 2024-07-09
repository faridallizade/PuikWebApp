﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Areas.Admin.DTOs;
using WebUI.Areas.Admin.ViewModels;
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



        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var viewModel = new CreateProductVM
            {
                Product = new CreateProductDTO(),
                Categories = categories
            };
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM viewModel)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = viewModel.Product.Name,
                    Price = viewModel.Product.Price,
                    CreatedAt = DateTime.Now,
                    CategoryId = viewModel.Product.CategoryId
                };

                if (FileManager.CheckLength(viewModel.Product.File, 3) && FileManager.CheckType(viewModel.Product.File, "image/"))
                {
                    product.ImgUrl = FileManager.Upload(viewModel.Product.File, _env.WebRootPath, @"\Upload\ProductImages\");
                }
                else
                {
                    ModelState.AddModelError("Product.File", "Only photo files below 3 mb allowed");
                    viewModel.Categories = await _context.Categories.ToListAsync(); 
                    return View(viewModel);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await _context.Categories.ToListAsync(); 
            return View(viewModel);
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

            var categories = await _context.Categories.ToListAsync();
            var viewModel = new UpdateProductVM
            {
                Product = new UpdateProductDTO
                {
                    ID = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImgUrl,
                    CategoryId = product.CategoryId
                },
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateProductVM viewModel)
        {
            if (viewModel.Product.ID <= 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(viewModel.Product.ID);
                if (product == null)
                {
                    return NotFound();
                }

                product.Name = viewModel.Product.Name;
                product.Price = viewModel.Product.Price;
                product.CategoryId = viewModel.Product.CategoryId;

                if (FileManager.CheckLength(viewModel.Product.File, 3) && FileManager.CheckType(viewModel.Product.File, "image/"))
                {
                    FileManager.Delete(product.ImgUrl, _env.WebRootPath, @"\Upload\ProductImages\");
                    product.ImgUrl = FileManager.Upload(viewModel.Product.File, _env.WebRootPath, @"\Upload\ProductImages\");
                }
                else
                {
                    ModelState.AddModelError("Product.File", "Only photo files below 3 mb allowed");
                    viewModel.Categories = await _context.Categories.ToListAsync(); 
                    return View(viewModel);
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            viewModel.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
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
