using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace pbt.WarehouseTransfers.Dto
{
    public class TransferItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TransferItemDto> Packages { get; set; }
    }
}
