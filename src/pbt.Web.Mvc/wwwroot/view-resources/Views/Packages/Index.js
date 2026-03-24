(function ($) {
    var _packageService = abp.services.app.package,
        _warehouseTransferService = abp.services.app.warehouseTransfer,

        _orderNoteService = abp.services.app.orderNote,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#packageCreateModal'),
        _$form = _$modal.find('form'),
        _$warehouseTransferForm = $('#WarehouseTransfer').find('form'),
        _$table = $('#PackagesTable');

    const warehouseStatusDescriptions = {
        "0": { text: l('MissingInformation'), color: 'red' },
        "1": { text: l('WarehouseStatus.InStock'), color: 'blue' },
        "2": { text: l('WarehouseStatus.OutOfStock'), color: 'green' }
    };

    const packageDeliveryStatusDescriptions = {
        "0": { text: l('MissingInfo'), color: 'danger' },
        "1": { text: l('Initiate'), color: 'secondary' },
        "2": { text: l('InTransit'), color: 'info' },
        "3": { text: l('WaitingForShipping'), color: 'warning' },
        "4": { text: l('Shipping'), color: 'primary' },
        "5": { text: l('InWarehouseVN'), color: 'info' },
        "6": { text: l('WaitingForDelivery'), color: 'warning' },
        "7": { text: l('DeliveryRequest'), color: 'info' },
        "8": { text: l('DeliveryInProgress'), color: 'info' },
        "9": { text: l('Delivered'), color: 'success' },
        "10": { text: l('Completed'), color: 'success' },
        "11": { text: l('WaitingForReturn'), color: 'warning' },
        "13": { text: l('WarehouseTransfer'), color: 'info' },

    };
    const shippingLines = {
        1: { text: l('Lô'), color: 'blue' },
        2: { text: l('TMDT'), color: 'green' },
        3: { text: l('CN'), color: 'info' },
        4: { text: l('XT'), color: 'warning' },
    };

    $('.select2').select2({
        theme: "bootstrap4", width: "80%",
        allowClear: true,
        placeholder: l('SelectCustomer'),
        allowClear: true,
        ajax: {
            delay: 550, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Package/GetCustomersByDateSelect',
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

    $('.btn-clear-value').on('click', function () {
        var target = $(this).attr('target');
        var targetInput = $(this).attr('target-date');
        if (target) {
            var targetValue = $(this).attr('target-value');
            if (targetValue) {
                $('[name="' + target + '"]').val(targetValue);
            }
            else {
                $('[name="' + target + '"]').val('');
                $('.' + targetInput).val('');
            }
        }
    });

    var _$packagesTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: function () {
                return _packageService.getAllPackagesFilter(arguments[0]).done(function (value) {
                    $("#totalPackage").text(formatThousand(value.totalCount));
                    $("#totalWeight").text(formatThousand(value.totalWeight));
                });
            },
            inputFilter: function () {
                return $('#PackageSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { _$packagesTable.draw(false); }
            }
        ],
        lengthMenu: [25, 50, 100, 200],
        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Package'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('zeroRecords')
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
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'packageNumber',
                sortable: false,
                render: function (data, type, row, meta) {
                    return '<a target="_blank" href="/Packages/Detail/' + row.id + '" class="text-primary font-weight-bold">' + data + '<strong>';
                }
            },
            {
                targets: 2,
                data: 'trackingNumber',
                sortable: false,
                render: function (data, type, row, meta) {
                    if (row.parentId && row.parentId > 0 && row.waybillNumber) {
                        return `<a target="_blank" href="/Orders/Detail/${row.orderId} " class="text-primary font-weight-bold">${row.trackingNumber} - (${row.waybillNumber})<strong>`;
                    }
                    else {
                        /* return `${row.trackingNumber}`;*/
                        return '<a target="_blank" href="/Orders/Detail/' + row.orderId + '" class="text-primary font-weight-bold">' + data + '<strong>';
                    }

                }
            },
            {
                targets: 3,
                data: 'bagNumber',
                sortable: false,
                render: function (data, type, row, meta) {
                    //return data;
                    if (data) {
                        return '<a target="_blank" href="/Bags/Detail/' + row.bagId + '" class="text-primary font-weight-bold">' + data + '<strong>';
                    }
                    return '';
                }
            },
            {
                targets: 4,
                data: 'weight',
                sortable: false,
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return row.weight && row.weight > 0 ? formatThousand(row.weight) + '(kg)' : '-';
                }
            },
            {
                targets: 5,
                data: 'volume',
                sortable: false,
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return row.volume ? row.volume + ' (m3)' : '-';
                }
            },
            {
                targets: 6,
                data: 'description',
                sortable: false
            },
            {
                targets: 7,
                sortable: false,
                render: function (data, type, row, meta) {
                    return row.customerName;
                }
            },
            {
                targets: 8,
                sortable: false,
                render: function (data, type, row, meta) {
                    var text = '';
                    if (row.isWoodenCrate) {
                        text += '<span class=" d-block">' + l('WoodenPackaging') + '</span>';
                    }
                    if (row.isShockproof) {
                        text += '<span class=" d-block">' + l('ShockproofPackaging') + '</span>';
                    }
                    if (row.isDomesticShipping) {
                        text += '<span class=" d-block">' + l('DomesticTransportation') + '</span>';
                    }
                    return text;
                }
            },
            {
                targets: 9,
                sortable: false,
                data: 'domesticShippingFee',
                render: function (data, type, row, meta) {
                    return row.domesticShippingFee > 0 ? formatThousand(row.domesticShippingFee) : '';
                }
            },
            {
                targets: 10,
                sortable: false,
                render: function (data, type, row, meta) {
                    return row.warehouseCreateName;
                }
            },
            {
                targets: 11,
                sortable: false,
                render: function (data, type, row, meta) {
                    return row.warehouseDestinationName;
                }
            },
            {
                // CurrentWarehouse
                targets: 12,
                sortable: false,
                render: function (data, type, row, meta) {

                    return row.warehouseName;
                }
            },
            {
                targets: 13,
                data: 'creationTimeFormat',
                sortable: false,
                width: 250,
                render: (data, type, row, meta) => {

                    // Ngày tạo
                    const creation = formatDateToDDMMYYYYHHmmss(row.creationTime) || '';
                    const matchTime = formatDateToDDMMYYYYHHmmss(row.matchTime) || '';
                    // Ngày xuất kho TQ
                    const exportDate = row.exportDate ? formatDateToDDMMYYYYHHmmss(row.exportDate) : '';
                    // Ngày nhập kho VN
                    const importDate = row.importDate ? formatDateToDDMMYYYYHHmmss(row.importDate) : '';

                    const deliveryTime = row.deliveryTime ? formatDateToDDMMYYYYHHmmss(row.deliveryTime) : '';
                    return [
                        `<div>${l('CreationTime')}: <strong> ${creation}</strong> </div>`,
                        `<div>${l('MatchTime')}: <strong> ${matchTime}</strong> </div>`,
                        `<div>${l('ExportDateCN')}: <strong> ${exportDate}</strong> </div>`,
                        `<div>${l('ImportDateVN')}: <strong> ${importDate}</strong> </div>`,
                        `<div>${l('DeliveryTimeVN')}: <strong> ${deliveryTime}</strong> </div>`
                    ].join('');
                }
            },

            {
                targets: 14,
                data: 'warehouseStatus',
                sortable: false,
                render: function (data, type, row, meta) {
                    var status = warehouseStatusDescriptions[data];
                    return '<span style="color: ' + status.color + '">' + status.text + '</span>';
                }
            },
            {
                targets: 15,
                data: 'shippingLineId',
                sortable: false,
                render: function (data, type, row, meta) {
                    const status = shippingLines[row.shippingLineId];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : ''}</strong>`;
                }
            },
            {
                targets: 16,
                data: 'shippingStatusName',
                sortable: false,
                render: function (data, type, row, meta) {

                    var status = _serverPackageDeliveryStatusOptions.find(x => x.value == row.shippingStatus);
                    var statusText = status ? status.text : 'Không xác định';
                    var statusColor = status && packageDeliveryStatusDescriptions[row.shippingStatus] ? packageDeliveryStatusDescriptions[row.shippingStatus].color : 'secondary';

                    // Chỉ hiển thị nút sửa khi isAdmin == true
                    var editButtonHtml = '';
                    if (typeof isAdmin !== 'undefined' && isAdmin === true) {
                        editButtonHtml = `<a href="javascript:;" class="edit-status-btn ml-2" title="${l('Edit')}">
                                <i class="fas fa-pencil-alt"></i>
                              </a>`;
                    }

                    return `<div class="status-container" data-package-id="${row.id}" data-current-status="${row.shippingStatus}">
                  <div class="status-display d-flex align-items-center">
                    <span class="badge badge-${statusColor}">${statusText}</span>
                    ${editButtonHtml}                </div>
                   <div class="status-edit" style="display: none;">
                    <div class="d-flex align-items-center">
                        <select class="form-control status-select mr-1" style="width: auto; display: inline-block;">
                            ${Object.entries(_serverPackageDeliveryStatusOptions).map(([key, desc]) =>
                        `<option value="${desc.value}" ${desc.value == row.shippingStatus ? 'selected' : ''}>${desc.text}</option>`
                    ).join('')}</select>
                        <button class="btn btn-success btn-sm save-status-btn mr-1"><i class="fas fa-check"></i></button>
                        <button class="btn btn-secondary btn-sm cancel-status-btn"><i class="fas fa-times"></i></button>
                    </div>
                </div>
            </div>
        `;
                }
            },
            {
                targets: 17,
                data: null,
                sortable: false,
                width: 40,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    var adminEditHtml = isAdmin
                        ? `<a href="/Packages/EditByAdmin/${row.id}" type="button" class="dropdown-item bg-primary"> <i class="fas fa-wrench"></i> ${l('AdminEdit')} </a>`
                        : '';
                
                    return [
                        ' <div class="btn-group"> ',
                        '   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">',
                        '</button>',
                        ' <div class="dropdown-menu" style="">',
                        '<a href="/Packages/Detail/' + row.id + '" type="button" class="dropdown-item  bg-primary" data-package-id="' + row.orderId + '"> <i class="fas fa-eye"></i> ' + l('Detail') + ' </a>',
                        `   <a type="button" class="dropdown-item bg-warning view-order-mote" data-order-id="${row.id}" title="${l('Note')}" data-toggle="modal" data-target="#OrderNote" > ` +
                        `       <i class="fas fa-pencil-alt"></i> ${l('Note')}` +
                        '   </a>',

                        (row.bagId && row.bagId > 0) ? ('<a href="#" type="button"  class="dropdown-item  bg-danger unbag" data-package-id="' + row.id + '" data-package-name="' + row.trackingNumber + '">  <i class="fas fa-unlink"></i> ' + l('Unbag') + '   </a>') : '',

                        row.shippingStatus === 8 // Chỉ hiển thị nếu trạng thái là "Đang giao"
                            ? '<button type="button" class="dropdown-item bg-success mark-as-delivered" data-package-id="' + row.id + '"  data-package-name="' + row.trackingNumber + '"><i class="fas fa-truck"></i> ' + l('MarkAsDelivered') + '</button>'
                            : '',
                        row.shippingStatus === 9
                            ? '<button type="button" class="dropdown-item bg-success mark-as-completed" data-package-id="' + row.id + '"  data-package-name="' + row.trackingNumber + '"><i class="fas fa-check"></i> ' + l('MarkAsCompleted') + '</button>'
                            : '',

                        row.shippingStatus === 5
                            ? '<button   type="button" class="dropdown-item  bg-info btn-change-warehouse"  data-toggle="modal" data-target="#WarehouseTransfer" data-package-id="' + row.id + '"data-warehouse-id="' + data.warehouseId + '">  <i class="fas fa-pallet"></i> ' + l('ChangeWarehouse') + '</button>'
                            : '',
                        row.bagId && row.bagId > 0 ? `` : `<a href="/Packages/Edit/${row.id}" type="button" class="dropdown-item  bg-primary" data-package-id="${row.id}">  <i class="fas fa-pencil-alt"></i> ${l('UpdateInfo')} </a>`,
                        isAdminOrSaleAdmin ? ('<a href="/Packages/Finance/' + row.id + '" type="button" class="dropdown-item bg-success"> <i class="fas fa-dollar-sign"></i> ' + l('PackageFinance') + ' </a>') : '',
                        adminEditHtml,
                        '<a href="#" type="button"  class="dropdown-item  bg-danger delete-package" data-package-id="' + row.id + '" data-package-name="' + row.trackingNumber + '">  <i class="fas fa-trash"></i> ' + l('Delete') + '   </a>',
                        '    </div>',
                        '   </div>'
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
        var data = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _packageService.create(data).done(function () {

            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$packagesTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.edit-status-btn', function (e) {
        e.stopPropagation();
        var container = $(this).closest('.status-container');
        container.find('.status-display').css({
            'display': 'none !important',
            'visibility': 'hidden'
        });
        container.find('.status-edit').show();
    });

    $(document).on('click', '.save-status-btn', function (e) {
        e.stopPropagation();
        var container = $(this).closest('.status-container');
        var packageId = container.data('package-id');
        var newStatus = container.find('.status-select').val();
        const saveButton = $(this);
        const originalHtml = saveButton.html();
        saveButton.html('<i class="fas fa-spinner fa-spin"></i>');
        saveButton.prop('disabled', true);
        _packageService.editStatus({
            id: packageId,
            shippingStatus: parseInt(newStatus)
        }).done(function () {
            abp.notify.info(l('SavedSuccessfully'));
            _$packagesTable.ajax.reload();
        }).fail(function (error) {
            abp.notify.error(l('ErrorWhileSaving'));
        }).always(function () {
            saveButton.html(originalHtml);
            saveButton.prop('disabled', false);
        });
    });

    $(document).on('click', '.cancel-status-btn', function (e) {
        e.stopPropagation();
        var container = $(this).closest('.status-container');
        container.find('.status-edit').hide();
        container.find('.status-display').css({
            'display': 'block !important',
            'visibility': 'initial'
        });
    });

    $(document).on('click', '.btn-change-warehouse', function () {

        var packageId = $(this).data('package-id');
        var currentWarehouse = $(this).data('warehouse-id');

        // Gán giá trị packageId vào input ẩn trong modal
        $('#WarehouseTransfer').find('input[name="PackageId"]').val(packageId);


        // Reset các trường trong modal
        $('#WarehouseTransfer').find('select[name="ToWarehouse"]').val(currentWarehouse);
        $('#WarehouseTransfer').find('textarea[name="Note"]').val('');
    });

    $(document).on('click', '.btn-save-warehouse-transfer', function () {
        var data = $(_$warehouseTransferForm).serializeFormToObject();
        _warehouseTransferService.changePackageWarehouse(data).done(function () {
            $('#WarehouseTransfer').modal('hide');

            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$packagesTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy($('#WarehouseTransfer'));
        });
    });




    $('.btn-export').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Packages/Download?' + url;
        abp.ui.clearBusy();
    });

    $('.btn-export-min').on('click', function () {
        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#PackageSearchForm').serializeFormToObject();

        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Packages/DownloadMin?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    $(document).on('click', '.delete-package', function () {
        var packageId = $(this).attr("data-package-id");
        var packageCode = $(this).attr('data-package-name');

        deletePackage(packageId, packageCode);
    });

    $(document).on('click', '.mark-as-delivered', function () {

        var packageId = $(this).data('package-id');
        var packageCode = $(this).attr('data-package-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToMarkAsDelivered'),
                '<strong class="text-danger">' + packageCode + '</strong>')
            ,
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    abp.ui.setBusy();
                    _packageService.markAsDelivered(packageId).done(() => {
                        abp.notify.info(l('SuccessfullyUpdated'));
                        _$packagesTable.ajax.reload();
                    });
                    abp.ui.clearBusy();
                }
            }
        );

    });

    $(document).on('click', '.mark-as-completed', function () {
        var packageId = $(this).data('package-id');
        var packageCode = $(this).attr('data-package-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToMarkAsCompleted'),
                '<strong class="text-danger">' + packageCode + '</strong>')
            ,
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    abp.ui.setBusy();
                    _packageService.markAsCompleted(packageId).done(() => {
                        abp.notify.info(l('SuccessfullyUpdated'));
                        _$packagesTable.ajax.reload();
                    });
                    abp.ui.clearBusy();
                }
            }
        );
    });


    $(document).on("click", ".nav-package", function (event) {
        var type = $(this).attr("data-type");
        $("[name='FilterType']").val(type);
        $(".nav-package").removeClass("active");
        $(this).addClass("active")
        _$packagesTable.ajax.reload();
    })

    function deletePackage(packageId, packageCode) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                '<strong class="text-danger">' + packageCode + '</strong>'),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _packageService.delete({
                        id: packageId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$packagesTable.ajax.reload();
                    });
                }
            },
            { isHtml: true }
        );
    }

    $(document).on('click', '.unbag', function () {
        var packageId = $(this).attr("data-package-id");
        var packageCode = $(this).attr('data-package-name');

        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToUnbag'),
                '<strong class="text-danger">' + packageCode + '</strong>'),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _packageService.unBag(packageId).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$packagesTable.ajax.reload();
                    });
                }
            },
            { isHtml: true }
        );
    });


    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    })



    async function loadProductGroup() {
        return await abp.services.app.productGroupType.getAllList().done(function (data) {
            var dropdown = $("#Feature");
            dropdown.empty();
            dropdown.append('<option value="-1">' + l('Feature') + '</option>');
            $.each(data, function (index, feature) {
                dropdown.append('<option value="' + feature.id + '">' + feature.name + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
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

    // $(document).on('click', 'a[data-target="#packageCreateModal"]', (e) => {
    //     $('.nav-tabs a[href="#package-details"]').tab('show')
    //     loadProvince();
    // });

    abp.event.on('package.edited', (data) => {
        _$packagesTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$packagesTable.ajax.reload();
        // loadCustomer();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$packagesTable.ajax.reload();
            //  loadCustomer();
            return false;
        }
    });

    loadProductGroup();
    //  loadCustomer();
    //  loadWarehouse();



    $(document).on('click', '.view-order-mote', function () {
        var orderId = $(this).data('order-id');
        $('#hidden-order-id').val(orderId);
        _orderNoteService.getAllByOrderId(orderId).done(function (data) {
            $('#noteList').empty();
            if (Array.isArray(data) && data.length > 0) {
                data.forEach(function (note) {
                    const row = `
                        <tr>
                            <td>${note.creatorUserName}</td>
                            <td>${formatDateToDDMMYYYYHHmm(note.creationTime)}</td>
                            <td>${note.content}</td>
                        </tr>
                    `;
                    $('#noteList').append(row);
                });
            } else {
                $('#noteList').append('<tr><td colspan="3" class="text-center">Không có ghi chú nào.</td></tr>');
            }
        });
    });

    $('#btnSaveNote').on('click', function () {
        const content = $('#noteContent').val().trim();
        var orderId = $('#hidden-order-id').val();
        if (!content) {
            abp.message.warn('Vui lòng nhập nội dung ghi chú.');
            return;
        }
        var note = {
            OrderId: orderId,
            Content: content,
        };
        _orderNoteService.create(note).done(function (data) {

            $('#noteList tr').each(function () {
                if ($(this).find('td[colspan="3"]').length > 0) {
                    $(this).remove();
                }
            });

            const newRow = `
                <tr>
                    <td>${data.creatorUserName}</td>
                    <td>${formatDateToDDMMYYYYHHmm(data.creationTime)}</td >
                    <td>${data.content}</td>
                </tr>
            `;
            $('#noteList').prepend(newRow);
            $('#noteContent').val('');
        }).always(function () {
            abp.ui.clearBusy(_$waybillModal);
        });
    });

})(jQuery);
