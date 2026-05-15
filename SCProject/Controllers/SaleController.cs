using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SCProject.Models;
using SCProject.Models.Context;

namespace SCProject.Controllers
{
    public class SaleController : Controller
    {
        private readonly TestDBContext _testDBContext;

        public SaleController(TestDBContext testDBContext)
        {
            _testDBContext = testDBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> getSales(DataSourceLoadOptions options)
        {
            var sales = from s in _testDBContext.Sales
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
                            s.LISTPRICE,
                            s.DISCOUNTRATE,
                            s.SALESPRICE,
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
                p.SALESPRICE // Liste fiyatı olarak kullanılacak
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

        [HttpGet]
        //ürüne göre stock ve price bilgisini getir
        public async Task<IActionResult> GetProductPriceStock(int productId)
        {
            var product = await _testDBContext.Product.FindAsync(productId);
            if (product == null) return NotFound();

            var stock = await StockControl(productId);
            return Json(new { product.SALESPRICE, STOCK = stock });
        }


        [HttpPost]
        public async Task<IActionResult> AddSale(string values)
        {
            var sale = new Sales();
            JsonConvert.PopulateObject(values, sale);
            sale.DATE ??= DateTime.Now;

            //stok kontrol
            if (sale.PRODUCT_ID.HasValue && sale.QUANTITY.HasValue)
            {
                var stock = await StockControl(sale.PRODUCT_ID.Value);
                if (stock < sale.QUANTITY.Value)
                {
                    return BadRequest($"stok yetersiz, mevcut stok: {stock}");
                }
            }
            var newStock = new Stock
            {
                PRODUCT_ID = sale.PRODUCT_ID,
                QUANTITY = -(sale.QUANTITY), // negatif olarak kaydedildi satış yapılıdğı için
                DATE = DateTime.Now
            };

            _testDBContext.Sales.Add(sale);
            _testDBContext.Stock.Add(newStock);
            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

        private async Task<double> StockControl(int productId)
        {
            return await _testDBContext.Stock.Where(s => s.PRODUCT_ID == productId).SumAsync(s => s.QUANTITY != null ? s.QUANTITY : 0) ?? 0;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSale(int key, string values)
        {
            var sale = await _testDBContext.Sales.FindAsync(key);
            if (sale == null) return NotFound();

            JsonConvert.PopulateObject(values, sale);

            // Ürün değişiminde listprice güncelle
            if (sale.PRODUCT_ID.HasValue)
            {
                var product = await _testDBContext.Product.FindAsync(sale.PRODUCT_ID.Value);
                if (product != null)
                {

                    sale.LISTPRICE = product.SALESPRICE;
                }
            }

            // iskonto yeniden hesapla
            if (sale.LISTPRICE.HasValue && sale.SALESPRICE.HasValue && sale.LISTPRICE.Value > 0)
            {
                sale.DISCOUNTRATE = Math.Round(((sale.LISTPRICE.Value - sale.SALESPRICE.Value) / sale.LISTPRICE.Value) * 100, 2);
            }

            // toplamtutar yeniden hesapla
            if (sale.QUANTITY.HasValue && sale.SALESPRICE.HasValue)
            {
                sale.AMOUNT = sale.QUANTITY.Value * sale.SALESPRICE.Value;
            }

            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSale(int key)
        {
            var sale = await _testDBContext.Sales.FindAsync(key);
            if (sale == null) return NotFound();

            _testDBContext.Sales.Remove(sale);
            await _testDBContext.SaveChangesAsync();
            return Ok();
        }

    }
}
