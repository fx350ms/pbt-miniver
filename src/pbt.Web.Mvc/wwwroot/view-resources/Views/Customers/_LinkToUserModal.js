(function ($) {
    var _customerService = abp.services.app.customer,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CustomerLinkToUserModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var data = _$form.serializeFormToObject();
        abp.ui.setBusy(_$form);
        _customerService.linkToUser(data).done(function () {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
          /*  abp.event.trigger('customer.edited', customer);*/
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
})(jQuery);
