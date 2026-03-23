(function ($) {
    
    _barcode = abp.services.app.barCode,
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-scan-code'),
        _$hiddenCodeType = $('#hiddenCodeType'),
        _$table = $('#CodeTable'),
        _$tb_form = $('#CreateBarCodeSearchForm')

    var _$scanCodeTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _barcode.getAll,
            inputFilter: function () {
                return _$tb_form.serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$scanCodeTable.draw(false)
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
                data: null,
                defaultContent: '',
                orderable: false
            },
            {
                targets: 1,
                data: null,
                width: 80,
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 2,
                data: 'scanCode'
            },
            {
                targets: 3,
                data: 'customerName',
            },
            {
                targets: 4,
                data: 'creationTime',
                render: function (data) {
                    return moment(data).format('YYYY-MM-DD HH:mm:ss');
                }
            },
            {
                targets: 5,
                data: 'codeType',
                render: function (data) {
                    return data === '1' ? 'Bao' : 'Kiện';
                }
            },
          
           
            {
                targets: 6,
                data: 'action',
                render: function (data) {
                    return data === 1 ? 'Nhập' : 'Xuất';
                }
            },
            {
                targets: 7,
                data: 'sourceWarehouseName'
            },
           
            {
                targets: 8,
                data: 'creatorUserName'
            }

        ]
    });

    loadWarehouse();

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var department = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _warehouseService.create(department).done(function () {

            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$departmentsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-department', function () {
        var departmentId = $(this).attr("data-department-id");
        var departmentName = $(this).attr('data-department-name');

        deleteDepartments(departmentId, departmentName);
    });

    $(document).on("click",".nav-package", function (event) {
        var type = $(this).attr("data-type");
        $("[name='FilterType']").val(type);
        $(".nav-package").removeClass("active");
        $(this).addClass("active")
        _$departmentsTable.ajax.reload();
    })

    function deleteDepartments(departmentId, departmentName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                departmentName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _warehouseService.delete({
                        id: departmentId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$departmentsTable.ajax.reload();
                    });
                }
            }
        );
    }

    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    })

    async function loadWarehouse() {
        return await abp.services.app.warehouse.getFull().done(function (data) {
            var dropdown = $("#WarehouseCode");
            dropdown.empty();
            dropdown.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, warehouse) {
                dropdown.append('<option value="' + warehouse.id + '">' + warehouse.name + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }

    // $(document).on('click', '.edit-department', function (e) {
    //     var departmentId = $(this).attr("data-department-id");
    //
    //     e.preventDefault();
    //     abp.ajax({
    //         url: abp.appPath + 'Warehouses/EditModal?Id=' + departmentId,
    //         type: 'POST',
    //         dataType: 'html',
    //         success: function (content) {
    //             $('#DepartmentEditModal div.modal-content').html(content);
    //
    //             loadProvince("#EditProvinceId").then(() => {
    //                 $("#EditProvinceId").trigger("change");
    //             });
    //         },
    //         error: function (e) {
    //         }
    //     });
    // });

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


    $(document).on("change","#EditProvinceId", function () {
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
                    districtDropdown.append('<option ' + selected +' value="' + item.id + '">' + item.name + '</option>');
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

    $(document).on("change","#EditDistrictId", function () {
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
                    wardDropdown.append('<option ' + selected +' value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    // $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', (e) => {
    //     $('.nav-tabs a[href="#department-details"]').tab('show')
    //     loadProvince();
    // });

    abp.event.on('department.edited', (data) => {
        _$departmentsTable.ajax.reload();
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
