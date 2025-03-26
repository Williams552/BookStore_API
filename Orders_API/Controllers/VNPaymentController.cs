using CodeMegaVNPay.Models;
using CodeMegaVNPay.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orders_API.Models;
using Orders_API.Repository;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private IRepository<Order> _orderRepository;

        public VNPaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
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
                var orderId = int.Parse(response.OrderDescription);
                var order = await _orderRepository.GetById(orderId);
                order.Status = "Paid";
                await _orderRepository.Update(order);
            }

            return new JsonResult(response);
        }
    }
}
