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
    public class Regions_MarketsController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();

        // GET: /Regions_MarketsController/
        public ActionResult Index()
        {
            return View(db.REGION.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE).ToList());
        }

        // GET: /Regions_MarketsController/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REGION region = db.REGION.Find(id);
            if (region == null)
            {
                return HttpNotFound();
            }
            return View(region);
        }

        // GET: /region/Create
        public ActionResult Create()
        {
            Session["markets"] = null;

            return View();
        }
        public JsonResult Add(string marketName,string REGION_NAME)
        {

            var markets = new List<MARKET>();
            if (Session["markets"] != null)
                markets = (List<MARKET>)Session["markets"];
            var currObj = new MARKET();
            currObj.REGION = new REGION();
            currObj.REGION.REGION_NAME = REGION_NAME;
           currObj.MARKET_NAME = marketName;
                markets.Add(currObj);
            Session["markets"] = markets;
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/Regions_Markets/Partial/_MarketTRegPartial.cshtml", markets.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /markets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( REGION region)
        {
            try
            {
                if (Session["markets"] != null)
                {
                    List<MARKET> marketList = (List<MARKET>)Session["markets"];
                    long idMaket = 0;
                    long idRegion = 0;
                    if (db.MARKET.Any())
                        idMaket = db.MARKET.Max(i => i.SYSTEMID);
                    if (db.REGION.Any())
                        idRegion = db.REGION.Max(i => i.SYSTEMID);
                    idRegion += 1;
                    region.SYSTEMID = idRegion;
                    region.STATUS = CommonUtils.STATUS_ACTIVE;
                    region.LAST_UPDATE_DATE = DateTime.Now;
                    region.CREATED_BY = CurrentUserID;
                    region.CREATION_DATE = DateTime.Now;
                    db.REGION.Add(region);
                    foreach (var item in marketList)
                    {
                        var curObj = new MARKET();
                        idMaket += 1;
                        curObj.SYSTEMID = idMaket;
                        curObj.REGION_ID = idRegion;
                        curObj.MARKET_NAME = item.MARKET_NAME;
                        curObj.STATUS =CommonUtils.STATUS_ACTIVE;
                        curObj.LAST_UPDATE_DATE = DateTime.Now;
                        curObj.CREATED_BY = CurrentUserID ;
                        curObj.CREATION_DATE = DateTime.Now;
                        db.MARKET.Add(curObj);
                    }
                    db.SaveChanges();
                    Session["markets"] = null;

                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    return RedirectToAction("Index");
                }
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View();
            }
            catch (Exception ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(region);
            }


            return View(region);
        }


        // GET: /markets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REGION region = db.REGION.Find(id);
            if (region == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = new SelectList(CommonUtils.getStatus(), "Value", "Text", region.STATUS);

            return View(region);
        }

        // POST: /region/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( REGION region ,List<MARKET> markets)
        {
            try
            {
               // if (ModelState.IsValid)
                //{
                    REGION regions = db.REGION.Where(i => i.SYSTEMID == region.SYSTEMID).SingleOrDefault();
                    foreach (var item in markets)
                    {
                        MARKET mrk = regions.MARKET.Where(i => i.SYSTEMID == item.SYSTEMID).SingleOrDefault();
                        if (mrk != null)
                        {
                            mrk.MARKET_NAME = item.MARKET_NAME;
                            db.Entry(mrk).State = EntityState.Modified;

                        }
                        else
                        {
                            MARKET mkt = new MARKET();
                            long idMaket = 0;
                            if (db.MARKET.Any())
                                idMaket = db.MARKET.Max(i => i.SYSTEMID);
                            idMaket += 1;
                            mkt.SYSTEMID = idMaket;
                            mkt.REGION_ID = regions.SYSTEMID;
                            mkt.MARKET_NAME = item.MARKET_NAME;
                            mkt.STATUS = CommonUtils.STATUS_ACTIVE;
                            mkt.LAST_UPDATE_DATE = DateTime.Now;
                            mkt.CREATED_BY = CurrentUserID;
                            mkt.CREATION_DATE = DateTime.Now;
                            db.MARKET.Add(mkt);
                            regions.MARKET.Add(mkt);
                        }
                    }
                    regions.REGION_NAME = region.REGION_NAME;
                    regions.LAST_UPDATE_DATE = DateTime.Now;
                    db.Entry(regions).State = EntityState.Modified;
                    db.SaveChanges();
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);

                    return RedirectToAction("Index");
               // }
                //return View(region);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(region);
            }
        }

        // GET: /region/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REGION region = db.REGION.Find(id);
            if (region == null)
            {
                return HttpNotFound();
            }
            return View(region);
        }

        // POST: /region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            REGION region = db.REGION.Find(id);
            region.AUTHORIZATION_DATE = DateTime.Now;
            region.STATUS = CommonUtils.STATUS_DELETED;
            foreach (var item in region.MARKET)
            {
                item.STATUS = CommonUtils.STATUS_DELETED;
                db.Entry(item).State = EntityState.Modified;

            }

            db.Entry(region).State = EntityState.Modified;
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
