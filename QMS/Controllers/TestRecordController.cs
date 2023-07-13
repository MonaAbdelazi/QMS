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
using QMS.Models;
using QMS.ViewModels;

namespace QMS.Controllers
{
    public class TestRecordController : BaseController
    {
        #region Private fields
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "testRecord"; 
        #endregion

        #region Index
        public ActionResult Index()
        {
            Session[sessionName] = null;
            IList<ListItem> activeBatches = GetActiveBatches();
            ViewBag.ActiveBatches = new SelectList(activeBatches, "Value", "Display");

            return View();
        }
        public JsonResult FillBatchDetails(long batchId)
        {

            var models = db.TEST_SUMMARY.Where(a => a.BATCH_ID == batchId).ToList();
            ViewBag.BatchTests = models;

            var model = db.Batchs.Find(batchId);


            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/TestRecord/Partial/_TypeTAddPartial.cshtml", model);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region Record

        public ActionResult Record(long? batchId, long? testId)
        {
            TESTS test = db.TESTS.Find(testId);
            if (test == null)
            {
                return HttpNotFound();
            }
            if (test.Sample == "Full")
            {
                return RedirectToAction("RecordFull", new { batchId = batchId, testId = testId });
            }
            else
            {
                return RedirectToAction("RecordNumbPerc", new { batchId = batchId, testId = testId });
            }
        }
        
        #region NumbPerc
        public ActionResult RecordNumbPerc(long? batchId, long? testId)
        {

            try
            {
                TestRecordViewModel model = FillTestRecordVM(batchId, testId);

                IList<ListItem> testResultsItems = GetTestResultsItems();
                ViewBag.TestResults = new SelectList(testResultsItems, "Value", "Display");

                IList<TestedAnimalsVM> testedAnimals = GetTestedAnimals(batchId, testId);
                Session[sessionName + "NumPerc"] = testedAnimals;
                ViewBag.TestedAnimals = testedAnimals;

                return View(model);
            }
            catch
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordNumbPerc(TestRecordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<TestedAnimalsVM> vmModels = ((List<TestedAnimalsVM>)Session[sessionName + "NumPerc"])
                                                            .Where(a => a.IsNew).ToList();

                    long idChild = 0;
                    if (db.TESTED_ANIMALS.Any())
                    {
                        idChild = db.TESTED_ANIMALS.Max(i => i.SYSTEMID);
                    }

                    foreach (var item in vmModels)
                    {
                        TESTED_ANIMALS newModel = new TESTED_ANIMALS();
                        idChild++;
                        newModel.SYSTEMID = idChild;
                        newModel.BATCH_ID = model.BatchId;
                        newModel.TEST_ID = model.TestId;
                        newModel.ANIMAL_LABEL = item.AnimalTag;
                        newModel.TEST_RESULT = (item.Result == "Positive") ? 1 : 0;
                        newModel.STATUS = CommonUtils.STATUS_ACTIVE;
                        newModel.CREATED_BY = CurrentUserID;
                        newModel.AUTHORIZED_BY = CurrentUserID;
                        newModel.CREATION_DATE = DateTime.Now;
                        newModel.AUTHORIZATION_DATE = DateTime.Now;

                        db.TESTED_ANIMALS.Add(newModel);
                    }

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
        public JsonResult AddNumbPerc(string animalTag, string testResult)
        {
            //todo:validateTag
            IList<TestedAnimalsVM> models = (IList<TestedAnimalsVM>)Session[sessionName + "NumPerc"];
            TestedAnimalsVM model = new TestedAnimalsVM();
            model.DateEntered = DateTime.Now;
            model.AnimalTag = animalTag;
            model.Result = testResult;
            model.IsNew = true;

            models.Add(model);

            Session[sessionName + "NumPerc"] = models;
            ViewBag.TestedAnimals = models;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/TestRecord/Partial/AddNumbPercPartial.cshtml", null);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        #endregion
        
        #region Full
        public ActionResult RecordFull(long? batchId, long? testId)
        {
            try
            {
                TestRecordViewModel model = FillTestRecordVM(batchId, testId);                

                IList<TestedAnimalsVM> testedAnimals = GetTestedRanges(batchId, testId);
                Session[sessionName + "Full"] = testedAnimals;
                ViewBag.TestedAnimals = testedAnimals;

                List<string> positiveTags = new List<string>();
                Session["PositiveTags"] = positiveTags;

                return View(model);
            }
            catch
            {
                return HttpNotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordFull(TestRecordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<string> positiveTags = ((List<string>)Session["PositiveTags"]);
                                                            

                    long id = 0;
                    if (db.TESTED_RANGES.Any())
                    {
                        id = db.TESTED_RANGES.Max(i => i.SYSTEMID);
                    }

                    TESTED_RANGES newModel = new TESTED_RANGES();
                    id++;
                    newModel.SYSTEMID = id;
                    newModel.BATCH_ID = model.BatchId;
                    newModel.TEST_ID = model.TestId;
                    newModel.RANGE_FROM = model.RangeFrom;
                    newModel.RANGE_TO = model.RangeTo;
                    newModel.STATUS = CommonUtils.STATUS_ACTIVE;
                    newModel.CREATED_BY = CurrentUserID;
                    newModel.AUTHORIZED_BY = CurrentUserID;
                    newModel.CREATION_DATE = DateTime.Now;
                    newModel.AUTHORIZATION_DATE = DateTime.Now;
                    db.TESTED_RANGES.Add(newModel);

                    long idChild = 0;
                    if (db.TESTED_ANIMALS.Any())
                    {
                        idChild = db.TESTED_ANIMALS.Max(i => i.SYSTEMID);
                    }

                    foreach (var item in positiveTags)
                    {
                        TESTED_ANIMALS childModel = new TESTED_ANIMALS();
                        idChild++;
                        childModel.SYSTEMID = idChild;
                        childModel.BATCH_ID = model.BatchId;
                        childModel.TEST_ID = model.TestId;
                        childModel.ANIMAL_LABEL = item;
                        childModel.TEST_RESULT = 1;
                        childModel.TestRange = id;
                        childModel.STATUS = CommonUtils.STATUS_ACTIVE;
                        childModel.CREATED_BY = CurrentUserID;
                        childModel.AUTHORIZED_BY = CurrentUserID;
                        childModel.CREATION_DATE = DateTime.Now;
                        childModel.AUTHORIZATION_DATE = DateTime.Now;

                        db.TESTED_ANIMALS.Add(childModel);
                    }

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

        public JsonResult AddFull(string rangeFrom, string rangeTo, string PositiveCase)
        {
            //todo:validateTag
            IList<TestedAnimalsVM> models = (IList<TestedAnimalsVM>)Session[sessionName + "Full"];
            TestedAnimalsVM model;
            if (models.Any(a => a.SystemId == -1))
            {
                model = models.Where(a => a.SystemId == -1).ToList()[0];
            }
            else 
            {
                model = new TestedAnimalsVM();
                model.SystemId = -1;
                model.DateEntered = DateTime.Now;
                model.IsNew = true;
                models.Add(model);
            }

            model.RangeFrom = rangeFrom;
            model.RangeTo = rangeTo;

            Session[sessionName + "Full"] = models;
            ViewBag.TestedAnimals = models;

            List<string> positiveTags = (List<string>)Session["PositiveTags"];
            positiveTags.Add(PositiveCase);
            Session["PositiveTags"] = positiveTags;

            

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/TestRecord/Partial/AddFullPartial.cshtml", null);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRangePositiveCases(long id)
        {
            //todo:validateTag
            if (id == -1)
            {
                ViewBag.TestedPositive = Session["PositiveTags"];
            }
            else
            {
                IList<TESTED_ANIMALS> subModels = db.TESTED_ANIMALS.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE && a.TestRange == id).ToList();
                List<string> positiveTags = new List<string>();
                foreach (var item in subModels)
                {
                    positiveTags.Add(item.ANIMAL_LABEL);
                }
                ViewBag.TestedPositive = positiveTags;
            }

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/TestRecord/Partial/AddFull_Positive_Partial.cshtml", null);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
        
        #region Private Methods
        private IList<ListItem> GetActiveBatches()
        {
            IList<ListItem> items = new List<ListItem>();

            IList<Batchs> list = db.Batchs.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE).ToList();

            foreach (var item in list)
            {
                items.Add(new ListItem(item.EXPORTER.EXPORTER_NAME + " - " + item.ANIMAL_TYPE.TYPE_NAME, item.SYSTEMID.ToString()));
            }
            return items;
        }

        private TestRecordViewModel FillTestRecordVM(long? batchId, long? testId)
        {
            if (batchId == null || testId == null)
            {
                throw new Exception();
            }

            TestRecordViewModel model = new TestRecordViewModel();

            model.BatchId = (long)batchId;
            Batchs batch = db.Batchs.Find(batchId);
            if (batch == null)
            {
                throw new Exception();
            }
            model.BatchDetails = batch;

            model.TestId = (long)testId;
            TESTS test = db.TESTS.Find(testId);
            if (test == null)
            {
                throw new Exception();
            }
            model.TestDetails = test;

            List<TEST_SUMMARY> summaryList = db.TEST_SUMMARY.Where(a => a.BATCH_ID == batchId && a.TEST_ID == testId).ToList();
            if (summaryList.Count == 0)
            {
                throw new Exception();
            }
            TEST_SUMMARY summary = summaryList[0];
            if (summary == null)
            {
                throw new Exception();
            }

            model.Summary = summary;

            return model;
        }

        private IList<ListItem> GetTestResultsItems()
        {
            IList<ListItem> items = new List<ListItem>();
            items.Add(new ListItem(QMSRes.Negative, "Negative"));
            items.Add(new ListItem(QMSRes.Positive, "Positive"));
            return items;
        }

        private IList<TestedAnimalsVM> GetTestedAnimals(long? batchId, long? testId)
        {
            IList<TESTED_ANIMALS> dbModels = db.TESTED_ANIMALS.Where(a => a.BATCH_ID == batchId && a.TEST_ID == testId).ToList();
            IList<TestedAnimalsVM> vmModels = new List<TestedAnimalsVM>();
            foreach (var item in dbModels)
            {
                var vmModel = new TestedAnimalsVM();
                vmModel.AnimalTag = item.ANIMAL_LABEL;
                vmModel.DateEntered = item.CREATION_DATE;
                vmModel.IsNew = false;
                vmModel.Result = (item.TEST_RESULT == 1) ? "Positive" : "Negative";
                vmModels.Add(vmModel);
            }

            return vmModels;
        }

        private IList<TestedAnimalsVM> GetTestedRanges(long? batchId, long? testId)
        {
            IList<TESTED_RANGES> dbModels = db.TESTED_RANGES.Where(a => a.BATCH_ID == batchId && a.TEST_ID == testId).ToList();
            IList<TestedAnimalsVM> vmModels = new List<TestedAnimalsVM>();
            foreach (var item in dbModels)
            {
                var vmModel = new TestedAnimalsVM();
                vmModel.DateEntered = item.CREATION_DATE;
                vmModel.RangeFrom = item.RANGE_FROM;
                vmModel.RangeTo = item.RANGE_TO;
                vmModel.SystemId = item.SYSTEMID;
                vmModel.IsNew = false;
                vmModels.Add(vmModel);
            }

            return vmModels;
        }
        
        private bool ValidateRange(long batchId, long testId, string rangeFrom, string rangeTo)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
