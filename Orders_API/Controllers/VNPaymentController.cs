using CodeMegaVNPay.Models;
using CodeMegaVNPay.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orders_API.Models;
using Orders_API.Repository;
using static System.Net.WebRequestMethods;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IRepository<Order> _orderRepository;
        private readonly ILogger<VNPaymentController> _logger;

        public VNPaymentController(IVnPayService vnPayService, IRepository<Order> orderRepository, ILogger<VNPaymentController> logger)
        {
            _vnPayService = vnPayService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpPost("CreatePayment")]
        public IActionResult CreatePaymentUrl(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return new JsonResult(url);
        }

        [HttpGet("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.Success)
            {
                _logger.LogDebug("OrderDescription: {OrderDescription}", response.OrderDescription);
                var orderId = int.Parse(response.OrderDescription);
                _logger.LogDebug("OrderId: {OrderId}", orderId);
                var order = await _orderRepository.GetById(orderId);
                order.Status = "Paid";
                await _orderRepository.Update(order);
            }

            return Redirect("https://localhost:7106/");
        }
    }
}
