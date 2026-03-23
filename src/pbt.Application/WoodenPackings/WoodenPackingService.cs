using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using pbt.ApplicationUtils;
using pbt.Commons.Dto;
using pbt.ConfigurationSettings;
using pbt.Entities;
using pbt.WoodenPackings.Dto;

namespace pbt.WoodenPackings;

public class WoodenPackingService: AsyncCrudAppService<WoodenPacking, WoodenPackingDto, long, PagedResultRequestDto,WoodenPackingDto, WoodenPackingDto> , IWoodenPackingService
{
    public readonly IRepository<Package> _packageRepository; 
    public readonly IRepository<Order, long> _orderRepository; 
    private readonly IConfigurationSettingAppService _configurationSettingAppService;
    
    public WoodenPackingService(
        IRepository<WoodenPacking, long> repository
        , IRepository<Package> packageRepository,
        IConfigurationSettingAppService configurationSettingAppService,
        IRepository<Order, long> orderRepository
        )
        : base(repository)
    {
        _packageRepository = packageRepository;
        _configurationSettingAppService = configurationSettingAppService;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Tạo mới đóng bao chung
    /// </summary>
    /// <param name="createWoodenPacking"></param>
    /// <param name="customerCode"></param>
    /// <param name="orderCode"></param>
    /// <returns></returns>
    public async Task<Package> CreateWoodenPacking(CreateWoodenPackingDto createWoodenPacking, string customerCode, string orderCode)
    {
        var totalWeight = createWoodenPacking.Package.Weight ?? 0;
        if (createWoodenPacking.Package.WoodenPackingId.HasValue)
        {
            createWoodenPacking.Package.WoodenPackagingFee = await UpdatePackageWoodenCost(
                createWoodenPacking.Package.WoodenPackingId ?? 0,
                createWoodenPacking.packageNumber, createWoodenPacking.Package);
        }
        else
        {
            if (createWoodenPacking.packageNumber != null)
            {
                var lstPackage = (await _packageRepository.GetAllAsync()).Where(x =>
                    createWoodenPacking.packageNumber.Contains(x.PackageNumber)).ToList();
                totalWeight += lstPackage.Sum(x => x.Weight ?? 0);
            }

            var woodenPacking = new WoodenPacking
            {
                WoodenPackingCode = customerCode + "-" + orderCode,
                WeightTotal = totalWeight,
                VolumeTotal = 0,
                CostTotal = await CalcCost(totalWeight)
            };
            var woodenPackingId = await Repository.InsertAndGetIdAsync(woodenPacking);
            createWoodenPacking.Package.WoodenPackagingFee = await UpdatePackageWoodenCost(woodenPackingId,
                createWoodenPacking.packageNumber, createWoodenPacking.Package);
            createWoodenPacking.Package.WoodenPackingId = woodenPackingId;
        }
        return createWoodenPacking.Package;
    }

    /// <summary>
    /// Cập nhật đóng gỗ khi cập nhật kiện
    /// </summary>
    /// <param name="package"></param>
    /// <param name="sharedCrateSelectId"></param>
    /// <param name="customerCode"></param>
    /// <param name="orderCode"></param>
    public async Task<Package> UpdateWoodenPacking(Package package, long? sharedCrateSelectId, string customerCode, string orderCode)
    {
        if (!package.IsWoodenCrate)
        {
            package.WoodenPackingId = null;
            //package.WoodenPackagingFee = await CalcCost(package.Weight ?? 0);
            if (package.WoodenPackingId != null)
            {
                var oldWoodenPackingId = package.WoodenPackingId.Value;
                await UpdatePackageWoodenCost(oldWoodenPackingId, null);
            }
            if (package.WoodenPackingId == null)
            {
                var oldWoodenPackingId = package.WoodenPackingId;
                if (oldWoodenPackingId != null) await UpdatePackageWoodenCost(oldWoodenPackingId ?? 0, null, null, package);
            }
        }
        else if (package.IsWoodenCrate && sharedCrateSelectId == null)
        {
            var oldWoodenPackingId = package.WoodenPackingId;
            package.WoodenPackingId = null;
            package.WoodenPackagingFee = await CalcCost(package.Weight ?? 0);
            if (oldWoodenPackingId != null) await UpdatePackageWoodenCost(oldWoodenPackingId ?? 0, null, null, package);
        }
        // // Add to new shared packing
        // else if (woodenCrateType == 2 && package.WoodenPackingId == null)
        // {
        //     await CreateWoodenPacking(package, customerCode, orderCode);
        //     await _packageRepository.UpdateAsync(package);
        // }
        // Move to new shared packing
        else if (sharedCrateSelectId != null)
        {
            var oldWoodenPackingId = package.WoodenPackingId;
            // Remove from old
            package.WoodenPackingId = sharedCrateSelectId;
            package.IsWoodenCrate = true;
            //await _packageRepository.UpdateAsync(package);
            if (oldWoodenPackingId != null)
            {
                await UpdatePackageWoodenCost(oldWoodenPackingId ?? 0, null, null, package);
            }
            // Add to new
            await UpdatePackageWoodenCost(sharedCrateSelectId ?? 0, null, package);

        }
        return package;
    }
    
    // function update giá đóng gỗ của kiện đóng gỗ chung, dựa trên tỉ lệ cân nặng của kiện trong thùng gỗ
    // tính phí đóng gỗ: 20 tệ/kg đầu tiên. Từ kg tiếp theo 1 tệ.
    /// <summary>
    /// Cập nhật chi phí đóng gỗ cho các kiện hàng liên quan đến WoodenPacking
    /// </summary>
    /// <param name="woodenPackingId"></param>
    /// <param name="packageNumbers"></param>
    /// <param name="newPackage"></param>
    private async Task<decimal> UpdatePackageWoodenCost(long woodenPackingId, [CanBeNull] List<string> packageNumbers, Package newPackage = null, Package oldPackage = null)
    {
        var woodenPacking = await Repository.GetAsync(woodenPackingId);
        List<Package> packages = new List<Package>();
        if (packageNumbers != null)
        {
            packages = (await _packageRepository.GetAllAsync()).Where(p => packageNumbers.Contains(p.PackageNumber)).ToList();
        }
        else
        {
            packages = (await _packageRepository.GetAllAsync()).Where(p => p.WoodenPackingId == woodenPackingId).ToList();
        }
        if (newPackage != null && packages.FindIndex(x => x.Id == newPackage.Id) == -1)
        {
            packages.Add(newPackage);
        }

        if (oldPackage != null && packages.FindIndex(x => x.Id == oldPackage.Id) > -1)
        {
            packages.Remove(oldPackage);
        }
        if (packages.Count == 0) return 0;
        var totalWeight = packages.Sum(p => p.Weight ?? 0);
        if (totalWeight == 0) return 0;
        var woodenCost = await CalcCost(totalWeight);
        foreach (var package in packages)
        {
            var weight = package.Weight ?? 0;
            var packageWoodenCost = woodenCost * weight / totalWeight;
            package.WoodenPackagingFee = packageWoodenCost;
            package.WoodenPackingId = woodenPackingId;
            package.IsWoodenCrate = true;
            if (package.Id > 0)
            {
                _packageRepository.UpdateAsync(package);
            }
            else
            {
                if (newPackage != null) newPackage.WoodenPackagingFee = package.WoodenPackagingFee;
            }

            if (package.OrderId != null && package.OrderId > 0)
            {
                var _order =  await _orderRepository.GetAsync(package.OrderId.Value);
                _order.UseWoodenPackaging = true;
                _order.WoodenPackagingFee =packageWoodenCost;
                _orderRepository.UpdateAsync(_order);
            }
        }
        woodenPacking.CostTotal = woodenCost;
        woodenPacking.WeightTotal = totalWeight;
        Repository.UpdateAsync(woodenPacking);
        return newPackage?.WoodenPackagingFee ?? 0;
    }

    public async Task<List<OptionItemDto>> GetAllForSelectAsync(string q = "")
    {
        try
        {
            var packages = await _packageRepository.GetAllListAsync(p =>
                p.WoodenPackingId != null && p.ShippingStatus > (int)PackageDeliveryStatusEnum.WaitingForShipping);

            var woodenPackingIds = packages.Select(p => p.WoodenPackingId.Value).Distinct().ToList();

            var query = (await Repository.GetAllAsync()).Where(u => woodenPackingIds.Contains(u.Id));

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(u =>
                    u.WoodenPackingCode.Contains(q));
            }

            return await query.Select(u => new OptionItemDto
            {
                id = u.Id.ToString(),
                text = u.WoodenPackingCode + " - " + u.WeightTotal + " (kg)"
            }).ToListAsync();
        }
        catch (Exception ex)
        {
            // ignored
        }

        return new List<OptionItemDto>();
    }
    
    /// <summary>
    /// Tính chi phí đóng gỗ dựa trên cân nặng
    /// </summary>
    /// <param name="weight"></param>
    /// <returns></returns>
    public async Task<decimal> CalcCost(decimal weight)
    {
        decimal cost = 0;
        string key = "ExchangeRateRMB";
        var rsString = await _configurationSettingAppService.GetValueAsync(key);
        long rs = 1;
        if (long.TryParse(rsString, out long result))
        {
            rs = result;
        }

        if (weight <= 1)
        {
            cost = 20;
        }
        else
        {
            cost = 20 + (weight - 1) * 1;
        }

        return cost * rs;
    }
    
}