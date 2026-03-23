(function ($) {
    var _orderService = abp.services.app.order,
        _packageService = abp.services.app.package,
        _orderNoteService = abp.services.app.orderNote,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#OrderCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#OrdersTable');

    const orderStatusDescriptions = {
        0: { text: 'Thiếu thông tin', color: 'danger' },
        1: { text: 'Đã ký gửi', color: 'info' },
        2: { text: 'Hàng về kho TQ', color: 'success' },
        3: { text: 'Đang VC QT', color: 'info' },
        4: { text: 'Đã đến kho VN', color: 'success' },
        5: { text: 'Đang giao đến khách', color: 'info' },
        6: { text: 'Đã giao', color: 'success' },
        7: { text: 'Khiếu nại', color: 'danger' },
        8: { text: 'Hoàn tiền', color: 'warning' },
        9: { text: 'Huỷ', color: 'danger' },
        10: { text: 'Hoàn thành đơn', color: 'success' }
    };

    const shippingLines = {
        0: { text: '', color: 'blue' },
        1: { text: l('Lô'), color: 'blue' },
        2: { text: l('TMDT'), color: 'green' },
        3: { text: l('CN'), color: 'green' },
        4: { text: l('XT'), color: 'green' },

    };
    const bagTypes = {

        1: { text: 'Bao riêng', color: 'red' },
        2: { text: 'Bao ghép', color: 'green' }
    }

    var _$ordersTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _orderService.getAllMyOrders,
            inputFilter: function () {
                return $('#OrderSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$ordersTable.draw(false)
            }
        ],
        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('ZeroRecords')
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
                width: 20,
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 2,
                data: 'customerName',
                sortable: false,
                width: 150
            },
            {
                targets: 3,
                data: 'waybillNumber',
                sortable: false,
                /*className: 'dt-control',*/
                render: function (data, type, row, meta) {
                    return `<a target="_blank"  href="/Orders/Detail/${row.id}" title="${l('Detail')}">${data}</a>`;
                }
            },

            {
                targets: 4,
                data: 'shippingLine',
                width: 90,
                render: function (data, type, row, meta) {
                    const status = shippingLines[row.shippingLine];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : ''}</strong>`;
                }
            },

            {
                targets: 5,
                width: '50px',
                data: 'status',
                render: function (data, type, row, meta) {
                    var status = orderStatusDescriptions[row.orderStatus];
                    if (status) {
                        return '<span class="badge badge-' + status.color + '">' + status.text + '</span>';
                    }
                    return '<span class="badge badge-secondary">Không xác định</span>';
                }
            },

            {
                //BagCoverWeight
                targets: 6,
                width: 50,
                className: 'text-right',
                data: 'bagCoverWeight',
                render: (data, type, row, meta) => {
                    return data | '-';
                }
            },
            {
                //Weight
                targets: 7,
                data: 'weight',
                className: 'text-right',
                width: '50px',
                render: (data, type, row, meta) => {
                    return FormatNumberToDisplay(data, 2);
                }
            },
            {
                //Volume
                targets: 8,
                data: 'volume',
                className: 'text-right',
                width: '50px',
                render: (data, type, row, meta) => {
                    return FormatNumberToDisplay(data, 2);
                }
            },

            {
                //TotalFee -> phí vận chuyển QT
                targets: 9,
                data: 'totalFee',
                className: 'text-right',
                width: '80px',
                render: (data, type, row, meta) => {
                    return data ? FormatNumberToDisplay(data, 0) : '-';
                }
            },
            {
                //TotalFee -> phí vận chuyển QT
                targets: 10,
                data: 'totalFee',
                width: '150px',
                render: (data, type, row, meta) => {
                    var woodenPackagingFee = row.woodenPackagingFee ? FormatNumberToDisplay(row.woodenPackagingFee, 0) : 0;
                    var shockproofFee = row.shockproofFee ? FormatNumberToDisplay(row.shockproofFee, 0) : 0;
                    var domesticShippingFee = row.domesticShippingFee ? FormatNumberToDisplay(row.domesticShippingFee, 0) : 0;
                    var insuranceFee = row.insuranceFee ? FormatNumberToDisplay(row.insuranceFee, 0) : 0;
                    return [
                        `<div class="timeline-item">
                            <span class="label">${l('WoodenPackagingFee')}:</span>
                            <span class="value"><strong>${woodenPackagingFee}</strong></span>
                          </div>`,
                        `<div class="timeline-item">
                            <span class="label">${l('ShockproofFee')}:</span>
                            <span class="value"><strong>${shockproofFee}</strong></span>
                          </div>`,
                        `<div class="timeline-item">
                            <span class="label">${l('DomesticShippingFee')}:</span>
                            <span class="value"><strong>${domesticShippingFee}</strong></span>
                          </div>`,
                        `<div class="timeline-item">
                            <span class="label">${l('InsuranceFee')}:</span>
                            <span class="value"><strong>${insuranceFee}</strong></span>
                          </div>`
                    ].join('');

                }
            },
            {
                targets: 11,
                data: 'woodenPackagingFee',
                width: '80px',
                className: 'text-right',
                render: (data, type, row, meta) => {
                    var woodenPackagingFee = row.woodenPackagingFee | 0;
                    var shockproofFee = row.shockproofFee | 0;
                    var domesticShippingFee = row.domesticShippingFee | 0;
                    var insuranceFee = row.insuranceFee | 0;
                    return `<strong>${FormatNumberToDisplay((woodenPackagingFee + shockproofFee + domesticShippingFee + insuranceFee), 0)}</strong>`;
                }
            },

            {
                //TotalCost
                targets: 12,
                className: 'text-right',
                data: 'totalPrice',
                width: 150,
                render: (data, type, row, meta) => {
                    return data ? `<strong class="text-primary">${FormatNumberToDisplay(data, 0)}</strong>` : '-';

                }
            },
            {
                targets: 13,
                width: 250,
                render: (data, type, row) => {
                    // Ngày tạo
                    const creation = formatDateToDDMMYYYYHHmm(row.creationTime) || '';
                    // Ngày xuất kho TQ
                    const exportChina = row.inTransitTime ? formatDateToDDMMYYYYHHmm(row.inTransitTime) : '';
                    // Ngày nhập kho VN
                    const importVN = row.inTransitToVietnamWarehouseTime ? formatDateToDDMMYYYYHHmm(row.inTransitToVietnamWarehouseTime) : '';

                    return [
                        `<div class="timeline-item">
                            <span class="label">${l('CreationTime')}:</span>
                            <span class="value">${creation}</span>
                          </div>`,
                        `<div class="timeline-item">
                            <span class="label">${l('ExportDateCN')}:</span>
                            <span class="value">${exportChina}</span>
                          </div>`,
                        `<div class="timeline-item">
                            <span class="label">${l('ImportDateVN')}:</span>
                            <span class="value">${importVN}</span>
                          </div>`
                    ].join('');
                }
            },

            {
                targets: 14,
                width: 20,
                render: (data, type, row, meta) => {
                    const isEditable = row.orderStatus === 1; // Only allow edit/delete if status is 1
                    const canCreatePackage = row.orderStatus === 1 || row.orderStatus === 2; // Allow creating package if status is 1 or 2
                    const canCreateDeliveryRequest = row.orderStatus === 4; // Allow creating delivery request if status is 4
                    const canMakePayment = row.paymentStatus === 1; // Allow making payment if payment status is 'Chờ thanh toán'

                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        ` </button>`,
                        ` <div class="dropdown-menu" style="">`,

                        `   <a target="_blank" type="button" class="dropdown-item  bg-primary" data-order-id="${row.id}" href="/Orders/Detail/${row.id}" title="${l('Detail')}" data-toggle="tooltip"> `,
                        `       <i class="fas fa-eye"></i> ${l('View')}`,
                        '   </a>',

                        `   <a type="button" class="dropdown-item bg-secondary view-order-mote" data-order-id="${row.id}" title="${l('Note')}" data-toggle="modal" data-target="#OrderNote" > ` +
                        `       <i class="fas fa-pencil-alt"></i> ${l('Note')}` +
                        '   </a>',

                        `   <a type="button" class="dropdown-item bg-secondary" data-order-id="${row.id}" href="/Orders/Edit/${row.id}" title="${l('Edit')}" data-toggle="tooltip"> ` +
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}` +
                        '   </a>',

                        (row.orderStatus === 5 // Chỉ hiển thị nếu trạng thái là "Đang giao"
                            ? `   <button type="button" class="dropdown-item bg-success mark-as-delivered" data-order-id="${row.id}" data-order-name="${row.waybillNumber}"  title="${l('MarkAsDelivered')}" data-toggle="tooltip">` +
                            `       <i class="fas fa-truck"></i> ${l('MarkAsDelivered')}` +
                            '   </button>'
                            : '')
                        ,

                        (row.orderStatus === 6 // Chỉ hiển thị nếu trạng thái là "Đã giao"
                            ? `   <button type="button" class="dropdown-item bg-success mark-as-completed" data-order-id="${row.id}" data-order-name="${row.waybillNumber}"  title="${l('MarkAsCompleted')}" data-toggle="tooltip">` +
                            `       <i class="fas fa-check"></i> ${l('MarkAsCompleted')}` +
                            '   </button>'
                            : '')
                        ,
                        `   <button type="button" class="dropdown-item bg-danger delete-order" data-order-id="${row.id}" data-order-name="${row.waybillNumber}" title="${l('Delete')}" data-toggle="tooltip">` +
                        `       <i class="fas fa-trash"></i> ${l('Delete')}` +
                        '   </button>',

                        `   <a target="_blank" type="button" class="dropdown-item bg-success btn-sync-order" data-id="${row.id}"  title="${l('Sync')}" data-toggle="tooltip"> `,
                        `       <i class="fas fa-sync"></i> ${l('Sync')}`,
                        '   </a>',
                        `    </div>`,
                        `   </div>`
                    ].join('');
                }
            }
        ]
    });


    _$ordersTable.on('click', 'tbody td.dt-control', function (e) {
        return;
        let tr = e.target.closest('tr');
        let row = _$ordersTable.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
        }
        else {
            // Open this row
            row.child(getRow(row.data()), 'child-row').show();

        }
    });

    function getRow(d) {

        // `d` is the original data object for the row

        var orderId = d.id;
        var rowContent = '';
        $.ajax({
            url: '/Orders/GetPackagesByOrder?orderId=' + orderId,
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


    $('.date-range').daterangepicker(
        {
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
        }

    ).val('');

    $('.btn-export-order').on('click', function () {
        const filterData = $('#OrderSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        const url = toQueryString(filterData);
        window.location.href = '/Orders/ExportMyOrder?' + url;
        abp.ui.clearBusy();
    });

    $('.btn-export-package').on('click', function () {
        const filterData = $('#OrderSearchForm').serializeFormToObject();
        filterData.isExcel = true;
        const url = toQueryString(filterData);
        window.location.href = '/Orders/DownloadPackage?' + url;
        abp.ui.clearBusy();
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

    $('#input-date-range').on('apply.daterangepicker', function (ev, picker) {
        $('#start-date').val(picker.startDate.format('DD-MM-YYYY'));
        $('#end-date').val(picker.endDate.format('DD-MM-YYYY'));
    });

    _$form.find('.save-button').on('click', (e) => {

        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var order = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _orderService.create(order).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlayAudio('success', function () {

            });
            _$ordersTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.btn-sync-order', function () {
        var orderId = $(this).attr("data-id");
        _orderService.syncWeightAndFee(orderId).done(() => {
            abp.notify.info(l('SuccessfullyUpdated'));
            _$ordersTable.ajax.reload();
        });

    });
    $(document).on('click', '.mark-as-completed', function () {
        var orderId = $(this).attr("data-order-id");
        var orderName = $(this).attr('data-order-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToChangeOrderToCompleted'),
                orderName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _orderService.markAsCompleted(orderId).done(() => {
                        abp.notify.info(l('SuccessfullyUpdated'));
                        _$ordersTable.ajax.reload(null, false);
                    });
                }
            }
        );
    });

    $(document).on('click', '.mark-as-delivered', function () {
        var orderId = $(this).attr("data-order-id");
        var orderName = $(this).attr('data-order-name');

        markAsDelivered(orderId, orderName);
    });


    function markAsDelivered(orderId, orderName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToChangeOrderToDelivered'),
                orderName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _orderService.markAsDelivered(orderId).done(() => {
                        abp.notify.info(l('SuccessfullyUpdated'));
                        _$ordersTable.ajax.reload(null, false);
                    });
                }
            }
        );
    }

    $(document).on('click', '.delete-order', function () {
        var orderId = $(this).attr("data-order-id");
        var orderName = $(this).attr('data-order-name');

        deleteOrders(orderId, orderName);
    });

    function deleteOrders(orderId, orderName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                orderName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _orderService.delete({
                        id: orderId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$ordersTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-order', function (e) {
        var orderId = $(this).attr("data-order-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Orders/EditModal?Id=' + orderId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#OrderEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.view-package-list', function () {
        var orderId = $(this).data('order-id');

        abp.ajax({
            url: abp.appPath + 'Orders/GetPackageList/' + orderId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#PackageListModel div.modal-body').html(content);
                $('#PackageListModel').modal('show');
            },
            error: function (e) {
                abp.notify.error(l('ErrorLoadingPackageList'));
            }
        });
    });

    $(document).on('click', 'a[data-target="#OrdersCreateModal"]', (e) => {
        $('.nav-tabs a[href="#order-details"]').tab('show')
    });

    abp.event.on('order.edited', (data) => {
        _$ordersTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$ordersTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$ordersTable.ajax.reload();
            return false;
        }
    });
    $('#CustomerSelect').select2();


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
