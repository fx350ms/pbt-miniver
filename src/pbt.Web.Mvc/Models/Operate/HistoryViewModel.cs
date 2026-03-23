using DocumentFormat.OpenXml.Office2010.ExcelAc;
using pbt.Users.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Operate
{
    public class ScanCodeHistoryViewModel
    {
        public List<UserSelectDto> WarehouseUser { get; set; } = new List<UserSelectDto>();
    }
}
