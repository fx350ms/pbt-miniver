using Abp.Application.Services;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.DeliveryNotes.Dto;
using System.Collections.Generic;

namespace pbt.DeliveryNotes
{
    public interface IDeliveryNoteAppService : IAsyncCrudAppService<Dto.DeliveryNoteDto, int, PagedResultRequestDto, CreateUpdateDeliveryNoteDto, DeliveryNoteDto>
    {
        public Task<ListResultDto<DeliveryNoteDto>> getDeliveryNotesFilter(PagedDeliveryNoteResultRequestDto input);
        public Task<DeliveryNoteItemsDto> GetItemByDeliveryNote(int deliveryNoteId);
        public Task<DeliveryNoteDetail> getDeliveryNoteByIdAsync(int id);
        public Task<DeliveryNoteDto> UpdateStatusDeliveryNoteAsync(UpdateDeliveryNoteStatusDto input);
        public Task<DeliveryNoteDto> GetOrCreateByCustomerIdAsync(long customerId);
        public Task DeleteDeliveryNoteAsync(int deliveryNoteId, int deliveryRequestId);
        Task<List<DeliveryNoteExportViewDto>> GetDeliveryNotesByExportView(DeliveryNoteExportViewInputDto input);
        Task<DeliveryNoteDto> GetWithCreatorInfoAsync(EntityDto<int> input);
    }
}
