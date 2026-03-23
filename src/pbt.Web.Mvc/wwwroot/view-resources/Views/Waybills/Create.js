(function ($) {
    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#waybillCreate'),
        _$form = _$modal.find('form');


    $('.select2').select2({
        theme: "bootstrap4"
    });

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var waybill = _$form.serializeFormToObject();
        waybill.WaybillCodes = waybill.WaybillCodes
            .replace(/[\s\r\n]+/g, ',')        // Thay khoảng trắng và xuống dòng bằng dấu phẩy
            .replace(/,+/g, ',')               // Gộp nhiều dấu phẩy liên tiếp thành một
            .replace(/^,|,$/g, '');            // Xoá dấu phẩy ở đầu hoặc cuối

        abp.ui.setBusy(_$modal);
        _orderService.createList(waybill).done(function (result) {
            $('[name="WaybillCodes"]').val('');
            $('[name="Note"]').val('');
            if (result.successCount > 0) {
                abp.message.success(result.message);
            }
            else {
                abp.message.error(
                    result.message, l('InsertResult')
                );
            }
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });



})(jQuery);