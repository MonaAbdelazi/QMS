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

namespace QMS.Controllers
{
    public class Exporter_TypeController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /ExporterType/
        public ActionResult Index()
        {
            return View(db.EXPORTER_TYPE.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /ExporterType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EXPORTER_TYPE Exporter = db.EXPORTER_TYPE.Find(id);
            if (Exporter == null)
            {
                return HttpNotFound();
            }
            return View(Exporter);
        }

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session["Exporters"] = null;
            ViewBag.IndvCorporate = new SelectList(CommonUtils.getIndvCorporate(), "Value", "Text");

            return View();
        }
        public JsonResult Add(string TYP_NAME, string INDIVIDUAL_CORPORATE, string NOTES)
        {

            var Exporters = new List<EXPORTER_TYPE>();
            if (Session["Exporters"] != null)
                Exporters = (List<EXPORTER_TYPE>)Session["Exporters"];
            var currObj = new EXPORTER_TYPE();
           currObj.TYP_NAME = TYP_NAME;
            currObj.NOTES = NOTES;

            currObj.INDIVIDUAL_CORPORATE = INDIVIDUAL_CORPORATE;
           Exporters.Add(currObj);
            Session["Exporters"] = Exporters;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/EXPORTER_TYPE/Partial/_exporterTAddPartial.cshtml", Exporters.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /EXPORTER_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( EXPORTER_TYPE Exporter)
        {
            try
            {
                if (Session["Exporters"] != null)
                {
                    List<EXPORTER_TYPE> ExporterList = (List<EXPORTER_TYPE>)Session["Exporters"];
                    long id = 0;
                    if (db.EXPORTER_TYPE.Any())
                        id = db.EXPORTER_TYPE.Max(i => i.SYSTEMID);

                    foreach (var item in ExporterList)
                    {
                        var curObj = new EXPORTER_TYPE();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.TYP_NAME = item.TYP_NAME;
                        curObj.INDIVIDUAL_CORPORATE = item.INDIVIDUAL_CORPORATE;
                        curObj.NOTES = item.NOTES;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.EXPORTER_TYPE.Add(curObj);
                        db.SaveChanges();
                    }
                    Session["Exporters"] = null;

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                }
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(Exporter);
            }


            return View(Exporter);
        }


        // GET: /Exporters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           EXPORTER_TYPE Exporter = db.EXPORTER_TYPE.Find(id);
            if (Exporter == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", Exporter.STATUS);
            ViewBag.IndvCorporate = new SelectList(CommonUtils.getIndvCorporate(), "Value", "Text",Exporter.INDIVIDUAL_CORPORATE);

            return View(Exporter);
        }

        // POST: /Exporter/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( EXPORTER_TYPE Exporter)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Exporter.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(Exporter).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(Exporter);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(Exporter);
            }
        }

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EXPORTER_TYPE Exporter = db.EXPORTER_TYPE.Find(id);
            if (Exporter == null)
            {
                return HttpNotFound();
            }
            return View(Exporter);
        }

        // POST: /Exporter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EXPORTER_TYPE Exporter = db.EXPORTER_TYPE.Find(id);
            Exporter.AUTHORIZATION_DATE = DateTime.Now;
            Exporter.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(Exporter).State = EntityState.Modified;
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
