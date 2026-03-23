(function ($) {
    var _fundAccountService = abp.services.app.fundAccount,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#FundAccountCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#FundAccountsTable');

    var _$fundAccountsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _fundAccountService.getAll,
            inputFilter: function () {
                return $('#FundAccountSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$fundAccountsTable.draw(false)
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
                data: 'accountName',
                sortable: false
            },
            {
                targets: 2,
                data: 'accountType',
                sortable: false,
                render: function (data, type, row) {
                    return data === 1 ? 'Ngân hàng' : data === 2 ? 'Tiền mặt' : data;
                }
            },
            {
                targets: 3,
                data: 'currency',
                sortable: false
            },
            {
                targets: 4,
                data: 'bankName',
                sortable: false
            },
            {
                targets: 5,
                data: 'accountNumber',
                sortable: false
            },
            {
                targets: 6,
                data: 'accountHolder',
                sortable: false
            },
            {
                targets: 7,
                data: 'totalAmount',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return '<strong>' + formatThousand(data) + '</strong> ' + row.currency;
                }
            },
            {
                targets: 8,
                data: 'notes',
                sortable: false
            },
            {
                targets: 9,
                data: 'isActived',
                sortable: false,
                render: function (data) {
                    return data ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-secondary">Ngưng hoạt động</span>';
                }
            },
            {
                targets: 10,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    const canEdit = row.isActived; // Allow edit only if the fund account is active
                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `</button>`,
                        ` <div class="dropdown-menu" style="">`,
                        `   <a type="button" class="dropdown-item bg-info assign-users" data-toggle="modal"  data-target="#AssignToUserModal" data-fundAccount-id="${row.id}"  title="${l('AssignUsers')}" >`,
                        `       <i class="fas fa-user-plus"></i> ${l('AssignUsers')}`,
                        '   </a>',
                        `   <a type="button" class="dropdown-item bg-secondary edit-fundAccount" data-toggle="modal" data-target="#FundAccountEditModal" data-fundAccount-id="${row.id}"   title="${l('Edit')}" >` +
                        `       <i class="fas fa-pencil-alt"></i>${l('Edit')}` +
                        '   </a>',
                        `   <a type="button" class="dropdown-item bg-danger delete-fundAccount" data-fundAccount-id="${row.id}" data-fundAccount-name="${row.accountName}" title="${l('Delete')}" data-toggle="tooltip">` +
                        `       <i class="fas fa-trash"></i> ${l('Delete')}` +
                        '   </a>',
                        `    </div>`,
                        `   </div>`,
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
        var fundAccount = _$form.serializeFormToObject();
        fundAccount.IsActived = $('#CheckIsActived').is(':checked');

        //_$form.find('#CheckIsActived').change(function () {
        //    $('#IsActived').val(this.checked ? true : false);
        //});
        // Ensure IsActived is correctly serialized
        // fundAccount.IsActived = _$form.find('input[name="IsActived"]').is(':checked');

        abp.ui.setBusy(_$modal);
        _fundAccountService.create(fundAccount).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$fundAccountsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    //$(document).on('click', '.save-fund-user', function () {
    //    e.preventDefault();

    //    if (!_$form.valid()) {
    //        return;
    //    }
    //    var fundAccount = _$form.serializeFormToObject();
    //    fundAccount.IsActived = $('#CheckIsActived').is(':checked');


    //    abp.ui.setBusy(_$modal);
    //    _fundAccountService.create(fundAccount).done(function () {
    //        _$modal.modal('hide');
    //        _$form[0].reset();
    //        PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
    //        _$fundAccountsTable.ajax.reload();
    //    }).always(function () {
    //        abp.ui.clearBusy(_$modal);
    //    });
    //});

    $(document).on('click', '.delete-fundAccount', function () {
        var fundAccountId = $(this).attr("data-fundAccount-id");
        var fundAccountName = $(this).attr('data-fundAccount-name');
        deleteFundAccounts(fundAccountId, fundAccountName);
    });

    function deleteFundAccounts(fundAccountId, fundAccountName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                fundAccountName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _fundAccountService.delete({
                        id: fundAccountId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$fundAccountsTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.assign-users', function (e) {
        var fundAccountId = $(this).attr("data-fundAccount-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'FundAccount/AssignToUserModal?Id=' + fundAccountId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#AssignToUserModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.edit-fundAccount', function (e) {
        var fundAccountId = $(this).attr("data-fundAccount-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'FundAccount/EditModal?Id=' + fundAccountId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#FundAccountEditModal div.modal-content').html(content);
                toggleBankFields();
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#FundAccountCreateModal"]', (e) => {
        $('.nav-tabs a[href="#fundAccount-details"]').tab('show')
    });

    abp.event.on('fundAccount.edited', (data) => {
        _$fundAccountsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$fundAccountsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$fundAccountsTable.ajax.reload();
            return false;
        }
    });

    function toggleBankFields() {
        var accountType = _$form.find('input[name="AccountType"]:checked').val();
        if (accountType == '1') { // Bank
            _$form.find('input[name="BankName"]').closest('.form-group').show();
            _$form.find('input[name="AccountNumber"]').closest('.form-group').show();
            _$form.find('input[name="AccountHolder"]').closest('.form-group').show();
        } else { // Cash
            _$form.find('input[name="BankName"]').closest('.form-group').hide();
            _$form.find('input[name="AccountNumber"]').closest('.form-group').hide();
            _$form.find('input[name="AccountHolder"]').closest('.form-group').hide();
        }
    }

    _$form.find('input[name="AccountType"]').change(function () {
        toggleBankFields();
    });

    _$form.find('#CheckIsActived').change(function () {
        $('#IsActived').val(this.checked ? true : false);
    });


    // Initial call to set the correct visibility on page load
    toggleBankFields();
})(jQuery);