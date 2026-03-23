(function ($) {
    var _orderService = abp.services.app.order,
        _addressService = abp.services.app.customerAddress,
        _waybillService = abp.services.app.waybill,
        _$modal = $('#CustomerAddressCreateModal'),
        _$formAddress = _$modal.find('form'),

        _$waybillModal = $('#WaybillCreateModal'),
        _$waybillCreateForm = _$waybillModal.find('form'),
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-create-my-order');

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var order = _$form.serializeFormToObject();
        order.PriceInsurance = order.PriceInsurance.replaceAll('.', '');
        _orderService.createMyOrder(order).done(function () {
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlayAudio('success', function () {
                window.location.href = '/Orders';
            });
        })
            .always(function () {

            });
    });
    _$formAddress.find('.save-address-button').on('click', function (e) {
        e.preventDefault();

        getFullAddress();
        var address = _$formAddress.serializeFormToObject();

        abp.ui.setBusy(_$formAddress);
        _addressService.create(address).done(function () {
            _$modal.modal('hide');
            _$formAddress[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlaySound('success');

            var customerId = $('#hiddenCustomerId').val();
            abp.ajax({
                url: abp.appPath + 'Orders/GetAddressByCustomerId?customerId=' + customerId,
                type: 'GET',
                dataType: 'html',
                success: function (content) {
                    $('div#address-box').html(content);

                    //if (customerAddressId) {
                    //    $('[data-target="address-' + customerAddressId + '"]').prop('checked', true);
                    //}
                },
                error: function (e) {
                    alert(e);
                }
            });
            //_$addresssTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $('.save-waybill-button').on('click', function (e) {

        e.preventDefault();
        var data = _$waybillCreateForm.serializeFormToObject();
        abp.ui.setBusy(_$waybillCreateForm);
        _waybillService.createSimple(data).done(function () {
            _$waybillModal.modal('hide');
            _$waybillCreateForm[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_$waybillModal);
        });
        return false;
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


    $('.btn-add-new-address').on("click", function () {
        loadProvince();
        var customerId = $('#hiddenCustomerId').val();
        $('.form-content input[name="CustomerId"]').val(customerId);

    });

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

    $('input[type="radio"][name="address"]').change(function () {
        // Lấy giá trị của target (ID của card body sẽ được mở)
        var target = $(this).data('target');
        $('[data-type="address-id"]').each(function () {
            $(this).CardWidget('collapse');
        });

        $('div#' + target + '[data-type="address-id"]').CardWidget('expand');


        var value = $("input[name='address']:checked").val();
        $('#AddressId').val(value);
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

    function loadProvince() {
        abp.services.app.province.getFull().done(function (data) {
            var provinceDropdown = $('#ProvinceId');
            provinceDropdown.empty();
            // Thêm option mặc định
            provinceDropdown.append('<option value="">' + l('SelectProvince') + '</option>');
            // Duyệt qua mảng dữ liệu và thêm các option
            $.each(data, function (index, province) {
                provinceDropdown.append('<option value="' + province.id + '">' + province.name + '</option>');
            });
        }).fail(function (error) {
            // Xử lý lỗi nếu có
            PlaySound('warning'); abp.notify.error("Failed to load provinces: " + error.message);
        });
    }
    function getFullAddress() {
        // Lấy giá trị từ các dropdown và ô nhập địa chỉ
        var province = $('#ProvinceId option:selected').text();
        var district = $('#DistrictId option:selected').text();
        var ward = $('#WardId option:selected').text();
        var address = $('textarea[name="Address"]').val();

        // Tạo địa chỉ hoàn chỉnh
        var fullAddress = [address, ward, district, province].filter(function (part) {
            return part.trim() !== ""; // Loại bỏ phần trống
        }).join(", ");

        // Gán địa chỉ hoàn chỉnh vào input ẩn (hoặc hiển thị ra UI)
        $('input[name="FullAddress"]').val(fullAddress); // Nếu cần lưu vào input ẩn
        $('#fullAddress').text(fullAddress); // Hiển thị ra UI (tùy chỉnh id)
    }
    $('#ProvinceId').change(function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here

                var districtDropdown = $('#DistrictId');
                districtDropdown.empty();
                // Thêm option mặc định
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    districtDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });


    $('#DistrictId').change(function () {
        var districtId = $(this).val();
        if (districtId) {
            abp.services.app.ward.getFullByDistrict(districtId).done(function (data) {
                // code here

                var wardDropdown = $('#WardId');
                wardDropdown.empty();
                // Thêm option mặc định
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    wardDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $('[data-service-target]').change(function (e) {

        var target = $(this).attr('data-service-target');
        var value = $(this).val();
        $(target).val(value == 'on' ? 'true' : 'false');
    });

    $('#inputInsurance').maskNumber({ integer: true, thousands: '.' });
    $('#inputInsurance').hide();


})(jQuery);
