using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ECommerce.Models;
using PagedList;
using ECommerce.Classes;

namespace ECommerce.Controllers
{
    [Authorize(Roles = "User")]

    public class SuppliersController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        // GET: Suppliers
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);

            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var qry = (from su in db.Suppliers
                       join cs in db.CompanySuppliers on su.SupplierId equals cs.SupplierId
                       join co in db.Companies on cs.CompanyId equals co.CompanyId
                       where co.CompanyId == user.CompanyId
                       select new { su }).ToList();

            var suppliers = new List<Supplier>();
            foreach (var item in qry)
            {
                suppliers.Add(item.su);
            }
            return View(suppliers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToPagedList((int)page, 10));
        }

        // GET: Suppliers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(0), "CityId", "Name");
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name");
            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Suppliers.Add(supplier);
                        var response = DBHelper.SaveChanges(db);
                        if (!response.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, response.Message);
                            transaction.Rollback();
                            ViewBag.CityId = new SelectList(CombosHelper.GetCities(supplier.DepartmentId), "CityId", "Name", supplier.CityId);
                            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", supplier.DepartmentId);
                            return View(supplier);
                        }
                        if (supplier.PhotoFile != null)
                        {
                            var folder = "~/Content/Suppliers";
                            var file = string.Format("{0}.jpg", supplier.SupplierId);
                            var response2 = FilesHelper.UploadPhoto(supplier.PhotoFile, folder, file);
                            if (response2)
                            {
                                supplier.Photo = string.Format("{0}/{1}", folder, file);
                                db.Entry(supplier).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        UsersHelper.CreateUserASP(supplier.UserName, "Supplier");
                        var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                        var CompanySupplier = new CompanySupplier
                        {
                            CompanyId = user.CompanyId,
                            SupplierId = supplier.SupplierId,
                        };
                        db.CompanySuppliers.Add(CompanySupplier);
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
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(supplier.DepartmentId), "CityId", "Name", supplier.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", supplier.DepartmentId);
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(supplier.DepartmentId), "CityId", "Name", supplier.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", supplier.DepartmentId);
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                var db2 = new ECommerceContext();
                var currentUser = db2.Users.Find(supplier.UserName); // TODO: Check this !
                if (currentUser.UserName != supplier.UserName)
                {
                    UsersHelper.UpdateUserName(currentUser.UserName, supplier.UserName);
                }
                db2.Dispose();

                db.Entry(supplier).State = EntityState.Modified;
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    if (supplier.PhotoFile != null)
                    {
                        var folder = "~/Content/Suppliers";
                        var file = string.Format("{0}.jpg", supplier.SupplierId);
                        var response2 = FilesHelper.UploadPhoto(supplier.PhotoFile, folder, file);
                        if (response2)
                        {
                            supplier.Photo = string.Format("{0}/{1}", folder, file);
                            db.Entry(supplier).State = EntityState.Modified;
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
            ViewBag.CityId = new SelectList(CombosHelper.GetCities(supplier.DepartmentId), "CityId", "Name", supplier.CityId);
            ViewBag.DepartmentId = new SelectList(CombosHelper.GetDepartments(), "DepartmentId", "Name", supplier.DepartmentId);
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var supplier = db.Suppliers.Find(id);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var companySupplier = db.CompanySuppliers.Where(cs => cs.CompanyId == user.CompanyId && cs.SupplierId == supplier.SupplierId).FirstOrDefault();
            using (var transaccion = db.Database.BeginTransaction())
            {
                db.CompanySuppliers.Remove(companySupplier);
                db.Suppliers.Remove(supplier);
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    transaccion.Commit();
                    return RedirectToAction("Index");
                }
                transaccion.Rollback();
                ModelState.AddModelError(string.Empty, response.Message);
                return View(supplier);
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
