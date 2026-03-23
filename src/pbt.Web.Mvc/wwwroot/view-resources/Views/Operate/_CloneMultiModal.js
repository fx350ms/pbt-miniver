(function ($) {
    var _departmentService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CloneMultiModal'),
        _$form = _$modal.find('form');

    function createMulti() {
        if (!_$form.valid()) {
            return;
        }
        
        getFullAddress();
        var data = _$form.serializeFormToObject();
        var packages = [];
        $("#table-packages tbody tr.active").each(function (i, e) {
            var package = {};
            $(e).find("input").each((j, input) => {
                var type = $(input).attr("type");
                if (type == "text" || type == "number") {
                    package[$(input).attr("name")] = $(input).val();
                }
                if (type == "checkbox") {
                    package[$(input).attr("name")] = $(input).is(":checked");
                }
            })
            packages.push(package);
        })
        const dataMulti = {
            packages: packages,
            addressId: $("[name='AddressId']:checked").val() || 0,
            numberPackage: data.NumberPackage,
            numberVirtualPackage: data.NumberVirtualPackage,
        };

        abp.ui.setBusy(_$form);
        _departmentService.createPackages(dataMulti).done(function () {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            window.location.href = "/Packages"
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

    function getFullAddress() {
        var countryId = $('#CountryId').val();
        if (countryId == 2) {

            // Lấy giá trị từ các dropdown và ô nhập địa chỉ
            var province = $('#ProvinceId option:selected').text();
            var district = $('#DistrictId option:selected').text();
            var ward = $('#WardId option:selected').text();
            var address = $('#Address').val();

            // Tạo địa chỉ hoàn chỉnh
            var fullAddress = [address, ward, district, province].filter(function (part) {
                return part.trim() !== ""; // Loại bỏ phần trống
            }).join(", ");

            // Gán địa chỉ hoàn chỉnh vào input ẩn (hoặc hiển thị ra UI)
            $('input[name="FullAddress"]').val(fullAddress); // Nếu cần lưu vào input ẩn
            $('#fullAddress').text(fullAddress); // Hiển thị ra UI (tùy chỉnh id)
        }
        else {
            var address = $('#Address').val();
            $('#fullAddress').val(address);
        }

    }
    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        createMulti();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            createMulti();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });
})(jQuery);
