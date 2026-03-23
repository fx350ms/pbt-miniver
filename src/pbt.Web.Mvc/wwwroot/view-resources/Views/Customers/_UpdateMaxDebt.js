(function ($) {
    var _customerService = abp.services.app.customer,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CustomerUpdateMaxDebt'),
        _$form = _$modal.find('form');

    function save() {
   
        if (!_$form.valid()) {
            return;
        }

        var maxDebt = _$form.find('input[name="MaxDebt"]').val();

        if (maxDebt < 0) {
            abp.notify.error(l('MaxDebtMustBeGreaterThanZero'));
            return;
        }

        var data = _$form.serializeFormToObject();
        data.MaxDebt = maxDebt.replaceAll('.', '')
        abp.ui.setBusy(_$form);
        _customerService.updateMaxDebt(data).done(function () {
            _$modal.modal('hide');
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
            /*  abp.event.trigger('customer.edited', customer);*/
            abp.event.trigger('customer.edited', data);
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }
     
    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });

    $('.number-mask').maskNumber({ integer: true, thousands: '.' });
})(jQuery);
