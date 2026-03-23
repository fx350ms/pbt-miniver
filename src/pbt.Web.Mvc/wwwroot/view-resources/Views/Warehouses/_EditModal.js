(function ($) {
    var _departmentService = abp.services.app.warehouse,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#WarehouseEditModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }
        getFullAddress();
        const data = _$form.serializeFormToObject();

        abp.ui.setBusy(_$form);
        _departmentService.update(data).done(function () {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('warehouse.edited', data);
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }

    $('[name="CountryId"]').on('change', function () {
        var countryId = $(this).val();
        var country = $("#CountryId option:selected").text();
        $('#Country').val(country);
        if (countryId == 1) {
            $('[data-country="2"]').hide();
        } else {
            $('[data-country="2"]').show();
        }
    });

    async function loadProvince(ProvinceId = "#EditProvinceId") {
        return await abp.services.app.province.getFull().done(function (data) {
            var provinceDropdown = $(ProvinceId);
            var selectedId = provinceDropdown.attr('value');
            provinceDropdown.empty();
            // Thêm option mặc định
            provinceDropdown.append('<option value="">' + l('SelectProvince') + '</option>');
            // Duyệt qua mảng dữ liệu và thêm các option
            $.each(data, function (index, province) {
                var selected = selectedId == province.id ? "selected" : "";
                provinceDropdown.append('<option ' + selected + ' value="' + province.id + '">' + province.name + '</option>');
            });
            provinceDropdown.trigger('change');
        }).fail(function (error) {
            // Xử lý lỗi nếu có
            PlaySound('warning'); abp.notify.error("Failed to load provinces: " + error.message);
        });
    }

    $(document).on("change", "#EditProvinceId", function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here
                debugger
                var districtDropdown = $('#EditDistrictId');
                var selectedId = districtDropdown.attr("value");
                districtDropdown.empty();
                // Thêm option mặc định
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    var selected = selectedId == item.id ? "selected" : "";
                    districtDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                });
                districtDropdown.trigger("change");
            });
        } else {
            $('#EditDistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#EditWardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $('#DistrictId').change(function () {
        var districtId = $(this).val();
        if (districtId) {
            abp.services.app.ward.getFullByDistrict(districtId).done(function (data) {
                // code here
                var wardDropdown = $('#EditWardId');
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
            $('#EditWardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $(document).on("change", "#EditDistrictId", function () {
        var districtId = $(this).val();
        if (districtId) {
            abp.services.app.ward.getFullByDistrict(districtId).done(function (data) {
                // code here

                var wardDropdown = $('#EditWardId');
                var selectedId = wardDropdown.attr("value");
                wardDropdown.empty();
                // Thêm option mặc định
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    var selected = selectedId == item.id ? "selected" : "";
                    wardDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    function getFullAddress() {
        
        var countryId = $('#CountryId').val();
        if (countryId == 2) {

            // Lấy giá trị từ các dropdown và ô nhập địa chỉ
            var province = $('#EditProvinceId option:selected').text();
            var district = $('#EditDistrictId option:selected').text();
            var ward = $('#EditWardId option:selected').text();
            var address = $('#EditAddress').val();

            // Tạo địa chỉ hoàn chỉnh
            var fullAddress = [address, ward, district, province].filter(function (part) {
                return part.trim() !== ""; // Loại bỏ phần trống
            }).join(", ");

            // Gán địa chỉ hoàn chỉnh vào input ẩn (hoặc hiển thị ra UI)
            $('input[name="FullAddress"]').val(fullAddress); // Nếu cần lưu vào input ẩn
            $('#fullAddress').text(fullAddress); // Hiển thị ra UI (tùy chỉnh id)
        }
        else {
            var address = $('#EditAddress').val();
            $('#fullAddress').val(address);
        }
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

    loadProvince();
})(jQuery);
