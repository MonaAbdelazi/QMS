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
    public class ExportersController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /Pens/
        public ActionResult Index()
        {
            return View(db.EXPORTER.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /Pens/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PEN pen = db.PEN.Find(id);
            if (pen == null)
            {
                return HttpNotFound();
            }
            return View(pen);
        }

        // GET: /Pens/Create
        public ActionResult Create()
        {
            ViewBag.Banks = new SelectList(db.BANK, "SYSTEMID", "BANK_NAME");
            ViewBag.BANK_BRANCH_ID = new SelectList(db.BRANCH, "SYSTEMID", "BRANCH_NAME");
            ViewBag.ID_TYPE_ID = new SelectList(db.ID_TYPE, "SYSTEMID", "TYPE_NAME");
            ViewBag.EXPORTER_TYPE_ID = new SelectList(db.EXPORTER_TYPE, "SYSTEMID", "TYP_NAME");
            
            return View();
        }
        public JsonResult Add(string Name, string Note, string capacity)
        {

            var pens = new List<PEN>();
            if (Session["pens"] != null)
                pens = (List<PEN>)Session["pens"];
            var currObj = new PEN();
           currObj.Name = Name;
            currObj.NOTES = Note;
            if (!string.IsNullOrEmpty(capacity))
              currObj.CAPACITY = int.Parse(capacity);
            pens.Add(currObj);
            Session["pens"] = pens;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Pens/Partial/_PenTAddPartial.cshtml", pens.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /Pens/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( EXPORTER export)
        {
            try
            {
                    long id = 0;
                    if (db.EXPORTER.Any())
                        id = db.EXPORTER.Max(i => i.SYSTEMID);

                   
                        id += 1;
                export.SYSTEMID = id;

                export.STATUS =CommonUtils.STATUS_ACTIVE;
                export.LAST_UPDATE_DATE = DateTime.Now;
                export.CREATED_BY = CurrentUserID ;
                export.CREATION_DATE = DateTime.Now;
                        db.EXPORTER.Add(export);
                        db.SaveChanges();
                   
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(export);
            }


            return View(export);
        }


        // GET: /pen/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           EXPORTER exp = db.EXPORTER.Find(id);
            if (exp == null)
            {
                return HttpNotFound();
            }
            if ( exp.BRANCH!=null)
               ViewBag.Banks = new SelectList(db.BANK, "SYSTEMID", "BANK_NAME",exp.BRANCH.BANK_ID);
            else
                ViewBag.Banks = new SelectList(db.BANK, "SYSTEMID", "BANK_NAME");

            ViewBag.BANK_BRANCH_ID = new SelectList(db.BRANCH, "SYSTEMID", "BRANCH_NAME",exp.BANK_BRANCH_ID);
            ViewBag.ID_TYPE_ID = new SelectList(db.ID_TYPE, "SYSTEMID", "TYPE_NAME",exp.ID_TYPE_ID);
            ViewBag.EXPORTER_TYPE_ID = new SelectList(db.EXPORTER_TYPE, "SYSTEMID", "TYP_NAME",exp.EXPORTER_TYPE_ID);

            return View(exp);
        }

        // POST: /pen/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( EXPORTER exps)
        {
            try
            {
              //  if (ModelState.IsValid)
                {
                    EXPORTER exp = db.EXPORTER.Where(i => i.SYSTEMID == exps.SYSTEMID).SingleOrDefault();
                    exp.LAST_UPDATE_DATE = DateTime.Now;
                    exp.STATUS = "Active";
                    exp.BANK_BRANCH_ID = exps.BANK_BRANCH_ID;
                    exp.BIRTH_ESTABLISHMENT_DATE = exps.BIRTH_ESTABLISHMENT_DATE;
                    exp.EXPORTER_NAME = exps.EXPORTER_NAME;
                    exp.EXPORTER_TYPE_ID = exps.EXPORTER_TYPE_ID;
                    exp.ID_TYPE_ID = exps.ID_TYPE_ID;
                    exp.ID_REFERENCE = exps.ID_REFERENCE;
                    exp.NOTES = exps.NOTES;
                    db.Entry(exp).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(exps);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(exps);
            }
        }

        // GET: /pen/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EXPORTER exp = db.EXPORTER.Find(id);
            if (exp == null)
            {
                return HttpNotFound();
            }
            return View(exp);
        }

        // POST: /pen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EXPORTER exp = db.EXPORTER.Find(id);
            if (db.Batchs.Where(i => i.ExporterID == id && i.STATUS == CommonUtils.STATUS_ACTIVE).ToList().Count <= 0)
            {
                exp.AUTHORIZATION_DATE = DateTime.Now;
                exp.STATUS = CommonUtils.STATUS_DELETED;
                db.Entry(exp).State = EntityState.Modified;
                db.SaveChanges();
                CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
            }
            else
            {
                CommonUtils.SetFeedback(Feedback.DeleteRelatedDataFirst, Feedback.Feedback_Error);

            }
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
