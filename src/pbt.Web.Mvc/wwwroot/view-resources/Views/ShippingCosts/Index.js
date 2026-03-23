(function ($) {
    var _shippingCostGroupService = abp.services.app.shippingCostGroup,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#ShippingCostGroupCreateModal'),
        _$modalEdit = $('#ShippingCostGroupEditModal'),
        _$form = _$modal.find('form'),
        _$formUpdate = _$modalEdit.find('form'),
        _$table = $('#ShippingCostGroupsTable');

    var _$shippingCostGroupsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _shippingCostGroupService.getAll,
            inputFilter: function () {
                return $('#ShippingCostGroupSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { _$packagesTable.draw(false); }
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
                width: 40,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 1,
                data: 'name',
                sortable: true
            },
            {
                targets: 2,
                data: 'shippingPartnerId',
                sortable: true
                
            },
            {
                targets: 3,
                data: 'isActived',
                sortable: true,
                render: function (data, type, row, meta) {
                    return data ? '<span class="badge badge-success">Có</span>' : '<span class="badge badge-danger">Không</span>';
                }
            },
            {
                targets: 4,
                data: 'fromDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY') : '-';
                }
            },
            {
                targets: 5,
                data: 'toDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY') : '-';
                }
            },
            {
                targets: 6,
                data: 'note',
                sortable: false
            },
            {
                targets: 7,
                data: null,
                sortable: false,
                width: 20,
                render: function (data, type, row, meta) {
                    let actions = [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu" role="menu">`,
                        `       <a class="dropdown-item bg-primary edit-shipping-cost-group" data-id="${row.id}" title="${l('Edit')}">`,
                        `           <i class="fas fa-edit"></i> ${l('Edit')}`,
                        `       </a>`,
                        `       <a href="/ShippingCosts/ConfigCost/${row.id}" class="dropdown-item bg-success config-cost" data-id="${row.id}" title="${l('SetAsDefault')}">`,
                        `           <i class="fas fa-star"></i> ${l('ConfigShippingCost')}`,
                        `       </a>`,
                        `       <a class="dropdown-item bg-danger delete-shipping-cost-group" data-id="${row.id}" title="${l('Delete')}">`,
                        `           <i class="fas fa-trash"></i> ${l('Delete')}`,
                        `       </a>`,
                        `   </div>`,
                        `</div>`
                    ];
                    return actions.join('');
                }
            }
        ]
    });

    _$form.find('.save-button').on('click', function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var shippingCostGroup = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _shippingCostGroupService.create(shippingCostGroup).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$shippingCostGroupsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on("click","#ShippingCostGroupEditModal .save-button", function (e) {
        e.preventDefault();
        _$modalEdit = $('#ShippingCostGroupEditModal');
        _$formUpdate = $('#ShippingCostGroupEditModal form');
        if (!_$formUpdate.valid()) {
            return;
        }
        var shippingCostGroup = _$formUpdate.serializeFormToObject();
        abp.ui.setBusy(_$modalEdit);
        _shippingCostGroupService.update(shippingCostGroup).done(function () {
            _$modalEdit.modal('hide');
            _$formUpdate[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$shippingCostGroupsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modalEdit);
        });
    })

    $(document).on('click', '.edit-shipping-cost-group', function () {
        var id = $(this).attr('data-id');
        abp.ajax({
            url: abp.appPath + 'ShippingCosts/EditModal?id=' + id,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ShippingCostGroupEditModal div.modal-content').empty().html(content);
                $('#ShippingCostGroupEditModal').modal('show');
                initDateRangePicker();
            }
        });
    });

    $(document).on('click', '.delete-shipping-cost-group', function () {
        var id = $(this).attr('data-id');

        abp.message.confirm(
            l('AreYouSureWantToDelete'),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    _shippingCostGroupService.delete({ id: id }).done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$shippingCostGroupsTable.ajax.reload();
                    });
                }
            }
        );
    });

    initDateRangePicker();
    function initDateRangePicker() {
        debugger
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
        $('.date-single').daterangepicker({
            "singleDatePicker": true,
            "autoApply": true,
            "showCustomRangeLabel": false,
            "cancelClass": "btn-danger"
        }, function (start, end, label) {
            console.log('New date range selected: ' + start.format('YYYY-MM-DD') + ' to ' + end.format('YYYY-MM-DD') + ' (predefined range: ' + label + ')');
        });

        $('.btn-clear-date').on('click', function () {
            var targetInput = $(this).attr('target-date'); // Lấy giá trị của thuộc tính target-date
            if (targetInput) {
                // Xóa giá trị của input tương ứng
                $('input[name="' + targetInput + '"]').val('');
                $('.' + targetInput).val(''); // Xóa giá trị của hidden input nếu có
            }
        });
    }
})(jQuery);

