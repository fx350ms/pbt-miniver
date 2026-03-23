(function ($) {
   
    var  _bagService = abp.services.app.bag,
        l = abp.localization.getSource('pbt'),
        _$bagDiv = $('#BagFilterCollapse'),
        _$form = _$bagDiv.find('form'),
        _$bagTable = $('#BagTable');

    const shippingLines = {
        0: { text: 'Lô', color: 'blue' },
        1: { text: 'Lô', color: 'blue' },
        2: { text: 'TMĐT', color: 'purple' },
    };
    var formatDate = 'DD/MM/YYYY HH:mm:ss';
    // Initialize date range picker
    $('.date-range').daterangepicker({
        "locale": {
            "format": formatDate,
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
    });

    $('.date-range').on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format(formatDate));
        $('.end-date.' + target).val(picker.endDate.format(formatDate));
        $(this).val(picker.startDate.format(formatDate) + ' - ' + picker.endDate.format(formatDate));
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
   
    var bagsTable = _$bagTable.DataTable({
        paging: true,
        serverSide: true,
        processing: false,
       /* deferLoading: 0, */
        lengthMenu: [50, 100],
        listAction: {
            ajaxFunction: _bagService.getBagsForPartner,
            inputFilter: function () {
                return _$form.serializeFormToObject(true);
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
        lengthMenu: [25, 50, 100],
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'exportDateCN',
                sortable: false,
                width: 120,
                render: (data, type, row, meta) => {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '-'; // Ngày xuất kho TQ
                }
            },
            {
                targets: 2,
                data: 'importDateHN', width: 120,
                sortable: false,
                render: (data, type, row, meta) => {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '-'; // Ngày xuất kho TQ
                }
            },
            {
                targets: 3,
                data: 'exportDateVN',
                sortable: false,
                width: 120,
                render: (data, type, row, meta) => {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '-'; // Ngày xuất kho TQ
                }
            },
            {
                targets: 4,
                data: 'receiver',
                width: 120,
                sortable: false,
                render: (data) => data || '' // Người nhận
            },
            {
                targets: 5,
                width: 80,
                data: 'bagCode',
                sortable: false,
                render: (data) => data || '' // Mã bao
            },
            {
                targets:6,
                data: 'weight',
                width: 60,
                sortable: false,
                className: 'text-right',
                render: (data) => data ? `${data} kg` : '' // Cân nặng (Kg)
            },

            {
                targets: 7,
                data: 'volume',
                width: 60,
                sortable: false,
                className: 'text-right',
                render: (data) => data ? `${data} m³` : '-' // Kích thước (M3)
            },
             
            {
                targets: 8,
                data: 'totalPackages',
                width: 60,
                sortable: false,
                className: 'text-right',
                render: (data) => data || 0 // Tổng số kiện
            },
            {
                targets: 9,
                width: 120,
                data: 'shippingPartnerName',
                sortable: false,
                render: (data) => data || '' // Đối tác vận chuyển
            },
            {
                targets: 10,
                data: 'characteristic',
                width: 100,
                sortable: false,
                render: (data) => data || '-' // Đặc tính
            },
            
            {
                targets: 11,
                data: 'note',
              
                sortable: false,
                render: (data) => data || '-' // Ghi chú
            }
        ]
    });

    $('.btn-bag-search').on('click', (e) => {
        bagsTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            bagsTable.ajax.reload();
            return false;
        }
    });


    //$('select[name="CustomerId"]').select2({
    //    theme: 'bootstrap4',
    //    ajax: {
    //        delay: 250, // wait 250 milliseconds before triggering the request
    //        url: abp.appPath + 'api/services/app/Customer/GetAllForSelectBySale',
    //        dataType: 'json',
    //        processResults: function (data) {
    //            return {
    //                results: data.result
    //            };
    //        }
    //    }

    //}).addClass('form-control');
    $('.select2').select2({
        theme: "bootstrap4", width: "80%",
        allowClear: true,
        placeholder: l('SelectCustomer'),
        allowClear: true,
        ajax: {
            delay: 550, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/customer/GetCustomersBagWithPartner',
            type: "GET",
            data: function (params) {
                //return {
                //    Keyword: params.term, // search term
                //};
                var data = $('#BagSearchForm').serializeFormToObject(true);
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
    //$('.date-range').daterangepicker({
    //    opens: 'left',
    //    "locale": {
    //        "format": "DD/MM/YYYY",
    //        "separator": " - ",
    //        "applyLabel": l('Apply'),
    //        "cancelLabel": l('Cancel'),
    //        "fromLabel": l('From'),
    //        "toLabel": l('To'),
    //        "customRangeLabel": l('Select'),
    //        "weekLabel": "W",

    //    },
    //    autoUpdateInput: false,
    //    "cancelClass": "btn-danger",

        
    //}).on('apply.daterangepicker', function (ev, picker) {
        
    //    var target = $(this).attr('target');
    //    $('.start-date.' + target).val(picker.startDate.format('DD/MM/YYYY'));
    //    $('.end-date.' + target).val(picker.endDate.format('DD/MM/YYYY'));
    //    $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
    //}).on('cancel.daterangepicker', function (ev, picker) {
    //    $(this).val('');
    //    var target = $(this).attr('target');
    //    $('.start-date.' + target).val('');
    //    $('.end-date.' + target).val('');
    //});

    $('.btn-export-excel').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#BagSearchForm').serializeFormToObject();
        
        filterData.isExcel = true;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/SaleAdmin/BagWithPartnerDownload?' + url;
        abp.ui.clearBusy();
        
    });
})(jQuery);
