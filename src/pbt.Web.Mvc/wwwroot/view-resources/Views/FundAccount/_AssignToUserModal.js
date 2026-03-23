(function ($) {
    var _fundAccountService = abp.services.app.fundAccountPermission,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#AssignToUserModal'),
        _$form = _$modal.find('form');

    function save() {
       
        var fundAccount = _$form.serializeFormToObject();
        var selectedUserIds = getSelectedUserIds();
        //if (selectedUserIds.length === 0) {
        //    abp.notify.warn(l('PleaseSelectAtLeastOneUser'));
        //    return;
        //}

        fundAccount.UserIds = selectedUserIds;
        abp.ui.setBusy(_$form);
        _fundAccountService.assignUsersToFundAccount(fundAccount).done(function () {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }

    function getSelectedUserIds() {
        var selectedIds = [];
        _$form.find('.user-checkbox:checked').each(function () {
            selectedIds.push($(this).data('user-id'));
        });
        return selectedIds;
    }

    // Select/Deselect all users
    $('#select-all-users').on('change', function () {
        var isChecked = $(this).is(':checked');
        $('.user-checkbox').prop('checked', isChecked);
    });

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });
})(jQuery);