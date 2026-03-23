(function ($) {
    var _shippingRateService = abp.services.app.shippingRateGroup,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#ShippingRateEditModal'),
        _$form = _$modal.find('form');

    
    _$form.find('.save-button').on('click', (e) => {
       
        e.preventDefault();
        var shippingRates = [];
        var shippingRateGroupId = $('#ShippingRateGroupId').val();
        // Iterate through each rate-item to collect data
        $('.rate-item').each(function () {
         
            var $rateItem = $(this);
            var rateId = $rateItem.data('id') || 0;
            var shippingRate = {
                Id: rateId,
                ShippingRateGroupId: shippingRateGroupId,
                ShippingTypeId: $rateItem.data('line'),
                WarehouseFromId: $rateItem.data('from'),
                WarehouseToId: $rateItem.data('to'),
                Tiers: []
            };

            // Collect tier data
            $rateItem.find('tbody tr').each(function () {
                var $row = $(this);
                var tier = {
                    Id: $row.data('id') || 0,
                    ProductTypeId: $row.find('input[name="ProductGroupId"]').val(),
                    FromValue: $row.find('input[name="FromValue"]').val(),
                    ToValue: $row.find('input[name="ToValue"]').val(),
                    Unit: $row.find('select').val(),
                    PricePerUnit: $row.find('input[name="PricePerUnit"]').val().replace(/,/g, ''),
                    ShippingRateId: rateId
                };
                shippingRate.Tiers.push(tier);
            });

            shippingRates.push(shippingRate);
        });

       // var data = _$form.serializeFormToObject();
        var data = {
            ShippingRateGroupId: shippingRateGroupId,
            ShippingRates: shippingRates
        }
        abp.ui.setBusy(_$modal);
        _shippingRateService.saveShippingRates(data).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.edit-shipping-rate', function () {
        var id = $(this).attr("data-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'ShippingRate/ConfigureTiersModal?Id=' + id,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ShippingRateEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });
   
})(jQuery);