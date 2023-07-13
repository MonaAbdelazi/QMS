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
    public class Vehicles_SetupController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "VehicleModels";
        
        private readonly string addedModelsSessionName = "addedVehModels";
        private readonly string removedModelsSessionName = "removedVehModels";


        // GET: /Regions_MarketsController/
        public ActionResult Index()
        {
            return View(db.VEHICLES_MAKE.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /Regions_MarketsController/Details/5
        public ActionResult Details(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VEHICLES_MAKE model = db.VEHICLES_MAKE.Find(id);
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
        public JsonResult Add(string model)
        {

            var models = new List<VEHICLES_MODEL>();
            if (Session[sessionName] != null)
                models = (List<VEHICLES_MODEL>)Session[sessionName];
            var minIndex = 0L;
            if (models != null && models.Count > 0)
            {
                minIndex = models.Min(a => a.SYSTEMID);
            }
            var currObj = new VEHICLES_MODEL();
            currObj.MODEL = model;
            currObj.SYSTEMID = minIndex - 1;
            models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Vehicles_Setup/Partial/SubmodelTRegPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
                
        public JsonResult AddDelete(int? id)
        {
            List<VEHICLES_MODEL> models = (List<VEHICLES_MODEL>)Session[sessionName];
            VEHICLES_MODEL subModel = null;

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


            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Vehicles_setup/Partial/SubmodelsTRegPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);

        }
        // POST: /markets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VEHICLES_MAKE model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<VEHICLES_MODEL> ChildModelList = (List<VEHICLES_MODEL>)Session[sessionName];
                    long idChild = 0;
                    long idModel = 0;
                    if (db.VEHICLES_MODEL.Any())
                        idChild = db.VEHICLES_MODEL.Max(i => i.SYSTEMID);
                    if (db.BANK.Any())
                        idModel = db.VEHICLES_MAKE.Max(i => i.SYSTEMID);
                    idModel += 1;
                    model.SYSTEMID = idModel;
                    model.STATUS = CommonUtils.STATUS_ACTIVE;
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    model.CREATED_BY = CurrentUserID;
                    model.CREATION_DATE = DateTime.Now;
                    db.VEHICLES_MAKE.Add(model);
                    foreach (var item in ChildModelList)
                    {
                        var curObj = new VEHICLES_MODEL();
                        idChild += 1;
                        curObj.SYSTEMID = idChild;
                        curObj.MAKE_ID = idModel;
                        curObj.MODEL = item.MODEL;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.VEHICLES_MODEL.Add(curObj);
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
            VEHICLES_MAKE model = db.VEHICLES_MAKE.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", model.STATUS);

            List<VEHICLES_MODEL> subModels = new List<VEHICLES_MODEL>();
            foreach (var item in model.VEHICLES_MODEL.Where(a=> a.STATUS == CommonUtils.STATUS_ACTIVE && a.MAKE_ID == id).ToList())
            {
                subModels.Add(item);
            }
            ViewBag.EditVehicleModels = subModels;
            Session[sessionName] = subModels;
            Session[addedModelsSessionName] = new List<VEHICLES_MODEL>();
            Session[removedModelsSessionName] = new List<VEHICLES_MODEL>();
            
            return View(model);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VEHICLES_MAKE model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<VEHICLES_MODEL> addedSubModels = (List<VEHICLES_MODEL>)Session[addedModelsSessionName];
                    List<VEHICLES_MODEL> removedSubModels = (List<VEHICLES_MODEL>)Session[removedModelsSessionName];

                    VEHICLES_MAKE freshObject = db.VEHICLES_MAKE.Find(model.SYSTEMID);
                    //freshObject.STATUS = model.STATUS;
                    freshObject.MAKE = model.MAKE;
                    model = freshObject;
                    model.LAST_UPDATE_DATE = DateTime.Now;
                    model.LAST_UPDATED_BY = CurrentUserID;
                    db.Entry(model).State = EntityState.Modified;

                    long idChild = 0;
                    if (db.BRANCH.Any())
                        idChild = db.BRANCH.Max(i => i.SYSTEMID);

                    foreach (var item in addedSubModels)
                    {
                        var curObj = new VEHICLES_MODEL();
                        idChild += 1;
                        curObj.SYSTEMID = idChild;
                        curObj.MAKE_ID = model.SYSTEMID;
                        curObj.MODEL = item.MODEL;
                        curObj.STATUS = CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.VEHICLES_MODEL.Add(curObj);
                    }

                    foreach (var item in removedSubModels)
                    {
                        item.STATUS = CommonUtils.STATUS_DELETED;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.LAST_UPDATED_BY = CurrentUserID;
                        item.VEHICLES_MAKE = model;
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
            List<VEHICLES_MODEL> mainList = (List<VEHICLES_MODEL>)Session[sessionName];
            List<VEHICLES_MODEL> removedList = (List<VEHICLES_MODEL>)Session[removedModelsSessionName];
            List<VEHICLES_MODEL> addedList = (List<VEHICLES_MODEL>)Session[addedModelsSessionName];
            VEHICLES_MODEL subModel = null;
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
                       

            ViewBag.EditVehicleModels = mainList;
            Session[sessionName] = mainList;
            Session[removedModelsSessionName] = removedList;
            Session[addedModelsSessionName] = addedList;
                        
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Vehicles_Setup/Partial/SubmodelEditGridPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);

        }

        public JsonResult EditAdd(string newModel)
        {
            var addedSubModels = (List<VEHICLES_MODEL>)Session[addedModelsSessionName];
            var mainList = (List<VEHICLES_MODEL>)Session[sessionName];

            var minIndex = 0L;
            if(mainList != null && mainList.Count > 0)
            {
                minIndex = mainList.Min(a => a.SYSTEMID);
            }

            var subModel = new VEHICLES_MODEL();
            subModel.MODEL = newModel;
            subModel.SYSTEMID = (minIndex > 0) ? -1 : minIndex - 1;
            addedSubModels.Add(subModel);
            mainList.Add(subModel);

            Session[addedModelsSessionName] = addedSubModels;
            ViewBag.EditVehicleModels = mainList;
            Session[sessionName] = mainList ;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Vehicle_Setup/Partial/SubmodelEditGridPartial.cshtml", mainList);
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // GET: /Currencies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VEHICLES_MAKE model = db.VEHICLES_MAKE.Find(id);
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
            VEHICLES_MAKE model = db.VEHICLES_MAKE.Find(id);
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
