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
    public class Feed_typeController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "feedtypes";

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.FEED_TYPE.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /AnimalType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FEED_TYPE model = db.FEED_TYPE.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session[sessionName] = null;

            return View();
        }
        public JsonResult Add(string FEED_NAME, string NOTES)
        {

            var models = new List<FEED_TYPE>();
            if (Session[sessionName] != null)
                models = (List<FEED_TYPE>)Session[sessionName];
            var currObj = new FEED_TYPE();
           currObj.FEED_NAME = FEED_NAME;
            if(!string.IsNullOrEmpty(FEED_NAME))
            currObj.FEED_NAME = FEED_NAME;
            if (!string.IsNullOrEmpty(NOTES))
              currObj.NOTES = NOTES;
           models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/FEED_TYPE/Partial/_TypeTAddPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /FEED_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( FEED_TYPE model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<FEED_TYPE> modelList = (List<FEED_TYPE>)Session[sessionName];
                    long id = 0;
                    if (db.FEED_TYPE.Any())
                        id = db.FEED_TYPE.Max(i => i.SYSTEMID);

                    foreach (var item in modelList)
                    {
                        var curObj = new FEED_TYPE();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.FEED_NAME = item.FEED_NAME;
                        curObj.NOTES = item.NOTES;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.FEED_TYPE.Add(curObj);
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


            return View(model);
        }


        // GET: /animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           FEED_TYPE model = db.FEED_TYPE.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", model.STATUS);

            return View(model);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( FEED_TYPE model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(model).State = EntityState.Modified;
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
            FEED_TYPE model = db.FEED_TYPE.Find(id);
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
            FEED_TYPE model = db.FEED_TYPE.Find(id);
            model.AUTHORIZATION_DATE = DateTime.Now;
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
