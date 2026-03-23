(function ($) {
    var _shippingCostService = abp.services.app.shippingCostGroup;

    // Thêm cấu hình giá mới
    $(document).on('click', '.add-group', function () {
        var shippingType = $('#select-line').val();
        var fromWarehouse = $('#select-from-warehouse').val();
        var toWarehouse = $('#select-to-warehouse').val();

        if (shippingType === '-1' || fromWarehouse === '-1' || toWarehouse === '-1') {
            abp.notify.warn('Vui lòng chọn đầy đủ thông tin.');
            return;
        }

        abp.ui.setBusy();
        _shippingCostService.create({
            ShippingTypeId: shippingType,
            WarehouseFromId: fromWarehouse,
            WarehouseToId: toWarehouse
        }).done(function () {
            abp.notify.success('Thêm cấu hình giá thành công.');
            location.reload();
        }).always(function () {
            abp.ui.clearBusy();
        });
    });

    // Xóa cấu hình giá
    // $(document).on('click', '.remove-tier-data', function () {
    //     var id = $(this).data('id');
    //     abp.message.confirm('Bạn có chắc chắn muốn xóa cấu hình này?', function (isConfirmed) {
    //         if (isConfirmed) {
    //             abp.ui.setBusy();
    //             _shippingCostService.delete({ id: id }).done(function () {
    //                 abp.notify.success('Xóa cấu hình giá thành công.');
    //                 location.reload();
    //             }).always(function () {
    //                 abp.ui.clearBusy();
    //             });
    //         }
    //     });
    // });
})(jQuery);