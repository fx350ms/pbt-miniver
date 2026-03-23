using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.DeliveryNotes.Dto
{
    /// <summary>
    /// 
    /// </summary>
    public class DeliveryNoteDetail 
    {
        public DeliveryNoteDto Dto { get; set; }
        
    }
}
