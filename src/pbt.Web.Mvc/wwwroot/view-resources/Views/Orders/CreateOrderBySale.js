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
    $('[name="Insurance"]').hide();

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var order = _$form.serializeFormToObject();

        _orderService.create(order).done(function () {
            _$form[0].reset();
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
           
        }).always(function () {
           
        });
    });

    $('[name="CustomerId"]').change(function (e) {
        $('div#address-box').html('');
        var customerId = $(this).val(); 
        var customerName = $(this).find('option:selected').attr('data-name');
        $('[name="CustomerName"]').val(customerName);
        
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Orders/GetAddressByCustomerId?customerId=' + customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('div#address-box').html(content);
                $('input[type="radio"][name="AddressId"]').change(function () {
                    // Lấy giá trị của target (ID của card body sẽ được mở)
                    var target = $(this).data('target');
                    $('[data-type="address-id"]').each(function () {
                        $(this).CardWidget('collapse');
                    });

                    $('div#' + target + '[data-type="address-id"]').CardWidget('expand');

                    var value = $("input[name='AddressId']:checked").val();
                    $('#AddressId').val(value);
                });

            },
            error: function (e) {
            }
        });
    });


    $('select[name="WaybillNumber"]').select2({
        theme: 'bootstrap4',
        ajax: {
            delay: 500,
            url: abp.appPath + 'api/services/app/Waybill/getUnmatchedWaybillCodes',
            data: function (params) {
                return {
                    keyword: params.term, // search term
                };
            },
            processResults: function (data) {
                return {
                    results: data.result.map(function (item) {
                        return { id: item, text: item };
                    })
                };
            }
        }
    }).addClass('form-control');

    $('select[name="CustomerId"]').select2({
        theme: 'bootstrap4',
        ajax: {
            delay: 500, // wait 1000 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Customer/GetCustomerListByCurrentSaleForSelect',
            dataType: 'json',
            processResults: function (data) {
                return {
                    results: data.result
                };
            }

        }

    }).addClass('form-control');

    $('input[type="radio"][name="ChinaWarehouseId"]').change(function () {
        
        // Lấy giá trị của target (ID của card body sẽ được mở)
        var target = $(this).data('target');
        $('[data-type="cn-warehouse"]').each(function () {
            $(this).CardWidget('collapse');
        });

        $('div#' + target + '[data-type="cn-warehouse"]').CardWidget('expand');

        var value = $("input[name='ChinaWarehouseId']:checked").val();
        $('#CNWarehouseId').val(value);
    });


    $('input[type="radio"][name="VietnamWarehouseId"]').change(function () {

        // Lấy giá trị của target (ID của card body sẽ được mở)
        var target = $(this).data('target');
        $('[data-type="vn-warehouse"]').each(function () {
            $(this).CardWidget('collapse');
        });
        $('div#' + target + '[data-type="vn-warehouse"]').CardWidget('expand');

        var value = $("input[name='VietnamWarehouseId']:checked").val();
        $('#VNWarehouseId').val(value);
    });


    $('[data-type="cn-warehouse"],[data-type="vn-warehouse"],[data-type="address-id"]').each(function () {
        $(this).CardWidget('collapse');
    });


    $('#select-insurance').change(function () {
        if ($(this).prop('checked')) {
            $('[name="PriceInsurance"]').show();
        }
        else {
            $('[name="PriceInsurance"]').hide();
        }
    });


    _$formAddress.find('.save-address-button').on('click', function (e) {
        e.preventDefault();

        getFullAddress();
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
                    $('input[type="radio"][name="AddressId"]').change(function () {
                        var target = $(this).data('target');
                        $('[data-type="address-id"]').each(function () {
                            $(this).CardWidget('collapse');
                        });
                        $('div#' + target + '[data-type="address-id"]').CardWidget('expand');
                        var value = $("input[name='AddressId']:checked").val();
                        $('#AddressId').val(value);
                    });
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
            loadProvince();
            $('.form-content input[name="CustomerId"]').val(customerId);
            // Hiển thị modal nếu đã chọn khách hàng
            $('#CustomerAddressCreateModal').modal('show');
        } else {
            // Hiển thị thông báo hoặc xử lý khi chưa chọn khách hàng

            PlaySound('warning'); abp.notify.error('Vui lòng chọn khách hàng trước khi thêm địa chỉ mới.');
        }
    });


    $('[name="CustomerId"]').change(function (e) {
        $('div#address-box').html('');
        var customerId = $(this).val();
        var customerName = $(this).find('option:selected').attr('data-name');
        $('[name="CustomerName"]').val(customerName);

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Orders/GetAddressByCustomerId?customerId=' + customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('div#address-box').html(content);
                $('input[type="radio"][name="AddressId"]').change(function () {
                    // Lấy giá trị của target (ID của card body sẽ được mở)
                    var target = $(this).data('target');
                    $('[data-type="address-id"]').each(function () {
                        $(this).CardWidget('collapse');
                    });

                    $('div#' + target + '[data-type="address-id"]').CardWidget('expand');

                    var value = $("input[name='AddressId']:checked").val();
                    $('#AddressId').val(value);
                });

            },
            error: function (e) {
                alert(e);
            }
        });
    });
    $('#inputInsurance').maskNumber({ integer: true, thousands: '.' });
    $('#inputInsurance').hide();
})(jQuery);
