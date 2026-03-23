using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Castle.Core.Logging;
using MathNet.Numerics.Distributions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using pbt.ApplicationUtils;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace pbt.ShippingRates;

[UnitOfWork(false)]
public class ShippingCostAppService : IShippingCostAppService
{
    private readonly IRepository<ShippingRateCustomer, long> _shippingRateCustomerRepository;
    private readonly IRepository<ShippingRate, long> _shippingRateRepository;
    private readonly IRepository<ShippingRateTier, long> _shippingRateTierRepository;
    private readonly IRepository<ProductGroupType, int> _productGroupTypeRepository;
    private readonly ILogger<ShippingCostAppService> _logger;
    private readonly IConfigurationSettingAppService _configurationSettingAppService;
    private readonly IRepository<ShippingCostBase, long> _shippingCostBaseRepository;
    private readonly IRepository<ShippingCostGroup, long> _shippingCostGroupRepository;
    private readonly IRepository<ShippingCostTier, long> _shippingCostTierRepository;
    private readonly IRepository<Package> _packageRepository;
    private readonly IRepository<Bag> _bagRepository;

    public ShippingCostAppService(
        ILogger<ShippingCostAppService> logger,
        IRepository<ShippingRateCustomer, long> shippingRateCustomerRepository,
        IRepository<ShippingRate, long> shippingRateRepository,
        IRepository<ShippingRateTier, long> shippingRateTierRepository,
        IConfigurationSettingAppService configurationSettingAppService,
        IRepository<ProductGroupType, int> productGroupTypeRepository,
        IRepository<ShippingCostBase, long> shippingCostBaseRepository,
        IRepository<ShippingCostGroup, long> shippingCostGroupRepository,
        IRepository<ShippingCostTier, long> shippingCostTierRepository,

        IRepository<Package> packageRepository,
        IRepository<Bag> bagRepository)
    {
        _logger = logger;
        _shippingRateCustomerRepository = shippingRateCustomerRepository;
        _shippingRateRepository = shippingRateRepository;
        _shippingRateTierRepository = shippingRateTierRepository;
        _productGroupTypeRepository = productGroupTypeRepository;
        _shippingCostBaseRepository = shippingCostBaseRepository;
        _shippingCostGroupRepository = shippingCostGroupRepository;
        _shippingCostTierRepository = shippingCostTierRepository;
        _packageRepository = packageRepository;
        _bagRepository = bagRepository;
        _configurationSettingAppService = configurationSettingAppService;
    }


