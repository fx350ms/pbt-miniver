(function ($) {

    var _customerTransactionService = abp.services.app.customerTransaction,
        l = abp.localization.getSource('pbt'),
        _$table = $('#FinanceTable');

    const tranDirectionDescriptions = {
        3: { text: 'Cộng ví', class: 'badge badge-success' },
        4: { text: 'Trừ ví', class: 'badge badge-danger' }
        
    };
    $('.select2').select2({
        theme: "bootstrap4", width: "100%",
        allowClear: true,
        placeholder: l('SelectCustomer'),
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
    var _$financeTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _customerTransactionService.getTransaction,
            inputFilter: function () {
                return $('#FinanceSearchForm').serializeFormToObject(true);
            }
        },

        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$customersTable.draw(false)
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
                data: 'id',
                sortable: false
            },
            {
                targets: 1,
                data: 'transactionDate',
                sortable: false,
                render: function (data) {
                    return formatDateToDDMMYYYYHHmm(data);
                }
            },
            {
                targets: 2,
                data: 'referenceCode',
                sortable: false
            },
            {
                targets: 3,
                data: 'transactionType',
                sortable: false,
                render: function (data, type, row) {
                    const status = tranDirectionDescriptions[row.transactionType];
                    // Trả về mô tả với màu sắc được áp dụng
                    var html = `<strong class="${status ? status.class : 'black'}">${status ? status.text : 'Chưa xác định'}</strong> `;
                    /*html += `<strong> ${row.transactionId} </strong>`*/
                    return html;
                }
            },
            {
                targets: 4,
                data: 'description',
                sortable: false
            },
            {
                targets: 5,
                data: 'amount',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;

                }
            },
            {
                targets: 6,
                data: 'balanceAfterTransaction',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;
                }
            },

        ]
    });
    $('.btn-search').on('click', (e) => {
        _$customersTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$customersTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
