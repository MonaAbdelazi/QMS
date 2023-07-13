using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QMS.Tools;
using QMS.Data;
using QMS.Utility;
using QMS.Core.Resources;
using System.Data.Entity.Validation;
using QMS.ViewModels;

namespace QMS.Controllers
{
    public class DriversController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "drivers";

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.DRIVERS.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }
              

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session[sessionName] = null;

            return View();
        }
        
        public JsonResult Add(string name, string phoneNo, string driverLicense, string expiryDate, string address)
        {

            var models = new List<DRIVERS>();
            if (Session[sessionName] != null)
                models = (List<DRIVERS>)Session[sessionName];
            var currObj = new DRIVERS();
            currObj.DRIVER_NAME = name;
            currObj.PHONE_NO = phoneNo;
            currObj.ADDRESS = address;
            currObj.DRIVER_LICENSE = driverLicense;
            //currObj.EXPIRY_DATE = new DateTime(;

           models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Drivers/Partial/_TypeTAddPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /FEED_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DRIVERS model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<DRIVERS> modelList = (List<DRIVERS>)Session[sessionName];
                    long id = 0;
                    if (db.DRIVERS.Any())
                        id = db.DRIVERS.Max(i => i.SYSTEMID);

                    foreach (var item in modelList)
                    {
                        var curObj = new DRIVERS();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.DRIVER_NAME = item.DRIVER_NAME;
                        curObj.PHONE_NO = item.PHONE_NO;
                        curObj.ADDRESS = item.ADDRESS;
                        curObj.DRIVER_LICENSE = item.DRIVER_LICENSE;
                        curObj.EXPIRY_DATE = item.EXPIRY_DATE;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.DRIVERS.Add(curObj);
                        db.SaveChanges();
                    }
                    Session[sessionName] = null;

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                }
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(model);
            }


        }


        // GET: /animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DRIVERS model = (DRIVERS) db.DRIVERS.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            
            return View(model);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DRIVERS model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DRIVERS originalModel = db.DRIVERS.Find(model.SYSTEMID);

                    originalModel.DRIVER_NAME = model.DRIVER_NAME;
                    originalModel.PHONE_NO = model.PHONE_NO;
                    originalModel.ADDRESS = model.ADDRESS;
                    originalModel.DRIVER_LICENSE = model.DRIVER_LICENSE;
                    originalModel.EXPIRY_DATE = model.EXPIRY_DATE;
                    originalModel.LAST_UPDATE_DATE = DateTime.Now;
                    originalModel.LAST_UPDATED_BY = CurrentUserID;
                    db.Entry(originalModel).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(model);
            }
        }

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DRIVERS model = db.DRIVERS.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: /animal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DRIVERS model = db.DRIVERS.Find(id);
            model.LAST_UPDATED_BY = CurrentUserID;
            model.LAST_UPDATE_DATE = DateTime.Now;
            model.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
