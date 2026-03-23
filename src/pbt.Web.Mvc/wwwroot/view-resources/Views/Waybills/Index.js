(function ($) {
    var _orderService = abp.services.app.order,
        _orderNoteService = abp.services.app.orderNote,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#WaybillCreateModal'),
        _waybillNoteModal = $('#OrderNote'),
        _$form = _$modal.find('form'),
        _$table = $('#WaybillsTable');

    const orderStatusDescriptions = {
        0: { text: 'Thiếu thông tin', color: 'red' },
        1: { text: 'Đã ký gửi', color: 'blue' },
        2: { text: 'Hàng về kho TQ', color: 'purple' },
        3: { text: 'Đang VC QT', color: 'orange' },
        4: { text: 'Đã đến kho VN', color: 'green' },
        5: { text: 'Đang giao đến khách', color: 'cyan' },
        6: { text: 'Đã giao', color: 'gray' },
        7: { text: 'Khiếu nại', color: 'red' },
        8: { text: 'Hoàn tiền', color: 'pink' },
        9: { text: 'Huỷ', color: 'darkgray' },
        10: { text: 'Hoàn thành đơn', color: 'teal' }
    };
    //$('.select2').select2({
    //    theme: "bootstrap4"
    //});

    var _$waybillsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _orderService.getOrderWaybill,
            inputFilter: function () {
                return $('#WaybillSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$waybillsTable.draw(false)
            }
        ],
        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
        },
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                data: null,
                width: 20,
                className: 'dt-control',
                render: function (data, type, row, meta) {
                    return '<a href="javascript:void(0)"  data-row-id="${row.id}"><i class="fas fa-plus"></i></a>';
                    // `<input type="checkbox" data-check="row"  data-row-id="${row.id}"/>`;
                }
            },
            {
                targets: 1,
                data: null,
                width: 50,
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 2,
                data: 'waybillNumber',
                sortable: false,
                width: 250,

                render: function (data, type, row, meta) {
                    return `<a order-id='${row.id}' target="_blank" href="/Orders/Detail/${row.id}" class="text-primary">${data}</a>`;
                }
            },
            {
                targets: 3,
                data: 'customerName',
                sortable: false,
                width: 250,

            },


            {
                targets: 4,
                data: 'status',
                sortable: false,
                className: 'text-right',
                width: 80,
                render: (data, type, row, meta) => {

                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = orderStatusDescriptions[row.orderStatus];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong class="badge badge-success" style="background-color: ${status ? status.color : 'red'}"  >${status ? status.text : 'Chưa xác định'}</strong>`;

                }
            },
            {
                targets: 5,
                data: 'note',
                sortable: false,
                width: 250,

            },
            {
                targets: 6,
                sortable: false,
                className: 'text-right',
                width: 150,
                render: function (data, type, row, meta) {

                    return row && row.creationTime ? formatDateToDDMMYYYYHHmm(row.creationTime) : '';
                }

            },
            {
                targets: 7,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {

                    var rematchHtml = canRematch ? ` <a href="/Waybill/ReMatch?orderIds=${data.id}&currentCustomerId=${row.customerId}" class="dropdown-item text-success" data-id=${data.id}>
                                <i class="fas fa-user"></i> ${l('MatchNewCustomer')}
                            </a>`  : '';
                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `</button>`,
                        ` <div class="dropdown-menu" style="">`,
                        rematchHtml
                        ,
                        `<a href="javascript:void(0)" class="dropdown-item text-info btn-note" data-id=${data.id}>
                            <i class="fas fa-user"></i> ${l('Note')}
                        </a>` ,
                        `<a href="javascript:void(0)" class="dropdown-item text-info btn-add-note" data-id=${data.id} data-toggle="modal" data-target="#OrderNote">
                            <i class="fas fa-user"></i> ${l('AddNote')}
                        </a>` ,
                        `</div>`,
                        `</div>`
                    ].join('');
                }
            }
        ]
    });



    _$waybillsTable.on('click', 'tbody td.dt-control', function (e) {
        let tr = e.target.closest('tr');
        let row = _$waybillsTable.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
        }
        else {
            // Open this row
            row.child(getRow(row.data())).show();
        }
    });

    _$waybillsTable.on('click', '.btn-note', function (e) {

        let tr = e.target.closest('tr');
        let row = _$waybillsTable.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
        }
        else {
            // Open this row
            row.child(getNote(row.data())).show();
        }
    });

    function getNote(d) {
        var orderId = d.id;
        var rowContent = '';

        var rowContent = '';
        $.ajax({
            url: '/Orders/GetOrderNotes?orderId=' + orderId,
            type: "GET",
            async: false,
            dataType: "html",
            success: function (html) {
                rowContent = html;
            },
            error: function () {

            }
        });

        return rowContent;

    }

    function getRow(d) {

        // `d` is the original data object for the row

        var orderId = d.id;
        var rowContent = '';
        $.ajax({
            url: '/Orders/GetPackagesTrackingByOrder?orderId=' + orderId,
            type: "GET",
            async: false,
            dataType: "html",
            success: function (html) {
                rowContent = html;
            },
            error: function () {

            }
        });

        return rowContent;
    }

    // Initialize date range picker
    $('.date-range').daterangepicker({
        "timePicker": false,
        "locale": {
            "format": "DD-MM-YYYY",
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
        $('.start-date.' + target).val(picker.startDate.format('DD-MM-YYYY'));
        $('.end-date.' + target).val(picker.endDate.format('DD-MM-YYYY'));
        $(this).val(picker.startDate.format('DD-MM-YYYY') + ' - ' + picker.endDate.format('DD-MM-YYYY'));
    });

    $('.date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        var target = $(this).attr('target');
        $('.start-date.' + target).val('');
        $('.end-date.' + target).val('');
    });

    $(document).on('click', '.btn-add-note', function () {
        var orderId = $(this).data('id');
        $('#hidden-order-id').val(orderId);
        $('#noteContent').val('');
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
            _waybillNoteModal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            var rootTr = findRowByOrderId(orderId);
            let row = _$waybillsTable.row(rootTr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                // Open this row
                row.child(getNote(row.data())).show();
            }

        }).always(function () {
            //   abp.ui.clearBusy(_waybillNoteModal);

        });
    });

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var waybill = _$form.serializeFormToObject();
        waybill.WaybillCodes = waybill.WaybillCodes
            .replace(/[\s\r\n]+/g, ',')        // Thay khoảng trắng và xuống dòng bằng dấu phẩy
            .replace(/,+/g, ',')               // Gộp nhiều dấu phẩy liên tiếp thành một
            .replace(/^,|,$/g, '');            // Xoá dấu phẩy ở đầu hoặc cuối

        abp.ui.setBusy(_$modal);
        _orderService.createList(waybill).done(function (result) {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully') + ': ' + result + ' waybills added');
            PlaySound('success');
            _$waybillsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    function findRowByOrderId(orderId) {
        var tr = $('a[order-id="' + orderId + '"]').closest('tr');
        return tr;
    }

    $(document).on('click', '.btn-delete-order-note', function () {
        var noteId = $(this).data('note-id');

        abp.message.confirm(
            l('AreYouSureYouWantToDelete', noteId),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _orderNoteService.delete({
                        id: noteId
                    }).done(() => {
                       
                        abp.notify.info(l('SuccessfullyDeleted'));
                        // Reload notes
                        let tr = $(this).closest('tr');
                        var orderId = tr.data('order-id');
                        var rootTr = findRowByOrderId(orderId);
                        let row = _$waybillsTable.row(rootTr);

                        if (row.child.isShown()) {
                            // This row is already open - close it
                            row.child.hide();
                            // Open this row
                            row.child(getNote(row.data())).show();
                        }
                    });
                }
            }
        );
    });


    $(document).on('click', '.edit-waybill', function (e) {
        var waybillId = $(this).attr("data-waybill-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Waybills/EditModal?Id=' + waybillId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#WaybillEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#WaybillsCreateModal"]', (e) => {
        $('.nav-tabs a[href="#waybill-details"]').tab('show')
    });

    abp.event.on('waybill.edited', (data) => {
        _$waybillsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$waybillsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$waybillsTable.ajax.reload();
            return false;
        }
    });
})(jQuery);