    /// <inheritdoc/>
    public async Task<ShippingCostResult> GetShippingFee(ShippingCostDto input)
    {
        // 1. Xác định nhóm bảng giá cho khách hàng
        long shippingRateGroupId = 0;

        if (input.ShippingLineId == (int)LineShipping.InformalTrade
            || input.ShippingLineId == (int)LineShipping.OfficialChannel
            || input.ShippingLineId == (int)LineShipping.Portable) // Hàng lô, chinh ngach, xach tay
        {
            var shippingRateCustomer = await _shippingRateCustomerRepository.GetAll()
                .Where(x => x.CustomerId == input.CustomerId)
                .FirstOrDefaultAsync();

            if (shippingRateCustomer == null)
            {
                // Lấy nhóm giá mặc định
                shippingRateCustomer = await _shippingRateCustomerRepository.GetAll()
                    .Include(x => x.ShippingRateGroup)
                    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                    .FirstOrDefaultAsync();
            }

            if (shippingRateCustomer == null)
                throw new UserFriendlyException("Không tìm thấy nhóm bảng giá cho khách hàng.");

            shippingRateGroupId = shippingRateCustomer.ShippingRateGroupId;
        }
        else if (input.ShippingLineId == (int)LineShipping.Ecommerce) // Thương mại điện tử
        {
            // if (input.ProductGroupTypeId == ProductMiscellaneousConstants.ProductMiscellaneousId)
            // {
            //     var defaultProductGroupType = await _productGroupTypeRepository.FirstOrDefaultAsync(x => x.IsDefault);
            //     input.ProductGroupTypeId = defaultProductGroupType.Id;
            // }
            var shippingRateCustomer = await _shippingRateCustomerRepository.GetAll()
                .Include(x => x.ShippingRateGroup)
                .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                .FirstOrDefaultAsync();

            if (shippingRateCustomer == null)
                throw new UserFriendlyException("Không tìm thấy nhóm bảng giá mặc định cho khách hàng TMĐT.");

            shippingRateGroupId = shippingRateCustomer.ShippingRateGroupId;
        }
        else
        {
            throw new UserFriendlyException("Line vận chuyển không hợp lệ.");
        }

        // 2. Tìm tuyến giá phù hợp
        var shippingRate = await _shippingRateRepository.GetAll()
            .Where(x => x.WarehouseFromId == input.CNWarehouseId)
            .Where(x => x.WarehouseToId == input.VNWarehouseId)
            .Where(x => x.ShippingTypeId == input.ShippingLineId)
            .Where(x => x.ShippingRateGroupId == shippingRateGroupId)
            .FirstOrDefaultAsync();

        if (shippingRate == null)
            throw new UserFriendlyException("Không tìm thấy tuyến giá phù hợp với kho, line và nhóm giá.");

        // 3. Lấy các bậc giá theo nhóm hàng
        var shippingRateTiers = await _shippingRateTierRepository.GetAll()
            .Where(x => x.ShippingRateId == shippingRate.Id && x.ProductTypeId == input.ProductGroupTypeId)
            .ToListAsync();

        // 4. Tính giá theo cân nặng (KG)
        var shippingRateTierWeight = shippingRateTiers
            .Where(x => x.Unit == "KG")
            .Where(x => x.FromValue <= input.Weight)
            .OrderByDescending(x => x.FromValue)
            .FirstOrDefault();

        decimal? costWeight = 0;
        if (shippingRateTierWeight != null)
        {
            costWeight = shippingRateTierWeight.PricePerUnit * (input.Weight ?? 0);
        }

        // 5. Tính giá theo thể tích (M3)
        decimal? costDimension = 0;
        var shippingRateTierDimension = shippingRateTiers
            .Where(x => x.Unit == "M3")
            .Where(x => x.FromValue <= (input.Dimension ?? 0))
            .OrderByDescending(x => x.FromValue)
            .FirstOrDefault();

        if (shippingRateTierDimension != null)
        {
            costDimension = shippingRateTierDimension.PricePerUnit * (input.Dimension ?? 0);
        }

        // 6. Lấy giá lớn hơn
        var result = new ShippingCostResult();
        if ((costWeight ?? 0) > (costDimension ?? 0))
        {
            result.ShippingFee = (decimal)(costWeight ?? 0);
            result.PricePerUnit = shippingRateTierWeight?.PricePerUnit ?? 0;
            result.UnitType = 1; // Theo cân nặng
        }
        else
        {
            result.ShippingFee = (decimal)(costDimension ?? 0);
            result.PricePerUnit = shippingRateTierDimension?.PricePerUnit ?? 0;
            result.UnitType = 2; // Theo thể tích
        }

        return result;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="shippingCost"></param>
    /// <returns></returns>
    public async Task<ShippingCostResult> CalculateShippingCostAsync(ShippingCostDto shippingCost)
    {
        var result = new ShippingCostResult();
        double shippingFee = 0;
        long ShippingRateGroupId = 0;
        // hàng lô
        if (shippingCost.ShippingLineId == (int)LineShipping.InformalTrade
            || shippingCost.ShippingLineId == (int)LineShipping.OfficialChannel
            || shippingCost.ShippingLineId == (int)LineShipping.Portable
           )
        {
            var shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                .Where(x => x.CustomerId == shippingCost.CustomerId)
                .FirstOrDefaultAsync();
            if (shippingRateCustomers == null)
            {
                shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                    .Include(x => x.ShippingRateGroup)
                    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                    .FirstOrDefaultAsync();
            }

            ShippingRateGroupId = shippingRateCustomers.ShippingRateGroupId;
        }
        // thương mại điện tử
        else
        {
            // if (shippingCost.ProductGroupTypeId == ProductMiscellaneousConstants.ProductMiscellaneousId)
            // {
            //     var defaultProductGroupType = await _productGroupTypeRepository.FirstOrDefaultAsync(x => x.IsDefault);
            //     shippingCost.ProductGroupTypeId = defaultProductGroupType.Id;
            // }

            var shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                .Where(x => x.CustomerId == shippingCost.CustomerId)
                .FirstOrDefaultAsync();
            if (shippingRateCustomers == null)
            {
                shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                    .Include(x => x.ShippingRateGroup)
                    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                    .FirstOrDefaultAsync();
            }
            //var shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
            //    .Include(x => x.ShippingRateGroup)

            //    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
            //    .FirstOrDefaultAsync();
            ShippingRateGroupId = shippingRateCustomers.ShippingRateGroupId;
        }

        var shippingRate = (await _shippingRateRepository.GetAllAsync())
            .Where(x => x.WarehouseFromId == shippingCost.CNWarehouseId)
            .Where(x => x.WarehouseToId == shippingCost.VNWarehouseId)
            .Where(x => x.ShippingTypeId == shippingCost.ShippingLineId)
            .Where(x => x.ShippingRateGroupId == ShippingRateGroupId)
            .FirstOrDefault();
        if (shippingRate == null)
        {
            throw new UserFriendlyException("Khách hàng hoặc kho chưa được cấu hình giá.");
            //throw new InvalidOperationException("No shipping rate found for the given criteria.");
        }

        var shippingRateTiers = await _shippingRateTierRepository.GetAll()
            .Where(x => x.ShippingRateId == shippingRate.Id &&
                        x.ProductTypeId == shippingCost.ProductGroupTypeId)
            .ToListAsync();
        var shippingRateTierWeight = shippingRateTiers
            .Where(x => x.Unit == "KG")
            .Where(x => x.FromValue <= shippingCost.Weight)
            .OrderByDescending(x => x.FromValue)
            .FirstOrDefault();
        decimal? costWeight = 0;
        if (shippingRateTierWeight != null)
        {
            costWeight = shippingRateTierWeight.PricePerUnit * shippingCost.Weight;
        }

        decimal? costDimension = 0;
        ShippingRateTier shippingRateTierDimension = null;
        if (shippingCost.Dimension != null && shippingCost.Dimension > 0)
        {
            shippingRateTierDimension = shippingRateTiers
                .Where(x => x.Unit == "M3")
                .Where(x => x.FromValue <= shippingCost.Dimension)
                .OrderByDescending(x => x.FromValue)
                .FirstOrDefault();
            if (shippingRateTierDimension != null)
            {
                costDimension = shippingRateTierDimension.PricePerUnit * shippingCost.Dimension;
            }
        }

        if (costWeight > costDimension)
        {
            result.ShippingFee = (decimal)costWeight;
            if (shippingRateTierWeight != null) result.PricePerUnit = shippingRateTierWeight.PricePerUnit;
            result.UnitType = 1;
        }
        else
        {
            result.ShippingFee = (decimal)costDimension;
            if (shippingRateTierDimension != null)
                result.PricePerUnit = shippingRateTierDimension.PricePerUnit;
            result.UnitType = 2;
        }

        // }
        return result;
    }


    public async Task<ShippingCostResult> CalculateShippingCostAsync(Package package)
    {
        var result = new ShippingCostResult();
        double shippingFee = 0;
        long ShippingRateGroupId = 0;

        // tính bảo hiểm
        decimal insuranceValue = 0;

        string key = "ExchangeRateRMB";
        var rsString = await _configurationSettingAppService.GetValueAsync(key);
        long rs = 1;
        if (long.TryParse(rsString, out long _result))
        {
            rs = _result;
        }

        // tính phí đóng gỗ: 20 tệ/kg đầu tiên. Từ kg tiếp theo 1 tệ.
        decimal? woodenFee = 0;
        if (package.IsWoodenCrate)
        {
            woodenFee = package.Weight >= 1 ? 20 + (package.Weight - 1) : 20;
        }

        if (woodenFee > 0)
        {
            woodenFee = (woodenFee * rs);
        }

        // phí quấn bọt khí: 8 tệ/kg đầu tiên. Từ kg tiếp theo 1 tệ.
        decimal shockproofFee = 0;
        if (package.IsShockproof)
        {
            shockproofFee = (decimal)(package.Weight >= 1 ? 8 + (package.Weight - 1) : 8);
        }

        shockproofFee = shockproofFee * rs;
        var domesticShippingFeeCN = (package.DomesticShippingFeeRMB);
        var domesticShippingFee = (package.DomesticShippingFeeRMB) * rs;
        // hàng lô
        if (package.ShippingLineId == (int)LineShipping.InformalTrade
            || package.ShippingLineId == (int)LineShipping.OfficialChannel
            || package.ShippingLineId == (int)LineShipping.Portable
           )
        {
            var shippingRateCustomers = await _shippingRateCustomerRepository.FirstOrDefaultAsync(x => x.CustomerId == package.CustomerId);
                
            if (shippingRateCustomers == null)
            {
                shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                    .Include(x => x.ShippingRateGroup)
                    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                    .FirstOrDefaultAsync();
            }

            ShippingRateGroupId = shippingRateCustomers.ShippingRateGroupId;
        }
        // thương mại điện tử
        else
        {
            var shippingRateCustomers = await _shippingRateCustomerRepository.FirstOrDefaultAsync(x => x.CustomerId == package.CustomerId);
                
            if (shippingRateCustomers == null)
            {
                shippingRateCustomers = await _shippingRateCustomerRepository.GetAll()
                    .Include(x => x.ShippingRateGroup)
                    .Where(x => x.ShippingRateGroup.IsDefaultForCustomer)
                    .FirstOrDefaultAsync();
            }

            ShippingRateGroupId = shippingRateCustomers.ShippingRateGroupId;
        }

        var shippingRate = await _shippingRateRepository.FirstOrDefaultAsync(
            x => x.WarehouseFromId == package.WarehouseCreateId
            && x.WarehouseToId == package.WarehouseDestinationId
            && x.ShippingTypeId == package.ShippingLineId
            && x.ShippingRateGroupId == ShippingRateGroupId
            );
            
        if (shippingRate == null)
        {
            throw new UserFriendlyException("Khách hàng hoặc kho chưa được cấu hình giá.");
            //throw new InvalidOperationException("No shipping rate found for the given criteria.");
        }

        var shippingRateTiers = await _shippingRateTierRepository.GetAllListAsync
                    (x => x.ShippingRateId == shippingRate.Id 
                    && x.ProductTypeId == package.ProductGroupTypeId);
          
        var shippingRateTierWeight = shippingRateTiers
            .Where(x => x.Unit == "KG")
            .Where(x => x.FromValue <= package.Weight)
            .OrderByDescending(x => x.FromValue)
            .FirstOrDefault();
        decimal? costWeight = 0;
        if (shippingRateTierWeight != null)
        {
            costWeight = shippingRateTierWeight.PricePerUnit * package.Weight;
        }

        decimal? costDimension = 0;
        ShippingRateTier shippingRateTierDimension = null;

        var dimension = package.Length * package.Width * package.Height / 1000000;

        if (dimension != null && dimension > 0)
        {
            shippingRateTierDimension = shippingRateTiers
                .Where(x => x.Unit == "M3")
                .Where(x => x.FromValue <= dimension)
                .OrderByDescending(x => x.FromValue)
                .FirstOrDefault();
            if (shippingRateTierDimension != null)
            {
                costDimension = shippingRateTierDimension.PricePerUnit * dimension;
            }
        }

        if (costWeight > costDimension)
        {
            result.ShippingFee = (decimal)costWeight;
            if (shippingRateTierWeight != null) result.PricePerUnit = shippingRateTierWeight.PricePerUnit;
            result.UnitType = 1;
        }
        else
        {
            result.ShippingFee = (decimal)costDimension;
            if (shippingRateTierDimension != null)
                result.PricePerUnit = shippingRateTierDimension.PricePerUnit;
            result.UnitType = 2;
        }

        result.InsuranceValue = insuranceValue;
        result.WoodenFee = (decimal)woodenFee;
        result.ShockproofFee = shockproofFee;
        result.DomesticShippingFee = domesticShippingFee;
        result.DomesticShippingFeeCN = domesticShippingFeeCN;
        result.Rs = rs;
        result.PackagePrice = insuranceValue + (decimal)woodenFee + shockproofFee + domesticShippingFee +
                              (decimal)result.ShippingFee;
        return result;
    }

   

    // tính tổng chi phí gốc của bao

    /// <summary>
    /// Calculates the original cost of a bag.
    /// </summary>
    /// <param name="bag"></param>
    public async Task<Bag> CalcOriginBagShippingCost(Bag bag)
    {
        try
        {
            var packages = await (await _packageRepository.GetAllAsync())
                .Where(x => x.BagId == bag.Id)
                .ToListAsync();
            decimal totalOriginPackageShippingCost = 0;
            foreach (var package in packages)
            {
                int volum = (package.Height ?? 0 * package.Width ?? 0 * package.Length ?? 0) / 1000000;
                package.OriginShippingCost = await CalcOriginalCost(bag.ShippingPartnerId, package.ShippingLineId ?? 0,
                    package.WarehouseCreateId ?? 0, package.WarehouseDestinationId ?? 0, (package.Weight ?? 0), volum, package.UnitType);
                totalOriginPackageShippingCost += package.OriginShippingCost ?? 0;
                await _packageRepository.UpdateAsync(package);
            }
            bag.TotalOriginPackageShippingCost = totalOriginPackageShippingCost;
            var coverCost = await CalcOriginalCost(bag.ShippingPartnerId, bag.ShippingType,
                bag.WarehouseCreateId ?? 0, bag.WarehouseDestinationId ?? 0, (bag.WeightCover ?? 0), (bag.Volume ?? 0), 1);

            var bagCost = await CalcOriginalCost(bag.ShippingPartnerId, bag.ShippingType,
                bag.WarehouseCreateId ?? 0, bag.WarehouseDestinationId ?? 0, (bag.Weight ?? 0), bag.Volume ?? 0, 1);

            bag.TotalOriginShippingCost = bagCost + coverCost;
            if (bag.Id > 0)
            {
                //  await _bagRepository.UpdateAsync(bag);
            }
            return bag;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    public async Task<Bag> CalcOriginBagShippingCost(int bagId)
    {
        try
        {
            var bag = await _bagRepository.GetAsync(bagId);
            await CalcOriginBagShippingCost(bag);
            return bag;
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }

    /// <summary>
    /// Calculates the original cost.
    /// </summary>
    /// <returns></returns>
    public async Task<decimal> CalcOriginalCost(int shippingPartnerId, int shippingTypeId, long fromWarehouseId,
        long toWarehouseId, decimal weight, decimal volume, int type)
    {
        try
        {
            var now = DateTime.Now;
            var shippingCostGroup = await _shippingCostGroupRepository
                .FirstOrDefaultAsync(x => x.ShippingPartnerId == shippingPartnerId
                            && x.IsActived
                            && x.FromDate <= now
                            && x.ToDate >= now);
                 
            if (shippingCostGroup == null) return 0;
            var shippingCostBase = await _shippingCostBaseRepository.FirstOrDefaultAsync(x =>
                x.ShippingCostGroupId == shippingCostGroup.Id
                && x.WarehouseFromId == fromWarehouseId
                && x.WarehouseToId == toWarehouseId
                && x.ShippingTypeId == shippingTypeId);
            if (shippingCostBase == null) return 0;
            var unit = type == 1 ? "KG" : "M3";
            var tier = await _shippingCostTierRepository
                .FirstOrDefaultAsync(x => x.ShippingCostBaseId == shippingCostBase.Id && x.Unit == unit);
            if (tier == null) return 0;
            return type == 1 ? tier.PricePerUnit * weight : tier.PricePerUnit * volume;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalcOriginalCost error");
            return 0;
        }
    }

}