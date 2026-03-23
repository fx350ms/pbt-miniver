(function ($) {
    var _fundAccountService = abp.services.app.fundAccount,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#FundAccountEditModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var fundAccount = _$form.serializeFormToObject();
        fundAccount.IsActived = $('#CheckIsActived').is(':checked');

        abp.ui.setBusy(_$form);
        _fundAccountService.update(fundAccount).done(function () {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('fundAccount.edited', fundAccount);
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input:not([type=hidden]):first').focus();
        toggleBankFields();
    });

    function toggleBankFields() {
        var accountType = _$form.find('input[name="AccountType"]:checked').val();
        if (accountType == '1') { // Bank
            _$form.find('input[name="BankName"]').closest('.form-group').show();
            _$form.find('input[name="AccountNumber"]').closest('.form-group').show();
            _$form.find('input[name="AccountHolder"]').closest('.form-group').show();
        } else { // Cash
            _$form.find('input[name="BankName"]').closest('.form-group').hide();
            _$form.find('input[name="AccountNumber"]').closest('.form-group').hide();
            _$form.find('input[name="AccountHolder"]').closest('.form-group').hide();
        }
    }

    _$form.find('input[name="AccountType"]').change(function () {
        toggleBankFields();
    });

    _$form.find('#CheckIsActived').change(function () {
        $('#IsActived').val(this.checked ? true : false);
    });

    // Initial call to set the correct visibility on page load
    toggleBankFields();
})(jQuery);