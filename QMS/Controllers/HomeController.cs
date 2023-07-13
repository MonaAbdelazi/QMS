﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QMS;
using QMS.Data;
using QMS.Models;

namespace VMS.Controllers
{
    public class HomeController : BaseController
    {
        QMSEntities db = new QMSEntities();
        ApplicationDbContext dba = new ApplicationDbContext();
        public JsonResult GetChartData()
        {
            //var data =
            //(from a in db.Company_Type
            // group a by new
            // {
            //     Year = a.LastUpdate.Value.Year,

            // } into g
            // select new
            // {
            //     //  Year = g.Key.Year,

            //     Approved = g.Count(o => o.Status == "Approved"),
            //     Pending = g.Count(o => o.Status == "Pending")
            // }).AsEnumerable().Select(g => new
            // {
            //     // Period = g.Year,
            //     Approved = g.Approved,
            //     Pending = g.Pending
            // }).ToList();
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDonutChartData()
        {
            var data =
             (from a in dba.Users
              group a by new
              {
                  status = a.UserName
              }
                            into g
              select new
              {
                  label = g.Key.status,
                  value = g.Count()
              }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDriverDonutChartData()
        {
            var data = (from a in db.TESTS
                        group a by new
                        {
                            status = a.STATUS
                        }
                            into g
                        select new
                        {
                            label = g.Key.status,
                            value = g.Count()
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSpareDonutChartData()
        {
            var data = (from a in db.DiposedBatchs
                        group a by new
                        {
                            status = a.STATUS
                        }
                            into g
                        select new
                        {
                            label = g.Key.status,
                            value = g.Count()
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}