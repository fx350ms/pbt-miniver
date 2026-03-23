using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Bags.Dto;

namespace pbt.Packages.Dto
{
    public class PackageDeliveryRequestDto : PackageDto
    {
        public int? DeliveryRequestOrderId { get; set; }
        public string? DeliveryRequestOrderCode { get; set; }
        [CanBeNull] public string OrderCode { get; set; }
    }
}