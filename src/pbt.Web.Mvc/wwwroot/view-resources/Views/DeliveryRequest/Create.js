(function ($) {
    const _deliveryRequestServices = abp.services.app.deliveryRequest,
        _bagService = abp.services.app.bag,
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _customerService = abp.services.app.customer,
        
        _$modal = $('#CustomerAddressCreateModal'),
        _$form = $("#form-dr");

    var _$bagPackageTable = $('#bag-package-listTable'),
        _$drItemTable = $('#deliveryRequestItemTable');

    _$formAddress = _$modal.find('form');
    _addressService = abp.services.app.customerAddress;

    var bagPackageTable = _$bagPackageTable.DataTable({
        paging: false,
        serverSide: true,
        processing: false,
        deferLoading: 0,
        listAction: {
            ajaxFunction: _deliveryRequestServices.getDeliveryRequestItemsForCreateRequest,
            inputFilter: function () {
                var input = {
                    customerId: $('#customer-select').val(),
                    warehouseId: $('#select-warehouse').val(),
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

        columnDefs: [


            {
                targets: 0,
                data: 'bagNumber',
                className: 'dt-control',
                render: function (data, type, row, meta) {
                    if (row && row.bagNumber) {
                        return `<a href="javascript:void(0)"  > ${row.bagNumber} </a>`;
                    }
                    else
                        return '';
                }
            },
            {
                targets: 1,
                data: 'packageNumber',

            },
            {
                targets:2,
                data: 'waybillNumber',
            },
          
            {
                targets: 3,
                data: 'weight',
                render: function (data) {
                    return data ? `${data.toFixed(2)} kg` : '-';
                }
            },
           
            {
                targets: 4,
                data: 'importDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY HH:mm') : '-';
                }
            },
            {
                targets: 5,
                data: 'id',
                width: 20,
                render: function (data, type, row, meta) {
                    return `<a href="javascript:void(0)" data-id="${row.id}" data-type="${row.itemType}" class="btn btn-sm btn-info btn-add-item-2-delivery-request" title="Thêm vào phiếu yêu cầu giao"> <i class="fas fa-caret-right"></i> </a>`;
                }
            }
        ]
    });

    var deliveryRequestItemTable = _$drItemTable.DataTable({
        paging: false,
        serverSide: true,
        processing: false,
        deferLoading: 0,
        listAction: {
            ajaxFunction: _deliveryRequestServices.getDeliveryRequestItemsByRequestId,
            inputFilter: function () {
                var drId = $('#hiddenDeliveryRequestId').val();
                var input = {
                    deliveryRequestId: drId
                };
                return input;
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { deliveryRequestItemTable.draw(false); }
            }
        ],

        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('zeroRecords')
        },

        columnDefs: [

            {
                targets: 0,
                data: 'itemId',
                width: 20,
                render: function (data, type, row, meta) {
                    return `<a href="javascript:void(0)" data-id="${row.id}" data-type="${row.itemType}" class="btn btn-sm btn-info btn-remove-item" title="Thêm vào phiếu yêu cầu giao"> <i class="fas fa-caret-left"></i> </a>`;
                }
            },
            {
                targets: 1,
                data: 'bagNumber',
                className: 'dt-control',
                render: function (data, type, row, meta) {
                    if (row && row.bagNumber) {
                        return `<a href="javascript:void(0)"  > ${row.bagNumber} </a>`;
                    }
                    else
                        return '';
                }
            },
            {
                targets: 2,
                data: 'packageCode'

            },
            {
                targets: 3,
                data: 'weight',
                render: function (data) {
                    return data ? `${data.toFixed(2)} kg` : '-';
                }
            },
            {
                targets: 4,
                data: 'totalPackages',
                render: function (data, type, row, meta) {
                    if (row.itemType == 1) return 1;
                    return row.totalPackages;
                }
            }
        ]
    });


    function getRow(d) {
        var bagId = d.id;
        var rowContent = '';
        $.ajax({
            url: '/DeliveryRequest/GetPackagesByBagId?bagId=' + bagId,
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

    bagPackageTable.on('click', 'tbody td.dt-control', function (e) {
        let tr = e.target.closest('tr');
        let row = bagPackageTable.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
        }
        else {
            // Open this row
            row.child(getRow(row.data())).show();
        }
    });

    deliveryRequestItemTable.on('click', 'tbody td.dt-control', function (e) {
        let tr = e.target.closest('tr');
        let row = bagPackageTable.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
        }
        else {
            // Open this row
            row.child(getRow(row.data())).show();
        }
    });


    $('.btn-add-new-address').on("click", function () {
        var customerId = $('select[name="CustomerId"]').val();
        if (customerId && customerId > 0) {
            //   loadProvince();
            $('.form-content input[name="CustomerId"]').val(customerId);
            // Hiển thị modal nếu đã chọn khách hàng
            $('#CustomerAddressCreateModal').modal('show');
        } else {
            // Hiển thị thông báo hoặc xử lý khi chưa chọn khách hàng

            PlaySound('warning'); abp.notify.error('Vui lòng chọn khách hàng trước khi thêm địa chỉ mới.');
        }
    });

    $(document).on('click', '.btn-add-item-2-delivery-request', function () {
        var itemId = $(this).data('id');
        var itemType = $(this).data('type');
        var deliveryRequestId = $('#hiddenDeliveryRequestId').val();

        abp.ui.setBusy(_$drItemTable);
        var item = {
            itemId: itemId,
            itemType: itemType,
            deliveryRequestId: deliveryRequestId
        };
        _deliveryRequestServices.addItemToDeliveryRequest(item).done(function () {
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            deliveryRequestItemTable.ajax.reload();
            bagPackageTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$drItemTable);
        });

    });

    $(document).on('click', '.btn-remove-item', function () {
        var deliveryRequestItemId = $(this).data('id');

        abp.ui.setBusy(_$drItemTable);
        _deliveryRequestServices.removeItemFromDeliveryRequest(deliveryRequestItemId).done(function () {
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            deliveryRequestItemTable.ajax.reload();
            bagPackageTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$drItemTable);
        });
    });


    _$formAddress.find('.save-address-button').on('click', function (e) {
        e.preventDefault();

        var address = _$formAddress.serializeFormToObject();

        abp.ui.setBusy(_$formAddress);
        _addressService.create(address).done(function () {
            _$modal.modal('hide');
            _$formAddress[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            var customerId = $('select[name="CustomerId"]').val();
            abp.ajax({
                url: abp.appPath + 'Orders/GetAddressByCustomerId?customerId=' + customerId,
                type: 'GET',
                dataType: 'html',
                success: function (content) {
                    if (customerId) {
                        $('#CustomerId').trigger("change", customerId);
                    }
                },
                error: function (e) {
                    alert(e);
                }
            });
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

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
        var warehouseId = $('#select-warehouse').val();
        var customerId = $('#customer-select').val();
        if (customerId && customerId > 0 && warehouseId && warehouseId > 0) {
            bagPackageTable.ajax.reload();
            _deliveryRequestServices.getByCustomerAndWarehouseId(customerId, warehouseId).done(function (res) {
                if (res && res.data) {
                    $('#hiddenDeliveryRequestId').val(res.data.id);
                    $('#deliveryRequestNumber').html('#' + res.data.requestCode);
                    $('#btn-submit-dr').removeClass('d-none');
                    deliveryRequestItemTable.ajax.reload();
                }
            });
        }
    }


    $('input[type="radio"][name="AddressId"]').change(function () {
        // Lấy giá trị của target (ID của card body sẽ được mở)
        var target = $(this).data('target');
        $('[data-type="address-id"]').each(function () {
            $(this).CardWidget('collapse');
        });

        $('div#' + target + '[data-type="address-id"]').CardWidget('expand');
        var value = $("input[name='AddressId']:checked").val();
        $('#AddressId').val(value);
    });

    $('[data-type="address-id"]').each(function () {
        $(this).CardWidget('collapse');
    });

    $(document).on("click", "#createDeliveryRequest", function () {
        AddNewDeliveryRequest();
    })

    $("#checkAll").click((e) => {
        $(".selected-order").prop("checked", $(e.target).is(":checked"));
    })

    $('#CustomerId').trigger('change');

    $('.btn-submit').on('click', function () {
        
        const data = _$form.serializeFormToObject();
        data.id = $('#hiddenDeliveryRequestId').val();
        // var data = {
        //     id : $('#hiddenDeliveryRequestId').val(),
        //     CustomerId: $('#CustomerId').val(),
        //     ShippingMethod: $('#ShippingMethod').val(),
        //     DeliveryDate: $('#DeliveryDate').val(),
        //     Notes: $('#Notes').val(),
         
        // };
    
        // Kiểm tra dữ liệu trước khi gửi
        // if (!data.CustomerId || !data.AddressId || !data.DeliveryDate || data.SelectedOrders.length === 0) {
        //     abp.message.warn('Vui lòng điền đầy đủ thông tin và chọn ít nhất một đơn hàng.');
        //     return;
        // }
    
        // Gọi API submitDeliveryRequest
        abp.ui.setBusy(); // Hiển thị trạng thái đang xử lý
        _deliveryRequestServices.submitDeliveryRequest(data)
            .done(function (result) {
                if (result.success) {
                    abp.message.success('Yêu cầu giao hàng đã được gửi thành công.');
                    window.location.href = '/DeliveryRequest'; // Chuyển hướng sau khi thành công
                } else {
                    abp.message.error(result.message || 'Có lỗi xảy ra khi gửi yêu cầu giao hàng.');
                }
            })
            .fail(function (error) {
                abp.message.error('Có lỗi xảy ra khi gửi yêu cầu giao hàng. Vui lòng thử lại.');
            })
            .always(function () {
                abp.ui.clearBusy(); // Ẩn trạng thái đang xử lý
            });
    });
})(jQuery);
