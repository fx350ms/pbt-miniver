
(function ($) {
    var _transactionService = abp.services.app.transaction,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#TransactionCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#TransactionsTable');

    const tranDirectionDescriptions = {
        1: { text: 'PT', class: 'badge badge-success' },
        2: { text: 'PC', class: 'badge badge-danger' }
    };

    const tranStatusDescriptions = {
        1: { text: 'Chờ duyệt', class: 'badge badge-info' },
        2: { text: 'Duyệt', class: 'badge badge-success' },
        3: { text: 'Từ chối', class: 'badge badge-danger' }
    };

    var _$transactionsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _transactionService.getAllData,
            inputFilter: function () {
                return $('#TransactionSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$transactionsTable.draw(false)
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
                className: 'control',
                defaultContent: '',
            },

            {
                targets: 1,
                width: 130,
                data: 'creationTime',
                sortable: false,
                render: (data) => formatDateToDDMMYYYYHHmmss(data)
            },
            {
                targets: 2,
                width: 150,
                data: 'transactionId',
                sortable: false,
                render: function (data, type, row) {
                    const status = tranDirectionDescriptions[row.transactionDirection];
                    // Trả về mô tả với màu sắc được áp dụng
                    var html = `<strong class="${status ? status.class : 'black'}">${status ? status.text : 'Chưa xác định'}</strong> `;
                    html += `<strong> ${row.transactionId} </strong>`
                    return html;

                }
            },

            {
                targets: 3,
                width: 150,
                data: 'orderId',
                sortable: false
            },
            {
                targets: 4,
                width: 150,
                data: 'customerName',
                sortable: false
            },

            {
                targets: 5,
                width: 250,
                data: 'transactionContent',
                sortable: false
            },

            {
                targets: 6,
                data: 'amount',
                width: 180,
                className: "text-right", // Căn giữa nội dung
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatThousand(data) + `</strong>`;
                }
            },
            {
                targets: 7,
                data: 'totalAmount',
                width: 180,
                className: "text-right", // Căn phải nội dung
                render: function (data) {
                    if (data && data > 0) {
                        return `<strong class="text-success"> ` + formatThousand(data) + `</strong>`;
                    }
                    else {
                        return `<strong class="text-danger"> ` + formatThousand(data) + `</strong>`;
                    }
                }
            },
            {
                targets: 8,
                data: 'currency',
                sortable: false,
                width: 60,
            },
            {
                targets: 9,
                data: 'refCode',
                sortable: false,
                width: 120,
            },
           
            {
                targets: 10,
                data: 'transactionType',
                sortable: false,
                render: function (data) {
                    var badgeClass = '';
                    var typeText = '';
                    switch (data) {
                        case 1:
                            badgeClass = 'badge badge-primary'; // Deposit
                            typeText = 'Nạp tiền';
                            break;
                        case 2:
                            badgeClass = 'badge badge-info'; // Payment
                            typeText = 'Thanh toán';
                            break;
                        case 3:
                            badgeClass = 'badge badge-success'; // Withdraw
                            typeText = 'Rút tiền';
                            break;
                        default:
                            badgeClass = 'badge badge-secondary'; // Undefined
                            typeText = 'Khác';
                    }
                    return `<span class="${badgeClass}">${typeText}</span>`;
                }
            },
            {
                targets: 11,
                data: 'executionSource',
                sortable: false,
                render: function (data) {
                    var badgeClass = '';
                    var sourceText = '';
                    switch (data) {
                        case 1:
                            badgeClass = 'badge badge-info'; // Manual
                            sourceText = 'Thủ công';
                            break;
                        case 2:
                            badgeClass = 'badge badge-success'; // Auto
                            sourceText = 'Tự động';
                            break;
                        default:
                            badgeClass = 'badge badge-secondary'; // Undefined
                            sourceText = 'Khác';
                    }
                    return `<span class="${badgeClass}">${sourceText}</span>`;
                }
            },
            {
                targets: 12,
                
                data: 'status',
                sortable: false,
                render: function (data, type, row) {
                    const status = tranStatusDescriptions[row.status];
                    // Trả về mô tả với màu sắc được áp dụng
                    var html = `<strong class="${status ? status.class : 'black'}">${status ? status.text : ''}</strong> `;
                    return html;

                }
            },
            {
                targets: 13,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    // Nếu trạng thái đã duyệt (status = 2) thì không hiển thị nút Approve
                    return [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,

                        `<a target="_blank" href="/Transaction/Details/${row.id}"  type="button" class="dropdown-item text-info" data-id="${row.id}" >` +
                        `<i class="fas fa-info-circle"></i> ${l('Detail')} </a>` +
                        `</a >`,

                        (row.status == 1 && canApproveTransaction)  ? (
                            `<a type="button"  class="dropdown-item approve-transaction text-success" data-id="${row.id}"  data-text="${row.transactionCode}"  data-toggle="modal" data-target="#TransactionEditModal">` +
                            `<i class="fas fa-check-square"></i> ${l('Approve')}` +
                            `</a>`) : ''
                        ,
                        row.files && row.files.length > 0
                            ? (`<a  type="button"  class="dropdown-item view-file" data-id="${row.id}" data-toggle="modal" data-target="#TransactionViewFileModal">` +
                                `<i class="fas fa-paperclip"></i> ${l('Files')} </a>` +
                                `</a >`) : '',

                        `   </div>`,


                        `</div>`
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
        var transaction = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _transactionService.create(transaction).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$transactionsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });


    $(document).on('click', '.approve-transaction', function () {
        var dataId = $(this).attr("data-id");
        var dataText = $(this).attr('data-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToApprove'),
                '<strong class="text-danger">' + dataText + '</strong>'),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _transactionService.approve(dataId).done(() => {
                        abp.notify.info(l('Approved'));
                        _$transactionsTable.ajax.reload();
                    });
                }
            },
            { isHtml: true }
        );
    });
    var packageId = $(this).attr("data-package-id");
    var packageCode = $(this).attr('data-package-name');
  

    $(document).on('click', '.view-file', function (e) {
        // var customerId = $(this).attr("data-customer-id");
        var id = $(this).attr("data-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Transaction/Files?Id=' + id,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#TransactionViewFileModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    // Xử lý sự kiện khi chọn tài khoản FundAccount
    $('#select-fund-account').on('change', function () {
        // Lấy thông tin từ option được chọn
        var selectedOption = $(this).find('option:selected');
        var balance = selectedOption.data('ammount'); // Số dư
        var currency = selectedOption.data('currency'); // Tiền tệ
        $('#fund-account-balance').html(balance + ' ' + currency); // Hiển thị số dư và tiền tệ
        // Hiển thị số dư và tiền tệ vào các trường tương ứng
        //$('#fund-account-balance').val(balance || l('NotAvailable')); // Nếu không có giá trị, hiển thị "Không có sẵn"
        //$('#fund-account-currency').val(currency || l('NotAvailable'));
    });

    abp.event.on('transaction.reload', (data) => {
        _$transactionsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$transactionsTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$transactionsTable.ajax.reload();
            return false;
        }
    });

    $('.btn-export').on('click', function () {
        
        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#TransactionSearchForm').serializeFormToObject();
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Transaction/ExportExcel?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });
    $('#input-date-range').daterangepicker(
        {
            locale: { cancelLabel: 'Clear' }
        }
    ).val('');

    $('#input-date-range').on('apply.daterangepicker', function (ev, picker) {

        $('#start-date').val(picker.startDate.format('DD-MM-YYYY'));
        $('#end-date').val(picker.endDate.format('DD-MM-YYYY'));
    });
})(jQuery);