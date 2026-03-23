using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.FundAccounts;
using pbt.Messages;
using pbt.Messages.Dto;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    public class MessageController : pbtControllerBase
    {
        private readonly IMessageAppService _messageAppService;
        private readonly IFundAccountAppService _fundAccountAppService;
        public MessageController(IMessageAppService messageAppService,
            IFundAccountAppService fundAccountAppService
            )
        {
            _messageAppService = messageAppService;
            _fundAccountAppService = fundAccountAppService;
        }

        public async Task<IActionResult> Index()
        {
            var activeFundAccounts = await _fundAccountAppService.GetActiveFundAccountsAsync();
            ViewBag.ActiveFundAccounts = activeFundAccounts;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] ReceiveMessageDto input)
        {
            var message = await _messageAppService.ReceiveAsync(input);
            return Ok(message);
        }
    }
}