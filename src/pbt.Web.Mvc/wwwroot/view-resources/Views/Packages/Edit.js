(function ($) {
    const _packageService = abp.services.app.package;
    var _shippingPartnerServices = abp.services.app.shippingPartner;
    _$form = $('#form-package'),
        _$saveButtons = $('.save-package');
    // Localization source
    const l = abp.localization.getSource('pbt');

    // Initialize select2
    // $('.select2').select2({
    //     theme: "bootstrap4", width: "100%"
    // });
    //
    $("#btnPrintStamp").on('click', (ev, i) => {
        createPackage("")
    });
    $('#selectAllCustomer').select2({
        theme: "bootstrap4"
    });

    $('[name=rdCodeType]').on('change', function () {
        var selectedValue = $('input[name="rdCodeType"]:checked').val();
        _$hiddenCodeType.val(selectedValue);
    });

    $('[name=actionScan]').on('change', function () {
        var selectedValue = $('input[name="actionScan"]:checked').val();
        $('[name=Action]').val(selectedValue);
    });

    $(".un-match-package").on("click", function (e) {
        var packageId = $("[name='Id']").val();
        _packageService.unMatchOrder(packageId).done(function (result) {
            abp.notify.info(l('UnMatchSuccessfully'));
            // reload
            setTimeout(function () {
                window.location.reload();
            }, 2000)
        }).fail(function (error) {
            abp.notify.error(error);
        });
    })

    $('#packing').on('change', function () {
        $('#wooden-crate-group').toggle($(this).is(':checked'));
    }).trigger('change');

    $('input[name="WoodenCrateType"]').on('change', function () {
        if ($(this).val() == '2' && $(this).is(':checked')) {
            $('#sharedCrateSelectContainer').removeClass("d-none");
        } else if ($(this).val() == '1' && $(this).is(':checked')) {
            $('#sharedCrateSelectContainer').addClass('d-none');
            $('#sharedCrateSelect').val(null);
        } else {
            $('#sharedCrateSelectContainer').addClass("d-none");
        }
    }).filter(':checked').trigger('change');

    function savePackage(redirectTo) {

        if (!_$form.valid()) {
            return;
        }

        var packageData = _$form.serializeFormToObject();

        packageData.Length = packageData.Length ? parseInt(packageData.Length) : null;
        packageData.Width = packageData.Width ? parseInt(packageData.Width) : null;
        packageData.Height = packageData.Height ? parseInt(packageData.Height) : null;
        packageData.IsDomesticShipping = $('[name="IsDomesticShipping"]').prop('checked');
        packageData.DomesticShippingFee = $('[name="DomesticShippingFee"]').val();
        packageData.IsShockproof = $('[name="IsShockproof"]').prop('checked');
        packageData.IsWoodenCrate = $('[name="IsWoodenCrate"]').prop('checked');

        // Ensure numeric fields are properly formatted
        packageData.Weight = packageData.Weight ? parseFloat(packageData.Weight.replaceAll(",", ".")) : null;

        abp.ui.setBusy(_$form);
        _packageService.editPackages(packageData).done(function () {
            abp.notify.info(l('SavedSuccessfully'));

            if (redirectTo === 'detail') {
                window.location.href = `/Packages/Detail/${packageData.Id}`;
            } else if (redirectTo === 'list') {
                window.location.href = '/Packages';
            }
        }).fail(function (error) {
            if (error.responseJSON && error.responseJSON.error) {
                const validationErrors = error.responseJSON.error.details;
                if (validationErrors && Array.isArray(validationErrors)) {
                    validationErrors.forEach(function (e) {
                        $(`[id='${e.propertyName}']`).addClass("is-invalid");
                    })
                    let errorMessage = validationErrors.map(e => `<span>${e.propertyName}: <b>${e.errorMessage}</b></span>`).join('<br/>')
                    abp.message.error(errorMessage, l('ValidationError'), { isHtml: true });
                } else {
                    abp.message.error(error.responseJSON.error.message, l('Error'));
                }
            } else {
                abp.message.error(l('SaveFailed'), l('Error'));
            }
        })
            .always(function () {
                abp.ui.clearBusy(_$form);
            });
    }

    // Handle save button clicks
    _$saveButtons.on('click', function (e) {
        e.preventDefault();
        var redirectTo = $(this).data('redirect');
        savePackage(redirectTo);
    });

    // getShippingPartner();
    // loadCustomer();

    $(document).on("change", ["input", "select"], function (e) {
        $(e.target).removeClass("is-invalid");
    });

    function getShippingPartner() {
        _shippingPartnerServices.getAllShippingPartners().done(function (response) {
            if (response && response.items) {
                response.items.forEach(function (partner) {
                    $("#shippingPartnerSelect").append(`<option value="${partner.id}">(${partner.code})${partner.name}</option>`);
                });
            }
        })
    }


    $("[name='ProductNameCn']").on("change", function (el) {
        const cnName = $(this).val();
        if (cnName) {
            abp.services.app.dictionary.getDictionaryByCnName(cnName).done(function (data) {
                if (data)
                    $(el.target).closest("tr").find("[name='ProductNameVi']").val(data);
            })
        }
    })

    // on selectAllCustomer selected
    $(document).on("change", "#selectAllCustomer,#select-delivery-line", function () {
        loadBag();
    });

 

    
    function loadBag() {
        //
        if ($('#hidden-bag-id').val() == '') {
            $('.select-bag-group').removeClass('d-none'); // Hiển thị select để chọn bao mới
            $('.select-new-bag').select2({
                theme: 'bootstrap4',
                ajax: {
                    delay: 550, // wait 250 milliseconds before triggering the request
                    url: abp.appPath + 'api/services/app/Bag/getSelectableBagsForPackage',
                    type: "GET",
                    data: function (params) {
                        return {
                            q: params.term, // search term
                            shippingLine: $('#select-delivery-line').val(),
                            customerId: $('#selectAllCustomer').val(),
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
        }
    }


    $(document).on('click', '.unbag-package', function () {

        var packageId = $(this).data('package-id');

        // Gọi API để bỏ kiện hàng khỏi bao
        abp.ui.setBusy();
        abp.services.app.package.unBag(packageId).done(function () {
            abp.notify.info('Đã bỏ kiện hàng khỏi bao.');
            $('#hidden-bag-id').val(''); // Xóa giá trị bao hiện tại
            $('.bag-info').remove(); // Xóa nút "Bỏ khỏi bao"
            // Hiển thị select để chọn bao mới
          //  $('.select-bag-group').removeClass('d-none'); // Hiển thị select
            loadBag(); // Gọi hàm loadBag để tải danh sách bao mới
        }).fail(function (error) {
            abp.notify.error('Không thể bỏ kiện hàng khỏi bao.');
            console.error(error);
        }).always(function () {
            abp.ui.clearBusy();
        });
    });
      

    function printLabel() {
        var content = document.getElementById('labelContent').innerHTML;

        var iframe = document.createElement('iframe');
        iframe.style.position = "absolute";
        iframe.style.width = "0px";
        iframe.style.height = "0px";
        iframe.style.border = "none";
        document.body.appendChild(iframe);

        var doc = iframe.contentWindow.document;
        doc.open();
        doc.write('<html><head><title></title>');
        doc.write('<style>');
        doc.write(`
        @media print {
             @page { size: auto;  margin: 0mm; }
             body { font-family: Arial, sans-serif; font-size: 14px; margin: 10px; }
            .label-container { width: 100mm; height: 50mm; border: 0px solid black; padding: 5px; page-break-before: always;}
            .header { display: flex; justify-content: space-between; font-weight: bold; }
            .barcode { text-align: center; margin: 5px 0; }
            .footer { display: flex; justify-content: space-between; font-weight: bold; margin-top: 5px; }
        }
    `);
        doc.write('</style></head><body>');
        doc.write(content);
        doc.write('</body></html>');
        doc.close();

        iframe.contentWindow.onload = function () {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
            setTimeout(function () {
                document.body.removeChild(iframe);
            }, 100);
        };

    }

    $("[name='DomesticShippingFee']").on("change", function () {
        var value = $(this).val();
        var rate = $("[name='RMBRate']").val() * 1;
        var priceVn = value * rate;
        $(this).siblings().find(".price-vnd").text(formatCurrency(priceVn));
    }).trigger("change")

    $("#local-ship").on("change", function () {

        const isDomesticShipping = $(this).is(":checked");
        const textBox = $(this).parent(".form-check").find(".input-price");
        if (isDomesticShipping) {
            textBox.removeClass("d-none");

        } else {
            textBox.addClass("d-none");
            textBox.val(0);
        }
    }).trigger("change");

    $("#table-new-package #check-all").on("click", function () {

        if ($(this).is(":checked")) {
            $("#table-new-package input[name='selectPackage']").prop("checked", true);
        } else {
            $("#table-new-package input[name='selectPackage']").prop("checked", false);
        }
    })

    $("#quickCreateBag").on("click", function () {
        const listBagId = $("#table-new-package tbody input:checked").map(function () {
            return $(this).val();
        }).get();
        if (listBagId.length > 0) {
            const param = listBagId.join(",")
            window.location.href = "/Bags/Create?packagesId=" + param;
        }
    })

    // tìm kiếm khi nhập input searchWaybill
    let searchTimeout;
    $(document).on("keyup", "#searchWaybill", function () {
        const value = $(this).val();
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            getListWaybill(value);
        }, 300); // Adjust the delay as needed
    });

    $("[name='IsInsured']").on("change", function () {
        const isDomesticShipping = $(this).is(":checked");
        const textBox = $(this).parent(".form-check").find("input").last();
        if (isDomesticShipping) {
            textBox.removeClass("d-none");
        } else {
            textBox.addClass("d-none");
        }
    });

    $(document).on("change", "[name='TrackingNumber']", function (el) {
        const value = $(this).val();
        const tr = $(this).closest("tr");
        abp.ui.setBusy();
        _packageService.getByWaybillNumber(value).done(function (response) {
            if (response) {
                tr.find("[name='IsInsured']").prop("checked", response.insurance > 0);
                if (response.insurance > 0) {
                    tr.find("[name='InsuranceValue']").removeClass("d-none");
                    tr.find("[name='InsuranceValue']").val(response.insurance);
                }
                tr.find("[name='IsWoodenCrate']").prop("checked", response.useWoodenPackaging);
                tr.find("[name='IsShockproof']").prop("checked", response.useShockproofPackaging);
                tr.find("[name='IsDomesticShipping']").prop("checked", response.useDomesticTransportation);
                if (response.useDomesticTransportation) {
                    tr.find("[name='DomesticShippingFee']").removeClass("d-none");
                }
            } else {
                // thông báo lỗi.
                PlaySound('warning');
                abp.notify.error("Không tìm thấy đơn hàng");
            }
        }).always(function () {
            abp.ui.clearBusy();
        });
    })

    $('input[type="radio"][name="AddressId"]').change(function () {
        var target = $(this).data('target');
        $('[data-type="address-id"]').each(function () {
            $(this).CardWidget('collapse');
        });

        $('div#' + target + '[data-type="address-id"]').CardWidget('expand');

        var value = $("input[name='AddressId']:checked").val();
        $('#AddressId').val(value);
    });

    $("#select-delivery-line").click((ev, el) => {
        const deliveryLine = $(ev.target).val();
        $("[name='ShippingLineId']").val(deliveryLine);

        if (deliveryLine == 1) {
            $("#product-group-type").removeClass("d-none")
            $("[name='ProductNameCn']").removeClass("red-placeholder").attr("placeholder", l('ProductNameCn'))
                .removeAttr("tabindex");
            $("[name='Quantity']").removeClass("red-placeholder").attr("placeholder", l('Quantity')).removeAttr("tabindex");
            $("[name='Price']").removeClass("red-placeholder").attr("placeholder", l('Price')).removeAttr("tabindex");
        }
        if (deliveryLine == 2) {
            $("#product-group-type").addClass("d-none");
            $("[name='ProductNameCn']").addClass("red-placeholder").attr("placeholder", (l('ProductNameCn') + " *")).attr("tabindex", 3)
            $("[name='Quantity']").addClass("red-placeholder").attr("placeholder", (l('Quantity') + " *")).attr("tabindex", 4)
            $("[name='Price']").addClass("red-placeholder").attr("placeholder", (l('Price') + " *")).attr("tabindex", 5)
        }
    })


    $("#select-delivery-line").trigger("change").trigger("click")

})(jQuery);


function AddNewPackage() {
    var stt = $("#table-packages tbody tr").length + 1;
    var htmlContent = $("#table-packages tbody tr").first().clone();
    htmlContent.find("input").each((j, input) => {
        const type = $(input).attr("type");
        if (type === "checkbox") {
            $(input).prop("checked", false);
        }
        $(input).val("");
    })
    htmlContent.find(".stt").text(stt);
    htmlContent.find("checkbox").text(stt);
    htmlContent.removeClass("active");
    htmlContent.find(".row-image").html('<button class=\"btn btn-outline-primary upload-image-button\">+</button>');
    $("#table-packages tbody").append(htmlContent);
    updateRowId();
    $('.select2').select2({
        theme: "bootstrap4"
    });
}

function AddClonePackage() {
    var stt = $("#table-packages tbody tr").length + 1;
    var htmlContent = $("#table-packages tbody tr.active").clone();
    htmlContent.removeClass("active");
    htmlContent.find(".stt").text(stt);
    $("#table-packages tbody").append(htmlContent);
    $('.select2').select2({
        theme: "bootstrap4"
    });
}



