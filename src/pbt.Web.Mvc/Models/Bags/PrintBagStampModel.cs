using pbt.Bags.Dto;

namespace pbt.Web.Models.Bags
{
    public class PrintBagStampModel
    {
        public BagStampDto Bag { get; set; }
        public bool IsExport { get; set; } = true;
    }
}