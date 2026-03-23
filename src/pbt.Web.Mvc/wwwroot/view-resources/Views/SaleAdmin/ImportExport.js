(function ($) {
    var
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$packageDiv = $('#package-list'),
        _$form = $('#OrderSearchForm'),
        _$packageTable = $('#PackageTable');

    const shippingLines = {
        1: { text: 'Lô', color: 'blue' },
        2: { text: 'TMĐT', color: 'purple' },
    };
    $('.select2').select2({
        theme: "bootstrap4", width: "80%",
        allowClear: true,
        placeholder: l('SelectCustomer'),
        allowClear: true,
        ajax: {
            delay: 550, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/customer/GetCustomersImportExportView',
            type: "GET",
            data: function (params) {
                var data = _$form.serializeFormToObject(true);
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
        //startDate: moment(), // Set default start date to today
        //endDate: moment(),    // Set default end date to today
        "locale": {
            "format": "DD/MM/YYYY",
            "separator": " - ",
            "applyLabel": l('Apply'),
            "cancelLabel": l('Cancel'),
            "fromLabel": l('From'),
            "toLabel": l('To'),
            "customRangeLabel": l('Select'),
            "weekLabel": "W",

        },
        autoUpdateInput: false,
        "cancelClass": "btn-danger"
    });
    $('.date-range').on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format('DD/MM/YYYY'));
        $('.end-date.' + target).val(picker.endDate.format('DD/MM/YYYY'));
        $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
    });


    $('.date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        var target = $(this).attr('target');
        $('.start-date.' + target).val('');
        $('.end-date.' + target).val('');
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

    var packagesTable = _$packageTable.DataTable({
      /*  lengthMenu: [50, 100],*/
        paging: false,
        serverSide: true,
        processing: false,
        deferLoading: 0, 
        
        listAction: {
            ajaxFunction: _packageService.getPackageImportExportWithBag,
            inputFilter: function () {
                var data =  _$form.serializeFormToObject(true);
                return data;
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => ordersTable.draw(false)
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
                data: 'exportDate',
                sortable: false,
                width: 160,
                render: (data, type, row, meta) => {
                    return row.exportDate ? formatDateToDDMMYYYYHHmm(row.exportDate) : '';
                }
            },
            {
                targets: 1,
                data: 'customerName',
                sortable: false,
                width: 160,
            },
            {
                targets: 2,
                data: 'bagType',
                sortable: false,
                width: 140,
                render: (data, type, row, meta) => {
                    
                    return row.bagType == 1 ?
                        '<a target="_blank" href="/bags/detail/'+row.bagId+'"><strong class="text-success"> <i class="fas fa-cubes"></i> ' + row.bagNumber + '</strong></a>' :
                        '<a target="_blank" href="/Packages/detail/'+row.packageId+'"><strong class="text-info"> <i class="fas fa-cube"></i> ' + row.packageCode + '</strong></a>';
                }

            },

            {
                targets: 3,
                data: 'weight',
                sortable: false,
                width: 100,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return `${formatThousand(data)}`;
                }
            },
            {
                targets: 4,
                data: 'dimension',
                sortable: false,
                width: 100,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toFixed(1) : '0';
                }
            },
            {
                targets: 5,
                data: 'packageCount',
                sortable: false,
                className: 'text-right',
                width: 100
            },
            {
                targets: 6,
                data: 'shippingPartner',
                sortable: false,
                width: 140
            },
            {
                targets: 7,
                data: 'characteristic',
                sortable: false,
                width: 140
            },
            {
                targets: 8,
                data: 'importDate',
                sortable: false,
                width: 160,
                render: (data, type, row, meta) => {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '';
                }
            },

            {
                targets: 9,
                data: 'unitPrice',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 10,
                data: 'shippingFeeCN',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 11,
                data: 'securingCost',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 12,
                data: 'shippingFeeVN',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 13,
                data: 'totalFee',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 14,
                data: 'shippingFeeAbsorbedByWarehouse',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 15,
                data: 'originShippingCost',
                sortable: false,
                width: 120,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? data.toLocaleString() + ' ₫' : '0 ₫'
                }
            },
            {
                targets: 16,
                data: 'shippingPartnerVN',
                sortable: false,
                width: 140
            },
            {
                targets: 17,
                data: 'exportDateVN',
                sortable: false,
                width: 160,
                render: (data, type, row, meta) => {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '';
                }
            }


        ]
    });

    $('.btn-search').on('click', (e) => {
        packagesTable.ajax.reload();
     //   loadCustomer();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            packagesTable.ajax.reload();
          //  loadCustomer();
            return false;
        }
    });

    $('.btn-export-excel').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#OrderSearchForm').serializeFormToObject();
        filterData.isExcel = true;
     
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/SaleAdmin/ImportExportDownload?' + url;
        abp.ui.clearBusy();

    });

})(jQuery);
