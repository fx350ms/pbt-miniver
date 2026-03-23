(function ($) {
    const _deliveryRequestServices = abp.services.app.deliveryRequest;
    l = abp.localization.getSource('pbt');
    _$confirmPaymentDiv = $('#DivConfirmPayment'),
        _$form = _$confirmPaymentDiv.find('form');
    _customerService = abp.services.app.customer;

    $('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var confirmPayment = _$form.serializeFormToObject();
        _deliveryRequestServices.pay(confirmPayment).done(function () {
            abp.notify.info(l('SavedSuccessfully'));
            PlaySound('success', () => { window.location.href = '/DeliveryRequest' });

        }).always(function () {
        });

        return false;
    });


    
})(jQuery);
