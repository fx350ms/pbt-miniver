using pbt.Packages.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Packages
{
    internal class PrintStampModel
    {
        public PrintStampModel()
        {
        }

        public List<PackageDetailDto> Packages { get; set; }
        public string StampType { get; set; }
    }
}