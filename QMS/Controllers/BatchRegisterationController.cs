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
using System.Web.UI.WebControls;
using System.IO;

namespace QMS.Controllers
{
    public class BatchViewModel
    {
        public Batchs batch { get; set; }
        public bool verified { get; set; }

        public bool NotValid { get; set; }

        public string AnimalTag1 { get; set; }
        public string AnimalTag4 { get; set; }

        public string AnimalTag2 { get; set; }
        public string AnimalTag3 { get; set; }




    }
    public class BatchRegisterationController : BaseController
    {
        private QMSEntities db = new QMSEntities();
        private IdentityContext dbuser = new IdentityContext();
        private static long bno;
        // GET: /Batchs/
        public ActionResult Index()
        {
            return View(db.Batchs.Where(i => i.STATUS == CommonUtils.STATUS_ACTIVE || i.STATUS=="Entered" && i.VerfiyStatus!= "No").ToList());
        }
        public ActionResult VerifyIndex()
        {
            List<Batchs> list = db.Batchs.Where(i => i.STATUS != "Rejected" && i.VerfiyStatus!="Yes" && i.VerfiyStatus != "No").ToList();
            List<BatchViewModel> vmlist = new List<BatchViewModel>();
            foreach (var item in list)
            {
                BatchViewModel vm = new BatchViewModel();
                vm.batch = item;
                vmlist.Add(vm);
            }
            return View(vmlist);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyIndex(List<BatchViewModel> RepeaterVwMdl)
        {

            foreach (var item in RepeaterVwMdl)
            {
                Batchs batch = db.Batchs.Where(i => i.SYSTEMID == item.batch.SYSTEMID).SingleOrDefault();

                if (item.verified)
                {
                    batch.STATUS = "Active";
                    batch.VerfiyStatus = "Yes";
                    db.Entry(batch).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (item.NotValid)
                {
                    if (batch.STATUS.Trim() == "Entered")
                        batch.STATUS = "Rejected";
                    else
                        batch.STATUS = "Active";
                    batch.VerfiyStatus = "No";
                    db.Entry(batch).State = EntityState.Modified;
                    db.SaveChanges();

                    //   RecomObj.STATUS = "Rejected";
                }

            }
            return RedirectToAction("VerifyIndex");
        }


        // GET: /Pens/
        public ActionResult Done()
        {
            return View(db.Batchs.Where(i => i.SYSTEMID == bno).SingleOrDefault());
        }

        // GET: /Pens/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PEN pen = db.PEN.Find(id);
            if (pen == null)
            {
                return HttpNotFound();
            }
            return View(pen);
        }
        public JsonResult SaveMarkets(long idRegion)
        {
            if (Session["markets"] != null)
            {
                List<MARKET> marketList = (List<MARKET>)Session["markets"];
                long idMaket = 0;
                if (db.MARKET.Any())
                    idMaket = db.MARKET.Max(i => i.SYSTEMID);
                foreach (var item in marketList)
                {
                    var curObj = new MARKET();
                    idMaket += 1;
                    curObj.SYSTEMID = idMaket;
                    curObj.REGION_ID = idRegion;
                    curObj.MARKET_NAME = item.MARKET_NAME;
                    curObj.STATUS = CommonUtils.STATUS_ACTIVE;
                    curObj.LAST_UPDATE_DATE = DateTime.Now;
                    curObj.CREATED_BY = CurrentUserID;
                    curObj.CREATION_DATE = DateTime.Now;
                    db.MARKET.Add(curObj);
                }
                db.SaveChanges();
                Session["markets"] = null;
                return Json(idRegion, JsonRequestBehavior.AllowGet);// pass new id to dropdownlist


            }
            return Json(0, JsonRequestBehavior.AllowGet);// pass new id to dropdownlist

        }


        public JsonResult SaveRegionsMarkets()
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
                REGION region = new REGION();
                region.REGION_NAME = marketList.First().REGION.REGION_NAME;
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
                    curObj.STATUS = CommonUtils.STATUS_ACTIVE;
                    curObj.LAST_UPDATE_DATE = DateTime.Now;
                    curObj.CREATED_BY = CurrentUserID;
                    curObj.CREATION_DATE = DateTime.Now;
                    db.MARKET.Add(curObj);
                }
                db.SaveChanges();
                Session["markets"] = null;
                return Json(idRegion, JsonRequestBehavior.AllowGet);// pass new id to dropdownlist


            }
            return Json(0, JsonRequestBehavior.AllowGet);// pass new id to dropdownlist

        }

        public JsonResult AddData(string EXPORTER_NAME, string EXPORTER_TYPE_ID, string BANK_BRANCH_ID, string ID_TYPE_ID, string ID_REFERENCE, string BIRTH_ESTABLISHMENT_DATE)
        {
            EXPORTER exp = new EXPORTER();
            long id = 0;
            if (db.EXPORTER.Any())
                id = db.EXPORTER.Max(i => i.SYSTEMID);
            id += 1;
            exp.SYSTEMID = id;
            exp.LAST_UPDATE_DATE = DateTime.Now;
            exp.STATUS = "Active";
            if (!string.IsNullOrEmpty(BANK_BRANCH_ID))
            {
                long longid = long.Parse(BANK_BRANCH_ID);
                exp.BANK_BRANCH_ID = longid;
            }
            if (!string.IsNullOrEmpty(BIRTH_ESTABLISHMENT_DATE))

                exp.BIRTH_ESTABLISHMENT_DATE = DateTime.Parse(BIRTH_ESTABLISHMENT_DATE);
            exp.EXPORTER_NAME = EXPORTER_NAME;
            if (!string.IsNullOrEmpty(EXPORTER_TYPE_ID))
            {
                long longid = long.Parse(EXPORTER_TYPE_ID);
                exp.EXPORTER_TYPE_ID = longid;
            }
            if (!string.IsNullOrEmpty(ID_TYPE_ID))
            {
                long longid = long.Parse(ID_TYPE_ID);

                exp.ID_TYPE_ID = longid;
            }
            exp.ID_REFERENCE = ID_REFERENCE;
            db.EXPORTER.Add(exp);
            db.SaveChanges();
            return Json(id, JsonRequestBehavior.AllowGet);// pass new id to dropdownlist
        }
        // GET: /Pens/Create
        public ActionResult Create()
        {
            InitPage();

            return View();
        }
        private void InitPage(Batchs batch)
        {
            Session["rejecteds"] = null;
            Session["markets"] = null;
            Session["pensLabels"] = null;
            Session["BatchChemicals"] = null;
            //var list = (db.EXPORTER);//, "SYSTEMID", "EXPORTER_NAME");


            //var selectList = list.Select(x => new SelectListItem() { Value = x.SYSTEMID.ToString(), Text = x.EXPORTER_NAME })
            //    .ToList();
            //ViewBag.ExporterID = selectList;
            ViewBag.ExporterID = new SelectList(db.EXPORTER, "SYSTEMID", "EXPORTER_NAME", batch.ExporterID);

            ViewBag.DestenationID = new SelectList(db.DESTINATION, "SYSTEMID", "DESTINATION_NAME", batch.DestenationID);
            ViewBag.Regions = new SelectList(db.REGION, "SYSTEMID", "REGION_NAME");
            ViewBag.PenId = new SelectList(db.PEN, "SYSTEMID", "Name", batch.PenId);
            ViewBag.ChemicalType = new SelectList(db.CHEMICAL, "SYSTEMID", "CHEMICAL_NAME", batch.ChemicalType);
            ViewBag.marketID = new SelectList(db.MARKET, "SYSTEMID", "MARKET_NAME", batch.marketID);
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME", batch.typeID);
            ViewBag.FeedMethod = new SelectList(CommonUtils.getFeedMethod(), "Value", "Text", batch.FeedMethod);
            ViewBag.Banks = new SelectList(db.BANK, "SYSTEMID", "BANK_NAME");
            ViewBag.BANK_BRANCH_ID = new SelectList(db.BRANCH, "SYSTEMID", "BRANCH_NAME");
            ViewBag.ID_TYPE_ID = new SelectList(db.ID_TYPE, "SYSTEMID", "TYPE_NAME");
            ViewBag.EXPORTER_TYPE_ID = new SelectList(db.EXPORTER_TYPE, "SYSTEMID", "TYP_NAME");
        }

        private void InitPage()
        {
            Session["rejecteds"] = null;
            Session["markets"] = null;
            Session["pensLabels"] = null;
            Session["BatchChemicals"] = null;

            var list = (db.EXPORTER);//, "SYSTEMID", "EXPORTER_NAME");


            var selectList = list.Select(x => new SelectListItem() { Value = x.SYSTEMID.ToString(), Text = x.EXPORTER_NAME })
                .ToList();
            ViewBag.ExporterID = selectList;

            ViewBag.DestenationID = new SelectList(db.DESTINATION, "SYSTEMID", "DESTINATION_NAME");
            ViewBag.Regions = new SelectList(db.REGION, "SYSTEMID", "REGION_NAME");
            ViewBag.PenId = new SelectList(db.PEN, "SYSTEMID", "Name");
            ViewBag.ChemicalType = new SelectList(db.CHEMICAL, "SYSTEMID", "CHEMICAL_NAME");
            ViewBag.marketID = new SelectList(db.MARKET, "SYSTEMID", "MARKET_NAME");
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME");
            ViewBag.FeedMethod = new SelectList(CommonUtils.getFeedMethod(), "Value", "Text");
            ViewBag.Banks = new SelectList(db.BANK, "SYSTEMID", "BANK_NAME");
            ViewBag.BANK_BRANCH_ID = new SelectList(db.BRANCH, "SYSTEMID", "BRANCH_NAME");
            ViewBag.ID_TYPE_ID = new SelectList(db.ID_TYPE, "SYSTEMID", "TYPE_NAME");
            ViewBag.EXPORTER_TYPE_ID = new SelectList(db.EXPORTER_TYPE, "SYSTEMID", "TYP_NAME");
        }

        public JsonResult typeSelected(string typeID)
        {

            try

            {
                long id = long.Parse(typeID);
                ANIMAL_TYPE lms = db.ANIMAL_TYPE.Where(o => o.SYSTEMID == id).SingleOrDefault();

                return Json(lms.DEFAULT_AVERAGE_WEIGHT.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult regionSelected(string regionId)
        {

            try

            {
                Session["markets"] = null;
                long region = long.Parse(regionId);
                var lms = db.MARKET.Where(o => o.REGION_ID == region).ToList();
                MARKET mkt = new MARKET();
                mkt.SYSTEMID = 99;
                mkt.MARKET_NAME = "create New";
                lms.Add(mkt);
                var markets = new SelectList(lms, "SYSTEMID", "MARKET_NAME");

                return Json(markets, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult getpens(string typeID)
        {

            try

            {
                long typeIDl = long.Parse(typeID);
                var ids = db.PensLables.Select(i => i.PenId).ToList();
                var itemList = from b in db.PEN
                               where b.TypeID == typeIDl && !ids.Contains(b.SYSTEMID)
                               select new { desc = b.Name, code = b.SYSTEMID };
                var pens = new SelectList(itemList, "code", "desc");

                return Json(pens, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getMarkets(string Regions)
        {
            string resultsHtml = " ";
            if (!string.IsNullOrEmpty(Regions))
            {
                long no = long.Parse(Regions);
                var reg = db.REGION.Where(i => i.SYSTEMID == no).SingleOrDefault();


                resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/BatchRegisteration/Partial/_MarketsPartial.cshtml", reg);
            }


            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        public JsonResult addChemical(string ChemicalAMT, string ChemicalType)
        {

            var chemicals = new List<BatchChemicals>();
            if (Session["BatchChemicals"] != null)
                chemicals = (List<BatchChemicals>)Session["BatchChemicals"];
            var currObj = new BatchChemicals();
            if (!string.IsNullOrEmpty(ChemicalAMT))
            {
                decimal no = decimal.Parse(ChemicalAMT);

                currObj.Amount = no;
            }
            if (!string.IsNullOrEmpty(ChemicalType))
            {
                long no = long.Parse(ChemicalType);
                currObj.ChemcialTypeId = no;
            }
            chemicals.Add(currObj);
            Session["BatchChemicals"] = chemicals;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/BatchRegisteration/Partial/_ChemicalsPartial.cshtml", chemicals.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AddPens(string color, string AnimalsLabels, string PenId, string typeID, string label, string ToAnimalsLabels, string acceptedAnimals)
        {
            var pensLabels = new List<PensLables>();
            if (Session["pensLabels"] != null)
                pensLabels = (List<PensLables>)Session["pensLabels"];

            var currObj = new PensLables();

            currObj.lableRanges = label + AnimalsLabels + "-" + ToAnimalsLabels;
            if (!string.IsNullOrEmpty(PenId))
            {
                int no = int.Parse(PenId);
                int typeIDid = int.Parse(typeID);
                ANIMAL_TYPE typeObj = db.ANIMAL_TYPE.Where(i => i.SYSTEMID == typeIDid).SingleOrDefault();
                PEN penObj = db.PEN.Where(i => i.SYSTEMID == no).SingleOrDefault();
                currObj.PEN = penObj;
                currObj.PenId = no;
                currObj.color = color;
                int intAnimalsLabels = int.Parse(AnimalsLabels);
                int intToAnimalsLabels = int.Parse(ToAnimalsLabels);
                currObj.Range = intToAnimalsLabels - intAnimalsLabels + 1;
                var json = checkPen(currObj, typeObj, pensLabels, acceptedAnimals);
                if (json.Data.ToString() == "OK")
                    pensLabels.Add(currObj);
                else
                {
                    TempData["AlertMessage"] = "Pen is Full";
                    return Json("Error", JsonRequestBehavior.AllowGet);

                }

            }

            Session["pensLabels"] = pensLabels;

            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/BatchRegisteration/Partial/_PensAnimalsPartial.cshtml", pensLabels.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }
        public JsonResult checknumbersBefore(string AcceptedAnimals, string NoOfPermittedAnimals)
        {
            if (!string.IsNullOrEmpty(AcceptedAnimals)) { 
            List<Rejected_Animals> rejectedslist = new List<Rejected_Animals>();
            if (Session["rejecteds"] != null)

                rejectedslist = (List<Rejected_Animals>)Session["rejecteds"];
            List<BatchChemicals> chemialslist = (List<BatchChemicals>)Session["BatchChemicals"];
            int accpt = int.Parse(AcceptedAnimals);
            int NoOfPermittedAnimal = int.Parse(NoOfPermittedAnimals);

            int sumofrej = 0;
            if (rejectedslist.Count > 0)
                sumofrej = rejectedslist.Sum(i => i.RejectedAnimals);

            int sumall = sumofrej + accpt;
            if (sumall != NoOfPermittedAnimal)
            {
                TempData["AlertMessage"] = "Accepted and rejected animals should be less than number of permitted animals";
                CommonUtils.SetFeedback("Accepted and rejected animals should be less than number of permitted animals", Feedback.Feedback_Error);



                return Json("E", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }
        else
                            return Json("OK", JsonRequestBehavior.AllowGet);

        }

        public JsonResult checkPen(PensLables currObj, ANIMAL_TYPE type, List<PensLables> list, string acceptedAnimals)
        {
            bool result = false;
            int? sumCap = 0;
            if (currObj != null && currObj.PenId > 0)
                sumCap = list.Where(i => i.PenId == currObj.PenId).Sum(i => i.Range);
            int? lbl = currObj.Range;
            int? acceptedAnimalsint = int.Parse(acceptedAnimals);
            if (currObj.PEN.CAPACITY <= sumCap + lbl)
            {
                TempData["AlertMessage"] = "Pen is Full";

                return Json("Full", JsonRequestBehavior.AllowGet);
            }
            if ((sumCap + lbl) > acceptedAnimalsint)
            {
                TempData["AlertMessage"] = "Pens larger than number of accepted animals";

                return Json("larger", JsonRequestBehavior.AllowGet);
            }
            if (db.PensLables.Where(i => i.lableRanges == currObj.lableRanges && i.Batchs.STATUS == CommonUtils.STATUS_ACTIVE).ToList().Count > 0)
            {
                TempData["AlertMessage"] = "Lables are useed and currently Active";

                return Json("Used", JsonRequestBehavior.AllowGet);
            }
            return Json("OK", JsonRequestBehavior.AllowGet);

        }

        public JsonResult Add(string rejectedAnimals, string rejectedReasons, string acceptedAnimals, string permitedAnimals)
        {

            var rejecteds = new List<Rejected_Animals>();
            if (Session["rejecteds"] != null)
                rejecteds = (List<Rejected_Animals>)Session["rejecteds"];
            var currObj = new Rejected_Animals();
            if (rejecteds.Where(i => i.Reasons == rejectedReasons).ToList() != null && rejecteds.Where(i => i.Reasons == rejectedReasons).ToList().Count == 0)
            {
                currObj.Reasons = rejectedReasons;
                if (!string.IsNullOrEmpty(rejectedAnimals))
                {
                    int no = int.Parse(rejectedAnimals);
                    currObj.RejectedAnimals = no;
                }
                if (!string.IsNullOrEmpty(acceptedAnimals) && !string.IsNullOrEmpty(permitedAnimals))
                {
                    int noacceptedAnimals = int.Parse(acceptedAnimals);
                    int nopermitedAnimals = int.Parse(permitedAnimals);
                    int rej = rejecteds.Sum(i => i.RejectedAnimals);
                    rej = rej + currObj.RejectedAnimals;
                    if (noacceptedAnimals + currObj.RejectedAnimals > noacceptedAnimals)
                    {
                        TempData["AlertMessage"] = "Accepted and rejected animals should be less than number of permitted animals";

                    }
                    else
                        rejecteds.Add(currObj);
                }
                else
                    rejecteds.Add(currObj);
                Session["rejecteds"] = rejecteds;
            }
            string resultsHtml = CommonUtils.RenderPartialViewToString(this, "~/Views/BatchRegisteration/Partial/_RejectedAnimalsPartial.cshtml", rejecteds.ToList());
            return Json(resultsHtml, JsonRequestBehavior.AllowGet);
        }

        // POST: /Pens/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(Batchs batch, HttpPostedFileBase file)
        {
            try
            {
                long id = 0;
                if (db.Batchs.Any())
                    id = db.Batchs.Max(i => i.SYSTEMID);
                id += 1;
                string FileName = file.FileName;
                var pdf = new byte[file.ContentLength];
                file.InputStream.Read(pdf, 0, file.ContentLength);
                byte[] uploadedFile = new byte[file.InputStream.Length];
                batch.STATUS = CommonUtils.STATUS_ENTERED;
                batch.ScanOfPermit = uploadedFile;
                int sumofrej = 0;
                int sumofAccpt = 0;

                if (Session["rejecteds"] != null)
                {
                    batch.STATUS = CommonUtils.STATUS_ACTIVE;
                    List<Rejected_Animals> rejectedslist = (List<Rejected_Animals>)Session["rejecteds"];
                    long idRejected = 0;
                    if (db.Rejected_Animals.Any())
                        idRejected = db.Rejected_Animals.Max(i => i.SYSTEMID);
                    sumofrej = rejectedslist.Sum(i => i.RejectedAnimals);
                    foreach (var item in rejectedslist)
                    {
                        var curObj = new Rejected_Animals();
                        idRejected += 1;
                        item.SYSTEMID = idRejected;
                        item.BatchId = id;
                        item.STATUS = CommonUtils.STATUS_ACTIVE;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        item.CREATION_DATE = DateTime.Now;
                        batch.Rejected_Animals.Add(item);
                    }
                }
                if (Session["BatchChemicals"] != null)
                {
                    List<BatchChemicals> chemialslist = (List<BatchChemicals>)Session["BatchChemicals"];
                    long idchemical = 0;
                    if (db.BatchChemicals.Any())
                        idchemical = db.BatchChemicals.Max(i => i.SYSTEMID);
                    batch.STATUS = CommonUtils.STATUS_ACTIVE;
                    foreach (var item in chemialslist)
                    {
                        var curObj = new BatchChemicals();
                        idchemical += 1;
                        item.SYSTEMID = idchemical;
                        item.batchId = id;
                        item.STATUS = CommonUtils.STATUS_ACTIVE;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        item.CREATION_DATE = DateTime.Now;
                        batch.BatchChemicals.Add(item);
                    }
                }
                if (Session["pensLabels"] != null)
                {
                    List<PensLables> pensLables = (List<PensLables>)Session["pensLabels"];
                    long idpen = 0;
                    if (db.PensLables.Any())
                        idpen = db.PensLables.Max(i => i.SYSTEMID);
                    batch.STATUS = CommonUtils.STATUS_ACTIVE;
                    foreach (var item in pensLables)
                    {
                        var curObj = new PensLables();
                        idpen += 1;
                        item.SYSTEMID = idpen;
                        item.batchId = id;
                        item.PEN = null;
                        item.color = item.color;
                        item.lableRanges = item.lableRanges;
                        item.PenId = item.PenId;
                        item.STATUS = CommonUtils.STATUS_ACTIVE;
                        item.LAST_UPDATE_DATE = DateTime.Now;
                        item.CREATED_BY = CurrentUserName;
                        batch.PensLables.Add(item);
                    }
                }
                else
                    batch.PenId = null;
                if (batch.AcceptedAnimals > 0)
                {
                    batch.STATUS = CommonUtils.STATUS_ACTIVE;
                    //int sumall = sumofrej +int.Parse(batch.AcceptedAnimals.ToString());
                    //if (sumall != batch.NoOfPermittedAnimals )
                    //{
                    //    TempData["AlertMessage"] = "Accepted and rejected animals should be less than number of permitted animals";
                    //    CommonUtils.SetFeedback("Accepted and rejected animals should be less than number of permitted animals", Feedback.Feedback_Error);
                    //    InitPage(batch);


                    //    return View(batch);
                    //}
                }

                batch.EXPORTER = null;
                batch.SYSTEMID = id;
                batch.LAST_UPDATE_DATE = DateTime.Now;
                batch.CREATED_BY = CurrentUserName;
                batch.CREATION_DATE = DateTime.Now;
                db.Batchs.Add(batch);
                ////check animals recieved
                //if (batch.AcceptedAnimals > 0)
                //{
                //    //Save to batch labels
                //    //1- read label max from cp
                //    CONTROLRECORD cntrol = db.CONTROLRECORD.SingleOrDefault();
                //    decimal labelMax = decimal.Parse(cntrol.ANIMAL_TAG_MAX.ToString());
                //    decimal noOf = decimal.Parse(batch.AcceptedAnimals.ToString()) / labelMax;
                //    Char vlabel = 'Z';
                //    long idlbl = 0;
                //    if (db.BatchLabel.Any())
                //        idlbl = db.BatchLabel.Max(i => i.SYSTEMID);
                //    for (int i = 0; i < noOf; i++)
                //    {
                //        if (vlabel == 'Z')
                //            vlabel = 'A';
                //        else if (vlabel == 'z')
                //            vlabel = 'a';
                //        else
                //            vlabel = (char)(((int)vlabel) + 1);
                //        BatchLabel blbl = new BatchLabel();
                //        idlbl += 1;
                //        blbl.SYSTEMID = idlbl;
                //        blbl.STATUS = CommonUtils.STATUS_ACTIVE;
                //        blbl.LAST_UPDATE_DATE = DateTime.Now;
                //        blbl.CREATED_BY = CurrentUserName;
                //        blbl.CREATION_DATE = DateTime.Now;
                //        blbl.BatchId = id;
                //        blbl.batchLabel1 = vlabel + "1 - " + vlabel + labelMax.ToString();
                //        db.BatchLabel.Add(blbl);

                //}
                //}
                db.SaveChanges();
                Session["BatchChemicals"] = null;
                Session["rejecteds"] = null;
                Session["pensLabels"] = null;
                CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                bno = id;
                return RedirectToAction("Done");

            }
            catch (Exception ex)
            {
                InitPage(batch);

                return View(batch);
            }


            return View(batch);
        }



        // GET: /pen/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Batchs batch = db.Batchs.Find(id);
            if (batch == null || batch.VerfiyStatus == "No")
            {
                return HttpNotFound();
            }
            Session["rejecteds"] = null;
            InitPage(batch);

            ViewBag.ExporterID = new SelectList(db.EXPORTER, "SYSTEMID", "EXPORTER_NAME", batch.ExporterID);
            ViewBag.DestenationID = new SelectList(db.DESTINATION, "SYSTEMID", "DESTINATION_NAME", batch.DestenationID);
            MARKET market = db.MARKET.Where(i => i.SYSTEMID == batch.marketID).SingleOrDefault();
            ViewBag.Regions = new SelectList(db.REGION, "SYSTEMID", "REGION_NAME", market.REGION_ID);
            ViewBag.REGION_ID = market.REGION_ID;
            ViewBag.PenId = new SelectList(db.PEN, "SYSTEMID", "Name", batch.PenId);
            ViewBag.ChemicalType = new SelectList(db.CHEMICAL, "SYSTEMID", "CHEMICAL_NAME", batch.ChemicalType);
            ViewBag.marketID = new SelectList(db.MARKET, "SYSTEMID", "MARKET_NAME", batch.marketID);
            ViewBag.typeID = new SelectList(db.ANIMAL_TYPE, "SYSTEMID", "TYPE_NAME", batch.typeID);
            if(batch.FeedMethod!=null)
            ViewBag.FeedMethod = new SelectList(CommonUtils.getFeedMethod(), "Value", "Text", batch.FeedMethod.Trim());
            else
                ViewBag.FeedMethod = new SelectList(CommonUtils.getFeedMethod(), "Value", "Text", batch.FeedMethod);


            return View(batch);
        }

        // POST: /pen/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Batchs batch, string ExporterIDss, string DestenationIDss, string sstypeID, string marketID, string FeedMethodss)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    batch.ExporterID = int.Parse(ExporterIDss);
                    batch.DestenationID = int.Parse(DestenationIDss);
                    batch.typeID = int.Parse(sstypeID);
                    batch.marketID = int.Parse(marketID);
                    batch.FeedMethod = FeedMethodss;
                    batch.STATUS = CommonUtils.STATUS_ACTIVE;
                    if (Session["rejecteds"] != null)
                    {
                        
                        List<Rejected_Animals> rejectedslist = (List<Rejected_Animals>)Session["rejecteds"];
                        long idRejected = 0;
                        if (db.Rejected_Animals.Any())
                            idRejected = db.Rejected_Animals.Max(i => i.SYSTEMID);
                        foreach (var item in rejectedslist)
                        {
                            var curObj = new Rejected_Animals();
                            idRejected += 1;
                            item.SYSTEMID = idRejected;
                            item.BatchId = batch.SYSTEMID;
                            item.STATUS = CommonUtils.STATUS_ACTIVE;
                            item.LAST_UPDATE_DATE = DateTime.Now;
                            item.CREATED_BY = CurrentUserName;
                            item.CREATION_DATE = DateTime.Now;
                            db.Rejected_Animals.Add(item);
                            batch.Rejected_Animals.Add(item);
                        }
                    }
                    if (Session["BatchChemicals"] != null)
                    {
                        List<BatchChemicals> chemialslist = (List<BatchChemicals>)Session["BatchChemicals"];
                        long idchemical = 0;
                        if (db.BatchChemicals.Any())
                            idchemical = db.BatchChemicals.Max(i => i.SYSTEMID);
                        batch.STATUS = CommonUtils.STATUS_ACTIVE;
                        foreach (var item in chemialslist)
                        {
                            var curObj = new BatchChemicals();
                            idchemical += 1;
                            item.SYSTEMID = idchemical;
                            item.batchId = batch.SYSTEMID;
                            item.STATUS = CommonUtils.STATUS_ACTIVE;
                            item.LAST_UPDATE_DATE = DateTime.Now;
                            item.CREATED_BY = CurrentUserName;
                            item.CREATION_DATE = DateTime.Now;
                            db.BatchChemicals.Add(item);
                            batch.BatchChemicals.Add(item);
                        }
                    }
                    if (Session["pensLabels"] != null)
                    {
                        List<PensLables> pensLables = (List<PensLables>)Session["pensLabels"];
                        long idpen = 0;
                        if (db.PensLables.Any())
                            idpen = db.PensLables.Max(i => i.SYSTEMID);
                        batch.STATUS = CommonUtils.STATUS_ACTIVE;
                        foreach (var item in pensLables)
                        {
                            var curObj = new PensLables();
                            idpen += 1;
                            item.SYSTEMID = idpen;
                            item.batchId = batch.SYSTEMID;
                            item.PEN = null;
                            item.color = item.color;
                            item.lableRanges = item.lableRanges;
                            item.PenId = item.PenId;
                            item.STATUS = CommonUtils.STATUS_ACTIVE;
                            item.LAST_UPDATE_DATE = DateTime.Now;
                            item.CREATED_BY = CurrentUserName;
                            db.PensLables.Add(item);
                            batch.PensLables.Add(item);
                        }
                    }
                    if (batch.AcceptedAnimals > 0)
                    {
                        batch.STATUS = CommonUtils.STATUS_ACTIVE;
                        // batch.ScanOfPermit = db.Batchs.Where(i => i.SYSTEMID == batch.SYSTEMID).SingleOrDefault().ScanOfPermit;
                        //int sumall = sumofrej +int.Parse(batch.AcceptedAnimals.ToString());
                        //if (sumall != batch.NoOfPermittedAnimals )
                        //{
                        //    TempData["AlertMessage"] = "Accepted and rejected animals should be less than number of permitted animals";
                        //    CommonUtils.SetFeedback("Accepted and rejected animals should be less than number of permitted animals", Feedback.Feedback_Error);
                        //    InitPage(batch);


                        //    return View(batch);
                        //}
                    }

                    batch.EXPORTER = null;
                    batch.LAST_UPDATE_DATE = DateTime.Now;
                    batch.CREATED_BY = CurrentUserName;
                    batch.CREATION_DATE = DateTime.Now;
                    db.Entry(batch).State = EntityState.Modified;
                    ////check animals recieved
                    //if (batch.AcceptedAnimals > 0)
                    //{
                    //    //Save to batch labels
                    //    //1- read label max from cp
                    //    CONTROLRECORD cntrol = db.CONTROLRECORD.SingleOrDefault();
                    //    decimal labelMax = decimal.Parse(cntrol.ANIMAL_TAG_MAX.ToString());
                    //    decimal noOf = decimal.Parse(batch.AcceptedAnimals.ToString()) / labelMax;
                    //    Char vlabel = 'Z';
                    //    long idlbl = 0;
                    //    if (db.BatchLabel.Any())
                    //        idlbl = db.BatchLabel.Max(i => i.SYSTEMID);
                    //    for (int i = 0; i < noOf; i++)
                    //    {
                    //        if (vlabel == 'Z')
                    //            vlabel = 'A';
                    //        else if (vlabel == 'z')
                    //            vlabel = 'a';
                    //        else
                    //            vlabel = (char)(((int)vlabel) + 1);
                    //        BatchLabel blbl = new BatchLabel();
                    //        idlbl += 1;
                    //        blbl.SYSTEMID = idlbl;
                    //        blbl.STATUS = CommonUtils.STATUS_ACTIVE;
                    //        blbl.LAST_UPDATE_DATE = DateTime.Now;
                    //        blbl.CREATED_BY = CurrentUserName;
                    //        blbl.CREATION_DATE = DateTime.Now;
                    //        blbl.BatchId = id;
                    //        blbl.batchLabel1 = vlabel + "1 - " + vlabel + labelMax.ToString();
                    //        db.BatchLabel.Add(blbl);

                    //}
                    //}
                    db.SaveChanges();
                    Session["BatchChemicals"] = null;
                    Session["rejecteds"] = null;
                    Session["pensLabels"] = null;
                    CommonUtils.SetFeedback(Feedback.SavedSuccessfully, Feedback.Feedback_Success);
                    bno = batch.SYSTEMID;
                    return RedirectToAction("Done");

                }
                return View(batch);
            }
            catch (DbEntityValidationException ex)
            {
                CommonUtils.SetFeedback(Feedback.NotSavedSuccessfully, Feedback.Feedback_Error);
                return View(batch);
            }
        }

        // GET: /pen/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PEN pen = db.PEN.Find(id);
            if (pen == null)
            {
                return HttpNotFound();
            }
            return View(pen);
        }

        // POST: /pen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PEN pen = db.PEN.Find(id);
            pen.AUTHORIZATION_DATE = DateTime.Now;
            pen.STATUS = CommonUtils.STATUS_DELETED;
            db.Entry(pen).State = EntityState.Modified;
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
