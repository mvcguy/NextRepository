﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Repository.MsSql;
using Repository.MySql;

namespace NextRepository.WebSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMySqlRepository _mySqlRepository;
        private readonly IMsSqlRepository _msSqlRepository;

        public HomeController(IMySqlRepository mySqlRepository, IMsSqlRepository msSqlRepository)
        {
            _mySqlRepository = mySqlRepository;
            _msSqlRepository = msSqlRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
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
