(function ($) {

    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#OrderCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#OrdersTable');
    const orderStatusDescriptions = {
        1: { text: 'Đã ký gửi', color: 'blue' },
        2: { text: 'Hàng về kho TQ', color: 'purple' },
        3: { text: 'Đang vận chuyển quốc tế', color: 'orange' },
        4: { text: 'Đã đến kho VN', color: 'green' },
        5: { text: 'Đang giao đến khách', color: 'cyan' },
        6: { text: 'Đã giao', color: 'gray' },
        7: { text: 'Khiếu nại', color: 'red' },
        8: { text: 'Hoàn tiền', color: 'pink' },
        9: { text: 'Huỷ', color: 'darkgray' },
        10: { text: 'Hoàn thành đơn', color: 'teal' }
    };

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
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                data: 'orderDateString',
                sortable: false
            },
            {
                targets: 1,
                data: 'orderNumber',
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `   <a   data-order-id="${row.id}" href="/Orders/Detail/${row.id}" >`,
                        `      <strong>${row.orderNumber} </strong>`,
                        '   </a>',
                         
                    ].join('');
                }
            },
            {
                targets: 2,
                data: 'customerName',
                sortable: false
            },
            {
                targets: 3,
                data: 'orderType',
                sortable: false,
                render: (data, type, row, meta) => {
                    let orderTypeText = '';

                    // Kiểm tra giá trị của row.orderType và xác định màu sắc
                    if (row.orderType == 1) {
                        orderTypeText = `  <i class="fas fa-tag"></i> <strong class="text-success">Đơn của tôi</strong>`;
                    } else if (row.orderType == 2) {
                        orderTypeText = `  <i class="fas fa-tags"></i> <strong class="text-warning">Đơn của khách</strong>`;
                    } else {
                        
                    }

                    return orderTypeText;
                }
                
            },
            {
                targets: 4,
                data: 'statusString',
                sortable: false,
                render: (data, type, row, meta) => {
                   
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = orderStatusDescriptions[row.orderStatus];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : 'Chưa xác định'}</strong>`;
                    
                }
            },

            {
                targets: 5,
                data: null,
                sortable: false,
                width: 300,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <a type="button" class="btn btn-sm bg-secondary " data-order-id="${row.id}" href="/Orders/Detail/${row.id}" >`,
                        `       <i class="fas fa-eye"></i> ${l('View')}`,
                        '   </a>',
                        `   <a type="button" class="btn btn-sm bg-secondary" data-order-id="${row.id}"   href="/Orders/Edit/${row.id}" >`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </a>',
                        
                        `   <button type="button" class="btn btn-sm bg-danger delete-order" data-order-id="${row.id}" data-order-name="${row.name}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>'
                    ].join('');
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
        //;
        //console.log(picker.startDate.format('YYYY-MM-DD'));
        //console.log(picker.endDate.format('YYYY-MM-DD'));
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
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$ordersTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

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

    function loadOrderLogs() {
     
        var orderId = window.location.pathname.split('/').pop();
        return abp.services.app.entityChangeLogger.getLogsByMultiEntityTypeName(['ORDER', 'ORDERDTO'], orderId).done(function (data) {
           
            var logsContainer = $("#order-logs");
            logsContainer.empty();
            data.forEach(function (log) {
                logsContainer.append(`
                 <div class="log-item">
                    <div class="log-content">
                        <div class="log-text"><strong>${log.actor}</strong> - ${log.description}</div>
                        <div class="log-time">${log.timestamp}</div>
                    </div>
                 </div>
            `);
            })
        }).fail(function (error) {
            abp.notify.error("Failed to load log: " + error.message);
        });
    }

    $('#freeCheckbox').on('change', function () {
        if ($(this).is(':checked')) {
            $('#totalFeeInput').val(0); // Assuming totalFeeInput is the ID of the input field for totalFee
        }
    });
    
    $('.save-button').on('click', function (e) {
        e.preventDefault();

        var totalFee = $('#totalFeeInput').val(); // Get the updated totalFee value
        var formData = $('#editDeliveryFeeForm').serializeFormToObject(); // Serialize form data

        formData.totalFee = totalFee; // Ensure totalFee is included in the form data

        abp.ui.setBusy($('#changeFeeModal'));
        abp.services.app.order.updateDeliveryFee(formData).done(function () {
            $('#changeFeeModal').modal('hide');
            abp.notify.success('Delivery fee updated successfully!');
            window.location.reload();
        }).always(function () {
            abp.ui.clearBusy($('#changeFeeModal'));
        });
    });
    
    // edit insurance

    $(document).on('click', '#editInsurance', function () {
        $('#editInsuranceModal').modal('show');
    });

    $('#editInsuranceForm').on('submit', function (e) {
        e.preventDefault();
        var formData = $('#editInsuranceForm').serializeFormToObject(); // Serialize form data

        abp.ui.setBusy('#editInsuranceModal');
        _orderService.updateInsurance(formData).done(() => {
            abp.notify.success('Insurance updated successfully.');
            $('#editInsuranceModal').modal('hide');
            location.reload(); // Refresh the page to reflect changes
        }).always(() => {
            abp.ui.clearBusy('#editInsuranceModal');
        });
    });

    $(document).on('click', '#editWoodenPackagingFee', function () {
        
        $('#editWoodenPackagingFeeModal').modal('show');
    });

    $('#editWoodenPackagingFeeForm').on('submit', function (e) {
        e.preventDefault();
        var formData = $('#editWoodenPackagingFeeForm').serializeFormToObject(); // Serialize form data
        
        abp.ui.setBusy('#editWoodenPackagingFeeModal');
        _orderService.updateWoodenPackagingFee(formData).done(() => {
            abp.notify.success('Lưu thành công');
            $('#editWoodenPackagingFeeModal').modal('hide');
            location.reload(); // Refresh the page to reflect changes
        }).always(() => {
            abp.ui.clearBusy('#editWoodenPackagingFeeModal');
        });
    });

    $(document).on('click', '#editBubbleWrapFee, #editDomesticShipping', function () {
        
        const buttonId = $(this).attr('id');

        // Show relevant input field and hide others
        $('#bubbleWrapFeeGroup').toggle(buttonId === 'editBubbleWrapFee');
        $('#domesticShippingGroup').toggle(buttonId === 'editDomesticShipping');

        // Open the modal
        $('#editFeeModal').modal('show');
    });

    $('#editFeeForm').on('submit', function (e) {
        e.preventDefault();

        const formData = $(this).serializeFormToObject();

        abp.ui.setBusy('#editFeeModal');
        _orderService.updateFee(formData).done(() => {
            abp.notify.success('Fee updated successfully.');
            $('#editFeeModal').modal('hide');
            location.reload(); // Refresh the page to reflect changes
        }).always(() => {
            abp.ui.clearBusy('#editFeeModal');
        });
    });

    $(document).on('input', '[name="unitPrice"], [name="weight"]', function () {
        const unitPrice = parseFloat($('[name="unitPrice"]').val()) || 0;
        const weight = parseFloat($('[name="weight"]').val()) || 0;
        const totalFee = unitPrice * weight;

        $('#totalFeeInput').val(totalFee.toFixed(2)); // Update totalFee field
    });

    $(document).on('click', '#editGoodsValue', function () {
        $('#editGoodsValueModal').modal('show');
    });

    $('#editGoodsValueForm').on('submit', function (e) {
        e.preventDefault();

        var formData = $('#editGoodsValueForm').serializeFormToObject(); // Serialize form data

        abp.ui.setBusy('#editGoodsValueModal');
        _orderService.updateGoodsValue(formData).done(() => {
            abp.notify.success('Goods value updated successfully.');
            $('#editGoodsValueModal').modal('hide');
            location.reload(); // Refresh the page to reflect changes
        }).always(() => {
            abp.ui.clearBusy('#editGoodsValueModal');
        });
    });
    
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
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$ordersTable.ajax.reload();
            return false;
        }
    });

    loadOrderLogs();
})(jQuery);
