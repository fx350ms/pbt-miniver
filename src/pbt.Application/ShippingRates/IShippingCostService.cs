using System.Threading.Tasks;
using pbt.Entities;
using pbt.ShippingRates.Dto;

namespace pbt.ShippingRates;

public interface IShippingCostAppService
{
    /// <summary>
    /// Calculates the shipping cost based on the provided shipping cost details.
    /// </summary>
    /// <param name="shippingCost"></param>
    /// <returns></returns>
    public Task<ShippingCostResult> CalculateShippingCostAsync(ShippingCostDto shippingCost);
    public Task<ShippingCostResult> CalculateShippingCostAsync(Package package);

    Task<ShippingCostResult> GetShippingFee(ShippingCostDto input);

    /// <summary>
    /// Calculates the original shipping cost.
    /// </summary>
    /// <param name="shippingPartnerId"></param>
    /// <param name="shippingTypeId"></param>
    /// <param name="fromWarehouseId"></param>
    /// <param name="toWarehouseId"></param>
    /// <param name="weight"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<decimal> CalcOriginalCost(int shippingPartnerId, int shippingTypeId, long fromWarehouseId,
        long toWarehouseId, decimal weight, decimal volum, int type);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bag"></param>
    Task<Bag> CalcOriginBagShippingCost(Bag bag);


    Task<Bag> CalcOriginBagShippingCost(int bagId);

}