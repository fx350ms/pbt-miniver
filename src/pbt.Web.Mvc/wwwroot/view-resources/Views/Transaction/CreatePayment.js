(function ($) {
    var _transactionService = abp.services.app.transaction,
    //_customerWalletService = abp.services.app.customerWallet,
        _customerService = abp.services.app.customer,

        l = abp.localization.getSource('pbt'),
        _$table = $('#TransactionsTable');
    // Handle receipt creation
    var _$receiptModal = $('#DivReceiptCreate'),
        _$receiptForm = _$receiptModal.find('form');
    _$receiptForm.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$receiptForm.valid()) {
            return;
        }
        var receipt = _$receiptForm.serializeFormToObject();
        receipt.Amount = receipt.Amount.replaceAll('.', '');
        abp.ui.setBusy(_$receiptModal);
        _transactionService.createPaymentTransaction(receipt).done(function () {
            _$receiptModal.modal('hide');
            _$receiptForm[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlayAudio('success', function () {
                window.location.href = '/Transaction';
            });

 

        }).always(function () {
            abp.ui.clearBusy(_$receiptModal);
        });
    });

    _$receiptModal.on('shown.bs.modal', () => {
        _$receiptModal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$receiptForm.clearForm();
    });

    // Initialize Select2 for Payer field
    $('.select2').select2({
        theme: 'bootstrap4',
        ajax: {
            delay: 250, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Customer/SearchCustomers',
            type: "POST",
            data: function (params) {
                return {
                    keyword: params.term, // search term
                };
            },
            processResults: function (data) {
                return {
                    results: data.result.map(function (item) {
                        return { id: item.id, text: (item.fullName + ' - ' + item.phoneNumber) };
                    })
                };
            },
            dataType: 'json',
        }
    }).addClass('form-control')
        .on('select2:select', function (e) {
            var transactionType = $('#TransactionType').val();
            if (transactionType == '3') {
                var customerId = e.params.data.id;
                _customerService.getByCustomerWalletBalance
                    (customerId)
                    .done(function (result) {
                        $('[name=Amount]').val(result.CurrentAmount);
                    });
            }
        });

    // Show/hide fields based on TransactionType
    function toggleFields() {
        var transactionType = $('#TransactionType').val();
        if (transactionType == '0') {
            $('#PayerField').hide();
        } else {
            $('#PayerField').show();
        }
    }
    $('#TransactionType').change(function () {
        toggleFields();
    });
    // Initial call to set the correct visibility on page load
    toggleFields();
    $('[name=Amount]').maskNumber({ integer: true, thousands: '.' });
})(jQuery);