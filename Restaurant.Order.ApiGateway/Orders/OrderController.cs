using Microsoft.AspNetCore.Mvc;
using static Grpc.Core.Metadata;

namespace Restaurant.Order.ApiGateway.Orders;

[Route("[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    public struct OrderCreatedResponse
    {
        public required Guid Id { get; set; }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<OrderCreatedResponse> CreateOrder(CancellationToken cancellationToken)
    {
        var response = new OrderCreatedResponse()
        {
            Id = Guid.NewGuid()
        };

        return CreatedAtAction(nameof(GetOrder), new { orderId = response.Id }, response);
    }

    public struct OrderResponse
    {
        public required Guid Id { get; set; }
        public required int Version { get; set; }
    }

    [HttpGet("{orderId:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var response = new OrderResponse()
        {
            Id = orderId,
            Version = 1
        };

        return Ok(response);
    }
}
