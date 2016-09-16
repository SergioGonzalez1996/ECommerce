using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ECommerce.Models;
using ECommerce.Classes;
using PagedList;
using System;

namespace ECommerce.Controllers
{
    [Authorize(Roles = "User")]

    public class SalesController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        [HttpPost]
        public ActionResult FromOrder(AddFromOrder view)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                var response = MovementsHelper.SaleFromOrder(view.OrderId, User.Identity.Name);
                if (response.Succeeded)
                {
                    var newView = new NewSaleView
                    {
                        OrderId = view.OrderId,
                        CustomerId = response.CustomerId,
                        Date = response.Date,
                        Remarks = response.Remarks,
                        Details = db.SaleDetailTmps.Where(sdt => sdt.UserName == User.Identity.Name).ToList(),
                    };
                    //return PartialView(newView);
                    TempData["model"] = newView;
                    return RedirectToAction("Create");
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            ViewBag.OrderId = new SelectList(CombosHelper.GetOrders(user.CompanyId), "OrderId", "OrderId");
            return PartialView();
        }


        public ActionResult FromOrder()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.OrderId = new SelectList(CombosHelper.GetOrders(user.CompanyId), "OrderId", "OrderId");
            return PartialView();
        }


        public ActionResult DeleteProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var SaleDetailTmp = db.SaleDetailTmps.Where(pdt => pdt.UserName == User.Identity.Name && pdt.ProductId == id).FirstOrDefault();
            if (SaleDetailTmp == null)
            {
                return HttpNotFound(); 
            }

            db.SaleDetailTmps.Remove(SaleDetailTmp);
            var response = DBHelper.SaveChanges(db);
            if (!response.Succeeded)
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }
            return RedirectToAction("Create");
        }


        [HttpPost]
        public ActionResult AddProduct(AddProductView view)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {
                var SaleDetailTmp = db.SaleDetailTmps.Where(sdt => sdt.UserName == User.Identity.Name && sdt.ProductId == view.ProductId).FirstOrDefault();
                if (SaleDetailTmp == null)
                {
                    var product = db.Products.Find(view.ProductId);
                    SaleDetailTmp = new SaleDetailTmp
                    {
                        Description = product.Description,
                        Price = product.Price,
                        ProductId = product.ProductId,
                        Quantity = view.Quantity,
                        TaxRate = product.Tax.Rate,
                        UserName = User.Identity.Name,
                    };
                    db.SaleDetailTmps.Add(SaleDetailTmp);
                }
                else
                {
                    SaleDetailTmp.Quantity += view.Quantity;
                    db.Entry(SaleDetailTmp).State = EntityState.Modified;
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
            ViewBag.ProductId = new SelectList(CombosHelper.GetProducts(user.CompanyId), "ProductId", "Description");
            return PartialView();
        }

        public ActionResult AddProduct()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.ProductId = new SelectList(CombosHelper.GetProducts(user.CompanyId, true), "ProductId", "Description");
            return PartialView();
        }

        // GET: Sales
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var sales = db.Sales.Where(s => s.CompanyId == user.CompanyId).Include(s => s.Customer).Include(s => s.State);
            return View(sales.OrderBy(s => s.Customer.FirstName).ThenBy(s => s.Customer.LastName).ToPagedList((int)page, 10));
        }

        // GET: Sales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            NewSaleView view = (NewSaleView)TempData["model"];
            if (view == null)
            {
                view = new NewSaleView
                {
                    Date = DateTime.Now,
                    Details = db.SaleDetailTmps.Where(sdt => sdt.UserName == User.Identity.Name).ToList(),
                };
            }
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName", view.CustomerId);
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name", view.WarehouseId);
            return View(view);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewSaleView view)
        {
            if (ModelState.IsValid)
            {
                var response = MovementsHelper.NewSale(view, User.Identity.Name);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }

            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName");
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name");
            view.Details = db.SaleDetailTmps.Where(sdt => sdt.UserName == User.Identity.Name).ToList();
            return View(view);
        }

        // GET: Sales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName", sale.CustomerId);
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name", sale.WarehouseId);
            return View(sale);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Sale sale)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sale).State = EntityState.Modified;
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName", sale.CustomerId);
            ViewBag.WarehouseId = new SelectList(CombosHelper.GetWarehouses(user.CompanyId), "WarehouseId", "Name", sale.WarehouseId);
            return View(sale);
        }

        // GET: Sales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sale = db.Sales.Find(id);
            db.Sales.Remove(sale);
            var response = DBHelper.SaveChanges(db);
            if (response.Succeeded)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, response.Message);
            return View(sale);
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
