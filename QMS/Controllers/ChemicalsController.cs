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

namespace QMS.Controllers
{
    public class ChemicalsController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private readonly string sessionName = "chemicals";

        // GET: /AnimalType/
        public ActionResult Index()
        {
            return View(db.CHEMICAL.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /AnimalType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CHEMICAL model = db.CHEMICAL.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: /Currencies/Create
        public ActionResult Create()
        {
            Session[sessionName] = null;
            IList<ListItem> chemicalTypesitems = GetChemicalTypesItems(); 
            ViewBag.ChemicalTypes = new SelectList(chemicalTypesitems, "Value", "Display");

            IList<ListItem> quarentineProcesses = GetQuarentineProcessesItems();
            ViewBag.PreQuarentineProcess = new SelectList(quarentineProcesses, "Value", "Display");

            return View();
        }
        public JsonResult Add(string CHEMICAL_NAME, string CHEMICAL_TYPE, string INVENTORY_REFERENCE, string NOTES, string PREQUARENTINE_PROCESS)
        {

            var models = new List<CHEMICAL>();
            if (Session[sessionName] != null)
                models = (List<CHEMICAL>)Session[sessionName];
            var currObj = new CHEMICAL();
            currObj.CHEMICAL_NAME = CHEMICAL_NAME;
            currObj.CHEMICAL_TYPE = CHEMICAL_TYPE;
            currObj.INVENTORY_REFERENCE = INVENTORY_REFERENCE;
            currObj.PREQUARENTINE_PROCESS = PREQUARENTINE_PROCESS;
            currObj.NOTES = NOTES;
            models.Add(currObj);
            Session[sessionName] = models;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Chemicals/Partial/_TypeTAddPartial.cshtml", models.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /ANIMAL_TYPE/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CHEMICAL model)
        {
            try
            {
                if (Session[sessionName] != null)
                {
                    List<CHEMICAL> modelList = (List<CHEMICAL>)Session[sessionName];
                    long id = 0;
                    if (db.CHEMICAL.Any())
                        id = db.CHEMICAL.Max(i => i.SYSTEMID);

                    foreach (var item in modelList)
                    {
                        var curObj = new CHEMICAL();
                        id += 1;
                        curObj.SYSTEMID = id;
                        curObj.CHEMICAL_NAME = item.CHEMICAL_NAME;
                        curObj.CHEMICAL_TYPE = item.CHEMICAL_TYPE;
                        curObj.INVENTORY_REFERENCE = item.INVENTORY_REFERENCE;
                        curObj.PREQUARENTINE_PROCESS = item.PREQUARENTINE_PROCESS;
                        curObj.NOTES = item.NOTES;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        curObj.LAST_UPDATED_BY = CurrentUserID;
                        db.CHEMICAL.Add(curObj);
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
                Session[sessionName] = null;
                IList<ListItem> chemicalTypesitems = GetChemicalTypesItems();
                ViewBag.ChemicalTypes = new SelectList(chemicalTypesitems, "Value", "Display");

                IList<ListItem> quarentineProcesses = GetQuarentineProcessesItems();
                ViewBag.PreQuarentineProcess = new SelectList(quarentineProcesses, "Value", "Display");

                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(model);
            }


            return View(model);
        }


        // GET: /animals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CHEMICAL model = db.CHEMICAL.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", model.STATUS);

            IList<ListItem> items = GetChemicalTypesItems();
            ViewBag.ChemicalTypes = new SelectList(items, "Value", "Display");

            IList<ListItem> quarentineProcesses = GetQuarentineProcessesItems();
            ViewBag.PreQuarentineProcess = new SelectList(quarentineProcesses, "Value", "Display");

            return View(model);
        }

        // POST: /animal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( CHEMICAL model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CHEMICAL originalModel = db.CHEMICAL.Find(model.SYSTEMID);
                    originalModel.LAST_UPDATE_DATE = DateTime.Now;
                    originalModel.LAST_UPDATED_BY = CurrentUserID;
                    originalModel.CHEMICAL_NAME = model.CHEMICAL_NAME;
                    originalModel.CHEMICAL_TYPE = model.CHEMICAL_TYPE;
                    originalModel.PREQUARENTINE_PROCESS = model.PREQUARENTINE_PROCESS;
                    originalModel.INVENTORY_REFERENCE = model.INVENTORY_REFERENCE;
                    originalModel.NOTES = model.NOTES;
                    originalModel.STATUS = model.STATUS;
                    db.Entry(originalModel).State = EntityState.Modified;
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
            CHEMICAL model = db.CHEMICAL.Find(id);
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
            CHEMICAL model = db.CHEMICAL.Find(id);
            model.AUTHORIZATION_DATE = DateTime.Now;
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

        private IList<ListItem> GetChemicalTypesItems()
        {
            IList<ListItem> items = new List<ListItem>();
            items.Add(new ListItem(QMSRes.ChemicalTypeSpray, "Spray"));
            items.Add(new ListItem(QMSRes.ChemicalTypePills, "Pills"));
            items.Add(new ListItem(QMSRes.ChemicalTypeInjections, "Injections"));
            return items;
        }

        private IList<ListItem> GetQuarentineProcessesItems()
        {
            IList<ListItem> items = new List<ListItem>();
            items.Add(new ListItem(QMSRes.ChemicalPreQuarentineNever, "Never"));
            items.Add(new ListItem(QMSRes.ChemicalPreQuarentineMandatory, "Mandatory"));
            items.Add(new ListItem(QMSRes.ChemicalPreQuarentineOptional, "Optional"));
            return items;
        }
    }
}
