using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ECommerce.Models;
using PagedList;
using ECommerce.Classes;
using System;

namespace ECommerce.Controllers
{
    [Authorize(Roles = "User")]

    public class PurchasesController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        public ActionResult DeleteProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var PurchaseDetailTmp = db.PurchaseDetailTmps.Where(pdt => pdt.UserName == User.Identity.Name && pdt.ProductId == id).FirstOrDefault();
            if (PurchaseDetailTmp == null)
            {
                return HttpNotFound();
            }

            db.PurchaseDetailTmps.Remove(PurchaseDetailTmp);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return RedirectToAction("Create");
        }

        [HttpPost]
        public ActionResult AddProduct(AddProductViewForPurchase view)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                var PurchaseDetailTmp = db.PurchaseDetailTmps.Where(pdt => pdt.UserName == User.Identity.Name && pdt.ProductId == view.ProductId).FirstOrDefault();
                if (PurchaseDetailTmp == null)
                {
                    var product = db.Products.Find(view.ProductId);
                    PurchaseDetailTmp = new PurchaseDetailTmp
                    {
                        Description = product.Description,
                        Price = product.Price,
                        ProductId = product.ProductId,
                        Quantity = view.Quantity,
                        TaxRate = product.Tax.Rate,
                        UserName = User.Identity.Name,
                    };
                    db.PurchaseDetailTmps.Add(PurchaseDetailTmp);
                }
                else
                {
                    PurchaseDetailTmp.Quantity += view.Quantity;
                    db.Entry(PurchaseDetailTmp).State = EntityState.Modified;
                }
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Create");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.ProductId = new SelectList(CombosHelper.GetProducts(user.CompanyId, true), "ProductId", "Description");
            return PartialView();
        }

        public ActionResult AddProduct()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.ProductId = new SelectList(CombosHelper.GetProducts(user.CompanyId, true), "ProductId", "Description");
            return PartialView();

        }

        // GET: Purchases
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var purchases = db.Purchases.Where(p => p.CompanyId == user.CompanyId).Include(p => p.Supplier).Include(p => p.State);
            return View(purchases.OrderBy(p => p.Supplier.FirstName).ThenBy(p => p.Supplier.LastName).ToPagedList((int)page, 10));
            //var purchases = db.Purchases.Include(p => p.Company).Include(p => p.State).Include(p => p.Supplier).Include(p => p.Warehouse);
            //return View(purchases.ToList());
        }

        // GET: Purchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // GET: Purchases/Create
        public ActionResult Create()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.SupplierId = new SelectList(CombosHelper.GetSuppliers(user.CompanyId), "SupplierId", "FullName");
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name");
            var view = new NewPurchaseView  {
                Date = DateTime.Now,
                Details = db.PurchaseDetailTmps.Where(pdt => pdt.UserName == User.Identity.Name).ToList(),
            };
            return View(view);
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewPurchaseView view)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                var response = MovementsHelper.NewPurchase(view, User.Identity.Name);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            ViewBag.SupplierId = new SelectList(CombosHelper.GetSuppliers(user.CompanyId), "SupplierId", "UserName");
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name");
            view.Details = db.PurchaseDetailTmps.Where(pdt => pdt.UserName == User.Identity.Name).ToList();
            return View(view);
        }

        // GET: Purchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.SupplierId = new SelectList(CombosHelper.GetSuppliers(user.CompanyId), "SupplierId", "UserName", purchase.SupplierId);
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name", purchase.WarehouseId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Purchase purchase)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                db.Entry(purchase).State = EntityState.Modified;
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);

            }
            ViewBag.SupplierId = new SelectList(CombosHelper.GetSuppliers(user.CompanyId), "SupplierId", "UserName", purchase.SupplierId);
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name", purchase.WarehouseId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var purchase = db.Purchases.Find(id);
            db.Purchases.Remove(purchase);
            var response = DBHelper.SaveChanges(db);
            if (response.Succeeded)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, response.Message);
            return View(purchase);
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
