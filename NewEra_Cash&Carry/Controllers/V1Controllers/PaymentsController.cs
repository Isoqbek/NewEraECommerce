using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.Helpers;
using Stripe;

namespace NewEra_Cash_Carry.Controllers.V1Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly PaymentSettings _paymentSettings;
    public PaymentsController(ECommerceDbContext context, IOptions<PaymentSettings> paymentSettings)
    {
        _context = context;
        _paymentSettings = paymentSettings.Value;
        StripeConfiguration.ApiKey = _paymentSettings.SecretKey;
    }

    // POST: api/Payments/charge
    [HttpPost("charge")]
    public async Task<IActionResult> ProcessPayment(int orderId)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
        {
            return NotFound("Order not founded");
        }

        if (order.PaymentStatus == "Paid")
        {
            return BadRequest("Order is already paid.");
        }

        try
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Amount = (long)order.TotalAmount * 100,
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" }
            });

            order.PaymentStatus = "Paid";

            _context.Entry(order).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Payment processed successfully",
                PaymentIntentId = paymentIntent.Id
            });
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
