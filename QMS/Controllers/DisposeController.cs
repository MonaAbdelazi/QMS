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
using System.Text.RegularExpressions;

namespace QMS.Controllers
{
    public class DisposeController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /DestinationController/
        public ActionResult Index()
        {
            return View(db.DESTINATION.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }
        public JsonResult Add(string DisposeMethod,string Reason, string label, string Batchs)
        {

            var animals = new List<DisposeDetails>();
            if (Session["animals"] != null)
                animals = (List<DisposeDetails>)Session["animals"];
            var currObj = new DisposeDetails();
            if (!string.IsNullOrEmpty(Batchs))
                currObj.BatchId = long.Parse(Batchs);
                currObj.AnimalTag = (label);
            currObj.Reason = Reason;

            if (!string.IsNullOrEmpty(DisposeMethod))
            {
                currObj.MethodID = long.Parse(DisposeMethod);
                currObj.DisposeMethod = db.DisposeMethod.Where(i => i.SYSTEMID == currObj.MethodID).SingleOrDefault();
            }
             animals.Add(currObj);
            Session["animals"] = animals;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Dispose/Partial/_DisposeTAddPartial.cshtml", animals.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // GET: /Destination/Create
        public ActionResult Create()
        {
            var list = (db.Batchs.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE));
            var selectList = list.Select(x => new SelectListItem() { Value = x.SYSTEMID.ToString(), Text = x.ANIMAL_TYPE.TYPE_NAME+"__"+x.EXPORTER.EXPORTER_NAME })
                .ToList();

            ViewBag.Batchs = new SelectList(selectList, "Value", "Text");
            ViewBag.Reason = new SelectList(CommonUtils.getResasons(), "Value", "Text");
            
            ViewBag.DisposeMethod = new SelectList(db.DisposeMethod.Where(i=>i.STATUS==CommonUtils.STATUS_ACTIVE), "SYSTEMID", "DisposeMethod1");
            Session["animals"] = null;
            return View();
        }
        public JsonResult labelSelected(string label, string Batchs)
        {
            bool result = false;

            try

            {
                long id = long.Parse(Batchs);
                Batchs lms = db.Batchs.Where(o => o.SYSTEMID == id).SingleOrDefault();
                List<PensLables> lbls = lms.PensLables.ToList();
                foreach (var item in lbls)
                {
                    string lbl = item.lableRanges.Trim();
                    string from = lbl.Substring(0, item.lableRanges.IndexOf("-")).Trim();
                    int beg = lbl.IndexOf("-");
                    int leng = lbl.Length;
                    int endd = leng -beg-1;

                    string end = lbl.Substring(beg+1, endd);
                    var myNumber = Regex.Replace(from, "[^0-9]", "");
                    var labelNumber = Regex.Replace(label, "[^0-9]", "");

                    if (int.Parse(labelNumber.ToString()) >= int.Parse(myNumber.ToString()) && int.Parse(labelNumber.ToString()) <= int.Parse(end))
                        result = true;
                }
                if(result)
                return Json("OK", JsonRequestBehavior.AllowGet);
                else
                    return Json("Error", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }



        // POST: /Destination/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( DiposedBatchs disposeds)
        {
            try
            {
                DiposedBatchs disposeObj = new DiposedBatchs();
                long id = 0;
                if (db.DiposedBatchs.Any())
                    id = db.DiposedBatchs.Max(i => i.SYSTEMID);
                id += 1;
                long idDet = 0;
                if (db.DisposeDetails.Any())
                    idDet = db.DESTINATION_TESTS.Max(i => i.SYSTEMID);
                Batchs batch = new Batchs();
                disposeObj.SYSTEMID = id;
                disposeObj.STATUS = CommonUtils.STATUS_ACTIVE;
                disposeObj.LAST_UPDATE_DATE = DateTime.Now;
                disposeObj.CREATED_BY = CurrentUserName;
                disposeObj.CREATION_DATE = DateTime.Now;
                disposeObj.BatchId = disposeds.BatchId;
                disposeObj.DisposeMethod = disposeds.DisposeMethod;
                disposeObj.Reason = disposeds.Reason;
                List<DisposeDetails> list = new List<DisposeDetails>();
                if (Session["animals"] != null)
                {
                    list = (List<DisposeDetails>)Session["animals"];
                    foreach (var item in list)
                    {
                        var curObj = new DisposeDetails();
                        idDet += 1;
                        item.SYSTEMID = idDet;
                        item.DiposeId = id;
                        disposeObj.Reason = item.Reason;
                        disposeObj.BatchId = item.BatchId;
                        disposeObj.MethodID = item.MethodID;
                        if(batch.SYSTEMID==0)
                        batch = db.Batchs.Where(i => i.SYSTEMID == item.BatchId).SingleOrDefault();
                        item.DisposeMethod = null;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        item.CREATION_DATE = DateTime.Now;
                        disposeObj.DisposeDetails.Add(item);
                    }

                }
                batch.NoOFDisposed = list.Count;
                db.Entry(batch).State = EntityState.Modified;
                db.DiposedBatchs.Add(disposeObj);
                db.SaveChanges();
                Session["animals"] = null;

                CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                return RedirectToAction("Create");

            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                var list = (db.Batchs.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE));
                var selectList = list.Select(x => new SelectListItem() { Value = x.SYSTEMID.ToString(), Text = x.ANIMAL_TYPE.TYPE_NAME + "__" + x.EXPORTER.EXPORTER_NAME })
                    .ToList();

                ViewBag.Batchs = new SelectList(selectList, "Value", "Text");
                ViewBag.Reason = new SelectList(CommonUtils.getResasons(), "Value", "Text");

                ViewBag.DisposeMethod = new SelectList(db.DisposeMethod.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE), "SYSTEMID", "DisposeMethod1");
                return View(disposeds);
            }


            return View(disposeds);
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
        public ActionResult Edit( DESTINATION destination)
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
            if (dest == null || list.Count>0)
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
