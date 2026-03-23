(function ($) {
    var _userService = abp.services.app.user,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#UserCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#UsersTable'),
        _$modalResetPin = $('#UserResetPINModal'),
        _resetPinForm = _$modalResetPin.find('form'),
        _$modalSelectSaleAdmin = $('#SelectSaleAdminModal'),
        _selectSaleAdminForm = _$modalSelectSaleAdmin.find('form');


    var _$usersTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _userService.getAllUsersByCurentUser,
            inputFilter: function () {
                return $('#UsersSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$usersTable.draw(false)
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
                data: 'id',
                sortable: false
            },
            {
                targets: 2,
                data: 'userName',
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `   <a  href="javascript:void(0)" class="  edit-user" data-user-id="${row.id}" data-toggle="modal" data-target="#UserEditModal">`,
                        `        ${row.userName}  `,
                        '   </a>'
                    ].join('');
                }
            },
            {
                targets: 3,
                sortable: false,
                render: (data, type, row, meta) => {
                    return row.roleNames ? row.roleNames : '';
                }
            },
            {
                targets: 4,
                data: 'emailAddress',
                sortable: false
            },
            {
                targets: 5,
                data: 'isActive',
                sortable: false,
                render: data => `<strong class=" ${data ? 'text-success' : 'text-danger'}">${data ? l('Active') : l('Inactive')}</strong>`
            },
            {
                targets: 6,
                data: 'customerId',
                sortable: false,
                render: (data, type, row, meta) => {
                    return data
                        ? `<span class="badge badge-primary">Khách hàng</span>`
                        : ``;
                }
            },
            {
                targets: 7,
                data: 'warehouseName',
                sortable: false

            },
            {
                targets: 8,
                data: 'saleAdminUserName',
                sortable: false
            },

            {
                targets: 9,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                width: 20,
                render: (data, type, row, meta) => {

                    var resetPinHtml = allowResetPinPermission ? [
                        `<button type="button" class="btn btn-sm bg-success reset-user-PIN" data-user-id="${row.id}" data-toggle="modal" data-target="#UserResetPINModal" title="${l('ResetSpecialPin')}">`,
                        `<i class="fas fa-key"></i> ${l('ResetSpecialPin')}`,
                        '</button>'].join('')
                        : '';
                    var assignToSaleHtml = isAdmin && row.roleNames.includes('SALE') &&  row.roleNames.length == 1  ? [
                        `<button type="button" class="btn btn-sm bg-success select-sale-admin" data-user-id="${row.id}" data-toggle="modal" data-target="#SelectSaleAdminModal" title="Chọn SaleAdmin">`,
                        `<i class="fas fa-user"></i> Chọn SaleAdmin`,
                        '</button>'].join('')
                        : '';

                    return [

                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `</button>`,
                        ` <div class="dropdown-menu" style="">`,

                        `   <button type="button" class="btn btn-sm bg-primary edit-user" data-user-id="${row.id}" data-toggle="modal" data-target="#UserEditModal" title="${l('Edit')}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-secondary reset-user-password" data-user-id="${row.id}" data-toggle="modal" data-target="#UserResetPasswordModal" title="${l('ResetPassword')}">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('ResetPassword')}`,
                        '   </button>',
                        resetPinHtml,
                        assignToSaleHtml,
                        `   <button type="button" class="btn btn-sm bg-danger delete-user" data-user-id="${row.id}" data-user-name="${row.name}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>',
                        `    </div>`,
                        `   </div>`

                    ].join('');
                }
            }
        ]
    });

    _$form.validate({
        rules: {
            Password: "required",
            ConfirmPassword: {
                equalTo: "#Password"
            }
        }
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var user = _$form.serializeFormToObject();
        user.roleNames = [];
        var _$roleCheckboxes = _$form[0].querySelectorAll("input[name='role']:checked");
        if (_$roleCheckboxes) {
            for (var roleIndex = 0; roleIndex < _$roleCheckboxes.length; roleIndex++) {
                var _$roleCheckbox = $(_$roleCheckboxes[roleIndex]);
                user.roleNames.push(_$roleCheckbox.val());
            }
        }

        abp.ui.setBusy(_$modal);
        _userService.create(user).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$usersTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-user', function () {
        var userId = $(this).attr("data-user-id");
        var userName = $(this).attr('data-user-name');

        deleteUser(userId, userName);
    });

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

    function deleteUser(userId, userName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                userName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _userService.delete({
                        id: userId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$usersTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-user', function (e) {
        var userId = $(this).attr("data-user-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Users/EditModal?userId=' + userId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#UserEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.reset-user-password', function (e) {
        var userId = $(this).attr("data-user-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Users/ResetPassword?userId=' + userId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#UserResetPasswordModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.reset-user-PIN', function (e) {
        var userId = $(this).attr("data-user-id");
        $(_resetPinForm).find('[name="UserId"]').val(userId);

    });


    $(_resetPinForm).find('.save-button').on('click', (e) => {
        if (!_resetPinForm.valid()) {
            return;
        }
        var changePinDto = _resetPinForm.serializeFormToObject();
        abp.ui.setBusy(_resetPinForm);
        _userService.updateSpecialPin(changePinDto).done(success => {
            _$modalResetPin.modal('hide');
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_resetPinForm);
        });

        return false;
    });


    $(document).on('click', '.select-sale-admin', function (e) {
        var userId = $(this).attr("data-user-id");
        $(_selectSaleAdminForm).find('[name="UserId"]').val(userId);

    });

    $(_selectSaleAdminForm).find('.save-button').on('click', (e) => {
        if (!_selectSaleAdminForm.valid()) {
            return;
        }
        var changeSaleAdminDto = _selectSaleAdminForm.serializeFormToObject();
        abp.ui.setBusy(_selectSaleAdminForm);
        _userService.updateSaleAdminForUser(changeSaleAdminDto).done(success => {
            _$modalSelectSaleAdmin.modal('hide');
            PlaySound('success');
            abp.notify.info(l('SavedSuccessfully'));
            _$usersTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_selectSaleAdminForm);

        });

        return false;
    });

    
    $(document).on('click', 'a[data-target="#UserCreateModal"]', (e) => {
        $('.nav-tabs a[href="#user-details"]').tab('show')
    });

    abp.event.on('user.edited', (data) => {
        _$usersTable.ajax.reload(null, false);
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$usersTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$usersTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
