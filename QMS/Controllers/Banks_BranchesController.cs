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
    public class Banks_BranchesController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "branches";
        
        private readonly string addedModelsSessionName = "addedBranches";
        private readonly string removedModelsSessionName = "removedBranches";


        // GET: /Regions_MarketsController/
        public ActionResult Index()
        {
            return View(db.BANK.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /Regions_MarketsController/Details/5
        public ActionResult Details(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BANK model = db.BANK.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: /region/Create
        public ActionResult Create()
        {
            Session[sessionName] = null;

            return View();
        }
        public JsonResult Add(string branchName)
        {

            var models = new List<BRANCH>();
            if (Session[sessionName] != null)
                models = (List<BRANCH>)Session[sessionName];
            var minIndex = 0L;
            if (models != null && models.Count > 0)
            {
                minIndex = models.Min(a => a.SYSTEMID);
            }
            var currObj = new BRANCH();
            currObj.BRANCH_NAME = branchName;
            currObj.SYSTEMID = minIndex - 1;
            models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Banks_Branches/Partial/BranchesTRegPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
                
        public JsonResult AddDelete(int? id)
        {
            List<BRANCH> models = (List<BRANCH>)Session[sessionName];
            BRANCH subModel = null;

            for (int i = 0; i < models.Count; i++)
            {
                if (models[i].SYSTEMID == id)
                {
                    subModel = models[i];
                    models.RemoveAt(i);
                    break;
                }
            }

            Session[sessionName] = models;

            BANK model = db.BANK.Find(subModel.BANK_ID);

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Banks_Branches/Partial/BranchesTRegPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);

        }
        // POST: /markets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BANK model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<BRANCH> ChildModelList = (List<BRANCH>)Session[sessionName];
                    long idChild = 0;
                    long idModel = 0;
                    if (db.BRANCH.Any())
                        idChild = db.BRANCH.Max(i => i.SYSTEMID);
                    if (db.BANK.Any())
                        idModel = db.BANK.Max(i => i.SYSTEMID);
                    idModel += 1;
                    model.SYSTEMID = idModel;
                    model.STATUS = CommonUtils.STATUS_ACTIVE;
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    model.CREATED_BY = CurrentUserID;
                    model.CREATION_DATE = DateTime.Now;
                    db.BANK.Add(model);
                    foreach (var item in ChildModelList)
                    {
                        var curObj = new BRANCH();
                        idChild += 1;
                        curObj.SYSTEMID = idChild;
                        curObj.BANK_ID = idModel;
                        curObj.BRANCH_NAME = item.BRANCH_NAME;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.BRANCH.Add(curObj);
                    }
                    db.SaveChanges();
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


        }


        // GET: /animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BANK model = db.BANK.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", model.STATUS);

            List<BRANCH> bankBranches = new List<BRANCH>();
            foreach (var item in model.BRANCH.Where(a=> a.STATUS == CommonUtils.STATUS_ACTIVE && a.BANK_ID == id).ToList())
            {
                bankBranches.Add(item);
            }
            ViewBag.EditBranches = bankBranches;
            Session[sessionName] = bankBranches;
            Session[addedModelsSessionName] = new List<BRANCH>();
            Session[removedModelsSessionName] = new List<BRANCH>();
            
            return View(model);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BANK model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<BRANCH> addedSubModels = (List<BRANCH>)Session[addedModelsSessionName];
                    List<BRANCH> removedSubModels = (List<BRANCH>)Session[removedModelsSessionName];

                    BANK freshObject = db.BANK.Find(model.SYSTEMID);
                    freshObject.STATUS = model.STATUS;
                    freshObject.BANK_NAME = model.BANK_NAME;
                    model = freshObject;
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    model.LAST_UPDATED_BY = CurrentUserID;
                    db.Entry(model).State = EntityState.Modified;

                    long idChild = 0;
                    if (db.BRANCH.Any())
                        idChild = db.BRANCH.Max(i => i.SYSTEMID);

                    foreach (var item in addedSubModels)
                    {
                        var curObj = new BRANCH();
                        idChild += 1;
                        curObj.SYSTEMID = idChild;
                        curObj.BANK_ID = model.SYSTEMID;
                        curObj.BRANCH_NAME = item.BRANCH_NAME;
                        curObj.STATUS = CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.BRANCH.Add(curObj);
                    }

                    foreach (var item in removedSubModels)
                    {
                        item.STATUS = CommonUtils.STATUS_DELETED;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.LAST_UPDATED_BY = CurrentUserID;
                        item.BANK = model;
                        db.Entry(item).State = EntityState.Modified;
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

        public JsonResult EditDelete(int? id)
        {
            List<BRANCH> mainList = (List<BRANCH>)Session[sessionName];
            List<BRANCH> removedList = (List<BRANCH>)Session[removedModelsSessionName];
            List<BRANCH> addedList = (List<BRANCH>)Session[addedModelsSessionName];
            BRANCH subModel = null;
            for (int i = 0; i < mainList.Count; i++)
            {
                if (mainList[i].SYSTEMID == id)
                {
                    subModel = mainList[i];
                    mainList.RemoveAt(i);
                    removedList.Add(subModel);
                    if(id < 0)
                    {
                        for (int j = 0; j < addedList.Count; j++)
                        {
                            if (addedList[j].SYSTEMID == id)
                            {
                                addedList.RemoveAt(j);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
                       

            ViewBag.EditBranches = mainList;
            Session[sessionName] = mainList;
            Session[removedModelsSessionName] = removedList;
            Session[addedModelsSessionName] = addedList;

            BANK model = db.BANK.Find(subModel.BANK_ID);

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Banks_Branches/Partial/BranchesEditGridPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);

        }

        public JsonResult EditAdd(string newBranchName)
        {
            var addedSubModels = (List<BRANCH>)Session[addedModelsSessionName];
            var mainList = (List<BRANCH>)Session[sessionName];

            var minIndex = 0L;
            if(mainList != null && mainList.Count > 0)
            {
                minIndex = mainList.Min(a => a.SYSTEMID);
            }

            var subModel = new BRANCH();
            subModel.BRANCH_NAME = newBranchName;
            subModel.SYSTEMID = (minIndex > 0) ? -1 : minIndex - 1;
            addedSubModels.Add(subModel);
            mainList.Add(subModel);

            Session[addedModelsSessionName] = addedSubModels;
            ViewBag.EditBranches = mainList;
            Session[sessionName] = mainList ;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Banks_Branches/Partial/BranchesEditGridPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BANK model = db.BANK.Find(id);
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
            BANK model = db.BANK.Find(id);
            model.LAST_UPDATE_DATE = DateTime.Now;
            model.LAST_UPDATED_BY = CurrentUserID;

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
