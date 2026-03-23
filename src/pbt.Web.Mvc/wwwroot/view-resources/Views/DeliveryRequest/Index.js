(function ($) {
    var _deliveryRequestService = abp.services.app.deliveryRequest,
        l = abp.localization.getSource('pbt'),
        _$table = $('#DeliveryRequestTable');


    const deliveryStatusDescriptions = {
        1: { text: 'Yêu cầu mới', class: 'badge badge-secondary' },
        2: { text: 'Đang xử lý', class: 'badge badge-warning' },
        3: { text: 'Đã xử lý', class: 'badge badge-info' },
        4: { text: 'Đã giao hàng', class: 'badge badge-primary' },
        5: { text: 'Hoàn thành', class: 'badge badge-success' },
        6: { text: 'Hủy', class: 'badge badge-dark' },
        7: { text: 'Thất bại', class: 'badge badge-danger' }
    };


    const paymentStatusDescriptions = {
        1: { text: 'Chưa thanh toán', class: 'badge badge-warning' },
        2: { text: 'Đã thanh toán', class: 'badge badge-success' },
    };

    var _$deliveryRequestTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _deliveryRequestService.getDeliveryRequestFilter,
            inputFilter: function () {
                return $('#DeliveryRequestSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$deliveryRequestTable.draw(false)
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
                data: 'requestCode',
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `   <a target="_blank" href="/DeliveryRequest/Detail/${row.id}" >`,
                        `      <strong>${row.requestCode} </strong>`,
                        '   </a>'

                    ].join('');
                }
            },
            {
                targets: 2,
                data: 'customer.username',
                width: 150,
                sortable: false,
                render: (data, type, row, meta) => {
                    return `<strong>${row.customer.username} </strong>`
                }
            },
            {
                targets: 3,
                data: 'shippingMethod',
                sortable: false,
                width: 180,
                render: (data, type, row, meta) => {
                    var html = '';
                    if (data === 1) {
                        html = '<span class="badge badge-primary">Giao hàng</span> ' + html;
                    } else if (data === 2) {
                        html = '<span class="badge badge-success">Nhận tại kho</span>' + html;
                    } else {
                        html = '<span class="badge badge-secondary">Không xác định</span>' + html;
                    }
                    return html;
                }
            },
            {
                targets: 4,
                data: 'customerAddress.fullAddress',
                sortable: false
            },
            {
                targets: 5,
                data: 'note',
                width: 280,
                sortable: false
            },
            {
                targets: 6,
                data: 'creationTimeString',
                sortable: false,
                width: 180,
                className: 'text-right'
            },
            {
                targets: 7,
                data: 'status',
                sortable: false,
                render: (data, type, row, meta) => {
                    const status = deliveryStatusDescriptions[row.status];
                    const payment = paymentStatusDescriptions[row.paymentStatus];
                    // Trả về mô tả với màu sắc được áp dụng
                    return [`<span class="m-1 ${status ? status.class : 'badge badge-secondary'}">${status ? status.text : 'Chưa xác định'}</span> `,
                    `<span class="${payment ? payment.class : 'badge badge-secondary'}">${payment ? payment.text : 'Chưa xác định'}</span> `
                    ].join('');
                }
            },
            {
                targets: 8,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    const status = deliveryStatusDescriptions[row.status];
                    const payment = paymentStatusDescriptions[row.paymentStatus];

                    let actionButton = '';
                    if (row.status === 2) {
                        actionButton = `<a type="button" class="dropdown-item  mark-to-deliveried" data-id="${row.id}" data-status="${row.status}" data-status="4" title="Đánh dấu là đã giao"><i class="fas fa-truck"></i>  Đánh dấu là đã giao</a>`;
                    } else if (row.status === 4) {
                        actionButton = `<a type="button" class="dropdown-item  mark-to-completed" data-id="${row.id}"  data-status="${row.status}" data-status="6" title="Đánh dấu là hoàn thành"><i class="fas fa-check"></i>  Đánh dấu là hoàn thành</a>`;
                    }
                    
                    // get current role
                    const currentRole = abp.auth.grantedPermissions['Role.WareHouse'];
                    let createDeliveryNote = '';
                    if (currentRole){
                        createDeliveryNote = `<a href="/DeliveryNote/Create?customerId=${row.customer.id}&deliveryRequestId=${row.id}" class="dropdown-item" data-id="${row.id}" data-status="${row.status}" data-status="4" title="Tạo phiếu xuất kho"><i class="fas fa-address-book"></i>  Tạo phếu xuất</a>`;
                    }
                    const roleEdit = abp.auth.grantedPermissions['Pages.DeliveryRequest_edit'];
                    
                    if (roleEdit){
                        return [
                            ` <div class="btn-group"> `,
                            `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                            `   </button>`,
                            `   <div class="dropdown-menu" style="">`,
                            `       <a href="/DeliveryRequest/detail/${row.id}" type="button" class="dropdown-item " dr-id="${row.id}" dr-code="${row.requestCode}" title="Edit Request">`,
                            `          <i class="fas fa-info-circle"></i>  Chỉnh sửa`,
                            '       </a>',
                            actionButton,
                            createDeliveryNote,
                            `   </div>`,
                            ` </div>`
                        ].join('');
                    }else{
                        return '';
                    }
                }
            }
        ]
    });

    $('#CreateDateRange').daterangepicker({
        autoUpdateInput: false
    });

    $(document).on("click", ".nav-delivery-request", function (event) {
        var type = $(this).attr("data-type");
        $("[name='Status']").val(type);
        $(".nav-delivery-request").removeClass("active");
        $(this).addClass("active")
        _$deliveryRequestTable.ajax.reload();
    })

  
    abp.event.on('delivery-request.edited', (data) => {
        _$deliveryRequestTable.ajax.reload();
    });

    $('.btn-search').on('click', (e) => {
        _$deliveryRequestTable.ajax.reload();
        return false;
    });


    $(document).on('click', '.mark-to-deliveried', function () {
        var drId = $(this).attr("data-id");
        _deliveryRequestService.markAsDelivered(drId).done(function (response) {
            if (response.code === 1) {
                PlaySound('success');
                abp.notify.success(response.message);
            } else {
                PlaySound('warning');
                abp.notify.error(response.message);
            }
            _$deliveryRequestTable.ajax.reload();
        });
    });

    $(document).on('click', '.mark-to-completed', function () {
        var drId = $(this).attr("data-id");
        _deliveryRequestService.markAsCompleted(drId).done(function (response) {
            if (response.code === 1) {
                PlaySound('success');
                abp.notify.success(response.message);
            } else {
                PlaySound('warning');
                abp.notify.error(response.message);
            }
            _$deliveryRequestTable.ajax.reload();
        });
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$deliveryRequestTable.ajax.reload();
            return false;
        }
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

    $('#CustomerId').select2({
        theme: 'bootstrap4',
        ajax: {
            delay: 550, // wait 250 milliseconds before triggering the request
            url: abp.appPath + 'api/services/app/Customer/getCustomerBySaleOrParentForSelect',
            type: "GET",
            data: function (params) {
                return {
                    keyword: params.term, // search term
                };
            },
            processResults: function (data) {
                return {
                    results: data.result
                };
            },
            dataType: 'json',
        }
    });

   
})(jQuery);
