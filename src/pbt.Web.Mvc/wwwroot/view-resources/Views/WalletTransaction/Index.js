(function ($) {

    var _walletTransactionService = abp.services.app.customerTransaction,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#WalletTransactionCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#WalletTransactionTable');


    const tranDirectionDescriptions = {
        1: { text: 'Nạp tiền', class: 'badge badge-success' },
        2: { text: 'Thanh toán', class: 'badge badge-primary' },
        3: { text: 'Rút tiền', class: 'badge badge-primary' }
    };

    var _$walletTransactionTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _walletTransactionService.getCurrentCustomerTransaction,
            inputFilter: function () {
                return $('#WalletTransactionSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$walletTransactionTable.draw(false)
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
        _$walletTransactionTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$walletTransactionTable.ajax.reload();
            return false;
        }
    });


    $('#input-date-range').daterangepicker(
        {
            locale: { cancelLabel: 'Clear' }
        }
    ).val('');

    $('#input-date-range').on('apply.daterangepicker', function (ev, picker) {
        
        $('#StartDateStr').val(picker.startDate.format('DD-MM-YYYY'));
        $('#EndDateStr').val(picker.endDate.format('DD-MM-YYYY'));
    });

    $('.btn-export').on('click', function () {

        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#WalletTransactionSearchForm').serializeFormToObject();
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/WalletTransaction/Download?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url
    });

})(jQuery);
