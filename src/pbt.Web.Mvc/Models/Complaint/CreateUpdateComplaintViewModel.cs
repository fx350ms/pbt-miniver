using pbt.Commons.Dto;
using pbt.CompaintReasons.Dto;
using pbt.Complaints.Dto;
using pbt.Orders.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Complaint
{
    public class CreateUpdateComplaintViewModel
    {
        public CreateUpdateComplaintDto Complaint { get; set; }

        public List<OptionItemDto> Orders { get; set; }

        public List<ComplaintReasonDto> ComplaintReasons { get; set; }
    }
}
