(function ($) {

    var _customerService = abp.services.app.customer,
        _shippingRateGroupService = abp.services.app.shippingRateGroup,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CustomerCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#CustomersTable'),

        _$modalUpdateAmount = $('#CustomerUpdateAmountModal'),
        _$updateAmountForm = _$modalUpdateAmount.find('form');

    var _formSelectShippingRateGroupId = '#select-shipping-rate-group-form';
    const shippingLines = {
        1: { text: 'Vận chuyển hàng lô', color: 'blue' },
        2: { text: 'Vận chuyển TMĐT', color: 'purple' },
    };
    var _$customersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _customerService.getAllWithSaleInfo,
            inputFilter: function () {
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
                data: 'phoneNumber',
                sortable: false
            },
            {
                targets: 5,
                data: 'email',
                sortable: false
            },

            {
                targets: 6,
                data: 'currentAmount',
                sortable: false,
                render: function (data, type, row) {
                    let currentAmount = data; // Giá trị ban đầu của currentAmount
                    if (currentAmount < 0) {
                        return `<strong class="text-success"> 0 </strong >`;
                    }
                    else {
                        return `<strong class="text-success">` + formatCurrency(currentAmount) + `</strong >`;
                    }
                }
            },
            {
                targets: 7,
                data: 'currentAmount',
                sortable: false,
                render: function (data, type, row) {
                    let currentAmount = data; // Giá trị ban đầu của currentAmount
                    if (currentAmount < 0) {
                        return `<strong class="text-danger">` + formatCurrency(Math.abs(currentAmount)) + `</strong >`;
                    }
                    else {
                        return `<strong class="text-danger">` + formatCurrency(0) + ` </strong >`;
                    }
                }
            },
            {
                targets: 8,
                data: 'maxDebt',
                sortable: false,
                render: function (data) {
                    return `<strong> ` + formatCurrency(data) + `</strong>`;
                }
            },
            {
                targets: 9,
                data: 'saleUsername',
                sortable: false,
                render: function (data) {
                    return data ? data : '<span class="text-muted">Đang cập nhật</span>';
                }
            },
            {
                targets: 10,
                data: 'parentUsername',
                sortable: false,
                render: function (data) {
                    return data ? data : '';
                }
            },
            {
                targets: 11,
                data: 'warehouseName',
                sortable: false,
                render: function (data) {
                    return data ? data : '<span class="text-muted">Đang cập nhật</span>';
                }
            },
            {
                targets: 12,
                data: null,
                sortable: false,
                width: 20,
                className: 'text-right',
                defaultContent: '',
                render: (data, type, row, meta) => {
                    const btnChildCustomer = row.isAgent ?
                        `<a target="_blank" href="/Customers/IndexChild/${row.id}" type="button" class="dropdown-item customer-child-list text-info" data-customer-id="${row.id}"  data-customer-name="${row.name}" title="Khách hàng con">
                        <i class="fas fa-credit-card"></i> Khách hàng con</a>` : '';
                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `</button>`,
                        ` <div class="dropdown-menu" style="">`,

                        `   <a type="button" class="dropdown-item text-info edit-customer" data-customer-id="${row.id}" data-toggle="modal" data-target="#CustomerEditModal" title="${l('Edit')}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </a>',
                      
                        `   <a type="button" class="dropdown-item text-info reset-password" data-customer-id="${row.id}" data-toggle="modal" data-target="#CustomerResetPassword" title="${l('ChangePassword')}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('ChangePassword')}`,
                        '   </a>',

                     
                        `   <a type="button" class="dropdown-item lock-customer text-danger" data-customer-id="${row.userId}" data-customer-name-type="${row.user && row.user.isActive ? 'lock' : 'unlock'}" data-customer-name="${row.fullName}" title="${l('Lock')}">  `,
                        `       <i class="fas fa-lock"></i> ${row.user && row.user.isActive ? l('Lock') : l('UnLock')}`,
                        '   </a>',
                        `   <a type="button" class="dropdown-item delete-customer text-danger" data-customer-id="${row.id}"  data-toggle="modal"   data-customer-name="${row.username}" title="${l('Delete')}">  `,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </a>',
                        `    </div>`,
                        `   </div>`
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
        var customer = _$form.serializeFormToObject();
        customer.IsAgent = $('#IsAgent').is(':checked');
        abp.ui.setBusy(_$modal);
        _customerService.createWithAccount(customer).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$customersTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.lock-customer', function () {
        const customerId = $(this).attr("data-customer-id");
        const customerName = $(this).attr('data-customer-name');
        const type = $(this).attr('data-customer-name-type');
        lockCustomers(customerId, customerName, type);
    });

    function lockCustomers(userId, customerName, type) {
        abp.message.confirm(
            abp.utils.formatString(
                l(type === 'lock' ? 'AreYouSureWantToLock' : 'AreYouSureWantToUnLock'),
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
        var customerId = $(this).attr("data-customer-id");
        var customerName = $(this).attr('data-customer-name');

        deleteCustomers(customerId, customerName);
    });

    function deleteCustomers(customerId, customerName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                customerName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _customerService.deleteCustomer({
                        id: customerId
                    }).done(() => {
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


    $(document).on('click', '.edit-amount', function (e) {
        var customerId = $(this).attr("data-customer-id");
        // Set all input values in the modal to 0
        $(_$updateAmountForm).find('input').val('');
        $(_$updateAmountForm).find('[name="CustomerId"]').val(customerId);
        $('#txt-customer-new-amount').val(0);
    });

    $(_$updateAmountForm).find('.save-button').on('click', (e) => {

        if (!_$updateAmountForm.valid()) {
            return;
        }
        var updateAmount = _$updateAmountForm.serializeFormToObject();

        abp.ui.setBusy(_$modalUpdateAmount);
        _customerService.updateAmount(updateAmount)
            .done(function (result, status, xhr) {
              
                if (xhr.status === 200) {
                    abp.notify.info(l('SavedSuccessfully'));
                    _$modalUpdateAmount.modal('hide');
                    PlaySound('success');
                    abp.event.trigger('customer.edited');
                } else {
                    abp.notify.error("Cập nhật không thành công: " + result.message);
                    PlaySound('warning'); 
                }
            })
            .always(function () {
                abp.ui.clearBusy(_$modalUpdateAmount);
            });

        return false;
    });

    $(document).on('click', '.edit-balance', function (e) {
        var customerId = $(this).attr("data-customer-id");
        var url = '/Customers/Finance/' + customerId;
        window.location.href = url;
    });

    $(document).on('click', '.select-shipping-rate', function (e) {

        var customerId = $(this).attr("data-customer-id");
        var customerName = $(this).attr("data-name");
        $(_formSelectShippingRateGroupId).find('.customerId').val(customerId);
        $(_formSelectShippingRateGroupId).find('.customer-name').text(customerName);

        e.preventDefault();

        _shippingRateGroupService.getByCustomerId(customerId).done(function (data) {
            $('.select-shipping-rate-group-id').val(data.id);
        });
    });

    $('.select-shipping-rate-group-id').select2();
    $(_formSelectShippingRateGroupId).on('click', '.save-button', function (e) {

        e.preventDefault();
        var customerId = $(_formSelectShippingRateGroupId).find('.customerId').val();
        var select = $(_formSelectShippingRateGroupId).find('.select-shipping-rate-group-id');
        var optionSelected = $(select).find('option:selected');
        var groupId = optionSelected.val();
        _shippingRateGroupService.addCustomer(groupId, customerId).done(function () {
            abp.notify.info(l('SuccessfullyUpdated'));
            $('#CustomerSelectShippingRateGroup').modal('hide');
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

    $(document).on('click', '.update-max-debt', function (e) {
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

    $("#showParentCustomer").on('click', function (e) {
        _$customersTable.ajax.reload();
    })

    $(document).on('click', 'a[data-target="#CustomersCreateModal"]', (e) => {
        $('.nav-tabs a[href="#customer-details"]').tab('show')
    });

    abp.event.on('customer.edited', (data) => {
        _$customersTable.ajax.reload(null, false);
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
