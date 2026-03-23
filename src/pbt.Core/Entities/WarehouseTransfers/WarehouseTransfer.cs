using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace pbt.Entities;

public class WarehouseTransfer : FullAuditedEntity<int>
{
    public string TransferCode { get; set; }
    public int FromWarehouse { get; set; }
    public string FromWarehouseName { get; set; }
    public string FromWarehouseAddress { get; set; }
    public int ToWarehouse { get; set; }
    public string ToWarehouseName { get; set; }
    public string ToWarehouseAddress { get; set; }
    public string Note { get; set; }

    public string Data { get; set; }
    public int Status { get; set; }

    public decimal ShippingFee { get; set; }
    public long CustomerId { get; set; }
    public long TotalQuantity { get; set; }
    public decimal TotalWeight { get; set; }
     
}


public class WarehouseTransferDetail : Entity<long>
{
    public int WarehouseTransferId { get; set; }
    public int ItemId { get; set; }
    public string PackageCode { get; set; }
    public string BagNumber { get; set; }
    public int ItemType { get; set; } // 1: Kiện, 2: Bao
    public decimal Weight { get; set; }
}
