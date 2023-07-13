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
    public class DestinationController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /DestinationController/
        public ActionResult Index()
        {
            return View(db.DESTINATION.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }
        public JsonResult Add(List<long> TEST_ID, string QuaratinePeriod, string ANIMAL_TYPE_ID, string DESTINATION_NAME)
        {

            var dests = new List<DESTINATION_TESTS>();
            var Newdests = new List<DESTINATION_TESTS>();
            var newQuarantine = new List<QUARENTINE_PERIOD>();

            var Quarantine = new List<QUARENTINE_PERIOD>();
            if (Session["dests"] != null)
                dests = (List<DESTINATION_TESTS>)Session["dests"];
            if (Session["Newdests"] != null)
                Newdests = (List<DESTINATION_TESTS>)Session["Newdests"];
            if (Session["Quarantine"] != null)
                Quarantine = (List<QUARENTINE_PERIOD>)Session["Quarantine"];
            if (Session["newQuarantine"] != null)
                newQuarantine = (List<QUARENTINE_PERIOD>)Session["newQuarantine"];
            var currObj = new DESTINATION_TESTS();
            var curQuarn = new QUARENTINE_PERIOD();
            currObj.DESTINATION = new DESTINATION();
            currObj.DESTINATION.DESTINATION_NAME = DESTINATION_NAME;
            curQuarn.DESTINATION = new DESTINATION();

            curQuarn.DESTINATION.DESTINATION_NAME = DESTINATION_NAME;
            if (!string.IsNullOrEmpty(QuaratinePeriod))
            {
                int no = int.Parse(QuaratinePeriod);
                curQuarn.QUARENTINE_DAYS = no;
            }

            if (!string.IsNullOrEmpty(ANIMAL_TYPE_ID))
            {
                int no = int.Parse(ANIMAL_TYPE_ID);
                ANIMAL_TYPE animal = db.ANIMAL_TYPE.Where(i => i.SYSTEMID == no).SingleOrDefault();
                currObj.ANIMAL_TYPE_ID = no;
                curQuarn.ANIMAL_TYPE = animal;
                currObj.ANIMAL_TYPE = animal;
                curQuarn.ANIMAL_TYPE_ID = no;

            }
            foreach (var item in TEST_ID)
            {
                currObj.TEST_ID = item;
                dests.Add(currObj);
                Newdests.Add(currObj);


            }
            Quarantine.Add(curQuarn);
            newQuarantine.Add(curQuarn);
            Session["dests"] = dests;
            Session["Newdests"] = Newdests;

            Session["Quarantine"] = Quarantine;
            Session["newQuarantine"] = newQuarantine;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Destination/Partial/_DestinationsPartial.cshtml", Quarantine.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // GET: /Destination/Create
        public ActionResult Create()
        {
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");
            ViewBag.TEST_ID = new SelectList(db.TESTS, "SYSTEMID", "TEST_NAME");

            ViewBag.ChooseRight = new SelectList(string.Empty, "SYSTEMID", "TEST_NAME");

            return View();
        }

        // POST: /Destination/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DESTINATION_TESTS dest)
        {
            try
            {
                DESTINATION destObj = new DESTINATION();
                long id = 0;
                if (db.DESTINATION.Any())
                    id = db.DESTINATION.Max(i => i.SYSTEMID);
                id += 1;
                long idtest = 0;
                if (db.DESTINATION_TESTS.Any())
                    idtest = db.DESTINATION_TESTS.Max(i => i.SYSTEMID);
                long idQ = 0;
                if (db.QUARENTINE_PERIOD.Any())
                    idQ = db.QUARENTINE_PERIOD.Max(i => i.SYSTEMID);
                destObj.SYSTEMID = id;
                destObj.STATUS = CommonUtils.STATUS_ACTIVE;
                destObj.LAST_UPDATE_DATE = DateTime.Now;
                destObj.CREATED_BY = CurrentUserName;
                destObj.CREATION_DATE = DateTime.Now;
                destObj.DESTINATION_NAME = dest.DESTINATION.DESTINATION_NAME;
                if (Session["dests"] != null)
                {
                    List<DESTINATION_TESTS> list = (List<DESTINATION_TESTS>)Session["dests"];
                    foreach (var item in list)
                    {
                        var curObj = new DESTINATION_TESTS();
                        idtest += 1;
                        item.ANIMAL_TYPE = null;
                        item.SYSTEMID = idtest;
                        item.DESTINATION_ID = id;
                        item.DESTINATION = destObj;
                        item.STATUS = CommonUtils.STATUS_ACTIVE;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        item.CREATION_DATE = DateTime.Now;
                        destObj.DESTINATION_TESTS.Add(item);
                    }
                    Session["dests"] = null;
                    Session["Newdests"] = null;

                }
                if (Session["Quarantine"] != null)
                {
                    List<QUARENTINE_PERIOD> listQ = (List<QUARENTINE_PERIOD>)Session["Quarantine"];
                    foreach (var item in listQ)
                    {
                        var curObj = new QUARENTINE_PERIOD();
                        idQ += 1;
                        item.SYSTEMID = idQ;
                        item.DESTINATION_ID = id;
                        item.DESTINATION = destObj;
                        item.STATUS = CommonUtils.STATUS_ACTIVE;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        item.AUTHORIZED_BY = CurrentUserName;
                        item.ANIMAL_TYPE = null;
                        item.CREATION_DATE = DateTime.Now;
                        destObj.QUARENTINE_PERIOD.Add(item);
                    }
                    Session["Quarantine"] = null;
                }//////////////
                db.DESTINATION.Add(destObj);
                db.SaveChanges();
                CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");
                return View(dest);
            }


            return View(dest);
        }


        // GET: /Destination/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DESTINATION destination = db.DESTINATION.Find(id);
            if (destination == null)
            {
                return HttpNotFound();
            }
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");
            var list = (destination.DESTINATION_TESTS);
            var selectList = list.Select(x => new SelectListItem() { Value = x.SYSTEMID.ToString(), Text = x.TESTS.TEST_NAME })
                .ToList();
            ViewBag.TEST_ID = new SelectList(selectList, "Value", "Text");
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");

            ViewBag.ChooseRight = new SelectList(db.TESTS, "SYSTEMID", "TEST_NAME");
            Session["Olddests"] = destination.DESTINATION_TESTS.ToList();
            Session["dests"] = destination.DESTINATION_TESTS.ToList();
            Session["OldQuarantine"] = destination.QUARENTINE_PERIOD.ToList();
            Session["Quarantine"] = destination.QUARENTINE_PERIOD.ToList();

            return View(destination);
        }

        // POST: /Destination/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DESTINATION destination)
        {
            try
            {
                // if (ModelState.IsValid)
                {
                    long idtest = 0;
                    if (db.DESTINATION_TESTS.Any())
                        idtest = db.DESTINATION_TESTS.Max(i => i.SYSTEMID);
                    long idQ = 0;
                    if (db.QUARENTINE_PERIOD.Any())
                        idQ = db.QUARENTINE_PERIOD.Max(i => i.SYSTEMID);
                    destination.QUARENTINE_PERIOD = (List<QUARENTINE_PERIOD>)Session["OldQuarantine"];
                    foreach (var item in destination.QUARENTINE_PERIOD)
                    {
                        item.DESTINATION = null;
                    }
                    destination.DESTINATION_TESTS = (List<DESTINATION_TESTS>)Session["Olddests"];
                    foreach (var item in destination.DESTINATION_TESTS)
                    {
                        item.DESTINATION = null;
                    }
                    if (Session["Newdests"] != null)
                    {
                        List<DESTINATION_TESTS> list = (List<DESTINATION_TESTS>)Session["Newdests"];
                        foreach (var item in list)
                        {
                            var curObj = new DESTINATION_TESTS();
                            idtest += 1;
                            item.ANIMAL_TYPE = null;
                            item.SYSTEMID = idtest;
                            item.DESTINATION_ID = destination.SYSTEMID;
                            item.DESTINATION = null;
                            item.STATUS = CommonUtils.STATUS_ACTIVE;
                            item.LAST_UPDATE_DATE = DateTime.Now;
                            item.CREATED_BY = CurrentUserName;
                            item.CREATION_DATE = DateTime.Now;
                            db.DESTINATION_TESTS.Add(item);
                            destination.DESTINATION_TESTS.Add(item);
                        }
                        Session["Newdests"] = null;
                    }
                    if (Session["newQuarantine"] != null)
                    {
                        List<QUARENTINE_PERIOD> listQ = (List<QUARENTINE_PERIOD>)Session["newQuarantine"];
                        foreach (var item in listQ)
                        {
                            var curObj = new QUARENTINE_PERIOD();
                            idQ += 1;
                            item.SYSTEMID = idQ;
                            item.DESTINATION_ID = destination.SYSTEMID;
                            item.DESTINATION = null; ;
                            item.STATUS = CommonUtils.STATUS_ACTIVE;
                            item.LAST_UPDATE_DATE = DateTime.Now;
                            item.CREATED_BY = CurrentUserName;
                            item.AUTHORIZED_BY = CurrentUserName;
                            item.ANIMAL_TYPE = null;
                            item.CREATION_DATE = DateTime.Now;
                            db.QUARENTINE_PERIOD.Add(item);

                            destination.QUARENTINE_PERIOD.Add(item);
                        }
                        Session["newQuarantine"] = null;
                    }//////////////
                    destination.STATUS = CommonUtils.STATUS_ACTIVE;
                    destination.LAST_UPDATE_DATE = DateTime.Now;
                    destination.CREATED_BY = CurrentUserName;
                    destination.AUTHORIZED_BY = CurrentUserName;
                    destination.CREATION_DATE = DateTime.Now;
                    destination.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(destination).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    Session["OldQuarantine"] = null;
                    Session["NewQuarantine"] = null;
                    Session["Quarantine"] = null;
                    Session["Olddests"] = null;
                    Session["Newdests"] = null;
                    Session["dests"] = null;
                    return RedirectToAction("Index");
                }
                return View(destination);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(destination);
            }
        }

        // GET: /Destination/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DESTINATION dest = db.DESTINATION.Find(id);
            List<Batchs> list = db.Batchs.Where(i => i.DestenationID == id && i.STATUS == CommonUtils.STATUS_ACTIVE).ToList();
            if (dest == null || list.Count > 0)
            {
                return HttpNotFound();
            }
            return View(dest);
        }

        // POST: /Destination/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DESTINATION dest = db.DESTINATION.Find(id);
            dest.AUTHORIZATION_DATE = DateTime.Now;
            dest.STATUS = CommonUtils.STATUS_DELETED;

            //dest.QUARENTINE_PERIOD.s
            db.Entry(dest).State = EntityState.Modified;
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
