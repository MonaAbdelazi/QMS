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
    public class DiagnosesController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "diagnoses";

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.DIAGNOSES.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }
              

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session[sessionName] = null;

            return View();
        }
        
        public JsonResult Add(string shortDescription, string longDescription, string recommendDisposal)
        {

            var models = new List<DIAGNOSES>();
            if (Session[sessionName] != null)
                models = (List<DIAGNOSES>)Session[sessionName];
            var currObj = new DiagnosesVM();
            if(!string.IsNullOrEmpty(shortDescription))
            currObj.SHORT_DESCRIPTION = shortDescription;
            if (!string.IsNullOrEmpty(longDescription))
              currObj.LONG_DESCRIPTION = longDescription;
            currObj.RecommendDisposalBool = (recommendDisposal == "true");
           models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Diagnoses/Partial/_TypeTAddPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /FEED_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DIAGNOSES model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<DiagnosesVM> modelList = (List<DiagnosesVM>)Session[sessionName];
                    long id = 0;
                    if (db.DIAGNOSES.Any())
                        id = db.DIAGNOSES.Max(i => i.SYSTEMID);

                    foreach (var item in modelList)
                    {
                        var curObj = new DIAGNOSES();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.SHORT_DESCRIPTION = item.SHORT_DESCRIPTION;
                        curObj.LONG_DESCRIPTION = item.LONG_DESCRIPTION;
                        curObj.RECOMMEND_DISPOSAL = item.RECOMMEND_DISPOSAL;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.DIAGNOSES.Add(curObj);
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
            DiagnosesVM model = (DiagnosesVM) db.DIAGNOSES.Find(id);
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
        public ActionResult Edit(DiagnosesVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DIAGNOSES originalModel = db.DIAGNOSES.Find(model.SYSTEMID);

                    originalModel.SHORT_DESCRIPTION = model.SHORT_DESCRIPTION;
                    originalModel.LONG_DESCRIPTION = model.LONG_DESCRIPTION;
                    originalModel.RECOMMEND_DISPOSAL = model.RECOMMEND_DISPOSAL;
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
            DIAGNOSES model = db.DIAGNOSES.Find(id);
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
            DIAGNOSES model = db.DIAGNOSES.Find(id);
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
