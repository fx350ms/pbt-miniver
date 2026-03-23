(function ($) {

    var _messageService = abp.services.app.message,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#MessageCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#MessagesTable');


    const messageStatusDescriptions = {

        1: { text: 'Đã nhận', class: 'badge badge-primary' },
        2: { text: 'Đang xử lý', class: 'badge badge-warning' },
        3: { text: 'Hoàn thành', class: 'badge badge-success' },
        4: { text: 'Thất bại', class: 'badge badge-danger' }
    };

    const messageTypeDescriptions = {
        0: { text: 'Không xác định', class: 'badge badge-warning' },
        1: { text: 'Cộng tiền', class: 'badge badge-success' },
        2: { text: 'Trừ tiền', class: 'badge badge-danger' }
    };
    var _$messagesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _messageService.getAll,
            inputFilter: function () {
                
                return $('#MessageSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$messagesTable.draw(false)
            }
        ],
        columnDefs: [
            { targets: 0, data: 'deviceName', sortable: false, width: 130 },
            { targets: 1, data: 'deviceId', sortable: false, width: 130 },
            { targets: 2, data: 'content', sortable: false },
            {
                targets: 3, data: 'status', sortable: false, width: 100,
                render: (data, type, row, meta) => {
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = messageStatusDescriptions[row.status];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong class="${status ? status.class : 'black'}">${status ? status.text : 'Chưa xác định'}</strong>`;
                }
            },

            { targets: 4, data: 'progress', sortable: false, width: 120, },
            {
                targets: 5, data: 'messageType', sortable: false, width: 120,
                render: (data, type, row, meta) => {
                    const status = messageTypeDescriptions[row.messageType];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong class="${status ? status.class : 'black'}">${status ? status.text : 'Chưa xác định'}</strong>`;
                }
            },
            {
                targets: 6,
                data: 'isCorrectSyntax',
                sortable: false,
                width: 120,
                className: "text-center", // Căn giữa nội dung
                render: (data) => data
                    ? '<i class="fas fa-check text-success"></i>'
                    : '<i class="fas fa-times text-danger"></i>'
            },
            {
                targets: 7,
                data: null,
                sortable: true,
                width: 130,
                render: function (data, type, row, meta) {
                    return `<strong><div>${formatDateToDDMMYYYYHHmm(row.createdDate)}</div></strong>`
                        + (row.lastUpdatedDate ?
                            `Lần cuối cập nhật:
                            <strong><div>${formatDateToDDMMYYYYHHmm(row.lastUpdatedDate)}</div> </strong>`
                            : '')
                        ;
                }
            },

            {
                targets: 8,
                data: null,
                sortable: false,
                width: 20,
                render: (data, type, row) => {
                    // let actions = `<button type="button" class="btn btn-sm bg-primary view-log" data-message-id="${row.id}" title="Xem log">` +
                    //     `<i class="fas fa-receipt"></i> </button> ` +
                    //     `<button type="button" class="btn btn-sm bg-primary view-log" data-message-id="${row.id}" title="Ghi chú">` +
                    //     `<i class="fas fa-edit"></i> </button> `;
                    // if (row.status === 4 && row.messageType === 2) {
                    //     actions += ` <button type="button" class="btn btn-sm bg-warning create-expense" data-message-id="${row.id}" title="Tạo phiếu chi">` +
                    //         `<i class="fas fa-sign-out-alt"></i> </button> `;
                    // } else if (row.status === 4 && row.messageType === 1) {
                    //     actions += `<button type="button" class="btn btn-sm bg-info create-receipt"  data-message-id="${row.id}" title="Tạo phiếu thu">` +
                    //         `<i class="fas fa-sign-in-alt"></i> </button>`;
                    // }
                   
                    // return actions;
                    let actions = [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,
                        `       <a class="dropdown-item view-log" data-message-id="${row.id}" title="Xem log">`,
                        `           <i class="fas fa-receipt"></i> Xem log`,
                        `       </a>`,
                        `       <a class="dropdown-item edit-note" data-message-id="${row.id}" title="Ghi chú">`,
                        `           <i class="fas fa-edit"></i> Ghi chú`,
                        `       </a>`
                    ];
            
                    // Add "Tạo phiếu chi" action if conditions are met
                    if (row.status === 4 && row.messageType === 2) {
                        actions.push(
                            `       <a class="dropdown-item create-expense" data-message-id="${row.id}" title="Tạo phiếu chi">`,
                            `           <i class="fas fa-file-invoice-dollar"></i> Tạo phiếu chi`,
                            `       </a>`
                        );
                    }
            
                    actions.push(
                        `   </div>`,
                        `</div>`
                    );
            
                    return actions.join('');
                }
            }
        ]
    });

    $('#input-date-range').daterangepicker(
        {
            locale: { cancelLabel: 'Clear' }
        }
    ).val('');
    $('#input-date-range').on('apply.daterangepicker', function (ev, picker) {
        //debugger;
        //console.log(picker.startDate.format('YYYY-MM-DD'));
        //console.log(picker.endDate.format('YYYY-MM-DD'));
        $('#start-date').val(picker.startDate.format('DD-MM-YYYY'));
        $('#end-date').val(picker.endDate.format('DD-MM-YYYY'));
    });
    //Tạo phiếu thu
    $(document).on('click', '.create-receipt', function (e) {
        var messageId = $(this).data('message-id');
        var rowData = _$messagesTable.rows().data().toArray().find(row => row.id === messageId);
        if (rowData) {
            var messageContent = rowData.content; // Lấy nội dung tin nhắn
            // Hiển thị nội dung lên modal hoặc input
            $('#TransactionContent').val(messageContent);
            $('#MessageId').val(messageId);
           
        } else {
        }
        $('#ReceiptCreateModal').modal('show');
    });


    abp.event.on('message.reload', (data) => {
        _$messagesTable.ajax.reload();
    });

    $('.btn-search').on('click', () => {
        _$messagesTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$messagesTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
