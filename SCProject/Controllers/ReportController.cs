using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCProject.Models.Context;

namespace SCProject.Controllers
{
    public class ReportController : Controller
    {
        private readonly TestDBContext _testDBContext;

        public ReportController(TestDBContext testDBContext)
        {
            _testDBContext = testDBContext;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        public IActionResult ReportStock()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ReportSales()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStockReportData()
        {
            var report = await (from s in _testDBContext.Stock
                                join p in _testDBContext.Product on s.PRODUCT_ID equals p.ID
                                join c in _testDBContext.Category on p.CATEGORY_ID equals c.ID
                                group s by new
                                {
                                    s.PRODUCT_ID,
                                    ProductName = p.NAME,
                                    CategoryName = c.NAME
                                } into g
                                select new
                                {
                                    productId = g.Key.PRODUCT_ID,
                                    categoryName = g.Key.CategoryName,
                                    productName = g.Key.ProductName,
                                    totalStock = g.Sum(x => x.QUANTITY != null ? x.QUANTITY : 0)
                                }).ToListAsync();
            return Json(report);

        }




        public async Task<IActionResult> GetSalesReportData(DateTime? startDate, DateTime? endDate)
        {
            var sales = await (from s in _testDBContext.Sales
                               join p in _testDBContext.Product on s.PRODUCT_ID equals p.ID
                               join c in _testDBContext.Category on p.CATEGORY_ID equals c.ID
                               where (!startDate.HasValue || s.DATE >= startDate.Value) && (!endDate.HasValue || s.DATE <= endDate.Value)
                               group s by new { Kategori = c.NAME, Urun = p.NAME } into g
                               orderby g.Sum(x => x.QUANTITY != null ? x.QUANTITY : 0) descending
                               select new
                               {
                                   Kategori = g.Key.Kategori,
                                   Urun = g.Key.Urun,
                                   ToplamMiktar = g.Sum(x => x.QUANTITY != null ? x.QUANTITY : 0),
                                   ToplamTutar = g.Sum(x => x.AMOUNT != null ? x.AMOUNT : 0)
                               }).ToListAsync();

            return Json(sales);
        }

    }
}
