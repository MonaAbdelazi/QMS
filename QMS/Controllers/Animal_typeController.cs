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
    public class Animal_typeController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.ANIMAL_TYPE.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /AnimalType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ANIMAL_TYPE animal = db.ANIMAL_TYPE.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session["animals"] = null;

            return View();
        }
        public JsonResult Add(string TYPE_NAME, string DEFAULT_AVERAGE_WEIGHT, string FEED_WEIGHT_PERCENTAGE, string EX_Rate,string Last_Ex_Rare_Date)
        {

            var animals = new List<ANIMAL_TYPE>();
            if (Session["animals"] != null)
                animals = (List<ANIMAL_TYPE>)Session["animals"];
            var currObj = new ANIMAL_TYPE();
           currObj.TYPE_NAME = TYPE_NAME;
            if(!string.IsNullOrEmpty(TYPE_NAME))
            currObj.TYPE_NAME = TYPE_NAME;
            if (!string.IsNullOrEmpty(DEFAULT_AVERAGE_WEIGHT))
              currObj.DEFAULT_AVERAGE_WEIGHT = decimal.Parse(DEFAULT_AVERAGE_WEIGHT);
            if (!string.IsNullOrEmpty(FEED_WEIGHT_PERCENTAGE))
               currObj.FEED_WEIGHT_PERCENTAGE = decimal.Parse(FEED_WEIGHT_PERCENTAGE);
           animals.Add(currObj);
            Session["animals"] = animals;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Animal_Type/Partial/_TypeTAddPartial.cshtml", animals.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /ANIMAL_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( ANIMAL_TYPE animal)
        {
            try
            {
                if (Session["animals"] != null)
                {
                    List<ANIMAL_TYPE> animalList = (List<ANIMAL_TYPE>)Session["animals"];
                    long id = 0;
                    if (db.ANIMAL_TYPE.Any())
                        id = db.ANIMAL_TYPE.Max(i => i.SYSTEMID);

                    foreach (var item in animalList)
                    {
                        var curObj = new ANIMAL_TYPE();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.TYPE_NAME = item.TYPE_NAME;
                        curObj.DEFAULT_AVERAGE_WEIGHT = item.DEFAULT_AVERAGE_WEIGHT;
                        curObj.FEED_WEIGHT_PERCENTAGE = item.FEED_WEIGHT_PERCENTAGE;
                        curObj.NOTES = item.NOTES;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.ANIMAL_TYPE.Add(curObj);
                        db.SaveChanges();
                    }
                    Session["animals"] = null;

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                }
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(animal);
            }


            return View(animal);
        }


        // GET: /animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           ANIMAL_TYPE animal = db.ANIMAL_TYPE.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", animal.STATUS);

            return View(animal);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( ANIMAL_TYPE model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ANIMAL_TYPE dbModel = db.ANIMAL_TYPE.Find(model.SYSTEMID);
                    dbModel.LAST_UPDATE_DATE = DateTime.Now;
                    dbModel.TYPE_NAME = model.TYPE_NAME;
                    dbModel.DEFAULT_AVERAGE_WEIGHT = model.DEFAULT_AVERAGE_WEIGHT;
                    dbModel.FEED_WEIGHT_PERCENTAGE = model.FEED_WEIGHT_PERCENTAGE;
                    dbModel.NOTES = model.NOTES;
                    dbModel.STATUS = model.STATUS;
                    db.Entry(dbModel).State = EntityState.Modified;
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
            ANIMAL_TYPE animal = db.ANIMAL_TYPE.Find(id);
            if (animal == null)
            {
                return HttpNotFound();
            }
            return View(animal);
        }

        // POST: /animal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ANIMAL_TYPE animal = db.ANIMAL_TYPE.Find(id);
            animal.AUTHORIZATION_DATE = DateTime.Now;
            animal.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(animal).State = EntityState.Modified;
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
