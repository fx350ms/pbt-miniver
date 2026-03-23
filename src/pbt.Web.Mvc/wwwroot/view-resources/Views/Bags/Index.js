(function ($) {
    var _bagService = abp.services.app.bag,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#DepartmentsTable');

    // Initialize date range picker
    $('.date-range').daterangepicker({
        // startDate: moment().subtract(6, 'days').startOf('day'),
        "locale": {
            "format": "DD/MM/YYYY HH:mm:ss",
            "separator": " - ",
            "applyLabel": l('Apply'),
            "cancelLabel": l('Cancel'),
            "fromLabel": l('From'),
            "toLabel": l('To'),
            "customRangeLabel": l('Select'),
            "weekLabel": "W",

        },
        timePickerSeconds: true, // Cho phép chọn giây
        timePicker24Hour: true,
        //  autoUpdateInput: true,
        "cancelClass": "btn-danger",
        "timePicker": true,
    }
    );

    $('.date-range').on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format("DD/MM/YYYY HH:mm:ss"));
        $('.end-date.' + target).val(picker.endDate.format("DD/MM/YYYY HH:mm:ss"));
        $(this).val(picker.startDate.format("DD/MM/YYYY HH:mm:ss") + ' - ' + picker.endDate.format("DD/MM/YYYY HH:mm:ss"));
    });

    $('.date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        var target = $(this).attr('target');
        $('.start-date.' + target).val('');
        $('.end-date.' + target).val('');
    });

    $('.date-range').each(function () {

        var picker = $(this).data('daterangepicker'); // Lấy đối tượng daterangepicker
        var target = $(this).attr('target'); // Lấy 'target' từ input .date-range

        if (picker) {
            // Gán giá trị mặc định cho các trường .start-date và .end-date
            $('.start-date.' + target).val('');
            $('.end-date.' + target).val('');

            // Cập nhật giá trị hiển thị trên input .date-range chính
            $(this).val('');
        }
    });


    $('.btn-clear-date').on('click', function () {
        var target = $(this).attr('target');
        var targetInput = $(this).attr('target-date');
        if (target) {
            $('[name="' + target + '"]').val('');
            $('.' + targetInput).val('');
        }
    });

    const bagStatusDescriptions = {
        1: { text: l('BagShippingStatus.Initiated'), color: 'blue' },
        2: { text: l('BagShippingStatus.WaitingForShipping'), color: 'purple' },
        3: { text: l('BagShippingStatus.InTransit'), color: 'green' },
        4: { text: l('BagShippingStatus.GoToWarehouse'), color: 'orange' },
        5: { text: l('BagShippingStatus.WaitingForDelivery'), color: 'green' },
        6: { text: l('BagShippingStatus.Delivery'), color: 'blue' },
        7: { text: l('BagShippingStatus.Delivered'), color: 'green' },
        12: { text: l('BagShippingStatus.DeliveryRequest'), color: 'blue' },
        13: { text: l('BagShippingStatus.WarehouseTransfer'), color: 'blue' },
    };

    const bagTypes = {
        1: { text: l('SeparateBag'), color: 'blue' },
        2: { text: l('InclusiveBag'), color: 'green' },

    };
    const shippingTypes = {
        1: { text: l('Lô'), color: 'blue' },
        2: { text: l('TMDT'), color: 'green' },
        3: { text: l('Chính ngạch'), color: 'orange' },
        4: { text: l('Xách tay'), color: 'purple' }

    };
    //// Initialize select2
    //$('.select2').select2({
    //    theme: "bootstrap4", width: "100%",
    //    allowClear: true
    //});
    $('.select2').select2({
        theme: "bootstrap4", width: "80%",
        allowClear: true,
        placeholder: l('SelectCustomer'),
        allowClear: true,
        ajax: {
            delay: 550, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/bag/GetCustomersByDateSelect',
            type: "GET",
            data: function (params) {
                //return {
                //    Keyword: params.term, // search term
                //};
                var data = $('#PackageSearchForm').serializeFormToObject(true);
                data.keyword = params.term;
                return data;
            },

            processResults: function (data) {

                return {
                    results: data.result
                };

            },
            // dataType: 'json',
        },
    });

    var _$bagsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: function () {
                return _bagService.getPagedList(arguments[0]).done(function (data) {
                    $("#totalBag").text(formatThousand(data.totalCount));
                    $("#totalWeight").text(formatThousand(data.totalWeight, 2));
                });
            },
            inputFilter: function () {
                var data = $('#PackageSearchForm').serializeFormToObject(true);
                return data;
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
        lengthMenu: [25, 50, 100, 200],
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
                data: 'bagCode',
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `   <a target="_blank" href="/bags/detail/${row.id}"  >`,
                        row.bagCode,
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
                data: 'totalPackages',
                className: 'text-right',
                sortable: false,
                render: (data, type, row, meta) => {
                    return data | 0;
                }
            },
            {
                targets: 4,
                width: 80,
                className: 'text-right',
                data: 'weight',
                sortable: false,
                render: (data, type, row, meta) => {

                    return FormatNumberToDisplay(data, 2);//  return `${formatNumberThousand((data | 0), 2)}`;
                }
            },
            {
                targets: 5,
                data: 'bagType',
                sortable: false,
                render: (data, type, row, meta) => {
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const bagType = bagTypes[data];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${bagType ? bagType.color : 'black'};">${bagType ? bagType.text : 'Chưa xác định'}</strong>`;
                }
            },
            {
                targets: 6,
                data: 'customerName',
                sortable: false
            },

            {
                targets: 7,
                data: null,
                sortable: false,
                width: 250,
                render: (data, type, row, meta) => {

                    // Ngày tạo
                    const creation = formatDateToDDMMYYYYHHmmss(row.creationTime) || '';
                    // Ngày xuất kho TQ
                    const exportChina = row.exportDate ? formatDateToDDMMYYYYHHmmss(row.exportDate) : '';
                    // Ngày nhập kho VN
                    const importVN = row.importDate ? formatDateToDDMMYYYYHHmmss(row.importDate) : '';


                    return [
                        `<div>${l('CreationTime')}: <strong> ${creation}</strong> </div>`,
                        `<div>${l('ExportDateCN')}: <strong> ${exportChina}</strong> </div>`,
                        `<div>${l('ImportDateVN')}: <strong> ${importVN}</strong> </div>`
                    ].join('');
                }
            },
            {
                targets: 8,
                data: 'warehouseCreateName',
                sortable: false
            },
            {
                targets: 9,
                data: 'warehouseDestinationName',
                sortable: false
            },

            {
                targets: 10,
                data: 'shippingType',
                width: 160,
                sortable: false,
                render: (data, type, row, meta) => {
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = shippingTypes[data];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : 'Chưa xác định'}</strong>`;
                }
            },
            {
                targets: 11,
                data: 'shippingStatus',
                width: 160,
                sortable: false,
                render: (data, type, row, meta) => {
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = bagStatusDescriptions[data];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : 'Chưa xác định'}</strong>`;
                }
            },
            {
                targets: 12,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    var deleteHtml = hasRoleDeleteBag && (
                        row.shippingStatus == 1 || row.shippingStatus == 2 || row.shippingStatus == 3
                    ) ?
                        (`<a type="button" class=" dropdown-item btn btn-sm bg-danger delete-bag" data-id="${row.id}" data-name="${row.bagCode}">`
                            + `<i class="fas fa-trash"></i> ${l('Delete')}`
                            + `</a>`
                        ) : '';

                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        ` </button>`,
                        ` <div class="dropdown-menu" style="">`,

                        `   <a   href="/bags/detail/${row.id}" type="button" class=" dropdown-item btn btn-sm bg-primary" data-department-id="${row.id}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Detail')}`,
                        '   </a>',
                        deleteHtml,
                        (row.shippingStatus == 4 ?
                            '<button   type="button" class="dropdown-item  bg-info btn-change-warehouse"  data-toggle="modal" data-target="#WarehouseTransfer" data-id="' + row.id + '" >  <i class="fas fa-pallet"></i> ' + l('ChangeWarehouse') + '</button>'
                            : ''),

                        `    </div>`,
                        `   </div>`
                    ].join('');

                }
            }
        ],
        rowCallback: function (row, data, index) {
            // Kiểm tra nếu weight = 0
            if (!data.totalWeightPackage || data.totalWeightPackage === 0) {
                // Thêm class hoặc style để thay đổi màu sắc của hàng
                $(row).css('background-color', '#ffcccc'); // Màu nền đỏ nhạt
            }
        }
    });


    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var department = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _bagService.create(department).done(function () {

            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$bagsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $('.btn-export-excel').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Bags/Download?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    $('.btn-export-bag-detail').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Bags/DownloadPackage?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    $('.btn-export-manifest').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Bags/DownloadManifest?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    $('.btn-export-manifest-en').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Bags/DownloadManifestEn?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    function toQueryString(obj) {
        if (!obj) return '';
        return Object.keys(obj)
            .map(key => {
                const value = obj[key];
                if (value === undefined || value === null) return '';
                if (Array.isArray(value)) {
                    return value
                        .map(v => encodeURIComponent(key) + '=' + encodeURIComponent(v))
                        .join('&');
                }
                return encodeURIComponent(key) + '=' + encodeURIComponent(value);
            })
            .filter(x => x.length > 0)
            .join('&');
    }

    $(document).on('click', '.delete-bag', function () {
        var bagId = $(this).attr("data-id");
        var bagCode = $(this).attr('data-name');

        deleteBag(bagId, bagCode);
    });

    function deleteBag(bagId, bagCode) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                bagCode),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _bagService.deleteBag(bagId).done((result) => {
                        if (result.success) {
                            abp.notify.info(l('SuccessfullyDeleted'));
                            _$bagsTable.ajax.reload();
                        }
                        else {
                            abp.notify.error(result.message);
                        }
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
     
    function getShippingPartner() {
        abp.services.app.shippingPartner.getAllShippingPartnersByLocation(0).done(function (response) {
            if (response) {
                var selectShippingPartner = $("#shippingPartner");
                selectShippingPartner.append('<option value="">' + l('SelectShippingPartner') + '</option>');
                response.forEach(function (partner) {
                    selectShippingPartner.append(
                        `<option value="${partner.id}">(${partner.code})${partner.name}</option>`
                    );
                });
            }
        })
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

    $('.btn-remove-content').on('click', function () {
        var target = $(this).attr('target');
        var targetVal = $(this).attr('target-val');

        $(target).val(targetVal || '');
    });

    $('.btn-search').on('click', (e) => {
        _$bagsTable.ajax.reload();
    //    loadCustomer();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$bagsTable.ajax.reload();
          //  loadCustomer();
            return false;
        }
    });
   // loadCustomer();
    getShippingPartner();

    $(document).on('click', '.btn-change-warehouse', function () {
        var bagId = $(this).data('id');

        // Hiển thị modal chuyển kho
        $('#WarehouseTransfer').find('input[name="BagId"]').val(bagId);
        $('#WarehouseTransfer').modal('show');
    });

    $(document).on('click', '.btn-save-bag-warehouse-transfer', function () {

        var _form = $('#WarehouseTransfer').find('form');

        var data = _form.serializeFormToObject();

        abp.ui.setBusy();
        abp.services.app.warehouseTransfer.changeBagWarehouse(data).done(function (result) {
            abp.notify.info(result.message);
            $('#WarehouseTransfer').modal('hide');
            _$bagsTable.ajax.reload();
        }).fail(function (error) {
            abp.notify.error(error.message || 'Có lỗi xảy ra.');
        }).always(function () {
            abp.ui.clearBusy();
        });
    });


})(jQuery);
