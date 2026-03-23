(function ($) {
    var _bagService = abp.services.app.bag,
        _warehouseTransferService = abp.services.app.warehouseTransfer,
        _warehouseTransferDetailService = abp.services.app.warehouseTransferDetail,
        l = abp.localization.getSource('pbt');

    var _$bagPackageTable = $('#bag-package-listTable'),
        _$readyListTable = $('#ready-listTable'),
        _$formTransfer = $('#form-transfer');

    $('#customer-select').select2()
        .addClass('form-control')
        .on('select2:select', function (e) {
            var selectedOption = $(this).find('option:selected');
            var customerId = selectedOption.val();
            if (customerId && customerId > 0) {
                LoadWTByCustomer();
            }
        });

    function LoadWTByCustomer() {
        var warehouseId = $('#sourceWarehouse').val();
        var customerId = $('#customer-select').val();

        if (customerId && customerId > 0 && warehouseId && warehouseId > 0) {
            _warehouseTransferService.getWarehouseTransferByCustomerId(customerId, warehouseId).done(function (data) {
                if (data && data.id && data.id > 0) {
                    $('#hiddenWTId').val(data.id);
                    $('#lbWTCode').html(data.transferCode);
                    detailTable.ajax.reload();
                }
                else {
                    $('#hiddenWTId').val(0);
                    $('#lbWTCode').html('');
                    detailTable.ajax.reload();
                }
                bagPackageTable.ajax.reload();
            });
        }
    }

    new AutoNumeric('#shippingFee', {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0'
    });

    // Khởi tạo DataTable cho #bag-package-listTable
    var bagPackageTable = _$bagPackageTable.DataTable({
        paging: false,
        serverSide: true,
        processing: false,
        deferLoading: 0,
        listAction: {
            ajaxFunction: _bagService.getListByCustomerAndWarehouse,
            inputFilter: function () {
                var input = {
                    customerId: $('#customer-select').val(),
                    warehouseId: $('#sourceWarehouse').val(),
                };
                return input;
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { bagPackageTable.draw(false); }
            }
        ],

        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('zeroRecords')
        },

        columns: [
            {
                data: null,
                orderable: false,
                render: function () {
                    return `<input type="checkbox" class="select-item" />`;
                }
            },
            {
                data: 'packageNumber',

            },
            {
                data: 'bagNumber',
                title: l('BagNumber'),

            },
            {
                data: 'weight',
                title: l('Weight'),
                render: function (data) {
                    return data ? `${data.toFixed(2)} kg` : '-';
                }
            },

            {
                data: 'importDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY HH:mm') : '-';
                }
            }

        ]
    });

    var detailTable = _$readyListTable.DataTable({
        paging: false,
        serverSide: true,
        processing: false,
        deferLoading: 0,
        listAction: {
            ajaxFunction: _warehouseTransferDetailService.getByWarehouseTransferId,
            inputFilter: function () {
                var input = {
                    warehouseTransferId: $('#hiddenWTId').val()
                };
                return input;
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { detailTable.draw(false); }
            }
        ],

        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('zeroRecords')
        },
        columns: [
            {
                targets: 0,
                data: null,
                width: 80,
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 1,
                data: 'packageCode',

            },
            {
                targets: 2,
                data: 'bagNumber',
            },
            {
                targets: 3,
                data: 'weight',
                title: l('Weight'),
                render: function (data) {
                    return data ? `${data.toFixed(2)} kg` : '-';
                }
            },
            {
                targets: 4,
                data: 'id',
                width: 30,
                render: function (data, type, row, meta) {
                    debugger;
                    var html = `<button  class=" btn btn-remove btn-danger" data-type="${row.itemType}" data-bagNumber="${row.bagNumber}" data-packageCode="${row.packageCode}" data-id="${row.id}"> <i class="fas fa-trash"></i>  </button>`;
                    return html;
                }
            }

        ]
    });

    $(document).on('click', '.btn-remove', function () {
        var warehouseTransferId = $('#hiddenWTId').val();
        var itemId = $(this).attr('data-id');
        var itemType = $(this).attr('data-itemType');
        var bagNumber = $(this).attr('data-bagNumber');
        var packageCode = $(this).attr('data-packgeCode');

        if (!warehouseTransferId || warehouseTransferId <= 0) {
            abp.notify.warn('Không tìm thấy phiếu chuyển kho.');
            return;
        }

        var message = itemType == 1 ? 'Kiện ' + packageCode : 'Bao ' + bagNumber;

        abp.message.confirm(
            `Bạn có chắc chắn muốn xóa ${message} khỏi phiếu chuyển kho?`,
            'Xác nhận',
            function (isConfirmed) {
                if (isConfirmed) {
                    abp.ui.setBusy(); // Hiển thị trạng thái đang xử lý
                    _warehouseTransferDetailService.removeItem(itemId).done(function (result) {
                        if (result.success) {
                            abp.notify.success(result.message);
                            detailTable.ajax.reload(); // Reload lại bảng chi tiết
                            bagPackageTable.ajax.reload(); // Reload lại bảng kiện hàng/bao
                        } else {
                            abp.notify.error(result.message);
                        }
                    }).always(function () {
                        abp.ui.clearBusy(); // Ẩn trạng thái đang xử lý
                    });
                }
            }
        );
    });


    $('#scanCode').on('keyup', function (e) {

        if (e.key === 'Enter' || e.keyCode === 13) {
            var scanCode = $(this).val().trim();
            if (!scanCode) {
                abp.notify.warn(l('PleaseEnterScanCode'));
                return;
            }
            var data = {
            };
            data.code = scanCode;
            data.fromWarehouseId = $('#sourceWarehouse').val();
            data.toWarehouseId = $('#destinationWarehouse').val();
            if (data.fromWarehouseId && data.toWarehouseId && data.fromWarehouseId != data.toWarehouseId) {
                _warehouseTransferService.scanCode(data).done(function (rs) {
                    PlaySound('success');
                    $('#hiddenWTId').val(rs.warehouseTransferId);
                    $('#lbWTCode').html(rs.warehouseTransferCode);
                    $('#customer-select').select2('val', [rs.customerId]);
                    detailTable.ajax.reload();
                    bagPackageTable.ajax.reload();
                });
            }
            // Xóa giá trị trong ô quét mã
            $(this).val('');
            return false;
        }
        return false;
    });

    $('#select-customer-scaned').select2({
        tags: true, // Enable tagging
        tokenSeparators: [','],
         
        multiple : true,
        minimumInputLength: 9999,
        insertTag: function (data, tag) {
            // Insert the tag at the end of the results
            data.push(tag);
        }
    });

    // Xử lý khi nhấn nút "Xóa" trong bảng #ready-listTable
    _$readyListTable.on('click', '.btn-remove-from-ready', function () {
        var row = readyListTable.row($(this).closest('tr')).data();

        // Thêm lại bản ghi vào bảng #bag-package-listTable
        bagPackageTable.row.add(row).draw();

        // Xóa bản ghi khỏi bảng #ready-listTable
        readyListTable.row($(this).closest('tr')).remove().draw();
    });


    // Xử lý khi nhấn nút "Lưu"
    $('.btn-save-transfer').on('click', function () {
        var warehouseTransferId = $('#hiddenWTId').val();
        var transferFee = $('#shippingFee').val();
        if (!warehouseTransferId || warehouseTransferId <= 0) {
            abp.notify.warn('Không tìm thấy phiếu chuyển kho.');
            return;
        }

        abp.message.confirm(
            'Bạn có chắc chắn muốn lưu phiếu chuyển kho này?',
            'Xác nhận',
            function (isConfirmed) {
                if (isConfirmed) {
                    abp.ui.setBusy(); // Hiển thị trạng thái đang xử lý
                    _warehouseTransferService.saveWarehouseTransfer({
                        id: warehouseTransferId,
                        shippingFee: transferFee,
                        note: $('#txtNote').val()
                    }).done(function (result) {
                        if (result.success) {
                            abp.notify.success(result.message);

                        } else {
                            abp.notify.error(result.message);
                        }
                    }).always(function () {
                        abp.ui.clearBusy(); // Ẩn trạng thái đang xử lý
                    });
                }
            }
        );
    });
})(jQuery);