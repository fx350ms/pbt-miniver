using pbt.Departments.Dto;
using pbt.Warehouses.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.Web.Models.Warehouses
{
    public class CreateUpdateWarehouse
    {
        public WarehouseDto Warehouse { get; set; }
    }
}
