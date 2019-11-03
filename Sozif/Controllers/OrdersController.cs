using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index(
            string? order,
            string? customer,
            DateTime? orderFrom,
            DateTime? orderTo,
            string? address,
            int? positionsFrom,
            int? positionsTo,
            double? netFrom,
            double? netTo,
            double? taxFrom,
            double? taxTo,
            double? grossFrom,
            double? grossTo,
            DateTime? realisationFrom,
            DateTime? realisationTo,
            string? invoice,
            string? user
            )
        {
            var orders = await _context.Orders
                .Where(o => !o.OrderNumber.Contains("R"))
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                        .ThenInclude(p => p.TaxRate)
                .OrderByDescending(o => o.OrderId)
                .ToListAsync();

            var ordersToShow = new List<Orders>();
            orders.ForEach(o =>
            {
                bool show = true;
                if (order != null && order != "" && !o.OrderNumber.ToLower().Contains(order.ToLower()))
                {
                    show = false;
                }
                if (customer != null && customer != "" && !o.Customer.ToString().ToLower().Contains(customer.ToLower()))
                {
                    show = false;
                }
                if (orderFrom != null && o.OrderDate < orderFrom)
                {
                    show = false;
                }
                if (orderTo != null && o.OrderDate > orderTo)
                {
                    show = false;
                }
                if (address != null && address != "" && !o.Address.FullAddress.ToLower().Contains(address.ToLower()))
                {
                    show = false;
                }
                if (positionsFrom != null && o.PositionsCount < positionsFrom)
                {
                    show = false;
                }
                if (positionsTo != null && o.PositionsCount > positionsTo)
                {
                    show = false;
                }
                if (netFrom != null && o.NetValue < netFrom * 100)
                {
                    show = false;
                }
                if (netTo != null && o.NetValue > netTo * 100)
                {
                    show = false;
                }
                if (taxFrom != null && o.TaxValue < taxFrom * 100)
                {
                    show = false;
                }
                if (taxTo != null && o.TaxValue > taxTo * 100)
                {
                    show = false;
                }
                if (grossFrom != null && o.GrossValue < grossFrom * 100)
                {
                    show = false;
                }
                if (grossTo != null && o.GrossValue > grossTo * 100)
                {
                    show = false;
                }
                if (realisationFrom != null && (o.RealisationDate == null || o.RealisationDate < realisationFrom))
                {
                    show = false;
                }
                if (realisationTo != null && (o.RealisationDate == null || o.RealisationDate > realisationTo))
                {
                    show = false;
                }
                if (invoice != null && invoice != "" && (o.InvoiceId == null || !o.Invoice.InvoiceNumber.ToLower().Contains(invoice.ToLower())))
                {
                    show = false;
                }
                if (user != null && user != "" && !o.UserName.ToLower().Contains(user.ToLower()))
                {
                    show = false;
                }
                if (show)
                {
                    ordersToShow.Add(o);
                }
            });

            ViewBag.CurrentOrder = order;
            ViewBag.CurrentCustomer = customer;
            ViewBag.OrderFrom = orderFrom?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.OrderTo = orderTo?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.CurrentAddress = address;
            ViewBag.PositionsFrom = positionsFrom;
            ViewBag.PositionsTo = positionsTo;
            ViewBag.NetFrom = netFrom.ToString().Replace(',', '.');
            ViewBag.NetTo = netTo.ToString().Replace(',', '.');
            ViewBag.TaxFrom = taxFrom.ToString().Replace(',', '.');
            ViewBag.TaxTo = taxTo.ToString().Replace(',', '.');
            ViewBag.GrossFrom = grossFrom.ToString().Replace(',', '.');
            ViewBag.GrossTo = grossTo.ToString().Replace(',', '.');
            ViewBag.RealisationFrom = realisationFrom?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.RealisationTo = realisationTo?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.CurrentInvoice = invoice;
            ViewBag.CurrentUser = user;
            return View(ordersToShow);
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

        // GET: Orders/Completed/5
        public async Task<IActionResult> Completed(int? id)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);

            if (orders == null)
            {
                return NotFound();
            }

            orders.RealisationDate = DateTime.Now;
            _context.Update(orders);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Orders/ChooseCustomer
        public async Task<IActionResult> ChooseCustomer(string? name, string? nip, string? contact, string? phone, string? address)
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
            var customersToShow = new List<Customers>();
            customers.ForEach(c =>
            {
                bool isMatching = true;
                if (name != null && name != "" && !c.CustomerName.ToLower().Contains(name.ToLower()))
                {
                    isMatching = false;
                }
                if (nip != null && nip != "")
                {
                    string justNumber = "";
                    foreach (char ch in nip)
                    {
                        if (ch != '-') justNumber += ch;
                    }
                    if (!c.Nip.ToString().Contains(justNumber))
                    {
                        isMatching = false;
                    }
                }
                if (contact != null && contact != "" && (c.ContactPerson == null || !c.ContactPerson.ToLower().Contains(contact.ToLower())))
                {
                    isMatching = false;
                }
                if (phone != null && phone != "")
                {
                    string justNumber = "";
                    foreach (char ch in phone)
                    {
                        if (ch != '-') justNumber += ch;
                    }
                    if (c.PhoneNumber == null || !c.PhoneNumber.ToString().Contains(justNumber))
                    {
                        isMatching = false;
                    }
                }
                if (address != null && address != "" && !c.Addresses.First(a => a.IsMainAddress).FullAddress.ToLower().Contains(address.ToLower()))
                {
                    isMatching = false;
                }
                if (isMatching)
                {
                    customersToShow.Add(c);
                }
            });
            @ViewBag.Name = name;
            @ViewBag.Nip = nip;
            @ViewBag.Contact = contact;
            @ViewBag.Phone = phone;
            @ViewBag.Address = address;

            return View(customersToShow);
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
            string errorMessage = "";
            if (orderPosition.Discount < 0)
            {
                errorMessage = "Rabat nie może być ujemny!";
            }
            if (orderPosition.Discount > 10)
            {
                errorMessage = "Rabat nie może być większy niż 10%!";
            }
            if (orderPosition.Count < 0)
            {
                errorMessage = "Ilość produktu nie może być ujemna!";
            }
            if (orderPosition.Count == 0)
            {
                errorMessage = "Ilość produktu musi być różna od zera!";
            }

            if (ModelState.IsValid && errorMessage == "")
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
            ViewBag.ErrorMessage = errorMessage;
            var position = await _context.OrderPositions
                .Include(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.ProductId == id);
            position.Count = orderPosition.Count;
            position.Discount = orderPosition.Discount;

            return View(position);
        }

        // GET: Orders/EditOrderPosition/5
        public async Task<IActionResult> EditOrderPosition(int? id, int? orderId)
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

            if (order.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = orderId });
            }

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
            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.OrderDate = order.OrderDate;

            return View(position);
        }

        // POST: Orders/EditOrderPosition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrderPosition(int id, int? orderId, [Bind("ProductId,OrderId,Count,Discount")] OrderPositions orderPosition)
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
            if (order.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = orderId });
            }

            string errorMessage = "";
            if (orderPosition.Discount < 0)
            {
                errorMessage = "Rabat nie może być ujemny!";
            }
            if (orderPosition.Discount > 10)
            {
                errorMessage = "Rabat nie może być większy niż 10%!";
            }
            if (orderPosition.Count < 0)
            {
                errorMessage = "Ilość produktu nie może być ujemna!";
            }
            if (orderPosition.Count == 0)
            {
                errorMessage = "Ilość produktu musi być różna od zera!";
            }

            if (ModelState.IsValid && errorMessage == "")
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
                return RedirectToAction("Edit", new { id = orderId });
            }
            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;
            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.OrderDate = order.OrderDate;
            ViewBag.ErrorMessage = errorMessage;
            var position = await _context.OrderPositions
                .Include(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.ProductId == id);
            position.Count = orderPosition.Count;
            position.Discount = orderPosition.Discount;

            return View(position);
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
                    }
                    else
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
                orderToAdd.OrderDate = DateTime.Now;
                orderToAdd.UserId = orders.UserId;
                orderToAdd.CustomerId = orders.CustomerId;
                orderToAdd.AddressId = orders.AddressId;

                _context.Add(orderToAdd);
                await _context.SaveChangesAsync();

                List<OrderPositions> newOrderPositions = new List<OrderPositions>();
                orderPositions.ForEach(op =>
                {
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
            if (orders.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = id });
            }

            var addresses = _context.Addresses.Where(a => a.CustomerId == orders.CustomerId);
            ViewData["AddressId"] = new SelectList(addresses, "AddressId", "FullAddress", orders.AddressId);

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

            int orderPositionsCount = await _context.OrderPositions.Where(op => op.OrderId == id).CountAsync();
            if (orderPositionsCount == 0)
            {
                ViewBag.ErrorMessage = "Nie możesz zapisać zamówienia bez pozycji!";
                var orderWithNoPositions = await _context.Orders
                    .Include(o => o.Address)
                    .Include(o => o.Customer)
                    .Include(o => o.Invoice)
                    .Include(o => o.User)
                    .Include(o => o.OrderPositions)
                        .ThenInclude(op => op.Product)
                            .ThenInclude(p => p.TaxRate)
                    .FirstOrDefaultAsync(m => m.OrderId == id);
                if (orderWithNoPositions == null)
                {
                    return NotFound();
                }
                var addresses = _context.Addresses.Where(a => a.CustomerId == orderWithNoPositions.CustomerId);
                ViewData["AddressId"] = new SelectList(addresses, "AddressId", "FullAddress", orderWithNoPositions.AddressId);

                return View(orderWithNoPositions);
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.Invoice)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = id });
            }

            order.AddressId = orders.AddressId;

            try
            {
                _context.Update(order);
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
            return RedirectToAction("Details", new { id = id });
        }

        // GET: Orders/NewPosition/5
        public async Task<IActionResult> NewPosition(int? id, string? from)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            if (from == null && from != "New" && from != "Edit")
            {
                return NotFound();
            }
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;
            ViewBag.From = from;

            ViewData["ProductId"] = new SelectList(_context.Products.OrderBy(p => p.ProductName), "ProductId", "ProductName");
            return View();
        }

        // POST: Orders/NewPosition/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPosition(int id, string from, [Bind("ProductId,OrderId,Count,Discount")] OrderPositions orderPosition)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            string errorMessage = "";
            var orderPositionInDB = await _context.OrderPositions
                .Include(op => op.Product)
                .FirstOrDefaultAsync(op => op.OrderId == orderPosition.OrderId && op.ProductId == orderPosition.ProductId);
            if (orderPositionInDB != null)
            {
                errorMessage = "Zamówienie już zawiera pozycję \"" + orderPositionInDB.Product.ProductName + "\"!";
            }
            if (orderPosition.Discount < 0)
            {
                errorMessage = "Rabat nie może być ujemny!";
            }
            if (orderPosition.Discount > 10)
            {
                errorMessage = "Rabat nie może być większy niż 10%!";
            }
            if (orderPosition.Count < 0)
            {
                errorMessage = "Ilość produktu nie może być ujemna!";
            }
            if (orderPosition.Count == 0)
            {
                errorMessage = "Ilość produktu musi być różna od zera!";
            }

            if (ModelState.IsValid && errorMessage == "")
            {
                _context.Add(orderPosition);
                await _context.SaveChangesAsync();

                if (from == "New")
                {
                    var newOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
                    return RedirectToAction("Create", new { id = newOrder.CustomerId, orderId = id });
                }
                return RedirectToAction("Edit", new { id = id });
            }
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            ViewBag.ErrorMessage = errorMessage;
            return View(orderPosition);
        }

        // GET: Orders/DeleteNewOrderPosition/5/?orderId=5
        public async Task<IActionResult> DeleteNewOrderPosition(int? id, int? orderId)
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
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }
            var orderPosition = order.OrderPositions.FirstOrDefault(op => op.ProductId == id);
            if (orderPosition == null)
            {
                return NotFound();
            }

            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;

            return View(orderPosition);
        }

        // POST: Orders/DeleteNewOrderPosition/5?orderId=5
        [HttpPost, ActionName("DeleteNewOrderPosition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNewOrderPositionConfirmed(int id, int orderId)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var orderPosition = await _context.OrderPositions.FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == id);
            _context.OrderPositions.Remove(orderPosition);
            await _context.SaveChangesAsync();
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            return RedirectToAction("Create", new { id = order.CustomerId, orderId = orderId });
        }

        // GET: Orders/DeleteOrderPosition/5/?orderId=5
        public async Task<IActionResult> DeleteOrderPosition(int? id, int? orderId)
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
                .Include(o => o.Address)
                .Include(o => o.Customer)
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }
            if (order.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = orderId });
            }
            var orderPosition = order.OrderPositions.FirstOrDefault(op => op.ProductId == id);
            if (orderPosition == null)
            {
                return NotFound();
            }

            ViewBag.Customer = order.Customer;
            ViewBag.Address = order.Address;
            ViewBag.OrderId = order.OrderId;
            ViewBag.OrderNumber = order.OrderNumber;
            ViewBag.OrderDate = order.OrderDate;

            return View(orderPosition);
        }

        // POST: Orders/DeleteOrderPosition/5?orderId=5
        [HttpPost, ActionName("DeleteOrderPosition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderPositionConfirmed(int id, int orderId)
        {
            if (HttpContext.Session.GetString("EditOrders") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var orderPosition = await _context.OrderPositions.FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == id);
            _context.OrderPositions.Remove(orderPosition);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = orderId });
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
            if (orders.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = id });
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
            if (orders.RealisationDate != null)
            {
                return RedirectToAction("Details", new { id = id });
            }
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