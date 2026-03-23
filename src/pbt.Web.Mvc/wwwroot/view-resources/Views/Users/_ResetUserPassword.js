(function ($) {
  
    var _userService = abp.services.app.user,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#UserResetPasswordModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }
        var changePasswordDto = _$form.serializeFormToObject();
        abp.ui.setBusy(_$form);
        _userService.resetUserPassword(changePasswordDto).done(success => {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('user.edited', user);
        }).always(function () {
            if (!skipClearBusy) {
                abp.ui.clearBusy(_$form);
            }
        });
    }

    _$form.submit(function (e) {
        e.preventDefault();
        save();
    });


})(jQuery);