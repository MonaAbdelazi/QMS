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
    public class PensController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /Pens/
        public ActionResult Index()
        {
            return View(db.PEN.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
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
            Session["pens"] = null;
            ViewBag.ANIMAL_TYPE = new SelectList(db.ANIMAL_TYPE.Where(i=>i.STATUS==CommonUtils.STATUS_ACTIVE), "SYSTEMID", "TYPE_NAME");
            return View();
        }
        public JsonResult Add(string ANIMAL_TYPE, string Name, string Note, string capacity)
        {

            var pens = new List<PEN>();
            if (Session["pens"] != null)
                pens = (List<PEN>)Session["pens"];
            var currObj = new PEN();
            currObj.Name = Name;
            if (!string.IsNullOrEmpty(ANIMAL_TYPE))
            {
                long id = long.Parse(ANIMAL_TYPE);
                if (pens.Where(i => i.Name == Name && i.TypeID == id).Count() > 0)
                {
                    CommonUtils.SetFeedback("Data Already Entered", Feedback.Feedback_Error);

                    string result = CommonUtils.RenderPartialViewToString(this, "~/Views/Pens/Partial/_PenTAddPartial.cshtml", pens.ToList());
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                currObj.ANIMAL_TYPE = db.ANIMAL_TYPE.Where(i => i.SYSTEMID == id).SingleOrDefault();
                currObj.TypeID = int.Parse(ANIMAL_TYPE);
            }
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
        public ActionResult Create(PEN pen)
        {
            try
            {
                if (Session["pens"] != null)
                {
                    List<PEN> pensList = (List<PEN>)Session["pens"];
                    long id = 0;
                    if (db.PEN.Any())
                        id = db.PEN.Max(i => i.SYSTEMID);

                    foreach (var item in pensList)
                    {
                        var curObj = new PEN();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.PEN_ID = item.Name;
                        curObj.Name = item.Name;
                        curObj.CAPACITY = item.CAPACITY;
                        curObj.NOTES = item.NOTES;
                        curObj.TypeID = item.TypeID;
                        curObj.STATUS = CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.PEN.Add(curObj);
                        db.SaveChanges();
                    }
                    Session["pens"] = null;

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                }
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                ViewBag.ANIMAL_TYPE = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");

                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                ViewBag.ANIMAL_TYPE = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");

                return View(pen);
            }


            return View(pen);
        }


        // GET: /pen/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", pen.STATUS);
            ViewBag.ANIMAL_TYPE = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME", pen.TypeID);

            return View(pen);
        }

        // POST: /pen/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PEN pen)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    pen.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(pen).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(pen);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(pen);
            }
        }

        // GET: /pen/Delete/5
        public ActionResult Delete(int? id)
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
            ViewBag.ANIMAL_TYPE = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME", pen.TypeID);

            return View(pen);
        }

        // POST: /pen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PEN pen = db.PEN.Find(id);
            pen.AUTHORIZATION_DATE = DateTime.Now;
            pen.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(pen).State = EntityState.Modified;
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
