using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment) 
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index()
        {
             var products = context.Products.OrderBy(p => p.Id).ToList();

            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDetail productDetail)
        {
            if(productDetail.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }
            if(!ModelState.IsValid)
            {
                return View(productDetail);
            }

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDetail.ImageFile!.FileName);

            string imageFullPath=environment.WebRootPath +"/ProductImages/" +newFileName;
            using(var stream=System.IO.File.Create(imageFullPath))
            {
                productDetail.ImageFile.CopyTo(stream);
            }

            Product product = new Product()
            {
                Name = productDetail.Name,
                Brand = productDetail.Brand,
                Category = productDetail.Category,
                Price = productDetail.Price,
                Description = productDetail.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,

            };
            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index","Products");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            var productDetail = new ProductDetail()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,

            };
            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreateAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDetail);
        }
        [HttpPost]
        public IActionResult Edit(int id, ProductDetail productDetail)
        {
            var product = context.Products.Find(id);
            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if(!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreateAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

                return View(productDetail);
            }
            //update the image file if we have a new image file
            string newFileName= product.ImageFileName;
            if(productDetail.ImageFile != null)
            {

                 newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDetail.ImageFile!.FileName);

                string imageFullPath = environment.WebRootPath + "/ProductImages/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDetail.ImageFile.CopyTo(stream);
                }
                //delete the old image
                string oldImageFukkPath = environment.WebRootPath + "/ProductImages/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFukkPath);

            }
            //Update the product in the database
            product.Name = productDetail.Name;
            product.Brand = productDetail.Brand;
            product.Category = productDetail.Category;
            product.Price = productDetail.Price;
            product.Description = productDetail.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }
          public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFullPath= environment.WebRootPath +"/ProductImages/"+ product.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            context.Products.Remove(product);
            context.SaveChanges(true);
            return RedirectToAction("Index", "Products");
        }

    }
}
