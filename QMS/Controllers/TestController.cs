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
    public class TestController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /Test/
        public ActionResult Index()
        {
            return View(db.TESTS.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /Test/Create
        public ActionResult Create()
        {
            ViewBag.Sample = new SelectList(CommonUtils.getSample(), "Value", "Text");

            return View();
        }
  
        // POST: /Test/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( TESTS curObj ,bool RoundRequired)
        {
            try
            {
                   long id = 0;
                    if (db.TESTS.Any())
                        id = db.TESTS.Max(i => i.SYSTEMID);
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.RoundRequired = RoundRequired.ToString();
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserName ;
                       curObj.AUTHORIZED_BY = CurrentUserName;
                       curObj.LAST_UPDATED_BY = CurrentUserName;
                       curObj.CREATION_DATE = DateTime.Now;
                        db.TESTS.Add(curObj);
                        db.SaveChanges();
                    

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(curObj);
            }


            return View(curObj);
        }


        // GET: /Test/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           TESTS test = db.TESTS.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            ViewBag.Sample = new SelectList(CommonUtils.getSample(), "Value", "Text",test.Sample);

            return View(test);
        }

        // POST: /Test/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( TESTS test,bool RoundRequired)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    test.RoundRequired = RoundRequired.ToString();
                    test.STATUS = CommonUtils.STATUS_ACTIVE;
                    test.LAST_UPDATE_DATE = DateTime.Now;
                    test.CREATED_BY = CurrentUserName;
                    test.AUTHORIZED_BY = CurrentUserName;
                    test.LAST_UPDATED_BY = CurrentUserName;
                    test.CREATION_DATE = DateTime.Now;
                     test.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(test).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
                }
                return View(test);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(test);
            }
        }

        // GET: /Test/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TESTS test = db.TESTS.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        // POST: /test/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TESTS test = db.TESTS.Find(id);
            test.AUTHORIZATION_DATE = DateTime.Now;
            test.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(test).State = EntityState.Modified;
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
