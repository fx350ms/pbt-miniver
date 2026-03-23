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
            ajaxFunction: _orderService.getByCustomer,
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
                targets: 1,
                data: 'orderDateString',
                sortable: false
            },
            {
                targets: 2,
                data: 'lastModificationTime',
                sortable: false
            },
             
            {
                targets: 3,
                data: 'statusString',
                sortable: false,
                render: (data, type, row, meta) => {
                   
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const status = orderStatusDescriptions[row.orderStatus];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : 'Chưa xác định'}</strong>`;
                    
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
})(jQuery);
