using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pbt.Application.WarehouseTransfers.Dto;
using pbt.Core;
using pbt.Entities;
using pbt.WarehouseTransfers.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace pbt.Application.WarehouseTransfers
{
    public class WarehouseTransferDetailAppService : AsyncCrudAppService<
        WarehouseTransferDetail, // Entity
        WarehouseTransferDetailDto, // DTO chính
        long, // Kiểu dữ liệu của khóa chính
        PagedAndSortedResultRequestDto, // DTO cho phân trang và sắp xếp
        WarehouseTransferDetailDto, // DTO cho tạo mới
        WarehouseTransferDetailDto  // DTO cho cập nhật
    >, IWarehouseTransferDetailAppService
    {
        public WarehouseTransferDetailAppService(IRepository<WarehouseTransferDetail, long> repository)
            : base(repository)
        {
        }



        public async Task<PagedResultDto<WarehouseTransferDetailDto>> GetByWarehouseTransferIdAsync(GetByWarehouseTransferIdDto input)
        {
            if (input != null && input.WarehouseTransferId.HasValue)
            {
                var details = await Repository.GetAllListAsync(d => d.WarehouseTransferId == input.WarehouseTransferId.Value);
                var data = ObjectMapper.Map<List<WarehouseTransferDetailDto>>(details);

                return new PagedResultDto<WarehouseTransferDetailDto>()
                {
                    TotalCount = data.Count,
                    Items = data
                };
            }
            else
            {
                return new PagedResultDto<WarehouseTransferDetailDto>()
                {
                    TotalCount = 0,
                    Items = new List<WarehouseTransferDetailDto>()
                };
            }

        }

        public async Task<List<WarehouseTransferDetailDto>> GetDetailsByWarehouseTransferIdAsync(int warehouseTransferId)
        {
            var details = await Repository.GetAllListAsync(d => d.WarehouseTransferId == warehouseTransferId);
            return ObjectMapper.Map<List<WarehouseTransferDetailDto>>(details);
        }

        public async Task<JsonResult> RemoveItem(int id)
        {
            try
            {
                await ConnectDb.ExecuteNonQueryAsync("SP_WarehouseTransferDetails_RemoveItem", CommandType.StoredProcedure, new[]
                {
                  new SqlParameter("@id", SqlDbType.Int) { Value = id },
                });
                return new JsonResult(new
                {
                    Success = true,
                    Message = "Xóa phần tử khỏi phiếu chuyển kho thành công."
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi xóa phần tử khỏi phiếu chuyển kho: " + ex.Message, ex);
                throw new UserFriendlyException("Đã xảy ra lỗi khi xóa phần tử khỏi phiếu chuyển kho. Vui lòng thử lại.");
            }
        }

    }
}