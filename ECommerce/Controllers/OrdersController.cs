using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ECommerce.Models;
using ECommerce.Classes;
using PagedList;

namespace ECommerce.Controllers
{
    [Authorize(Roles = "User")]

    public class OrdersController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        public ActionResult DeleteProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var OrderDetailTmp = db.OrderDetailTmps.Where(odt => odt.UserName == User.Identity.Name && odt.ProductId == id).FirstOrDefault();
            if (OrderDetailTmp == null)
            {
                return HttpNotFound();
            }

            db.OrderDetailTmps.Remove(OrderDetailTmp);
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
        public ActionResult AddProduct(AddProductView view)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                var OrderDetailTmp = db.OrderDetailTmps.Where(odt => odt.UserName == User.Identity.Name && odt.ProductId == view.ProductId).FirstOrDefault();
                if (OrderDetailTmp == null)
                {
                    var product = db.Products.Find(view.ProductId);
                    OrderDetailTmp = new OrderDetailTmp
                    {
                        Description = product.Description,
                        Price = product.Price,
                        ProductId = product.ProductId,
                        Quantity = view.Quantity,
                        TaxRate = product.Tax.Rate,
                        UserName = User.Identity.Name,
                    };
                    db.OrderDetailTmps.Add(OrderDetailTmp);
                }
                else
                {
                    OrderDetailTmp.Quantity += view.Quantity;
                    db.Entry(OrderDetailTmp).State = EntityState.Modified;
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
            ViewBag.ProductId = new SelectList(CombosHelper.GetProducts(user.CompanyId,true), "ProductId", "Description");
            return PartialView();

        }


        // GET: Orders
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var orders = db.Orders.Where(o => o.CompanyId == user.CompanyId).Include(o => o.Customer).Include(o => o.State);
            return View(orders.OrderBy(o => o.Customer.FirstName).ThenBy(o => o.Customer.LastName).ToPagedList((int)page, 10));
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName");
            var view = new NewOrderView {
                Date = DateTime.Now,
                Details = db.OrderDetailTmps.Where(odt => odt.UserName == User.Identity.Name).ToList(),
            };
            return View(view);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewOrderView view)
        {
            if (ModelState.IsValid)
            { 
                var response = MovementsHelper.NewOrder(view, User.Identity.Name);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);  
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName");
            view.Details = db.OrderDetailTmps.Where(odt => odt.UserName == User.Identity.Name).ToList();
            return View(view);
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "FullName", order.CustomerId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                }
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CustomerId = new SelectList(CombosHelper.GetCustomers(user.CompanyId), "CustomerId", "UserName", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var order = db.Orders.Find(id);
            db.Orders.Remove(order);
            var response = DBHelper.SaveChanges(db);
            if (response.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }
            return View(order);
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
