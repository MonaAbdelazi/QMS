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
    public class FeedingController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "feeding";

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.FEED_RECORD.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /AnimalType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FEED_RECORD model = db.FEED_RECORD.Find(id);
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

            ViewBag.ActiveBatches = new SelectList(db.Batchs.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE).ToList(), "SYSTEMID", "SYSTEMID");
            ViewBag.FeedTypes = new SelectList(db.FEED_TYPE.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE).ToList(), "SYSTEMID", "FEED_NAME");

            return View();
        }
        public JsonResult Add(string BATCH_ID, string FEED_TYPE, string AMOUNT, string FEEDINGMETHOD)
        {

            var models = new List<FEED_RECORD>();
            if (Session[sessionName] != null)
                models = (List<FEED_RECORD>)Session[sessionName];
            var currObj = new FEED_RECORD();
            currObj.BATCH_ID = long.Parse(BATCH_ID);
            currObj.FEED_TYPE = long.Parse(FEED_TYPE);
            currObj.FEED_TYPE1 = db.FEED_TYPE.Find(currObj.FEED_TYPE);
            currObj.AMOUNT = int.Parse(AMOUNT);
            currObj.FEED_METHOD = (FEEDINGMETHOD == "true") ? 1 : 0;
            
            models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Feeding/Partial/_TypeTAddPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /ANIMAL_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( FEED_RECORD model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<FEED_RECORD> models = (List<FEED_RECORD>)Session[sessionName];
                    long id = 0;
                    if (db.FEED_RECORD.Any())
                        id = db.FEED_RECORD.Max(i => i.SYSTEMID);

                    foreach (var item in models)
                    {
                        var curObj = new FEED_RECORD();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.BATCH_ID = item.BATCH_ID;
                        curObj.FEED_TYPE = item.FEED_TYPE;
                        curObj.AMOUNT = item.AMOUNT;
                        curObj.FEED_METHOD = item.FEED_METHOD;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.FEED_RECORD.Add(curObj);
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

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FEED_RECORD model = db.FEED_RECORD.Find(id);
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
            FEED_RECORD model = db.FEED_RECORD.Find(id);
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
