using Abp.Application.Services;
using System.Threading.Tasks;
using pbt.Warehouses.Dto;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Export.Dto;

namespace pbt.Export
{
    public interface IExportAppService : IAsyncCrudAppService<Dto.ExportDto, int, PagedBagResultRequestDto, CreateUpdateExportDto, ExportDto>
    {
        public Task<ListResultDto<ExportDto>> getDeliveryNotesFilter(PagedExportResultRequestDto input);
        public Task<ExportItemsDto> GetItemByDeliveryNote(int deliveryNoteId);
        public Task<DeliveryNoteDetail> getDeliveryNoteByIdAsync(int id);
        public Task<ExportDto> UpdateStatusDeliveryNoteAsync(UpdateStatusDto input);

    }
}
