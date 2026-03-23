using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.Notifications;
using pbt.Controllers;

namespace pbt.Web.Host.Controllers
{
    public class WalletController : pbtControllerBase
    {
        private readonly INotificationPublisher _notificationPublisher;

        public WalletController(INotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }


        [HttpGet("addFunds")]
        public async Task<IActionResult> AddFunds([FromBody] int value)
        {
        ///    await _walletAppService.AddFundsAsync(input);
            return Ok(new { message = "Funds added successfully!" });
        }

    }
}
