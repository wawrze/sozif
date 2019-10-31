using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif;
using Sozif.Attributes;
using Sozif.Models;

namespace Sozif.Controllers
{
    [Auth]
    public class OrdersController : Controller
    {
        private readonly sozifContext _context;

        public OrdersController(sozifContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var sozifContext = _context.Orders
                .Where(o => !o.OrderNumber.Contains("R"))
                .OrderByDescending(o => o.OrderId)
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                        .ThenInclude(p => p.TaxRate);
            return View(await sozifContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/ChooseCustomer
        public async Task<IActionResult> ChooseCustomer()
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            string userId = HttpContext.Session.GetString("UserId");
            if (userId == null) RedirectToAction("Index", "Home");
            var userWorkingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == int.Parse(userId) && o.OrderNumber.Contains("R"));
            if (userWorkingOrder != null) return RedirectToAction("Create", new { id = userWorkingOrder.CustomerId, orderId = userWorkingOrder.OrderId });

            var customers = await _context.Customers.Include(c => c.Addresses).OrderBy(c => c.CustomerName).ToListAsync();
            return View(customers);
        }

        // GET: Orders/Create/5
        public async Task<IActionResult> Create(int? id, int? orderId)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            Orders order;
            if (orderId == null)
            {
                order = new Orders();
                var lastOrderInDB = await _context.Orders
                    .Where(o => o.OrderNumber.Contains("R"))
                    .OrderByDescending(o => o.OrderId)
                    .FirstOrDefaultAsync();
                string newOrderNumber;
                if (lastOrderInDB == null)
                {
                    newOrderNumber = "ZAM/R/0001";
                }
                else
                {
                    string[] lastOrderNumberSplit = lastOrderInDB.OrderNumber.Split('/');
                    int numberInt = int.Parse(lastOrderNumberSplit[2]);
                    string numberString = "" + (numberInt + 1);
                    while (numberString.Length < 4)
                    {
                        numberString = "0" + numberString;
                    }
                    newOrderNumber = "ZAM/R/" + numberString;
                }
                order.OrderNumber = newOrderNumber;
                order.OrderDate = DateTime.Now;
                order.UserId = int.Parse(HttpContext.Session.GetString("UserId"));
                order.CustomerId = (int)id;
                order.AddressId = customer.Addresses.First(a => a.IsMainAddress).AddressId;
                _context.Add(order);
                await _context.SaveChangesAsync();
            }
            else
            {
                order = await _context.Orders
                    .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
            }

            ViewBag.Customer = customer;
            ViewData["AddressId"] = new SelectList(customer.Addresses, "AddressId", "FullAddress");

            return View(order);
        }

