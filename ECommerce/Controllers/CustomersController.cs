using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ECommerce.Models;
using ECommerce.Classes;
using System.Collections.Generic;
using System;
using PagedList;

namespace ECommerce.Controllers
{
    [Authorize(Roles = "User")]

    public class CustomersController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        // GET: Customers
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var qry = (from cu in db.Customers
                       join cc in db.CompanyCustomers on cu.CustomerId equals cc.CustomerId
                       join co in db.Companies on cc.CompanyId equals co.CompanyId
                       where co.CompanyId == user.CompanyId
                       select new { cu }).ToList();

            var customers = new List<Customer>();
            foreach (var item in qry)
            {
                customers.Add(item.cu);
            }
            return View(customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToPagedList((int)page, 10));
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(0), "CityId", "Name");
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name");
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Customers.Add(customer);
                        var response = DBHelper.SaveChanges(db);
                        if (!response.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, response.Message);
                            transaction.Rollback();
                            ViewBag.CityId = new SelectList(CombosHelper.GetCities(customer.DepartmentId), "CityId", "Name", customer.CityId);
                            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", customer.DepartmentId);
                            return View(customer);
                        }
                        if (customer.PhotoFile != null)
                        {
                            var folder = "~/Content/Customers";
                            var file = string.Format("{0}.jpg", customer.CustomerId);
                            var response2 = FilesHelper.UploadPhoto(customer.PhotoFile, folder, file);
                            if (response2)
                            {
                                customer.Photo = string.Format("{0}/{1}", folder, file);
                                db.Entry(customer).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        UsersHelper.CreateUserASP(customer.UserName, "Customer");
                        var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                        var CompanyCustomer = new CompanyCustomer
                        {
                            CompanyId = user.CompanyId,
                            CustomerId = customer.CustomerId,
                        };
                        db.CompanyCustomers.Add(CompanyCustomer);
                        db.SaveChanges();
                        transaction.Commit();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(customer.DepartmentId), "CityId", "Name", customer.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", customer.DepartmentId);
            return View(customer);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(customer.DepartmentId), "CityId", "Name", customer.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", customer.DepartmentId);
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var db2 = new ECommerceContext();
                var currentUser = db2.Users.Find(customer.UserName); // TODO: Check this !
                if (currentUser.UserName != customer.UserName)
                {
                    UsersHelper.UpdateUserName(currentUser.UserName, customer.UserName);
                }
                db2.Dispose();

                db.Entry(customer).State = EntityState.Modified;
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    if (customer.PhotoFile != null)
                    {
                        var folder = "~/Content/Customers";
                        var file = string.Format("{0}.jpg", customer.CustomerId);
                        var response2 = FilesHelper.UploadPhoto(customer.PhotoFile, folder, file);
                        if (response2)
                        {
                            customer.Photo = string.Format("{0}/{1}", folder, file);
                            db.Entry(customer).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, response.Message);
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(customer.DepartmentId), "CityId", "Name", customer.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", customer.DepartmentId);
            return View(customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var customer = db.Customers.Find(id);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var companyCustomer = db.CompanyCustomers.Where(cc => cc.CompanyId == user.CompanyId && cc.CustomerId == customer.CustomerId).FirstOrDefault();

            using (var transaccion = db.Database.BeginTransaction())
            {
                db.CompanyCustomers.Remove(companyCustomer);
                db.Customers.Remove(customer);
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    transaccion.Commit();
                    return RedirectToAction("Index");
                }
                transaccion.Rollback();
                ModelState.AddModelError(string.Empty, response.Message);
                return View(customer); 
            }
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
