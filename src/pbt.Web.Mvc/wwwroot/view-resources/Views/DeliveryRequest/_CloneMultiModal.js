(function ($) {
    var _departmentService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CloneMultiModal'),
        _$form = _$modal.find('form');

    function createMulti() {
        if (!_$form.valid()) {
            return;
        }
        
        getFullAddress();
        var data = _$form.serializeFormToObject();
        var packages = [];
        
        $("#table-packages tbody tr.active").each(function (i, e) {
            var package = {};
            $(e).find("input").each((j, input) => {
                var type = $(input).attr("type");
                if (type == "text" || type == "number") {
                    package[$(input).attr("name")] = $(input).val();
                }
                if (type == "checkbox") {
                    package[$(input).attr("name")] = $(input).is(":checked");
                }
            })
            packages.push(package);
        })
        
        const dataMulti = {
            packages: packages,
            addressId: $("[name='AddressId']:checked").val() || 0,
            numberPackage: data.NumberPackage,
        };

        abp.ui.setBusy(_$form);
        _departmentService.createPackages(dataMulti).done(function () {
            _$modal.modal('hide');
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            window.location.href = "/Packages"
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }
    
    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        createMulti();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            createMulti();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });
})(jQuery);
