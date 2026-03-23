using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;
using System.Text.Json.Serialization;

namespace pbt.Customers.Dto
{
    public class CustomerForSelectBoxDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
