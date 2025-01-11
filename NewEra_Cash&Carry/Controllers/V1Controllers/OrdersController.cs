using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.DTOs.OrderDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Controllers.V1Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public OrdersController(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResultDto>>> GetOrders()
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .ToListAsync();

        var orderDto = _mapper.Map<List<OrderResultDto>>(order);

        return Ok(orderDto);
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResultDto>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        var orderDto = _mapper.Map<OrderResultDto>(order);
        return Ok(orderDto);
    }

    // GET: api/Orders/user/{userId}
    [HttpGet("userId")]
    public async Task<ActionResult<IEnumerable<object>>> GetOrdersGroupedByUser()
    {
        // Buyurtmalarni olib kelish
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();

        // UserId bo'yicha guruhlash
        var groupedOrders = orders
            .GroupBy(o => o.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                TotalOrders = group.Count(),
                TotalOrderAmount = group.Sum(o => o.TotalAmount),
                TotalProductAmount = group.Sum(o => o.OrderItems.Sum(oi => oi.Price)),
                Orders = group.Select(order => new
                {
                    order.Id,
                    order.TotalAmount,
                    OrderItems = order.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        oi.Quantity,
                        oi.Product.Price
                    }).ToList()
                }).ToList()
            }).ToList();

        return Ok(groupedOrders);
    }

    // GET: api/Orders/group-by-user/{userId}
    [HttpGet("group-by-user/{userId}")]
    public async Task<ActionResult<object>> GetOrdersByUserId(int userId)
    {
        // Foydalanuvchi buyurtmalarini olib kelish
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();

        if (!orders.Any())
        {
            return NotFound($"No orders found for user with ID {userId}");
        }

        // UserId bo'yicha buyurtmalarni guruhlash
        var groupedOrders = new
        {
            UserId = userId,
            TotalOrders = orders.Count,
            TotalOrderAmount = orders.Sum(o => o.TotalAmount),
            TotalProductAmount = orders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price)),
            Orders = orders.Select(order => new
            {
                order.Id,
                order.TotalAmount,
                OrderItems = order.OrderItems.Select(oi => new
                {
                    oi.ProductId,
                    oi.Quantity,
                    oi.Product.Price
                }).ToList()
            }).ToList()
        };

        return Ok(groupedOrders);
    }




    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<OrderResultDto>> PostOrder(OrderCreateDto orderDto)
    {
        //Validate user existence
        var user = await _context.Users.FindAsync(orderDto.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        var order = new Order
        {
            UserId = orderDto.UserId,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var itemDto in orderDto.OrderItems)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null || product.Stock < itemDto.Quantity)
            {
                return BadRequest($"Insufficient stock for product ID {itemDto.ProductId}");
            }

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                Price = product.Price * itemDto.Quantity,
                Product = product // This is not necessary, but it is useful for the response
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.Price;
            product.Stock -= itemDto.Quantity; // Update stock
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Map Order to OrderResultDto
        var result = _mapper.Map<OrderResultDto>(order);

        return CreatedAtAction("GetOrder", new { id = order.Id }, result);
    }


    // PUT: api/Orders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutOrder(int id, OrderCreateDto orderDto)
    {
        // Buyurtmani id bo'yicha qidirish
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound("Order not found");
        }

        // Foydalanuvchini tekshirish
        var user = await _context.Users.FindAsync(orderDto.UserId);
        if (user == null)
        {
            return BadRequest("User not found");
        }

        // Buyurtma elementlarini yangilash
        decimal totalAmount = 0;
        var warnings = new List<string>();

        foreach (var itemDto in orderDto.OrderItems)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                warnings.Add($"Product ID {itemDto.ProductId} not found");
                continue;
            }

            // Yangi order item qo'shish yoki mavjudini yangilash
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);
            int previousQuantity = orderItem?.Quantity ?? 0;

            if (itemDto.Quantity > product.Stock + previousQuantity)
            {
                //Agar kerakli miqdor yetarli bo'lmasa boricha olamiz
                warnings.Add($"Product ID {itemDto.ProductId} has insufficient stock. Only {product.Stock + previousQuantity} available.");
                itemDto.Quantity = product.Stock + previousQuantity; // Boricha miqdorini olamiz
            }
            // Buyurtma  elementlarini qo'shish yoki yangilash  
            if (orderItem == null)
            {
                orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    Price = product.Price * itemDto.Quantity,

                };
                order.OrderItems.Add(orderItem);
            }
            else
            {
                // Mavjud buyurtma elementini yangilash
                int quantityChange = itemDto.Quantity - previousQuantity;
                orderItem.Quantity = itemDto.Quantity;
                orderItem.Price = product.Price * itemDto.Quantity;

                // stokni moslashtirish
                if (quantityChange > 0) // Miqdor oshirilgan bo'lsa
                {
                    product.Stock -= quantityChange;
                }
                else if (quantityChange < 0) // Miqdor kamaytirilgan bo'lsa
                {
                    product.Stock += Math.Abs(quantityChange);
                }
                {

                }
            }


            totalAmount += orderItem.Price;
            product.Stock = Math.Max(0, product.Stock); // Stockni yangilash
        }

        // Yangi umumiy summa
        order.TotalAmount = totalAmount;

        // O'zgartirishlarni saqlash
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        if (warnings.Count > 0)
        {
            return Ok(new
            {
                Message = "Order updated with warnings",
                Warnings = warnings
            });
        }

        return NoContent(); // HTTP 204 - Yangilash muvaffaqiyatli amalga oshdi
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        // Buyurtmani id bo'yicha qidirish
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound("Order not found");
        }

        // Buyurtma elementlarini qayta tiklash va stockni yangilash
        foreach (var orderItem in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product != null)
            {
                product.Stock += orderItem.Quantity; // O'chirilgan buyurtma elementlarini stockga qaytarish
            }
        }

        // Buyurtmani o'chirish
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent(); // HTTP 204 - O'chirish muvaffaqiyatli amalga oshdi
    }
}
