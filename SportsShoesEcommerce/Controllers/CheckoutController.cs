using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.Enums;
using SportsShoesEcommerce.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;

namespace SportsShoesEcommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Info"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutViewModel();

            foreach (var item in cart.CartItems)
            {
                var variant = item.ProductVariant;

                if (variant != null)
                {
                    var image = variant.Product?.ProductImages?
                        .FirstOrDefault(x => x.IsMain)?.ImagePath
                        ?? variant.Product?.ProductImages?.FirstOrDefault()?.ImagePath;

                    viewModel.Cart.Items.Add(new CartItemViewModel
                    {
                        CartItemId = item.Id,
                        ProductVariantId = variant.Id,
                        ProductName = variant.Product?.Name ?? "",
                        ImagePath = image,
                        Size = variant.Size,
                        Color = variant.Color,
                        Price = variant.VariantPrice,
                        Quantity = item.Quantity
                    });
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
            {
                return await Index();
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Info"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var item in cart.CartItems)
            {
                if (item.ProductVariant == null || item.Quantity > item.ProductVariant.StockQuantity)
                {
                    TempData["Error"] = "Some products do not have enough stock.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var address = new Address
            {
                UserId = userId,
                City = model.City,
                Street = model.Street,
                PostalCode = model.PostalCode
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var order = new Order
            {
                UserId = userId,
                AddressId = address.Id,
                OrderStatus = OrderStatus.Pending,
                TotalPrice = cart.CartItems.Sum(i => i.Quantity * i.ProductVariant.VariantPrice),
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    Price = item.ProductVariant.VariantPrice
                };

                _context.OrderItems.Add(orderItem);
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                PaymentMethod = PaymentMethod.Stripe,
                PaymentStatus = PaymentStatus.Pending,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "payment",
                SuccessUrl = domain + $"/Checkout/PaymentSuccess?orderId={order.Id}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = domain + $"/Checkout/PaymentCancel?orderId={order.Id}",
                ClientReferenceId = order.Id.ToString(),
                LineItems = cart.CartItems.Select(item => new SessionLineItemOptions
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.ProductVariant.VariantPrice * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductVariant.Product?.Name ?? "Product"
                        }
                    }
                }).ToList()
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            payment.TransactionnId = session.Id;
            await _context.SaveChangesAsync();

            return Redirect(session.Url);
        }

        public async Task<IActionResult> PaymentSuccess(int orderId, string session_id)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Payment == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Cart");
            }

            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Payment was not completed.";
                return RedirectToAction("Index", "Cart");
            }

            order.Payment.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Processing;
            order.Payment.TransactionnId = session.Id;

            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    item.ProductVariant.StockQuantity -= item.Quantity;
                }
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment completed and order placed successfully.";

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        public async Task<IActionResult> PaymentCancel(int orderId)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order != null)
            {
                order.OrderStatus = OrderStatus.Cancelled;

                if (order.Payment != null)
                {
                    order.Payment.PaymentStatus = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();
            }

            TempData["Info"] = "Payment was cancelled.";
            return RedirectToAction("Cancel");
        }
        public IActionResult Success(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
        public IActionResult Cancel()
        {
            return View();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}