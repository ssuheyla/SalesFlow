using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SCProject.Models;
using SCProject.Models.Context;

namespace SCProject.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly TestDBContext _testDBContext;

        public PurchaseController(TestDBContext testDBContext)
        {
            _testDBContext = testDBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> getPurchase(DataSourceLoadOptions options)
        {
            var sales = from s in _testDBContext.Purchase
                        join p in _testDBContext.Product on s.PRODUCT_ID equals p.ID
                        join c in _testDBContext.Customer on s.CUSTOMER_ID equals c.ID
                        select new
                        {
                            s.ID,
                            s.PRODUCT_ID,
                            s.CUSTOMER_ID,
                            ProductName = p.NAME,
                            CustomerName = c.CUSTOMERTITLE,
                            s.AMOUNT,
                            s.QUANTITY,
                            s.PRICE,
                            s.DATE
                        };

            return Json(await DataSourceLoader.LoadAsync(sales, options));
        }

        [HttpGet]
        public async Task<IActionResult> getProduct()
        {
            var products = await _testDBContext.Product.Select(p => new
            {
                p.ID,
                p.NAME,
            }).ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _testDBContext.Customer.Select(x => new
            {
                x.ID,
                CUSTOMERTITLE = x.ID + "-" + x.CUSTOMERTITLE
            }).ToListAsync();
            return Json(customers);
        }

        [HttpPost]
        public async Task<IActionResult> AddPurchase(string values)
        {
            var purchase = new Purchase();
            JsonConvert.PopulateObject(values, purchase);

            purchase.DATE ??= DateTime.Now;
            purchase.AMOUNT = (purchase.QUANTITY ?? 0) * (purchase.PRICE ?? 0);

            var newStock = new Stock
            {
                PRODUCT_ID = purchase.PRODUCT_ID,
                QUANTITY = purchase.QUANTITY,
                DATE = purchase.DATE
            };

            _testDBContext.Purchase.Add(purchase);
            _testDBContext.Stock.Add(newStock);

            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePurchase(int key, string values)
        {
            var purchase = await _testDBContext.Purchase.FindAsync(key);
            if (purchase == null) return NotFound();

            JsonConvert.PopulateObject(values, purchase);
            purchase.AMOUNT = (purchase.QUANTITY ?? 0) * (purchase.PRICE ?? 0); //toplam satış hesabı
            await _testDBContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePurchase(int key)
        {
            var purchase = await _testDBContext.Purchase.FindAsync(key);
            if (purchase == null) return NotFound();

            _testDBContext.Purchase.Remove(purchase);
            await _testDBContext.SaveChangesAsync();

            return Ok();
        }


    }
}
