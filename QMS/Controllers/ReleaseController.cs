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
using System.Text.RegularExpressions;

namespace QMS.Controllers
{
    public class ReleaseController : BaseController
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
            ReleaseViewModel modelVM = new ReleaseViewModel();
            string resultsHtml = "";
            if (!db.Batchs.Any(a => a.SYSTEMID == batchId && a.STATUS == CommonUtils.STATUS_ACTIVE))
            {
                modelVM.Message = QMSRes.RelMsgBatchNotFound;
                ViewBag.EnableSave = "No";
                resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Release/Partial/_TypeTAddPartial.cshtml", modelVM);
                return Json(resultsHtml, JsonRequestBehavior.AllowGet);
            }

            List<string> declinedMessages = new List<string>();
            var model = db.Batchs.Where(a => a.SYSTEMID == batchId && a.STATUS == CommonUtils.STATUS_ACTIVE).ToList()[0];
            modelVM.BatchId = batchId;
            modelVM.ExporterName = model.EXPORTER.EXPORTER_NAME;
            modelVM.Destination = model.DESTINATION.DESTINATION_NAME;
            modelVM.AnimalType = model.ANIMAL_TYPE.TYPE_NAME;
            modelVM.OriginallyAccepted = (int)model.AcceptedAnimals;
            //modelVM.ReadyForRelease = "xx";
            modelVM.RegistrationDate = ((DateTime)model.RegisterationDate).ToString("dd/MM/yyyy");
            if (model.DESTINATION.QUARENTINE_PERIOD!=null && model.DESTINATION.QUARENTINE_PERIOD.Count>0)
            modelVM.QuarantinePeriod = model.DESTINATION.QUARENTINE_PERIOD
                                            .Where(a => a.ANIMAL_TYPE_ID == model.typeID).ToList()[0]
                                            .QUARENTINE_DAYS;
            DateTime quarantineEndDate = ((DateTime)model.RegisterationDate).AddDays((double)modelVM.QuarantinePeriod);
            modelVM.QuarantineEndDate = quarantineEndDate.ToString("dd/MM/yyyy");
            if (quarantineEndDate < DateTime.Now.Date)
            {
                declinedMessages.Add(QMSRes.QuarantinePeriodNotPassed);
            }

            List<TEST_SUMMARY> tests_summary = db.TEST_SUMMARY.Where(a => a.BATCH_ID == batchId && a.CURRENT_STATUS != "Test Complete").ToList();

            foreach (var item in tests_summary)
            {
                string testInclomplete = QMSRes.ReleaseTestValidation1
                                        + item.TEST_NAME
                                        + QMSRes.ReleaseTestValidation2;
                declinedMessages.Add(testInclomplete);
            }

            modelVM.DisposedAnimals = db.DisposeDetails.Count(a => a.DiposedBatchs.STATUS == CommonUtils.STATUS_ACTIVE 
                                                                && a.BatchId == batchId);
            modelVM.PendingAnimals = db.TESTED_ANIMALS.Count(a => a.STATUS == CommonUtils.STATUS_ACTIVE
                                                                           && a.TEST_RESULT == 1
                                                                           && a.BATCH_ID == batchId
                                                                           && !db.DisposeDetails.Any(b=> b.DiposedBatchs.STATUS == CommonUtils.STATUS_ACTIVE
                                                                                                      && b.BatchId == batchId
                                                                                                      && b.AnimalTag == a.ANIMAL_LABEL
                                                                                                    )
                                                                        );



            if (declinedMessages.Count == 0)
            {
                modelVM.Message = QMSRes.BatchReadyForRelease;
                ViewBag.EnableSave = "Yes";
                modelVM.ReadyForRelease = modelVM.OriginallyAccepted - modelVM.DisposedAnimals - modelVM.PendingAnimals;
            }
            else
            {
                modelVM.Message = QMSRes.BatchNotReadyForRelease + "\n";
                modelVM.ReadyForRelease = 0;
                foreach (var item in declinedMessages)
                {
                    modelVM.Message += " *  " + item + "\n";
                }
                ViewBag.EnableSave = "No";
            }
            

            resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Release/Partial/_TypeTAddPartial.cshtml", modelVM);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Batchs model)
        {
            try { 

            Batchs dbModel = db.Batchs.Find(model.SYSTEMID);
            dbModel.STATUS = CommonUtils.STATUS_RELEASE;
            dbModel.LAST_UPDATED_BY = CurrentUserName;
            dbModel.LAST_UPDATE_DATE = DateTime.Now;
            db.Entry(dbModel).State = EntityState.Modified;
            InsertReleaseRecords(dbModel);
            db.SaveChanges();

            CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                TempData["AlertMessage"] = Feedback.SavedSuccessfully + "Print ";

                return RedirectToAction("Index");
        }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Success);

                return View(model);
    }
}
        public JsonResult GetReport(string Status)
        {
            if (Session["Release_ID"] != null)
            {
                int Release_ID = int.Parse(Session["Release_ID"].ToString());
                ReleaseRecord release = db.ReleaseRecord.Where(i => i.SYSTEMID == Release_ID).SingleOrDefault();
                 List<BatchViewModel> vmlist = new List<BatchViewModel>();
                foreach (var item in release.ReleaseDetails.ToList())
                {
                    BatchViewModel vm = new BatchViewModel();
                    vm.AnimalTag1 = item.AnimalTag1;
                    vm.AnimalTag2 = item.AnimalTag2;
                    vm.AnimalTag3 = item.AnimalTag3;
                    vm.AnimalTag4 = item.AnimalTag4;

                    vmlist.Add(vm);
                }
                if (release != null && release.ReleaseDetails.Count > 0)
                {

                    var paremeters = new List<KeyValuePair<string, string>>();

                    // paremeters.Add(new KeyValuePair<string, string>("Name", "Active"));
                    paremeters.Add(new KeyValuePair<string, string>("ReportTitle", release.ReportTitle.ToString()));

                    paremeters.Add(new KeyValuePair<string, string>("LiveStockCount", release.AnimalCount.ToString()));
                    paremeters.Add(new KeyValuePair<string, string>("QuarantineFacility", release.QuarantineFacility));
                    paremeters.Add(new KeyValuePair<string, string>("ExporterName", release.EXPORTER.EXPORTER_NAME));
                    paremeters.Add(new KeyValuePair<string, string>("AnimalType", release.ANIMAL_TYPE.TYPE_NAME));
                    paremeters.Add(new KeyValuePair<string, string>("Destination", release.DESTINATION.DESTINATION_NAME));

                    Session["ReportParameter"] = paremeters;
                    Session["ReportData"] = vmlist;
                    Session["ReportName"] = "rptRleaseReport";
                    //   Session["ReportOption"] = "Invoiceslist";

                    return Json(true, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    // SetFeedback(Feedback.NoDataToShow, Core.Classes.Common.CommonUtils.FEEDBACK_ERROR);
                    return Json(false, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                // SetFeedback(Feedback.NoDataToShow, Core.Classes.Common.CommonUtils.FEEDBACK_ERROR);
                return Json(false, JsonRequestBehavior.AllowGet);

            }

        }


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

        private void InsertReleaseRecords(Batchs batch)
        {
            long id = 0;
            if (db.ReleaseRecord.Any())
            {
                id = db.ReleaseRecord.Max(i => i.SYSTEMID);
            }
            ReleaseRecord model = new ReleaseRecord();
            id++;
            Session["Release_ID"] = id;
            model.SYSTEMID = id;
            model.BatchId = batch.SYSTEMID;
            model.ExporterId = batch.ExporterID;
            model.DestinationId = batch.DestenationID;
            model.AnimalTypeId = batch.typeID;
            model.QuarantineFacility = QMSRes.QuarantineFacility;
            model.ReportTitle = QMSRes.ReleaseReportTitle;

            List<DisposeDetails> disposedAnimalsList = db.DisposeDetails.Where(a => a.DiposedBatchs.STATUS == CommonUtils.STATUS_ACTIVE
                                                                && a.BatchId == batch.SYSTEMID).ToList();
            List<TESTED_ANIMALS> pendingAnimalsList = db.TESTED_ANIMALS.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE
                                                                           && a.TEST_RESULT == 1
                                                                           && a.BATCH_ID == batch.SYSTEMID
                                                                           && !db.DisposeDetails.Any(b => b.DiposedBatchs.STATUS == CommonUtils.STATUS_ACTIVE
                                                                                                      && b.BatchId == batch.SYSTEMID
                                                                                                      && b.AnimalTag == a.ANIMAL_LABEL
                                                                                                    )
                                                                        ).ToList();
            int disposedAnimals = disposedAnimalsList.Count();
            int pendingAnimals = pendingAnimalsList.Count();

            model.AnimalCount = (int)batch.AcceptedAnimals - disposedAnimals - pendingAnimals;
            model.STATUS = CommonUtils.STATUS_ACTIVE;
            model.CREATED_BY = CurrentUserID;
            model.AUTHORIZED_BY = CurrentUserID;
            model.CREATION_DATE = DateTime.Now;
            model.AUTHORIZATION_DATE = DateTime.Now;

            db.ReleaseRecord.Add(model);

            List<PensLables> penLabels = batch.PensLables.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE && a.batchId == batch.SYSTEMID).ToList();
            
            int releaseDetailsTagIndex = 0;
            long releaseDetailsIndex = 0;
            if (db.ReleaseDetails.Any())
            {
                releaseDetailsIndex = db.ReleaseDetails.Max(a => a.SYSTEMID);
            }
            List<ReleaseDetails> releaseDetailsList = new List<ReleaseDetails>();

            foreach (var item in penLabels)
            {
                int dashIndex = item.lableRanges.IndexOf('-');
                string fromRangeStr = item.lableRanges.Substring(0, dashIndex).Trim();
                string toRangeStr = item.lableRanges.Substring(dashIndex + 1);
                string fromRangeNumeric = String.Empty;
                string alpha = String.Empty;
                for (int i = 0; i < fromRangeStr.Length; i++)
                {
                    if (Char.IsDigit(fromRangeStr[i]))
                    {
                        fromRangeNumeric += fromRangeStr[i];
                    }
                    else
                    {
                        alpha += fromRangeStr[i];
                    }
                }
                int fromRange = int.Parse(fromRangeNumeric);
                int toRange = int.Parse(toRangeStr);
                alpha = alpha.Trim();

                for (int i = fromRange; i <= toRange; i++)
                {
                    string tag = alpha + i.ToString();
                    if (!disposedAnimalsList.Any(a=> a.AnimalTag == tag) && !pendingAnimalsList.Any(a=> a.ANIMAL_LABEL == tag))
                    {
                        if (releaseDetailsTagIndex == 0)
                        {
                            ReleaseDetails subModel = new ReleaseDetails();
                            releaseDetailsIndex++;
                            subModel.SYSTEMID = releaseDetailsIndex;
                            subModel.ReleaseRecordId = id;
                            subModel.AnimalTag1 = tag;
                            releaseDetailsList.Add(subModel);
                        }
                        else if (releaseDetailsTagIndex == 1)
                        {
                            ReleaseDetails subModel = releaseDetailsList[releaseDetailsList.Count - 1];
                            subModel.AnimalTag2 = tag;
                        }
                        else if (releaseDetailsTagIndex == 2)
                        {
                            ReleaseDetails subModel = releaseDetailsList[releaseDetailsList.Count - 1];
                            subModel.AnimalTag3 = tag;
                        }
                        else
                        {
                            ReleaseDetails subModel = releaseDetailsList[releaseDetailsList.Count - 1];
                            subModel.AnimalTag4 = tag;
                        }
                        releaseDetailsTagIndex++;
                        int tempIndex;
                        Math.DivRem(releaseDetailsTagIndex, 4, out tempIndex);
                        releaseDetailsTagIndex = tempIndex;
                    }
                }

            }

            foreach (var item in releaseDetailsList)
            {
                db.ReleaseDetails.Add(item);
            }           
        }
        #endregion

    }
}
