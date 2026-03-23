(function ($) {
    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#waybillCreate'),
        _$form = _$modal.find('form');

    $('.select2').select2({
        theme: "bootstrap4"
    });

    var currentCustomerId = _$form.find('[name="CurrentCustomerId"]').val(); // Lấy giá trị currentCustomerId từ input ẩn
    if (currentCustomerId) {
        $('#select-customer').val(currentCustomerId).trigger('change'); // Đặt giá trị và kích hoạt sự kiện change
    }
    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var customerId = _$form.find('[name="CustomerId"]').val();

        var waybillNumbers = _$form.find('[name="WaybillCodes"]').val()
            .replace(/[\s\r\n]+/g, ',')        // Thay khoảng trắng và xuống dòng bằng dấu phẩy
            .replace(/,+/g, ',')               // Gộp nhiều dấu phẩy liên tiếp thành một
            .replace(/^,|,$/g, '');
        _orderService.rematch(waybillNumbers, customerId).done(function (result) {

            if (result && result.success) {
                abp.message.success(result.message);
            }
            else {
                abp.message.error(
                    result ? result.message : 'Xảy ra lỗi, vui lòng kiểm tra lại'
                );
            }
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });



})(jQuery);