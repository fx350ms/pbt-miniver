// filepath: /d:/Repos/fx350ms/pbt/src/pbt.Application/OrderNumbers/Dto/OrderNumberDto.cs

using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.OrderNumbers.Dto
{
    public class IdentityCodeDto : EntityDto<long>
    {
        
        public long Date { get; set; }

        [StringLength(10)]
        public string Prefix { get; set; }
        public long SequentialNumber { get; set; }

        public string Code {
            get
            {
                //var baseDate = new DateTime(2025, 6, 1);
                //var currentDate = DateTime.Now;
                //int monthOffset = ((currentDate.Year - baseDate.Year) * 12) + currentDate.Month - baseDate.Month;
                //string formattedMonth = monthOffset.ToString("D2");
                //string formattedDay = currentDate.Day.ToString("D2");
                //string formattedSequentialNumber = SequentialNumber < 100 ? SequentialNumber.ToString("D2") : SequentialNumber.ToString("D3");
                //return $"{Prefix}{formattedMonth}{formattedDay}{formattedSequentialNumber}";

                return $"{Prefix}{Date}{SequentialNumber:D3}"; 
            }
        }
    }
}