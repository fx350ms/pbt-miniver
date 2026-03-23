(function ($) {
    var _transactionService = abp.services.app.transaction,
        _customerService = abp.services.app.customer,
        _$modal = $('#modal-create-transaction'),
        _$form = _$modal.find('form'),
        l = abp.localization.getSource('pbt');

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        // Tạo FormData để gửi dữ liệu và tệp
        var formData = new FormData();
        var files = $("input[name='FileContent']")[0].files;

        // Thêm tệp vào FormData
        for (var i = 0; i < files.length; i++) {
            formData.append('Attachments', files[i]);
        }
        var localeName =   abp.localization.currentCulture.name.replaceAll('-', '').toLowerCase();
        // Lấy các dữ liệu từ form và thêm vào FormData
        var data = _$form.serializeFormToObject();
        data.Amount = formatNumberCur(data.Amount, localeName);
        for (var key in data) {
            formData.append(key, data[key]);
        }

        abp.ui.setBusy(_$modal);
        $.ajax({
            url: '/api/services/app/Transaction/CreateWithAttachment',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                abp.notify.info(l('SavedSuccessfully'));
                PlayAudio('success', function () {
                    window.location.href = '/Transaction';
                });
            },
            error: function (xhr) {
                abp.message.error(l('ErrorOccurred'));
                console.error(xhr);
            },
            complete: function () {
                abp.ui.clearBusy(_$modal);
            }
        });


    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });



    // Initialize Select2 for Payer field
    $('.select-customer-id').select2({
        allowClear: true,
        ajax: {
            delay: 1000, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Customer/GetAllForSelect',
            type: "GET",
            //data: function (params) {
            //    return {
            //        q: params.term, // search term
            //    };
            //},

            processResults: function (data) {
                return {
                    results: data.result
                };
                //return {
                //    results: data.result.map(function (item) {
                //        return { id: item.id, text: (item.userName) };
                //    })
                //};
            },
            // dataType: 'json',
        },
        tags: true
    }).addClass('form-control').change(function () {
        var customerId = $(this).val();
        _customerService.getByCustomerWalletBalance(customerId).done(function (customerData) {
            $('input[name="MaxDebt"]').val(formatNumberThousand(customerData.maxDebt)); // Format CreditLimit
            if (customerData.currentAmount >= 0) {
                $('input[name="TotalDebt"]').val(0); // Format TotalDebt
                $('input[name="CurrentAmount"]').val(formatNumberThousand(customerData.currentAmount));
            }
            else {
                $('input[name="TotalDebt"]').val(formatNumberThousand(Math.abs(customerData.currentAmount))); // Format TotalDebt
                $('input[name="CurrentAmount"]').val(0);
            }

        });
    });



   
    new AutoNumeric('#txt-amount', {
        currencySymbol: '',     // Ký hiệu tiền tệ
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        // decimalPlaces: 2,           // Số chữ số sau dấu thập phân
        minimumValue: '0'           // Giá trị tối thiểu có thể nhập
    });
    // Xử lý sự kiện khi chọn tài khoản FundAccount
    $('select[name="FundAccountId"]').on('change', function () {
        // Lấy thông tin từ option được chọn
        var selectedOption = $(this).find('option:selected');

        var balance = selectedOption.data('ammount'); // Số dư
        var currency = selectedOption.data('currency'); // Tiền tệ

        // Hiển thị số dư và tiền tệ vào các trường tương ứng
        $('#fund-account-balance').val(formatNumberThousand((balance || 0), 2)); // Nếu không có giá trị, hiển thị "Không có sẵn"
        $('#fund-account-currency').val(currency || l('NotAvailable'));
    });


})(jQuery);