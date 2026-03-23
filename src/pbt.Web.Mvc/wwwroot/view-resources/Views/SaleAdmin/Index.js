(function ($) {

    var _customerService = abp.services.app.customer,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CustomerCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#CustomersTable'),
        _orderService = abp.services.app.order,
        _$ordersTable = $('#OrdersTable');


    const shippingLines = {
        0: { text: '', color: 'blue' },
        1: { text: 'Vận chuyển hàng lô', color: 'blue' },
        2: { text: 'Vận chuyển TMĐT', color: 'purple' },
    };

    const orderStatusDescriptions = {
        1: { text: 'Đã ký gửi', color: 'blue' },
        2: { text: 'Hàng về kho TQ', color: 'purple' },
        3: { text: 'Đang vận chuyển quốc tế', color: 'orange' },
        4: { text: 'Đã đến kho VN', color: 'green' },
        5: { text: 'Đang giao đến khách', color: 'cyan' },
        6: { text: 'Đã giao', color: 'gray' },
        7: { text: 'Khiếu nại', color: 'red' },
        8: { text: 'Hoàn tiền', color: 'pink' },
        9: { text: 'Huỷ', color: 'darkgray' },
        10: { text: 'Hoàn thành đơn', color: 'teal' }
    };

    const orderPaymentStatusDescriptions = {
        0: { text: 'Chưa thanh toán', color: 'red' },
        1: { text: 'Chưa thanh toán', color: 'red' },
        2: { text: 'Đã thanh toán', color: 'green' }
    }
      

    $('select[name="CustomerId"]').select2({
        theme: 'bootstrap4',
        ajax: {
            delay: 250, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Customer/GetAllForSelectBySale',
            dataType: 'json',
            processResults: function (data) {
                return {
                    results: data.result
                };
            }

        }

    }).addClass('form-control');

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
        //  "showCustomRangeLabel": false,
    }).on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format('DD-MM-YYYY'));
        $('.end-date.' + target).val(picker.endDate.format('DD-MM-YYYY'));
    });
})(jQuery);
