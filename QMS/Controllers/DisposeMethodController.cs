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
    public class DisposeMethodController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /DisposeMethod/
        public ActionResult Index()
        {
            return View(db.DisposeMethod.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
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
            Session["methods"] = null;

            return View();
        }
        public JsonResult Add(string DisposeMethod1)
        {

            var methods = new List<DisposeMethod>();
            if (Session["methods"] != null)
                methods = (List<DisposeMethod>)Session["methods"];
            var currObj = new DisposeMethod();
           currObj.DisposeMethod1 = DisposeMethod1;
              methods.Add(currObj);
            Session["methods"] = methods;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/DisposeMethod/Partial/_methodsTAddPartial.cshtml", methods.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /ANIMAL_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( DisposeMethod animal)
        {
            try
            {
                if (Session["methods"] != null)
                {
                    List<DisposeMethod> methods = (List<DisposeMethod>)Session["methods"];
                    long id = 0;
                    if (db.DisposeMethod.Any())
                        id = db.DisposeMethod.Max(i => i.SYSTEMID);

                    foreach (var item in methods)
                    {
                        var curObj = new DisposeMethod();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.DisposeMethod1 = item.DisposeMethod1;
                       curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.DisposeMethod.Add(curObj);
                        db.SaveChanges();
                    }
                    Session["methods"] = null;

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
            DisposeMethod method = db.DisposeMethod.Find(id);
            if (method == null)
            {
                return HttpNotFound();
            }

            return View(method);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( DisposeMethod method)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DisposeMethod old = db.DisposeMethod.Where(i => i.SYSTEMID == method.SYSTEMID).SingleOrDefault();
                    old.LAST_UPDATE_DATE = DateTime.Now;
                    old.STATUS = CommonUtils.STATUS_ACTIVE;
                    old.LAST_UPDATE_DATE = DateTime.Now;
                    old.CREATED_BY = old.CREATED_BY;
                    old.CREATION_DATE = old.CREATION_DATE;
                    old.DisposeMethod1 = method.DisposeMethod1;
                    db.Entry(old).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(method);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(method);
            }
        }

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DisposeMethod method = db.DisposeMethod.Find(id);
            if (method == null)
            {
                return HttpNotFound();
            }
            return View(method);
        }

        // POST: /animal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DisposeMethod method = db.DisposeMethod.Find(id);
            method.AUTHORIZATION_DATE = DateTime.Now;
            method.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(method).State = EntityState.Modified;
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