        // GET: Orders/CreateNewOrderPosition/5
        public async Task<IActionResult> CreateNewOrderPosition(int? id)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;

            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.ProductName), "ProductId", "ProductName");
            return View();
        }

        // POST: Orders/CreateNewOrderPosition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewOrderPosition(int? id, [Bind("ProductId,OrderId,Count,Discount")] OrderPositions orderPosition)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(orderPosition);
                await _context.SaveChangesAsync();
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
                return RedirectToAction("Create", new { id = order.CustomerId, orderId = id });
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View(orderPosition);
        }

        // GET: Orders/EditNewOrderPosition/5
        public async Task<IActionResult> EditNewOrderPosition(int? id, int? orderId)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null || orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.Address)
                                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            var position = await _context.OrderPositions
                .Include(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.ProductId == id);
            if (order == null || position == null)
            {
                return NotFound();
            }
            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;

            return View(position);
        }

        // POST: Orders/EditNewOrderPosition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNewOrderPosition(int id, int? orderId, [Bind("ProductId,OrderId,Count,Discount")] OrderPositions orderPosition)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != orderPosition.ProductId || orderId != orderPosition.OrderId)
            {
                return NotFound();
            }
            var order = await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Address)
        .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderPosition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orderPosition.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Create", new { id = order.CustomerId, orderId = order.OrderId });
            }
            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;

            return View(orderPosition);
        }

        // POST: Orders/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("OrderId,OrderNumber,OrderDate,RealisationDate,InvoiceId,UserId,CustomerId,AddressId")] Orders orders)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var orderPositions = await _context.OrderPositions.Where(op => op.OrderId == orders.OrderId).ToListAsync();
            if (orderPositions.Count == 0) ViewBag.ErrorMessage = "Nie możesz zapisać zamówienia bez pozycji!";
            if (orderPositions.Count > 0)
            {
                Orders orderToAdd = new Orders();
                var lastOrderInDB = await _context.Orders
                    .Where(o => !o.OrderNumber.Contains("R"))
                    .OrderByDescending(o => o.OrderId)
                    .FirstOrDefaultAsync();
                string month = DateTime.Now.ToString("MM", DateTimeFormatInfo.InvariantInfo);
                string year = DateTime.Now.ToString("yy", DateTimeFormatInfo.InvariantInfo);
                string newOrderNumber;
                if (lastOrderInDB == null)
                {
                    newOrderNumber = "ZAM/0001/" + month + "/" + year;
                }
                else
                {
                    string[] lastOrderNumberSplit = lastOrderInDB.OrderNumber.Split('/');
                    int numberInt;
                    DateTime lastOrderDate = lastOrderInDB.OrderDate;
                    DateTime todayDate = DateTime.Now;
                    if (lastOrderDate.Month == todayDate.Month && lastOrderDate.Year == todayDate.Year)
                    {
                        numberInt = int.Parse(lastOrderNumberSplit[1]);
                    } else
                    {
                        numberInt = 1;
                    }

                    string numberString = "" + (numberInt + 1);
                    while (numberString.Length < 4)
                    {
                        numberString = "0" + numberString;
                    }
                    newOrderNumber = "ZAM/" + numberString + "/" + month + "/" + year;
                }
                orderToAdd.OrderNumber = newOrderNumber;
                orderToAdd.OrderDate = orders.OrderDate;
                orderToAdd.UserId = orders.UserId;
                orderToAdd.CustomerId = orders.CustomerId;
                orderToAdd.AddressId = orders.AddressId;

                _context.Add(orderToAdd);
                await _context.SaveChangesAsync();

                List<OrderPositions> newOrderPositions = new List<OrderPositions>();
                orderPositions.ForEach(op => {
                    var newPosition = new OrderPositions();
                    newPosition.OrderId = orderToAdd.OrderId;
                    newPosition.ProductId = op.ProductId;
                    newPosition.Count = op.Count;
                    newPosition.Discount = op.Discount;
                    newOrderPositions.Add(newPosition);
                });
                _context.RemoveRange(orderPositions);
                await _context.SaveChangesAsync();
                _context.AddRange(newOrderPositions);
                await _context.SaveChangesAsync();
                _context.Remove(orders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            Orders order;
            order = await _context.Orders
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(o => o.OrderId == orders.OrderId);

            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Customer = customer;
            ViewData["AddressId"] = new SelectList(customer.Addresses, "AddressId", "FullAddress");
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "AddressId", "City", orders.AddressId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", orders.CustomerId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "CustomerAddress", orders.InvoiceId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Firstname", orders.UserId);
            return View(orders);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderNumber,OrderDate,RealisationDate,InvoiceId,UserId,CustomerId,AddressId")] Orders orders)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != orders.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "AddressId", "City", orders.AddressId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", orders.CustomerId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "CustomerAddress", orders.InvoiceId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Firstname", orders.UserId);
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var orders = await _context.Orders.Include(o => o.OrderPositions).FirstOrDefaultAsync(o => o.OrderId == id);
            _context.OrderPositions.RemoveRange(orders.OrderPositions);
            await _context.SaveChangesAsync();
            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var orders = await _context.Orders.Include(o => o.OrderPositions).FirstOrDefaultAsync(o => o.OrderId == id);
            _context.OrderPositions.RemoveRange(orders.OrderPositions);
            await _context.SaveChangesAsync();
            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
