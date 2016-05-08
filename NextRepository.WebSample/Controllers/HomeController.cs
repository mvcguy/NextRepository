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

        public HomeController(IMySqlRepository mySqlRepository, IMsSqlRepository msSqlRepository,AppQueriesService appQueriesService)
        {
            _mySqlRepository = mySqlRepository;
            _msSqlRepository = msSqlRepository;
            _appQueriesService = appQueriesService;
        }

        public IActionResult Index()
        {
            ViewData["RepoType"] = "My SQL";
            var products = _mySqlRepository.Query<Product>(_appQueriesService.MySqlQueries[DatabaseConstants.GetProducts] as string).ToList();
            return View(products);
        }

        public IActionResult MsSql()
        {
            ViewData["RepoType"] = "MS SQL";
            var products = _msSqlRepository.Query<Product>(_appQueriesService.MsSqlQueries[DatabaseConstants.GetProducts] as string).ToList();
            return View(products);
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
