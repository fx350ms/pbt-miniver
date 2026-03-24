(function ($) {
    const _departmentService = abp.services.app.package;
    const l = abp.localization.getSource('pbt');
    const _$modal = $('#CloneMultiModal');
    const _$form = _$modal.find('form');
    var _lastWeight = 0;

    function AddCloneMultiPackage() {
        if (!_$form.valid()) {
            return;
        }
        const data = _$form.serializeFormToObject();
        const numberPackage = data.NumberPackage;
        const stt = $("#table-packages tbody tr").length + 1;
        const parentRow = $("#table-packages tbody tr.active");
        const htmlContent = parentRow.clone();
        htmlContent.attr("type", "child")
        htmlContent.removeClass("active").addClass("fake-package");
        htmlContent.find(".stt").text(stt);
        const selectShippingLine = parentRow.find("[name='ShippingLineId']").val();
        
        htmlContent.find("[name='ShippingLineId']").find(`option[value='${selectShippingLine}']`).attr("selected","selected").trigger("change");

        for (let i = 1; i <= numberPackage; i ++) {
            htmlContent.find("input[name='Weight']").val(i < numberPackage ? 10 : _lastWeight)
            htmlContent.clone().insertAfter(parentRow);
        }
        $('.select2').select2({
            theme: "bootstrap4"
        });
        $("#CloneMultiModal").modal("hide");
    }
    
    function createMulti() {
        if (!_$form.valid()) {
            return;
        }

        getFullAddress();
        const data = _$form.serializeFormToObject();
        const packages = collectPackages();

        const addressId = $("[name='AddressId']:checked").val() || 0;
        if (addressId === 0) {
            notifyAddressSelection();
            return;
        }

        const dataMulti = {
            packages: packages,
            addressId: addressId,
            numberPackage: data.NumberPackage,
        };

        abp.ui.setBusy(_$form);
        _departmentService.createPackages(dataMulti)
            .done(handleSuccess)
            .always(() => abp.ui.clearBusy(_$form));
    }

    function collectPackages() {
        const packages = [];
        $("#table-packages tbody tr.active").each((i, e) => {
            const package = {};
            $(e).find("input").each((j, input) => {
                const type = $(input).attr("type");
                const name = $(input).attr("name");
                if (type === "text" || type === "number") {
                    package[name] = $(input).val();
                } else if (type === "checkbox") {
                    package[name] = $(input).is(":checked");
                }
            });
            packages.push(package);
        });
        return packages;
    }

    function notifyAddressSelection() {
        PlaySound('warning');
        abp.notify.error(l('Vui lòng chọn địa chỉ'));
    }

    function handleSuccess() {
        _$modal.modal('hide');
        PlaySound('success');
        abp.notify.info(l('SavedSuccessfully'));
        window.location.href = "/Packages";
    }

    // _$form.closest('div.modal-content').find(".clone-multi-button").click(function (e) {
    //     e.preventDefault();
    //     AddCloneMultiPackage();
    // });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            AddCloneMultiPackage();
        }
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
        let weight = $("#table-packages tbody tr.active").find("input[name='Weight']").val();
        weight = parseFloat(weight);
        const numberPackage = weight - 10 > 0 ? Math.floor((weight - 10) / 10) + 1 : 0;
        $("#NumberVirtualPackage").val(numberPackage)
        _lastWeight = weight % 10 > 0 ? weight % 10 : 10;
    });
})(jQuery);
