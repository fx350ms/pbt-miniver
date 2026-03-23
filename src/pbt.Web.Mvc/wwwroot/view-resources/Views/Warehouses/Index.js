(function ($) {
    var _departmentService = abp.services.app.warehouse,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#DepartmentsTable');

    var _$departmentsTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _departmentService.getAll,
            inputFilter: function () {
                return $('#DepartmentSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$departmentsTable.draw(false)
            }
        ],
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'code',
                sortable: false
            },
            {
                targets: 2,
                data: 'name',
                sortable: false
            },
            {
                targets: 3,
                data: 'phone',
                sortable: false
            },
            {
                targets: 4,
                data: 'fullAddress',
                sortable: false
            },
            {
                targets: 5,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {

                    return [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,
                        `       <a class="dropdown-item edit-warehouse" data-warehouse-id="${row.id}" data-toggle="modal" data-target="#WarehouseEditModal">`,
                        `           <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        `       </a>`,
                        `       <a class="dropdown-item delete-warehouse" data-warehouse-id="${row.id}" data-warehouse-name="${row.name}">`,
                        `           <i class="fas fa-trash"></i> ${l('Delete')}`,
                        `       </a>`,
                        `   </div>`,
                        `</div>`
                    ].join('');
                }
            }
        ]
    });


    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        getFullAddress();

        var data = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _departmentService.create(data).done(function () {

            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$departmentsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-warehouse', function () {
        var departmentId = $(this).attr("data-warehouse-id");
        var departmentName = $(this).attr('data-warehouse-name');

        deleteDepartments(departmentId, departmentName);
    });

    function deleteDepartments(departmentId, departmentName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                departmentName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _departmentService.delete({
                        id: departmentId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$departmentsTable.ajax.reload();
                    });
                }
            }
        );
    }
    async function loadProvince(ProvinceId = "#ProvinceId") {
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
        }).fail(function (error) {
            // Xử lý lỗi nếu có
            PlaySound('warning'); abp.notify.error("Failed to load provinces: " + error.message);
        });
    }

    $(document).on('click', '.edit-warehouse', function (e) {
        var warehouseId = $(this).attr("data-warehouse-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Warehouses/EditModal?Id=' + warehouseId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#WarehouseEditModal div.modal-content').html(content);
                loadProvince("#EditProvinceId").then(() => {
                    $("#EditProvinceId").trigger("change");
                });
            },
            error: function (e) {
            }
        });
    });

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


    $(document).on("change", "#EditProvinceId", function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here

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

    $('#CountryId').on('change', function () {
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

    $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', (e) => {
        $('.nav-tabs a[href="#department-details"]').tab('show')
        loadProvince();
    });

    abp.event.on('department.edited', (data) => {
        _$departmentsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$departmentsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$departmentsTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
