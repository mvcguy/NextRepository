using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using NextRepository.WebSample.AppConstants;
using NextRepository.WebSample.Models;
using NextRepository.WebSample.Services;
using Repository.MsSql;
using Repository.MySql;

namespace NextRepository.WebSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMySqlRepository _mySqlRepository;
        private readonly IMsSqlRepository _msSqlRepository;
        private readonly AppQueriesService _appQueriesService;

        public HomeController(IMySqlRepository mySqlRepository, IMsSqlRepository msSqlRepository, AppQueriesService appQueriesService)
        {
            _mySqlRepository = mySqlRepository;
            _msSqlRepository = msSqlRepository;
            _appQueriesService = appQueriesService;
        }

        public IActionResult Index()
        {
            try
            {
                ViewData["RepoType"] = "MS SQL";
                var products = _msSqlRepository.Query<Product>(_appQueriesService.MsSqlQueries[DatabaseConstants.GetProducts] as string).ToList();
                return View(products);
            }
            catch (Exception)
            {
                return View("Error");
            }

        }

        public IActionResult MySql()
        {

            try
            {
                ViewData["RepoType"] = "My SQL";
                var products = _mySqlRepository.Query<Product>(_appQueriesService.MySqlQueries[DatabaseConstants.GetProducts] as string).ToList();
                return View(products);
            }
            catch (Exception)
            {
                return View("Error");
            }

        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View(new Product());
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var query = _appQueriesService.MsSqlQueries[DatabaseConstants.InsertProduct] as string;

                    var aRows = _msSqlRepository.NonQuery(query, paramCollection: product);
                    if (aRows > 0)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "No record is saved. Try again later.");
                    }

                }
            }
            catch (Exception excep)
            {
                ModelState.AddModelError("", excep.Message);
            }

            return View(product);
        }

        [HttpGet]
        public IActionResult CreateProductMySql()
        {
            return View(new Product());
        }

        [HttpPost]
        public IActionResult CreateProductMySql(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var query = _appQueriesService.MySqlQueries[DatabaseConstants.InsertProduct] as string;

                    var aRows = _mySqlRepository.NonQuery(query, paramCollection: product);
                    if (aRows > 0)
                    {
                        return RedirectToAction("MySql", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "No record is saved. Try again later.");
                    }

                }
            }
            catch (Exception excep)
            {
                ModelState.AddModelError("", excep.Message);
            }

            return View(product);
        }

        public IActionResult Search(string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index", "Home");

            query = string.Format("%{0}%", query);

            var sql = "SELECT * FROM NextDatalayerWeb.dbo.Products WHERE Name like @query OR Description like @query";
            var records = _msSqlRepository.Query<Product>(sql, paramValueCollection: new { query });
            
            return View("Search", records);
        }

        public IActionResult SearchMySql(string query = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("MySql", "Home");

            query = string.Format("%{0}%", query);

            var sql = "SELECT * FROM NextDatalayerWeb.Products WHERE Name like @query OR Description like @query";
            var records = _mySqlRepository.Query<Product>(sql, paramValueCollection: new { query });

            return View("SearchMySql", records);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
