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
    public class ClinicCaseController : BaseController
    {
        #region Private fields
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "clinicCase";
        private readonly string addedModelsSessionName = "clinicCaseTestsAdded";
        private readonly string removedModelsSessionName = "clinicCaseTestsRemoved";
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

            var models = db.CLINIC_CASES.Where(a => a.BATCH_ID == batchId).ToList();
            ViewBag.ClinicCases = models;

            var model = db.Batchs.Find(batchId);

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/_TypeTAddPartial.cshtml", model);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CaseEdit(long? batchId, long? caseId)
        {
            try
            {
                CLINIC_CASES model = db.CLINIC_CASES.Find(caseId);
                if (model == null)
                {
                    return HttpNotFound();
                }

                IList<CASE_SYMPTOMS> caseSymptoms = db.CASE_SYMPTOMS.Where(a => a.CASE_ID == caseId && a.STATUS == CommonUtils.STATUS_ACTIVE).ToList();
                Session["CaseSymptoms"] = caseSymptoms;
                IList<CASE_DIAGNOSIS> caseDiagnoses = db.CASE_DIAGNOSIS.Where(a => a.CASE_ID == caseId && a.STATUS == CommonUtils.STATUS_ACTIVE).ToList();
                Session["CaseDiagnoses"] = caseSymptoms;
                IList<CASE_TESTS> caseTests = db.CASE_TESTS.Where(a => a.CASE_ID == caseId && a.STATUS == CommonUtils.STATUS_ACTIVE).ToList();
                Session["CaseTests"] = caseSymptoms;                

                return View(model);
            }
            catch
            {
                return HttpNotFound();
            }
        }
        #endregion

        #region Create

        public ActionResult Create(long? batchId)
        {

            Session[sessionName] = null;

            Session["CaseTests"] = new List<CaseTestVM>();
            Batchs model = db.Batchs.Find(batchId);
            ViewBag.CaseBatch = model;
            Session["CaseBatch"] = model;
            ViewBag.CaseTypes = new SelectList(GetCaseTypes(), "Value", "Text");
            ViewBag.DiagnosesList = new SelectList(db.DIAGNOSES.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE).ToList(), "SYSTEMID", "SHORT_DESCRIPTION");

            return View();
        }

        public JsonResult AddTest(long? testId)
        {
            var models = new List<CaseTestVM>();
            if (Session["CaseTests"] != null)
                models = (List<CaseTestVM>)Session["CaseTests"];

            if (models.Where(a => a.SYSTEMID == testId).ToList().Count > 0)
            {
                CommonUtils.SetFeedback("Item already added", Feedback.Feedback_Error);
            }
            else
            {
                var currObj = new CaseTestVM();

                var test = db.TESTS.Find(testId);
                currObj.SYSTEMID = (long)testId;
                currObj.TestName = test.TEST_NAME;
                models.Add(currObj);
                Session["CaseTests"] = models;
            }
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/AddTestPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteTest(long? testId)
        {
            List<CaseTestVM> models = (List<CaseTestVM>)Session["CaseTests"];

            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].SYSTEMID == testId)
                {
                    models.RemoveAt(i);
                    break;
                }
            }

            Session["CaseTests"] = models;


            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/AddTestPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClinicCaseVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<CaseTestVM> vmModels = ((List<CaseTestVM>)Session["CaseTests"]);
                    Batchs caseBatch = (Batchs)Session["CaseBatch"];
                    long id = 0;
                    if (db.CLINIC_CASES.Any())
                    {
                        id = db.TESTED_ANIMALS.Max(i => i.SYSTEMID);
                    }
                    
                    long idChild = 0;
                    if (db.CASE_TESTS.Any())
                    {
                        idChild = db.TESTED_ANIMALS.Max(i => i.SYSTEMID);
                    }

                    CLINIC_CASES newModel = new CLINIC_CASES();
                    id++;
                    newModel.SYSTEMID = id;
                    newModel.BATCH_ID = caseBatch.SYSTEMID;
                    newModel.CASE_TYPE = model.CASE_TYPE;
                    newModel.ANIMAL_LABEL = model.ANIMAL_LABEL;
                    newModel.Diagnosis = model.Diagnosis;
                    newModel.DISPOSED = model.DISPOSED;
                    newModel.Notes = model.Notes;
                    newModel.STATUS = newModel.STATUS = CommonUtils.STATUS_ACTIVE;
                    newModel.CREATED_BY = CurrentUserID;
                    newModel.AUTHORIZED_BY = CurrentUserID;
                    newModel.CREATION_DATE = DateTime.Now;
                    newModel.AUTHORIZATION_DATE = DateTime.Now;
                    db.CLINIC_CASES.Add(newModel);
                    foreach (var item in vmModels)
                    {
                        CASE_TESTS childModel = new CASE_TESTS();
                        idChild++;
                        childModel.SYSTEMID = idChild;
                        childModel.CASE_ID = id;
                        childModel.TEST_ID = item.SYSTEMID;
                        childModel.ANIMAL_LABEL = model.ANIMAL_LABEL;
                        childModel.STATUS = CommonUtils.STATUS_ACTIVE;
                        childModel.CREATED_BY = CurrentUserID;
                        childModel.AUTHORIZED_BY = CurrentUserID;
                        childModel.CREATION_DATE = DateTime.Now;
                        childModel.AUTHORIZATION_DATE = DateTime.Now;

                        db.CASE_TESTS.Add(childModel);
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
        #endregion

        #region Edit
        public ActionResult Edit(long? batchId , long? caseId )
        {
            if (caseId == null || batchId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClinicCaseVM model = (ClinicCaseVM)db.CLINIC_CASES.Find(caseId);

            if (model == null)
            {
                return HttpNotFound();
            }

            List<CaseTestVM> subModels = new List<CaseTestVM>();
            foreach (var item in model.CASE_TESTS.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE && a.CASE_ID == caseId).ToList())
            {
                CaseTestVM subModel = new CaseTestVM();
                subModel.SYSTEMID = item.SYSTEMID;
                subModel.TestName = item.TESTS.TEST_NAME;
                subModel.Result = item.TEST_RESULT;
                subModels.Add(subModel);
            }
            ViewBag.EditSubmodels = subModels;
            Session["CaseTests"] = subModels;
            Session[addedModelsSessionName] = new List<CaseTestVM>();
            Session[removedModelsSessionName] = new List<CaseTestVM>();

            Batchs parentModel = db.Batchs.Find(batchId);
            ViewBag.CaseBatch = model;
            Session["CaseBatch"] = model;
            ViewBag.CaseTypes = new SelectList(GetCaseTypes(), "Value", "Text");
            ViewBag.DiagnosesList = new SelectList(db.DIAGNOSES.Where(a => a.STATUS == CommonUtils.STATUS_ACTIVE).ToList(), "SYSTEMID", "SHORT_DESCRIPTION");


            return View(model);
        }

        public JsonResult EditAddTest(long? testId)
        {   
            var addedSubModels = (List<CaseTestVM>)Session[addedModelsSessionName];
            var mainList = (List<CaseTestVM>)Session["CaseTests"];

            var subModel = new CaseTestVM();
            var test = db.TESTS.Find(testId);
            subModel.TestName = test.TEST_NAME;
            subModel.SYSTEMID = (long)testId;
            subModel.NewModel = true;
            addedSubModels.Add(subModel);
            mainList.Add(subModel);

            Session[addedModelsSessionName] = addedSubModels;
            ViewBag.EditSubmodels = mainList;
            Session["CaseTests"] = mainList;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/EditTestPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditDeleteTest(long? testId)
        {

            List<CaseTestVM> mainList = (List<CaseTestVM>)Session["CaseTests"];
            List<CaseTestVM> removedList = (List<CaseTestVM>)Session[removedModelsSessionName];
            List<CaseTestVM> addedList = (List<CaseTestVM>)Session[addedModelsSessionName];
            
            for (int i = 0; i < mainList.Count; i++)
            {
                if (mainList[i].SYSTEMID == testId)
                {
                    var subModel = mainList[i];
                    mainList.RemoveAt(i);
                    removedList.Add(subModel);
                    if (subModel.NewModel)
                    {
                        for (int j = 0; j < addedList.Count; j++)
                        {
                            if (addedList[j].SYSTEMID == testId)
                            {
                                addedList.RemoveAt(j);
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            ViewBag.EditSubmodels = mainList;
            Session["CaseTests"] = mainList;
            Session[removedModelsSessionName] = removedList;
            Session[addedModelsSessionName] = addedList;

            
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/EditTestPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);


        }

        public ActionResult Edit(ClinicCaseVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<CaseTestVM> addedSubModels = (List<CaseTestVM>)Session[addedModelsSessionName];
                    List<CaseTestVM> removedSubModels = (List<CaseTestVM>)Session[removedModelsSessionName];

                    ClinicCaseVM freshObject = (ClinicCaseVM)db.CLINIC_CASES.Find(model.SYSTEMID);
                    freshObject.CASE_TYPE = model.CASE_TYPE;
                    freshObject.ANIMAL_LABEL = model.ANIMAL_LABEL;
                    freshObject.Notes = model.Notes;
                    freshObject.Diagnosis = model.Diagnosis;
                    freshObject.DISPOSED = model.DISPOSED;                    
                    model = freshObject;
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    model.LAST_UPDATED_BY = CurrentUserID;
                    db.Entry((CLINIC_CASES)model).State = EntityState.Modified;

                    long idChild = 0;
                    if (db.CASE_TESTS.Any())
                        idChild = db.CASE_TESTS.Max(i => i.SYSTEMID);

                    foreach (var item in addedSubModels)
                    {
                        var childModel = new CASE_TESTS();
                        idChild += 1;
                        childModel.CASE_ID = model.SYSTEMID;
                        childModel.TEST_ID = item.SYSTEMID;
                        childModel.ANIMAL_LABEL = model.ANIMAL_LABEL;
                        childModel.STATUS = CommonUtils.STATUS_ACTIVE;
                        childModel.CREATED_BY = CurrentUserID;
                        childModel.AUTHORIZED_BY = CurrentUserID;
                        childModel.CREATION_DATE = DateTime.Now;
                        childModel.AUTHORIZATION_DATE = DateTime.Now;
                    }

                    foreach (var item in removedSubModels)
                    {
                        var childModel = db.CASE_TESTS.Where(a => a.CASE_ID == model.SYSTEMID).ToList()[0];
                        childModel.STATUS = CommonUtils.STATUS_DELETED;
                        childModel.LAST_UPDATE_DATE = DateTime.Now;
                        childModel.LAST_UPDATED_BY = CurrentUserID;
                        childModel.CLINIC_CASES = model;
                        db.Entry(childModel).State = EntityState.Modified;
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


        #endregion

        #region Delete

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClinicCaseVM model =(ClinicCaseVM) db.CLINIC_CASES.Find(id);
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
            CLINIC_CASES model = db.CLINIC_CASES.Find(id);
            model.LAST_UPDATE_DATE = DateTime.Now;
            model.LAST_UPDATED_BY = CurrentUserID;

            model.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

            return RedirectToAction("Index");
        }

        #endregion

        #region RecordTest
        public ActionResult CaseTestIndex()
        {
            return View(db.CASE_TESTS.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE 
                                              && i.CLINIC_CASES.STATUS == CommonUtils.STATUS_ACTIVE
                                              && i.CLINIC_CASES.Batchs.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        public JsonResult RecordTest(long caseTestId)
        {
            var model = db.CASE_TESTS.Find(caseTestId);

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/ClinicCase/Partial/TestRecordPartial.cshtml", model);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecordTestResult(long caseTestId, string animalLabel, string result)
        {
            var model = db.CASE_TESTS.Find(caseTestId);

            model.ANIMAL_LABEL = animalLabel;
            model.TEST_RESULT = result;

            model.LAST_UPDATED_BY = CurrentUserID;
            model.LAST_UPDATE_DATE = DateTime.Now;

            db.Entry(model).State = EntityState.Modified;

            db.SaveChanges();

            return Json(null, JsonRequestBehavior.AllowGet);
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



        private List<SelectListItem> GetCaseTypes()
        {
            var selectList = new List<SelectListItem>
            {

                new SelectListItem {Value = "SingleCase",Text = "Single Case" },
                //new SelectListItem { Value ="MultipleCase",Text ="Multiple Case"},
                new SelectListItem { Value ="Outbreak",Text ="Outbreak"}

            };
            return selectList;
        }
        #endregion

    }
}
