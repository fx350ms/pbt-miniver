(function ($) {
    var _customerService = abp.services.app.customer,
    _userService = abp.services.app.user,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#AddChildToCustomerForm'),
        _$form = _$modal.find('form'),
        _$table = $('#CustomersTable');  

    const shippingLines = {
        1: { text: 'Vận chuyển hàng lô', color: 'blue' },
        2: { text: 'Vận chuyển TMĐT', color: 'purple' },
    };
    var _$customersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _customerService.getAllChild,
            inputFilter: function () {
                var parentId = $("#customerId").val();
                if(parentId){
                    $('#CustomerSearchForm').append(`<input type='hidden' name='ParentId' value='${parentId}'>`);
                }
                return $('#CustomerSearchForm').serializeFormToObject(true);
                
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$customersTable.draw(false)
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
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" data-check="row"  data-row-id="${row.id}"/>`;
                }
            },
            {
                targets: 1,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 2,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
               targets: 3,
               data: 'username',
               sortable: false,
               render: (data, type, row, meta) => {
                   return [
                       `<a type="button" class="edit-customer" data-customer-id="${row.id}" data-toggle="modal" data-target="#CustomerEditModal" title="${l('Edit')}">`,
                       data,
                       `</a>`
                   ].join('');
               }
            },
            {
                targets: 4,
              
                sortable: false,
                render: (data, type, row, meta) => {
                    const agentIcon = row.isAgent
                        ? `<i class="fas fa-star text-warning" title="${l('AgentCustomer')}"></i>` // Icon đại lý màu vàng
                        : '';

                    return [
                        ` <a target="_blank" href="/customers/details/${row.id}">${agentIcon} ${row.fullName}</a>`,
                    ].join('');
                }
            },

            {
                targets: 5, 
                data: 'phoneNumber',
                sortable: false
            },
            {
                targets: 6,
                data: 'email',
                sortable: false
            },
            {
                targets: 7,
                data: 'currentAmount',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;
                }

            },
            {
                targets: 8,
                data: 'currentDebt',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;
                }
            },
            {
                targets: 9,
                data: 'maxDebt',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;
                }
            },
            {
                targets: 10,
                data: null,
                sortable: false,
                width: 20,
                className : 'text-right',

                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `</button>`,
                        ` <div class="dropdown-menu" style="">`,
                      
                        `   <a type="button" class="dropdown-item delete-customer text-danger" data-customer-id="${row.id}" data-customer-name="${row.fullName}" title="${l('Delete')}">  `,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </a>',
                        `    </div>`,
                        `   </div>`
                    ].join('');
                }
            }
        ]
    });


    // _$form.find('.save-button').on('click', (e) => {
    //     e.preventDefault();
    //     if (!_$form.valid()) {
    //         return;
    //     }
    //     var customer = _$form.serializeFormToObject();
    //
    //     abp.ui.setBusy(_$modal);
    //     _customerService.addChildToParent(customer).done(function () {
    //         _$modal.modal('hide');
    //         _$form[0].reset();
    //         PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
    //         _$customersTable.ajax.reload();
    //     }).always(function () {
    //         abp.ui.clearBusy(_$modal);
    //     });
    // });


    $(document).on('click', '[data-check=all]', function () {
        // Kiểm tra nếu checkbox "select-all" được check hay chưa
        var isChecked = $(this).prop('checked');

        // Cập nhật trạng thái của tất cả các checkbox có data-check="customer"
        $('input[data-check=row]').prop('checked', isChecked);
    });

    $(document).on('change', 'input[data-check=row]', function () {
        // Nếu có checkbox "customer" nào không được check, bỏ check "select-all"
        if ($('input[data-check=row]:not(:checked)').length > 0) {
            $('input[data-check=all]').prop('checked', false);
        } else {
            // Nếu tất cả checkbox "customer" đều được check, thì check "select-all"
            $('input[data-check=all]').prop('checked', true);
        }
    });

    $(document).on('click', '.delete-customer', function () {
        const customerId = $(this).attr("data-customer-id");
        const customerName = $(this).attr('data-customer-name');
        debugger
        deleteCustomers(customerId, customerName);
    });

    $(document).on('click', '.add-customer', function (e) {
        
        var parentId = $("#customerId").val();
        // get id from url http://localhost:5000/Customers/IndexChild/214
        var customerId = window.location.pathname.split('/').pop();
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/AddChildCustomer/'+customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#AddChildCustomerModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });
   


    function deleteCustomers(customerId, customerName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                customerName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    debugger
                    _customerService.removeParentId(customerId).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$customersTable.ajax.reload();
                    });
                }
            }
        );
    }

    async function loadWarehouse() {
        return await abp.services.app.warehouse.getFull().done(function (data) {
            const warehouse = $("[name='WarehouseId']");
            warehouse.empty();
            warehouse.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, _warehouse) {
                warehouse.append('<option value="' + _warehouse.id + '">' + _warehouse.name + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Failed to load Warehouse: " + error.message);
        });
    }

    loadWarehouse();

    $(document).on('click', '.edit-customer', function (e) {
        var customerId = $(this).attr("data-customer-id");
     
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/EditModal?Id=' + customerId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#CustomerEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });


    $(document).on('click', '.assign-to-sale', function (e) {
        // var customerId = $(this).attr("data-customer-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/UserSales',
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#CustomerAssignToSaleModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.link-to-user', function (e) {
        // var customerId = $(this).attr("data-customer-id");
        var customerId = $(this).attr("data-customer-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/LinkToUser?Id=' + customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#CustomerLinkToUserModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });


    $(document).on('click', '.reset-password', function (e) {
        // var customerId = $(this).attr("data-customer-id");
        var customerId = $(this).attr("data-customer-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/ResetPassword?Id=' + customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {

                $('#CustomerResetPassword div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });
    
    // lock-customer click
    $(document).on('click', '.lock-customer', function () {
        const customerId = $(this).attr("data-customer-id");
        const customerName = $(this).attr('data-customer-name');
        const type = $(this).attr('data-customer-name-type');
        lockCustomers(customerId, customerName, type);
    });

    function lockCustomers(userId, customerName, type) {
        abp.message.confirm(
            abp.utils.formatString(
                l(type === 'lock' ? 'AreYouSureWantToLock': 'AreYouSureWantToUnLock'),
                customerName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    
                    _customerService.lockUserAccount(userId).done(() => {
                        abp.notify.info(type === 'lock' ? l('SuccessfullyLock') : l('SuccessfullyUnLock'));
                        _$customersTable.ajax.reload();
                    });
                }
            }
        );
    }


    $(document).on('click', '.update-max-debt', function (e) {
        // var customerId = $(this).attr("data-customer-id");
        var customerId = $(this).attr("data-customer-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Customers/UpdateDebtLimit?Id=' + customerId,
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#CustomerUpdateMaxDebt div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });
     
    $(document).on('click', 'a[data-target="#CustomersCreateModal"]', (e) => {
        $('.nav-tabs a[href="#customer-details"]').tab('show')
    });

    abp.event.on('customer.edited', (data) => {
        _$customersTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$customersTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$customersTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
