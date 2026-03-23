(function ($) {
    var _messageService = abp.services.app.message,
        l = abp.localization.getSource('pbt'),
        _$table = $('#TransactionsTable');
    var _$receiptModal = $('#ReceiptCreateModal'),
        _$receiptForm = _$receiptModal.find('form');
    _$receiptForm.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$receiptForm.valid()) {
            return;
        }
        var receipt = _$receiptForm.serializeFormToObject();

        abp.ui.setBusy(_$receiptModal);
        _messageService.createReceiptTransaction(receipt).done(function () {
            _$receiptModal.modal('hide');
            _$receiptForm[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('message.reload', receipt);
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
    }).addClass('form-control');

    // Show/hide fields based on TransactionType
    function toggleFields() {
        var transactionType = $('#TransactionType').val();
        if (transactionType === '2') {
            $('#OrderIdField').show();
            $('#PayerField').hide();
        } else {
            $('#OrderIdField').hide();
            $('#PayerField').show();
        }
    }

    $('#TransactionType').change(function () {
        toggleFields();
    });

    // Initial call to set the correct visibility on page load
    toggleFields();
})(jQuery);