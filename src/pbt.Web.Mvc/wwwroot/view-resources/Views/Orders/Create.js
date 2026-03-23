(function ($) {
    var _orderService = abp.services.app.order,
        _addressService = abp.services.app.customerAddress,
        _waybillService = abp.services.app.waybill,
        _customerService = abp.services.app.customer,
        _$modal = $('#CustomerAddressCreateModal'),
        _$formAddress = _$modal.find('form'),
        _$waybillModal = $('#WaybillCreateModal'),
        _$waybillCreateForm = _$waybillModal.find('form'),
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-create-my-order');
    let nextUrl = '';
    _$form.find('.save-button').on('click', (e,a,b,c,d) => {
        e.preventDefault();
        
        nextUrl = e.currentTarget.getAttribute('data-url');

        if (!_$form.valid()) {
            return;
        }
        var order = _$form.serializeFormToObject();
        order.PriceInsurance = order.PriceInsurance.replaceAll('.', '');
        _orderService.createCustomerOrder(order).done(function () {
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlayAudio('success', function () {
              
                window.location.href = nextUrl;
            });

        }).always(function () {

        });
    });

    $('select[name="CustomerId"').select2(
    ).addClass('form-control')
        .on('change', function () {
            var customerId = $(this).val();
            var $addressSelect = $('#select-customer-address');
            $addressSelect.empty();
            if (customerId) {
                abp.services.app.customerAddress.getByCustomerId(customerId).done(function (data) {
                    $addressSelect.append('<option value="">'+l('SelectAddress')+'</option>');
                    $.each(data, function (i, item) {
                        $addressSelect.append('<option value="' + item.id + '">' + item.fullAddress + '</option>');
                    });
                });
            }
        });
    $('#select-insurance').change(function () {
        if ($(this).prop('checked')) {
            $('#inputInsurance').val('').removeClass('d-none').show();
        } else {
            $('#inputInsurance').val('').addClass('d-none').hide();
        }
    });

    _$formAddress.find('.save-address-button').on('click', function (e) {
        e.preventDefault();
        var address = _$formAddress.serializeFormToObject();
        abp.ui.setBusy(_$formAddress);
        _addressService.create(address).done(function () {
            _$modal.modal('hide');
            _$formAddress[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            var customerId = $('select[name="CustomerId"]').val();
            abp.ajax({
                url: abp.appPath + 'Orders/GetAddressByCustomerId?customerId=' + customerId,
                type: 'GET',
                dataType: 'html',
                success: function (content) {
                    $('div#address-box').html(content);
                    SelectAddressOnChange();
                    if (customerId) {
                        $('[data-target="address-' + customerId + '"]').prop('checked', true);
                    }
                },
                error: function (e) {
                    alert(e);
                }
            });
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });


    $('.save-waybill-button').on('click', function (e) {
        e.preventDefault();

        var waybill = _$waybillCreateForm.serializeFormToObject();
        abp.ui.setBusy(_$waybillCreateForm);
        _waybillService.createSimple(waybill).done(function () {
            _$waybillModal.modal('hide');
            _$waybillCreateForm[0].reset();
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_$waybillModal);
        });
    });

    $('.btn-add-new-address').on("click", function () {
        var customerId = $('select[name="CustomerId"]').val();
        if (customerId && customerId > 0) {
         //   loadProvince();
            $('.form-content input[name="CustomerId"]').val(customerId);
            // Hiển thị modal nếu đã chọn khách hàng
           $('#CustomerAddressCreateModal').modal('show');
        } else {
            // Hiển thị thông báo hoặc xử lý khi chưa chọn khách hàng

            PlaySound('warning'); abp.notify.error('Vui lòng chọn khách hàng trước khi thêm địa chỉ mới.');
        }
    });


    $('#select-insurance').change(function () {
        if ($(this).prop('checked')) {
            $('[name="PriceInsurance"]').show();
        }
        else {
            $('[name="PriceInsurance"]').hide();
        }
    });



    $('[data-service-target]').change(function (e) {
        var target = $(this).attr('data-service-target');
        var value = $(this).val();
        $(target).val(value == 'on' ? 'true' : 'false');
    });
    $('.mask-number').maskNumber({ integer: true, thousands: '.' });
    $('#inputInsurance').hide();
})(jQuery);
