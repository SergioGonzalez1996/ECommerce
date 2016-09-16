using ECommerce.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace ECommerce.Classes
{
    public class MovementsHelper : IDisposable
    {
        private static ECommerceContext db = new ECommerceContext();

        public static Response NewOrder(NewOrderView view, string userName)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == userName).FirstOrDefault();
                    var order = new Order
                    {
                        CompanyId = user.CompanyId,
                        CustomerId = view.CustomerId,
                        Date = view.Date,
                        Remarks = view.Remarks,
                        StateId = DBHelper.GetState("Created", db),
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    var details = db.OrderDetailTmps.Where(odt => odt.UserName == userName).ToList();

                    foreach (var detail in details)
                    {
                        var orderDetail = new OrderDetail
                        {
                            Description = detail.Description,
                            OrderId = order.OrderId,
                            Price = detail.Price,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            TaxRate = detail.TaxRate,
                        };
                        db.OrderDetails.Add(orderDetail);
                        db.OrderDetailTmps.Remove(detail);
                    }

                    db.SaveChanges();
                    transaccion.Commit();
                    return new Response { Succeeded = true };
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    if (ex.InnerException != null && ex.InnerException.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.InnerException.Message };
                    }
                    else if (ex.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.Message };
                    }
                    else
                    {
                        return new Response { Succeeded = false, Message = ex.Message };
                    }
                }
            }
        }

        public static Response NewPurchase(NewPurchaseView view, string userName)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == userName).FirstOrDefault();
                    var purchase = new Purchase
                    {
                        CompanyId = user.CompanyId,
                        SupplierId = view.SupplierId,
                        Date = view.Date,
                        Remarks = view.Remarks,
                        StateId = DBHelper.GetState("Created", db),
                        WarehouseId = view.WarehouseId,
                    };

                    db.Purchases.Add(purchase);
                    db.SaveChanges();

                    var details = db.PurchaseDetailTmps.Where(pdt => pdt.UserName == userName).ToList();

                    foreach (var detail in details)
                    {
                        var purchaseDetail = new PurchaseDetail
                        {
                            Description = detail.Description,
                            PurchaseId = purchase.PurchaseId,
                            Price = detail.Price,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            TaxRate = detail.TaxRate,
                        };
                        db.PurchaseDetails.Add(purchaseDetail);
                        db.PurchaseDetailTmps.Remove(detail);


                        var inventory = db.Inventories.Where(i => i.WarehouseId == view.WarehouseId && i.ProductId == detail.ProductId).FirstOrDefault();
                        if (inventory == null)
                        {
                            inventory = new Inventory
                            {
                                WarehouseId = view.WarehouseId,
                                ProductId = detail.ProductId,
                                Stock = detail.Quantity,
                            };
                            db.Inventories.Add(inventory);
                        }
                        else
                        {
                            inventory.Stock += inventory.Stock + detail.Quantity;
                            db.Entry(inventory).State = EntityState.Modified;
                        }
                        //db.SaveChanges();
                    }
                    db.SaveChanges();
                    transaccion.Commit();
                    return new Response { Succeeded = true };
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    if (ex.InnerException != null && ex.InnerException.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.InnerException.Message };
                    }
                    else if (ex.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.Message };
                    }
                    else
                    {
                        return new Response { Succeeded = false, Message = ex.Message };
                    }
                }
            }
        }

        public static Response NewSale(NewSaleView view, string userName)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == userName).FirstOrDefault();
                    var sale = new Sale
                    {
                        CompanyId = user.CompanyId,
                        CustomerId = view.CustomerId,
                        Date = view.Date,
                        Remarks = view.Remarks,
                        StateId = DBHelper.GetState("Created", db),
                        WarehouseId = view.WarehouseId,
                    };

                    if (view.OrderId > 0)
                    {
                        var orderDetails = db.Orders.Where(o => o.OrderId == view.OrderId).FirstOrDefault();
                        if (orderDetails == null)
                        {
                            transaccion.Rollback();
                            return new Response { Succeeded = false, Message = "Can not get the details of this order." };
                        }
                        sale.OrderId = view.OrderId;
                        sale.StateId = DBHelper.GetState("Invoiced", db);
                        orderDetails.StateId = DBHelper.GetState("Invoiced", db);
                        db.Entry(orderDetails).State = EntityState.Modified;
                        db.Entry(sale).State = EntityState.Modified;
                        //db.SaveChanges();
                    }

                    db.Sales.Add(sale);
                    db.SaveChanges();

                    var details = db.SaleDetailTmps.Where(sdt => sdt.UserName == userName).ToList();

                    foreach (var detail in details)
                    {
                        var saleDetail = new SaleDetail
                        {
                            Description = detail.Description,
                            SaleId = sale.SaleId,
                            Price = detail.Price,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            TaxRate = detail.TaxRate,
                        };
                        db.SaleDetails.Add(saleDetail);
                        db.SaleDetailTmps.Remove(detail);


                        var inventory = db.Inventories.Where(i => i.WarehouseId == view.WarehouseId && i.ProductId == detail.ProductId).FirstOrDefault();
                        if (inventory == null)
                        {
                            inventory = new Inventory
                            {
                                WarehouseId = view.WarehouseId,
                                ProductId = detail.ProductId,
                                Stock = detail.Quantity,
                            };
                            db.Inventories.Add(inventory);
                        }
                        else
                        {
                            inventory.Stock = inventory.Stock - detail.Quantity;
                            db.Entry(inventory).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                    db.SaveChanges();
                    transaccion.Commit();
                    return new Response { Succeeded = true };
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    if (ex.InnerException != null && ex.InnerException.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.InnerException.Message };
                    }
                    else if (ex.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.Message };
                    }
                    else
                    {
                        return new Response { Succeeded = false, Message = ex.Message };
                    }
                }
            }
        }

        public static Response SaleFromOrder(int orderId, string userName)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                try
                {
                    var user = db.Users.Where(u => u.UserName == userName).FirstOrDefault();
                    var details = db.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                    var orderDetails = db.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                    if (details == null || details.Count == 0 || orderDetails == null)
                    {
                        return new Response { Succeeded = false, Message = "Can not get the details of this order." };
                    }

                    // If user has temp details, delete them
                    var userOldDetails = db.SaleDetailTmps.Where(sdt => sdt.UserName == userName).ToList();
                    foreach (var oldDetails in userOldDetails)
                    {
                        db.SaleDetailTmps.Remove(oldDetails);
                    }
                    // NOTE: This could be unnecessary

                    foreach (var detail in details)
                    {
                        var saleDetailTmp = new SaleDetailTmp
                        {
                            Description = detail.Description,
                            Price = detail.Price,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            TaxRate = detail.TaxRate,
                            UserName = userName,
                            OrderId = orderId,
                        };
                        db.SaleDetailTmps.Add(saleDetailTmp);
                    }

                    db.SaveChanges();
                    transaccion.Commit();
                    return new Response { Succeeded = true, CustomerId = orderDetails.CustomerId, Date = orderDetails.Date, Remarks = orderDetails.Remarks };
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    if (ex.InnerException != null && ex.InnerException.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.InnerException.Message };
                    }
                    else if (ex.InnerException != null)
                    {
                        return new Response { Succeeded = false, Message = ex.InnerException.Message };
                    }
                    else
                    {
                        return new Response { Succeeded = false, Message = ex.Message };
                    }
                }
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}