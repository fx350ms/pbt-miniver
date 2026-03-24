(function ($) {
    var _bagService = abp.services.app.bag,
        _shippingRate = abp.services.app.shippingRate,
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        //_$modal = $('#form-edit-package-finance'),
        //_$form = _$modal.find('form')
        _$form = $('#form-edit-package-finance');


    var autoNumericFields = {};

    $(document).find('.mask-number').each(function (c) {
        var fieldName = $(this).attr('name');

        autoNumericFields[fieldName] = new AutoNumeric(this, {
            currencySymbol: '',
            decimalCharacter: '.',
            digitGroupSeparator: ',',
            decimalPlaces: 0,
            minimumValue: '0'
        });
    });

    $(document).find('.mask-number-2').each(function (c) {
        var fieldName = $(this).attr('name');
        autoNumericFields[fieldName] = new AutoNumeric(this, {
            currencySymbol: '',
            decimalCharacter: '.',
            digitGroupSeparator: ',',
            decimalPlaces: 2,
            minimumValue: '0'
        });
    });

    $(document).find('.mask-number-3').each(function (c) {
        var fieldName = $(this).attr('name');
        autoNumericFields[fieldName] = new AutoNumeric(this, {
            currencySymbol: '',
            decimalCharacter: '.',
            digitGroupSeparator: ',',
            decimalPlaces: 3,
            minimumValue: '0'
        });
    });

    // Bắt sự kiện khi giá trị của DomesticShippingFeeRMB thay đổi
    $('[name="DomesticShippingFeeRMB"]').on('input', function () {
        var rmbValue = autoNumericFields['RMB'].get() || 0; // Lấy giá trị RMB
        var domesticShippingFeeRMB = autoNumericFields['DomesticShippingFeeRMB'].get() || 0; // Lấy giá trị DomesticShippingFeeRMB
        // Tính toán DomesticShippingFee
        var domesticShippingFee = rmbValue * domesticShippingFeeRMB;
        // Gán giá trị vào trường DomesticShippingFee
        autoNumericFields['DomesticShippingFee'].set(domesticShippingFee);

        calculateTotalServiceFee();
        calculateTotalFee();

    });

    // Hàm tính toán thể tích
    function calculateVolume() {
        var length = autoNumericFields['Length'].get() || 0; // Lấy giá trị Dài
        var width = autoNumericFields['Width'].get() || 0; // Lấy giá trị Rộng
        var height = autoNumericFields['Height'].get() || 0; // Lấy giá trị Cao

        // Tính thể tích
        var volume = (length * width * height) / 1000000; // Chuyển đổi sang m³ (giả sử đơn vị là mm)

        // Gán giá trị vào trường Volume
        autoNumericFields['Volume'].set(volume); // Hiển thị 6 chữ số thập phân
    }

    // Bắt sự kiện khi Length, Width hoặc Height thay đổi
    $('[name="Length"], [name="Width"], [name="Height"]').on('input', function () {
        calculateVolume();
    });


    // Hàm tính toán phí bảo hiểm
    function calculateInsuranceFee() {

        var goodsValue = autoNumericFields['Price'].get() || 0; // Giá trị hàng hóa
        var insuranceRate = autoNumericFields['InsuranceValue'].get() || 0; // Giá trị bảo hiểm (%)

        // Tính phí bảo hiểm
        var insuranceFee = (goodsValue * insuranceRate) / 100;

        // Gán giá trị vào trường InsuranceFee
        autoNumericFields['InsuranceFee'].set(insuranceFee);
    }

    // Hàm tính toán tổng phí dịch vụ
    function calculateTotalServiceFee() {

        var insuranceFee = autoNumericFields['InsuranceFee'].get() || 0; // Phí bảo hiểm
        var domesticShippingFee = autoNumericFields['DomesticShippingFee'].get() || 0; // Phí vận chuyển nội địa (VND)
        var woodenPackagingFee = autoNumericFields['WoodenPackagingFee'].get() || 0; // Phí đóng gỗ
        var shockproofFee = autoNumericFields['ShockproofFee'].get() || 0; // Phí chống sốc

        // Tính tổng phí dịch vụ
        var totalServiceFee = parseFloat(insuranceFee) + parseFloat(domesticShippingFee) + parseFloat(woodenPackagingFee) + parseFloat(shockproofFee);

        // Gán giá trị vào trường TotalServiceFee
        autoNumericFields['TotalServiceFee'].set(totalServiceFee);
    }

    // Hàm tính toán tổng phí
    function calculateTotalFee() {
        var totalServiceFee = autoNumericFields['TotalServiceFee'].get() || 0; // Tổng phí dịch vụ
        var shippingFee = autoNumericFields['TotalFee'].get() || 0; // Phí vận chuyển

        // Tính tổng phí
        var totalFee = parseFloat(totalServiceFee) + parseFloat(shippingFee);

        // Gán giá trị vào trường TotalFee
        autoNumericFields['TotalPrice'].set(totalFee);
    }

    // Bắt sự kiện khi các trường liên quan thay đổi
    $('[name="Price"], [name="InsuranceValue"]').on('input', function () {
        calculateInsuranceFee();
        calculateTotalServiceFee();
        calculateTotalFee();
    });

    $('[name="DomesticShippingFee"], [name="WoodenPackagingFee"], [name="ShockproofFee"]').on('input', function () {
        calculateTotalServiceFee();
        calculateTotalFee();
    });

    $('[name="TotalFee"]').on('input', function () {
        calculateTotalFee();
    });

    // Bắt sự kiện cho các checkbox
    _$form.find('input[type="checkbox"]').on('change', function () {

        var targetInputSelector = $(this).attr('target'); // Lấy giá trị của thuộc tính "data-target"
        var targetInput = _$form.find(`[data-target="${targetInputSelector}"]`); // Tìm input theo data-target

        if ($(this).is(':checked')) {
            // Nếu checkbox được tick chọn, bỏ readonly của input
            targetInput.prop('readonly', false);
        } else {
            // Nếu checkbox không được chọn, đặt lại readonly và xóa giá trị
            targetInput.prop('readonly', true);
        }
    });


    _$form.find('.btn-calc-total-price').on('click', function (e) {

        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }
        // Tạo đối tượng data để lưu trữ dữ liệu từ form
        var data = {};
        // Duyệt qua tất cả các input trong form
        _$form.find('input, select, textarea').each(function () {
            var inputName = $(this).attr('name'); // Lấy tên của input
            if (inputName) {
                if ($(this).attr('type') === 'checkbox') {
                    // Nếu là checkbox, lưu giá trị true/false
                    data[inputName] = $(this).is(':checked');

                } else if (autoNumericFields[inputName]) {
                    // Nếu input được quản lý bởi AutoNumeric, lấy giá trị số
                    data[inputName] = autoNumericFields[inputName].get();
                } else {
                    // Lấy giá trị thông thường
                    data[inputName] = $(this).val();
                }
            }
        });

        _shippingRate.calculateShippingCostForFinance(data).done(function (response) {
            // Kiểm tra kết quả trả về
            if (response) {
                // Ánh xạ kết quả vào các trường input
               /* autoNumericFields['PricePerUnit'].set(response.pricePerUnit); // Giá vận chuyển trên 1 đơn vị*/
                /* autoNumericFields['InsuranceFee'].set(response.insuranceValue); // Phí bảo hiểm*/

                PlaySound('success', function () {
                    abp.notify.success("Tính toán phí vận chuyển thành công!");
                });

                autoNumericFields['WoodenPackagingFee'].set(response.woodenFee); // Phí đóng gỗ
                autoNumericFields['ShockproofFee'].set(response.shockproofFee); // Phí chống sốc
                autoNumericFields['DomesticShippingFee'].set(response.domesticShippingFee); // Phí vận chuyển nội địa (VND)
                autoNumericFields['DomesticShippingFeeRMB'].set(response.domesticShippingFeeCN); // Phí vận chuyển nội địa (RMB)
                /*autoNumericFields['TotalServiceFee'].set(response.packagePrice); // Tổng phí dịch vụ*/
                autoNumericFields['TotalFee'].set(response.shippingFee); // Phí vận chuyển

                calculateTotalFee();
                // Hiển thị thông báo thành công
               
            } else {
                // Hiển thị thông báo lỗi nếu không có dữ liệu trả về
                abp.notify.error("Không nhận được dữ liệu từ máy chủ.");
            }
        }).fail(function (error) {
            // Hiển thị thông báo lỗi nếu API thất bại
            abp.notify.error("Có lỗi khi tính toán phí vận chuyển: " + error.message);
        }).always(function () {
            // Xử lý sau khi hoàn thành API (nếu cần)
        });
    });

    _$form.find('.save-button').on('click', function (e) {
        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }
        // Tạo đối tượng data để lưu trữ dữ liệu từ form
        var data = {};

        // Duyệt qua tất cả các input trong form
        _$form.find('input, select, textarea').each(function () {
            var inputName = $(this).attr('name'); // Lấy tên của input
            if (inputName) {
                if ($(this).attr('type') === 'checkbox') {
                    // Nếu là checkbox, lưu giá trị true/false
                    data[inputName] = $(this).is(':checked');

                } else if (autoNumericFields[inputName]) {
                    // Nếu input được quản lý bởi AutoNumeric, lấy giá trị số
                    data[inputName] = autoNumericFields[inputName].get();
                } else {
                    // Lấy giá trị thông thường
                    data[inputName] = $(this).val();
                }
            }
        });

        _packageService.updateFinance(data).done(function (response) {
            PlaySound('success', function () {
                var prevUrl = $('#PrevUrl').val();
                window.location.href = (prevUrl || '/packages/');
            });

        }).fail(function (error) {
            abp.notify.error("Có lỗi khi cập nhật tài chính kiện" + error.message)
        }). always(function () { });
    });

})(jQuery);
