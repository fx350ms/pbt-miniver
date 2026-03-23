(function ($) {
    var _shippingCostService = abp.services.app.shippingCostGroup,
         _shippingCostBaseService = abp.services.app.shippingCostBase,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#editShippingCost'),
        _$form = $("[name='ShippingCostEditForm']")

    // Lưu cấu hình giá vốn
    _$form.find('.save-button').on('click', function (e) {
        e.preventDefault();
        $('[readonly]').prop( "disabled", false );
        var shippingCosts = [];
        var shippingCostGroupId = $('#ShippingCostGroupId').val();

        // Thu thập dữ liệu từ các nhóm giá
        $('.rate-item').each(function () {
            var $rateItem = $(this);
            var rateId = $rateItem.data('id') || 0;
            var shippingCost = {
                Id: rateId,
                ShippingCostGroupId: shippingCostGroupId,
                ShippingTypeId: $rateItem.data('line'),
                WarehouseFromId: $rateItem.data('from'),
                WarehouseToId: $rateItem.data('to'),
                Tiers: []
            };

            // Thu thập dữ liệu từ các tier
            $rateItem.find('tbody tr').each(function () {
                var $row = $(this);
                var tier = {
                    Id: $row.data('id') || 0,
                    FromValue: $row.find('input[name="FromValue"]').val(),
                    ToValue: $row.find('input[name="ToValue"]').val(),
                    Unit: $row.find('select').val(),
                    PricePerUnit: $row.find('input[name="PricePerUnit"]').val().replace(/,/g, ''),
                    ShippingCostId: rateId
                };
                shippingCost.Tiers.push(tier);
            });

            shippingCosts.push(shippingCost);
        });

        var data = {
            ShippingCostGroupId: shippingCostGroupId,
            ShippingCosts: shippingCosts
        };
        
        abp.ui.setBusy(_$modal);
        _shippingCostService.saveShippingCosts(data).done(function () {
            _$modal.modal('hide');
            // window.location.reload()
            //_$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    // Xóa nhóm giá
    // $(document).on('click', '.remove-tier-data', function () {
    //     var id = $(this).data('id');
    //     abp.message.confirm(
    //         l('AreYouSureWantToDelete'),
    //         null,
    //         function (isConfirmed) {
    //             if (isConfirmed) {
    //                 abp.ui.setBusy();
    //                 _shippingCostService.delete({ id: id }).done(function () {
    //                     abp.notify.info(l('SuccessfullyDeleted'));
    //                     location.reload();
    //                 }).always(function () {
    //                     abp.ui.clearBusy();
    //                 });
    //             }
    //         }
    //     );
    // });

    $(document).on('click', '.remove-tier-data', function (e) {
        e.preventDefault();
        // Get the current row
        const currentDataId = $(this).data('id');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                ''),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    debugger
                    _shippingCostBaseService.deleteShippingCostBase(currentDataId).done(() => {
                        // Tìm đến thẻ div gần nhất có class là rate-item sau đó xóa đi
                        $(this).closest('.rate-item').remove();
                        abp.notify.info(l('SuccessfullyDeleted'));
                    });
                }
            }
        );

    });

    // Thêm nhóm giá mới
    $(document).on('click', '.add-group', function () {
        var shippingType = $('#select-line').val();
        var lineName = $('#select-line option:selected').text();
        var fromWarehouse = $('#select-from-warehouse').val();
        var fromWarehouseName = $('#select-from-warehouse option:selected').text();
        var toWarehouse = $('#select-to-warehouse').val();
        var toWarehouseName = $('#select-to-warehouse option:selected').text();

        if (shippingType === "-1" || fromWarehouse === "-1" || toWarehouse === "-1") {
            abp.notify.warn('Vui lòng chọn đầy đủ thông tin.');
            return;
        }

        // Kiểm tra xem nhóm đã tồn tại chưa
        var isDuplicate = false;
        $('.rate-item').each(function () {
            var line = $(this).data('line');
            var from = $(this).data('from');
            var to = $(this).data('to');

            if (line == shippingType && from == fromWarehouse && to == toWarehouse) {
                isDuplicate = true;
                return false; // Thoát khỏi vòng lặp
            }
        });

        if (isDuplicate) {
            abp.notify.error('Nhóm này đã tồn tại!');
            return;
        }

        abp.ajax({
            url: abp.appPath + 'ShippingCosts/RateTierContentItem',
            type: 'GET',
            dataType: 'html',
            data: {
                shippingType: shippingType,
                lineName: lineName,
                fromWarehouse: fromWarehouse,
                fromName: fromWarehouseName,
                toWarehouse: toWarehouse,
                toName: toWarehouseName,
            },
            success: function (content) {
                
                $('.rate-list').append(content);
                abp.notify.info('Nhóm giá đã được thêm thành công.');
            },
            error: function (xhr, status, error) {
                
                console.log('Error details:', xhr, status, error);
                abp.notify.error('Có lỗi xảy ra khi thêm nhóm giá.');
            }
        });
    });

$(document).on('click', '.add-tier-row', function (e) {
    e.preventDefault();
    // Get the current table body
    const $tbody = $(this).closest('tbody');
    // Check if there are already 2 or more rows
    if ($tbody.find('tr').length >= 2) {
        abp.notify.warn('Maximum 2 rows allowed.');
        return;
    }
    
    var $currentRow = $(this).closest('tr');
    var $newRow = $currentRow.clone();
    $newRow.find('.add-tier-row')
        .removeClass('add-tier-row btn-success')
        .addClass('remove-tier-row btn-danger')
        .attr('title', 'Xóa hàng')
        .html('<i class="fa fa-trash"></i>');
    $newRow.attr('data-id', 0);
    debugger
    $newRow.find("select[name='Unit']").html("<option value=\"M3\">M3</option>")
    $currentRow.after($newRow);
});


    $(document).on('click', '.remove-tier-row', function (e) {
        e.preventDefault();

        // Get the current row
        var $currentRow = $(this).closest('tr');
        // Get the row-data-id of the current row
        var rowDataId = $currentRow.attr('row-data-id');
        // Find the <td> element with the matching row-data-id
        var $firstColumn = $('td[row-data-id="' + rowDataId + '"]');
        // Remove the current row
        $currentRow.remove();
    });

})(jQuery);