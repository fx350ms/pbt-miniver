using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class ImportOrderDto
    {
        public List<IFormFile> Attachments { get; set; }
    }
}
