using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using pbt.Web.Models.Transactions;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace pbt.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

   
    public class WalletController : ControllerBase
    {

        private readonly ILogger<WalletController> _logger;

        public WalletController(ILogger<WalletController> logger)
        {
            _logger = logger;
        }

        //// GET: api/<Wallet>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<Wallet>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<Wallet>
        [HttpPost]
        public IActionResult Process([FromBody] CharingRequest request)
        {

            return Ok(request);
        }

        //// PUT api/<Wallet>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<Wallet>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
