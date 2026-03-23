(function ($) {
    var _warehouseTransferService = abp.services.app.warehouseTransfer,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#WarehouseTransferCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#WarehouseTransfersTable');

    const wtStatuses = {
        "1": { text: l('New'), color: 'secondary' },
        "2": { text: l('WTShipping'), color: 'info' },
        "3": { text: l('Transfered'), color: 'success' }
    };
    const formatDateTime = 'DD/MM/YYYY';
    $('.select2').select2();

    // Initialize date range picker
    $('.date-range').daterangepicker({
        // startDate: moment().subtract(6, 'days').startOf('day'),
        "locale": {
            "format": formatDateTime,
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
    }
    );

    $('.date-range').on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format(formatDateTime));
        $('.end-date.' + target).val(picker.endDate.format(formatDateTime));
        $(this).val(picker.startDate.format(formatDateTime) + ' - ' + picker.endDate.format(formatDateTime));
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

    var _$warehouseTransfersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _warehouseTransferService.getAllFilter,
            inputFilter: function () {
                return $('#SearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { _$warehouseTransfersTable.draw(false); }
            }
        ],
        lengthMenu: [100, 200, 300],
        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('zeroRecords')
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
                data: 'transferCode',
                sortable: false,
                width: 200,
                render: function (data, type, row, meta) {
                    return '<a target="_blank" href="/WarehouseTransfers/Detail/' + row.id + '" class="text-primary font-weight-bold">' + data + '<strong>';
                }
            },
            {
                targets: 2,
                data: 'customerName',
                width: 120,
                sortable: false
            },
            {
                targets: 3,
                data: 'fromWarehouseName',
                width: 120,
                sortable: false
            },
            {
                targets: 4,
                data: 'toWarehouseName',
                width: 120,
                sortable: false
            },

            {
                targets: 5,
                data: 'note',
                sortable: false,

            },
            {
                targets: 6,
                data: 'creationTime',
                width: 150,
                render: function (data, type, row, meta) {
                    debugger;
                    return formatDateToDDMMYYYYHHmmss(row.creationTime) || ''
                }
            },
            {
                targets: 7,
                data: 'status',
                width: 150,
                sortable: false,
                render: function (data, type, row, meta) {
                    var status = wtStatuses[row.status];
                    if (status) {
                        return '<span class="badge badge-' + status.color + '">' + status.text + '</span>';
                    }
                    return '<span class="badge badge-secondary"></span>';
                }
            },
            {
                targets: 8,
                data: null,
                sortable: false,
                width: 30,

                render: function (data, type, row, meta) {

                    var htmlReceived = data.status == 2 ?
                        ('<a href="#" type="button"  class="dropdown-item  bg-success confirm-wt" data-id="' + row.id + '" data-name="' + row.transferCode + '">  <i class="fas fa-check"></i> ' + l('Receiced') + '   </a>')
                        : '';

                    return [
                        '<div class="btn-group"> ',
                        '<button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">',
                        '</button>',
                        '<div class="dropdown-menu" style="">',
                        '<a href="/WarehouseTransfers/Detail/' + row.id + '" type="button" class="dropdown-item  bg-primary" data-id="' + row.id + '"> <i class="fas fa-eye"></i> ' + l('Detail') + ' </a>' +
                        htmlReceived +
                        '<a href="#" type="button"  class="dropdown-item  bg-danger delete-wt" data-id="' + row.id + '" data-name="' + row.transferCode + '">  <i class="fas fa-trash"></i> ' + l('Delete') + '   </a>',
                        '</div>',
                        '</div>'
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

        var data = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _warehouseTransferService.create(data).done(() => {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$warehouseTransfersTable.ajax.reload();
        }).always(() => {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-wt', function () {
        var dataId = $(this).attr("data-id");
        var dataCode = $(this).attr('data-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                '<strong class="text-danger">' + dataCode + '</strong>'),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _warehouseTransferService.delete({
                        id: dataId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$warehouseTransfersTable.ajax.reload();
                    });
                }
            },
            { isHtml: true }
        );
    });

    $('.btn-search').on('click', (e) => {
        _$warehouseTransfersTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$warehouseTransfersTable.ajax.reload();
            return false;
        }
    });

    $(document).on('click', '.confirm-wt', function () {
        
        var dataId = $(this).attr("data-id");
        var dataCode = $(this).attr('data-name');
        abp.message.confirm(
            abp.utils.formatString(
                l('Bạn có xác nhận phiếu chuyển đã tới đích'),
                '<strong class="text-danger">' + dataCode + '</strong>'),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _warehouseTransferService.received( dataId).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$warehouseTransfersTable.ajax.reload();
                    });
                }
            },
            { isHtml: true }
        );

         
    });


})(jQuery);