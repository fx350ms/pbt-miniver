using Abp.Application.Services.Dto;
using System;

namespace pbt.ShippingCosts.Dto
{
    public class ShippingCostGroupDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public int ShippingPartnerId { get; set; }
        public bool IsActived { get; set; } = true;

        public string FromDateStr
        {
            get
            {
                return FromDate?.ToString(DateFormat);
            }
            set
            {
                
            }
        }

        public string ToDateStr
        {
            get
            {
                 return ToDate?.ToString(DateFormat);   
            }
            set
            {
                
            }
        }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        //public DateTime? FromDate
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(FromDateStr)) return null;
        //        if (DateTime.TryParseExact(FromDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
        //        {
        //            return startDate;
        //        }
        //        return null;
        //    }
        //}

        //public DateTime? ToDate
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(ToDateStr)) return null;
        //        if (DateTime.TryParseExact(ToDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
        //        {
        //            return endDate;
        //        }
        //        return null;
        //    }
        //}


        private static readonly string DateFormat = "dd/MM/yyyy";
    }
}