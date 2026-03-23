(function ($) {
    var _warehouseService = abp.services.app.bag,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#DepartmentsTable');
    // Initialize date range picker
    $('.date-range').daterangepicker({
        opens: 'left',
        startDate: moment(), // Set default start date to today
        endDate: moment(),    // Set default end date to today
        ranges: {
            [l('Today')]: [moment(), moment()],
            [l('Yesterday')]: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
            [l('Last7Days')]: [moment().subtract(6, 'days'), moment()],
            [l('Last30Days')]: [moment().subtract(29, 'days'), moment()],
            [l('ThisMonth')]: [moment().startOf('month'), moment().endOf('month')],
            [l('LastMonth')]: [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        },
        "showCustomRangeLabel": true,
    }, function(start, end, label) {
        // Reload the DataTable after the date range is selected
        setTimeout(() => {
            _$bagsTable.ajax.reload();
        },200)
    });
    var _$bagsTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _warehouseService.getBagsFilter,
            inputFilter: function () {
                return $('#PackageSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$bagsTable.draw(false)
            }
        ],

        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Bag'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('ZeroRecords')
        },
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: ''
            },
            {
                targets: 1,
                data: null,
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `   <a href="/bags/bagging/${row.id}" type="button" class="btn btn-sm btn-link" data-bag-id="${row.id}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${row.bagCode}`,
                        '   </a>'
                    ].join('');
                }
            },
            {
                targets: 2,
                data: 'receiver',
                sortable: false
            },
            {
                targets: 3,
                data: 'bagTypeName',
                sortable: false
            },
            {
                targets: 4,
                data: 'customerCodeName',
                sortable: false
            },
            {
                targets: 5,
                data: 'id',
                sortable: false
            },
            {
                targets: 6,
                data: 'creationTimeFormat',
                sortable: false
            },
            {
                targets: 7,
                data: 'warehouseCreateName',
                sortable: false
            },
            {
                targets: 8,
                data: 'warehouseDestinationName',
                sortable: false
            },
            {
                targets: 9,
                data: null,
                sortable: false,
                width: 300,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <a target="_blank" href="/bags/detail/${row.id}" type="button" class="btn btn-sm bg-secondary" data-department-id="${row.id}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Detail')}`,
                        '   </a>',

                        `   <button type="button" class="btn btn-sm bg-danger delete-department" data-department-id="${row.id}" data-department-name="${row.name}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>'
                    ].join('');
                }
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
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
            _$bagsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-department', function () {
        var departmentId = $(this).attr("data-department-id");
        var departmentName = $(this).attr('data-department-name');

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
                    _warehouseService.delete({
                        id: departmentId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$bagsTable.ajax.reload();
                    });
                }
            }
        );
    }

    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    })
    $(document).on("click", ".nav-bag", function (event) {
        var type = $(this).attr("data-type");
        $("[name='FilterType']").val(type);
        $(".nav-bag").removeClass("active");
        $(this).addClass("active")
        _$bagsTable.ajax.reload();
    })

    async function loadWarehouse() {
        return await abp.services.app.warehouse.getFull().done(function (data) {
            var dropdown = $(".search-warehouse");
            dropdown.empty();
            dropdown.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, warehouse) {
                dropdown.append('<option value="' + warehouse.id + '">' + warehouse.name + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }

    $(document).on('click', '.edit-department', function (e) {
        var departmentId = $(this).attr("data-department-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Warehouses/EditModal?Id=' + departmentId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#DepartmentEditModal div.modal-content').html(content);

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

    $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', (e) => {
        $('.nav-tabs a[href="#department-details"]').tab('show')
        loadProvince();
    });

    abp.event.on('department.edited', (data) => {
        _$bagsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$bagsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$bagsTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
