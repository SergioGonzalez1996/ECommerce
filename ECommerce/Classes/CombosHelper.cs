using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace ECommerce.Classes
{
    public class CombosHelper : IDisposable 
    {
        private static ECommerceContext db = new ECommerceContext();

        public static List<Department> GetDepartments()
        {
            var departments = db.Departments.ToList();
            departments.Add(new Department { DepartmentId = 0, Name = "[Select a department...]" });
            return departments = departments.OrderBy(d => d.Name).ToList();
        }

        public static List<City> GetCities(int departmentId)
        {
            var cities = db.Cities.Where(c => c.DepartmentId == departmentId).ToList();
            cities.Add(new City
            {
                CityId = 0,
                Name = "[Select a city...]",
            });

            return cities.OrderBy(d => d.Name).ToList();
        }


        public static List<Company> GetCompanies()
        {
            var companies = db.Companies.ToList();
            companies.Add(new Company { CompanyId = 0, Name = "[Select a company...]" });
            return companies = companies.OrderBy(d => d.Name).ToList();
        }

        public static List<Category> GetCategories(int companyId)
        {
            var categories = db.Categories.Where(c => c.CompanyId == companyId).ToList();
            categories.Add(new Category { CategoryId = 0, Description = "[Select a category...]" });
            return categories = categories.OrderBy(d => d.Description).ToList();
        }

        public static List<Tax> GetTaxes(int companyId)
        {
            var taxes = db.Taxes.Where(c => c.CompanyId == companyId).ToList();
            taxes.Add(new Tax { TaxId = 0, Description = "[Select a tax...]" });
            return taxes.OrderBy(d => d.Description).ToList();
        }

        public static List<Warehouse> GetWarehouses(int companyId)
        {
            var warehouse = db.Warehouses.Where(w => w.CompanyId == companyId).ToList();
            warehouse.Add(new Warehouse { WarehouseId = 0, Name = "[Select a warehouse...]" });
            return warehouse.OrderBy(w => w.Name).ToList();
        }

        public static List<Customer> GetCustomers(int companyId)
        {
            var qry = (from cu in db.Customers
                       join cc in db.CompanyCustomers on cu.CustomerId equals cc.CustomerId
                       join co in db.Companies on cc.CompanyId equals co.CompanyId
                       where co.CompanyId == companyId
                       select new { cu }).ToList();

            var customers = new List<Customer>();
            foreach (var item in qry)
            {
                customers.Add(item.cu);
            }
            customers.Add(new Customer { CustomerId = 0, FirstName = "[Select a customer...]" });
            return customers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList();
        }


        public static List<Supplier> GetSuppliers(int companyId)
        {
            var qry = (from su in db.Suppliers
                       join cs in db.CompanySuppliers on su.SupplierId equals cs.SupplierId
                       join co in db.Companies on cs.CompanyId equals co.CompanyId
                       where co.CompanyId == companyId
                       select new { su }).ToList();

            var suppliers = new List<Supplier>();
            foreach (var item in qry)
            {
                suppliers.Add(item.su);
            }
            suppliers.Add(new Supplier { SupplierId = 0, FirstName = "[Select a supplier...]" });
            return suppliers.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList();
        }

        public static List<Product> GetProducts(int companyId)
        {
            var products = db.Products.Where(p => p.CompanyId == companyId).ToList();
            products.Add(new Product { ProductId = 0, Description = "[Select a product...]" });
            return products.OrderBy(p => p.Description).ToList();
        }

        public static List<Product> GetProducts(int companyId, bool sw)
        {
            var products = db.Products.Where(p => p.CompanyId == companyId).ToList();
            return products.OrderBy(p => p.Description).ToList();
        }


        public void Dispose()
        {
            db.Dispose();
        }
    }
}