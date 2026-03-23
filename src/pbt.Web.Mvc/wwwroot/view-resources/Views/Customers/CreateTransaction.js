(function ($) {
    var _transactionService = abp.services.app.customerTransaction,

        _$modal = $('#modal-create-transaction'),
        _$form = _$modal.find('form'),
        l = abp.localization.getSource('pbt');
    let nextUrl = '';

    const moneyOptions = {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0',          // must be string for AutoNumeric
        modifyValueOnWheel: false,
        unformatOnSubmit: true
    };
    const moneyOptionReadonly = {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0',          // must be string for AutoNumeric
        modifyValueOnWheel: false,
        unformatOnSubmit: true,
        readOnly: true
    };

    const anAmount = new AutoNumeric('#txt-amount',  moneyOptions );

    const anMaxDebt = new AutoNumeric('input[name="MaxDebt"]', moneyOptionReadonly );
    const anTotalDebt = new AutoNumeric('input[name="TotalDebt"]', moneyOptionReadonly );
    const anCurrentAmount = new AutoNumeric('input[name="CurrentAmount"]', moneyOptionReadonly );

    const anTotalAmountAfter = new AutoNumeric('#txt-TotalAmountAfterChange', moneyOptionReadonly );
    const anTotalDebtAfter = new AutoNumeric('#txt-TotalDebtAfterChange', moneyOptionReadonly);

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();
        nextUrl = e.currentTarget.getAttribute('data-url');
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

        // Lấy các dữ liệu từ form và thêm vào FormData
        var data = _$form.serializeFormToObject();
        data.Amount = data.Amount.replaceAll(',','');
       
        for (var key in data) {
            formData.append(key, data[key]);
        }

        abp.ui.setBusy(_$modal);
        $.ajax({
            url: '/api/services/app/CustomerTransaction/CreateWithAttachment',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                abp.notify.info(l('SavedSuccessfully'));
                PlayAudio('success', function () {
                    window.location.href = nextUrl;
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


    function setMoney(an, value) {
        const v = (value == null || Number.isNaN(value)) ? 0 : value;
        an.set(v);
    }

    function getMoney(an) {
        const n = an.getNumber(); // returns Number
        return (n == null || Number.isNaN(n)) ? 0 : n;
    }
 

    function calculateBalances() {
        var transactionType = parseInt($('select[name="CustomerTransactionUpdateType"]').val(), 10);

        const amount = getMoney(anAmount);
        const currentAmount = getMoney(anCurrentAmount);
        const totalDebt = getMoney(anTotalDebt);

        let newWalletBalance = currentAmount;
        let newTotalDebt = totalDebt;

        if (transactionType === 3) {
            newWalletBalance += amount;
        } else if (transactionType === 4) {
            if (amount > currentAmount) {
                newTotalDebt += (amount - currentAmount);
                newWalletBalance = 0;
            } else {
                newWalletBalance -= amount;
            }
        }

        setMoney(anTotalAmountAfter, newWalletBalance);
        setMoney(anTotalDebtAfter, newTotalDebt);
    }

    // initial render for computed fields
    calculateBalances();

    $('#txt-amount').on('change', calculateBalances);
    $('select[name="CustomerTransactionUpdateType"]').on('change', calculateBalances);


})(jQuery);