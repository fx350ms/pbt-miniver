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
    $('.date-single').daterangepicker({
        "singleDatePicker": true,
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
        autoUpdateInput: true,
        "cancelClass": "btn-danger"
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

    LoadData();
    function LoadData() {
        var data = _$form.serializeFormToObject(true);
        abp.ui.setBusy(_$packageTable);
        $.ajax({
            url: abp.appPath + 'DeliveryNote/LoadExportDelivery',
            type: 'GET',
            data: data,
            success: function (result) {
                _$packageTable.find('tbody').html(result);
            },
            error: function () {
                abp.notify.error(l('LoadFailed'));
            },
            complete: function () {
                abp.ui.clearBusy(_$packageTable);
            }
        });
    }

     
    $('.btn-search').on('click', (e) => {
        //  packagesTable.ajax.reload();
        //   loadCustomer();
        LoadData();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            // packagesTable.ajax.reload();
            //  loadCustomer();

            LoadData();

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
        window.location.href = '/DeliveryNote/DownloadExportDelivery?' + url;
        abp.ui.clearBusy();

    });
    

})(jQuery);
