(function ($) {
    var _addressService = abp.services.app.customerAddress,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#AddressEditModal'),
        _$form = _$modal.find('form');
   

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

    $(document).ready(function () {
        // Trigger khi checkbox được thay đổi
        $('input[type="checkbox"]').change(function () {
            // Lấy tên của checkbox hiện tại
            var checkboxName = $(this).attr('name');

            // Tìm hidden input tương ứng
            var hiddenInput = $('input[type="hidden"][name="' + checkboxName + '"]');

            // Nếu checkbox được chọn, set hidden input thành "true", ngược lại set "false"
            if ($(this).is(':checked')) {
                hiddenInput.val("true");
            } else {
                hiddenInput.val("false");
            }
        });
    });
    
 

    function loadProvince() {
        abp.services.app.province.getFull().done(function (data) {
            var provinceDropdown = $('#ProvinceId');
            provinceDropdown.empty();
            // Thêm option mặc định
            provinceDropdown.append('<option value="">' + l('SelectProvince') +'</option>');
            // Duyệt qua mảng dữ liệu và thêm các option
            $.each(data, function (index, province) {
                provinceDropdown.append('<option value="' + province.id + '">' + province.name + '</option>');
            });
        }).fail(function (error) {
            // Xử lý lỗi nếu có
            PlaySound('warning'); abp.notify.error("Failed to load provinces: " + error.message);
        });
    }

    $('#ProvinceId').change(function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here

                var districtDropdown = $('#DistrictId');
                districtDropdown.empty();
                // Thêm option mặc định
                districtDropdown.append('<option value="">'+l('SelectDistrict') +'</option>');
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
                wardDropdown.append('<option value="">' + l('SelectWard') +'</option>');
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


    $(document).on('click', '.edit-address', function (e) {
        var addressId = $(this).attr("data-address-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Users/EditAddressModal?id=' + addressId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#AddressEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    abp.event.on('address.edited', (data) => {
        _$addresssTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$addresssTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$addresssTable.ajax.reload();
            return false;
        }
    });

    loadProvince();


})(jQuery);
