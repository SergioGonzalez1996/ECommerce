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

    public class ProductsController : Controller
    {
        private ECommerceContext db = new ECommerceContext();

        // GET: Products
        public ActionResult Index(int? page = null)
        {
            page = (page ?? 1);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var products = db.Products.Where(p => p.CompanyId == user.CompanyId).Include(p => p.Category).Include(p => p.Tax);
            return View(products.OrderBy(p => p.Description).ToPagedList((int)page, 10));
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.CategoryId = new SelectList(CombosHelper.GetCategories(user.CompanyId), "CategoryId", "Description");
            ViewBag.TaxId = new SelectList(CombosHelper.GetTaxes(user.CompanyId), "TaxId", "Description");
            var product = new Product { CompanyId = user.CompanyId };
            return View(product);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                var response = DBHelper.SaveChanges(db);
                if (response.Succeeded)
                {
                    if (product.ImageFile != null)
                    {
                        var folder = "~/Content/Products";
                        var file = string.Format("{0}.jpg", product.ProductId);
                        var response2 = FilesHelper.UploadPhoto(product.ImageFile, folder, file);
                        if (response2)
                        {
                            product.Image = string.Format("{0}/{1}", folder, file);
                            db.Entry(product).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                }
            }

            ViewBag.CategoryId = new SelectList(CombosHelper.GetCategories(user.CompanyId), "CategoryId", "Description", product.CategoryId);
            ViewBag.TaxId = new SelectList(CombosHelper.GetTaxes(user.CompanyId), "TaxId", "Description", product.TaxId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(CombosHelper.GetCategories(user.CompanyId), "CategoryId", "Description", product.CategoryId);
            ViewBag.TaxId = new SelectList(CombosHelper.GetTaxes(user.CompanyId), "TaxId", "Description", product.TaxId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    var folder = "~/Content/Products";
                    var file = string.Format("{0}.jpg", product.ProductId);
                    var response2 = FilesHelper.UploadPhoto(product.ImageFile, folder, file);
                    if (response2)
                    {
                        product.Image = string.Format("{0}/{1}", folder, file); ;
                    }
                }

                db.Entry(product).State = EntityState.Modified;
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
            ViewBag.CategoryId = new SelectList(CombosHelper.GetCategories(user.CompanyId), "CategoryId", "Description", product.CategoryId);
            ViewBag.TaxId = new SelectList(CombosHelper.GetTaxes(user.CompanyId), "TaxId", "Description", product.TaxId);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            var response = DBHelper.SaveChanges(db);
            if (response.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, response.Message);
            }
            return View(product);
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
