using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SCProject.Models;
using SCProject.Models.Context;

namespace SCProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly TestDBContext _testDBContext;

        public ProductController(TestDBContext testDBContext)
        {
            _testDBContext = testDBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(DataSourceLoadOptions dataSourceLoadOptions)
        {
            var products = from p in _testDBContext.Product
                           join c in _testDBContext.Category  on p.CATEGORY_ID equals c.ID                       
                           select new
                           {
                               p.ID,
                               p.NAME,
                               p.IMAGE_SRC,
                               p.SALESPRICE,
                               p.CATEGORY_ID,
                               CategoryName = p.CATEGORY_ID != null ? c.NAME : ""
                           };

            return Json(await DataSourceLoader.LoadAsync(products, dataSourceLoadOptions));

        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string values)
        {
            var product = new Product();

            JsonConvert.PopulateObject(values, product);

            if (!TryValidateModel(product))
                return BadRequest(ModelState);

            _testDBContext.Product.Add(product);
            await _testDBContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPut]
        public async Task<IActionResult> UpdateProduct(int key, string values)
        {
            var product = await _testDBContext.Product.FindAsync(key);
            if (product == null) return NotFound();               

            // jsona çevir
            JsonConvert.PopulateObject(values, product);
            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int key)
        {
            var product = await _testDBContext.Product.FindAsync(key);
            if (product == null) return NotFound();            

            _testDBContext.Product.Remove(product);
            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetCategory()
        //{
        //    var categories = await _testDBContext.Category
        //        .Select(c => new
        //        {
        //            id = c.ID,
        //            name = c.NAME
        //        })
        //        .ToListAsync();

        //    return Json(categories);
        //}

        [HttpGet]
        public async Task< IActionResult> GetCategory()
        {
            // dropdown için kategori listesi
            var cat = await _testDBContext.Category.Select(x => new { value = x.ID, text = x.NAME }).ToListAsync();
            return Json(cat);
        }


    }
}
