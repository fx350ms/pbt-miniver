(function ($) {
    const _packageService = abp.services.app.package;
    const _shippingPartnerServices = abp.services.app.shippingPartner;

    var reCustomer = null;
    var localeName = abp.localization.currentCulture.name.replaceAll('-', '').toLowerCase();
    // Localization source
    const l = abp.localization.getSource('pbt');
    $('#selectAllCustomer').select2({
        placeholder: l('SelectCustomer')
    }).on('select2:select', function (e) {
        var selectedOption = $(this).find('option:selected');
        var customerId = selectedOption.val();
        if (customerId && customerId > 0) {
            var warehouseCode = selectedOption.data('warehousecode');
            var warehouseName = selectedOption.data('warehousename');
            $("#customerWarehouse").text(warehouseName + "-" + warehouseCode);
        }
        RememberCustomerWithShippingLine();
    });

    $("#rememberCustomer").on("change", function () {
        RememberCustomerWithShippingLine();
    });

    function RememberCustomerWithShippingLine() {
        const rememberCustomer = $("#rememberCustomer").is(":checked");
        if (rememberCustomer) {
            const customerId = $('#selectAllCustomer').val() || 0;
            const shippingLineId = $("#select-delivery-line").val() || 0;
            if (customerId && customerId > 0) {
                reCustomer = { customerId: customerId, shippingLine: shippingLineId };
                _packageService.setOrRemoveRememberCustomerId(reCustomer);
            }
        }
        else {
            reCustomer = { customerId: 0, shippingLine: 0 };
            _packageService.setOrRemoveRememberCustomerId(reCustomer);
            reCustomer = null;
        }
    }

    function initValue() {

        _packageService.getRememberCustomerId().done(function (response) {
            if (response) {
                reCustomer = response;
                $("#select-delivery-line").val(response.shippingLine);
                $('#selectAllCustomer').val(response.customerId);
                $("#rememberCustomer").prop("checked", true);

                $("#select-delivery-line,#selectAllCustomer,#rememberCustomer").trigger('change');


                var selectedOption = $('#selectAllCustomer').find(':selected');
                var warehouseCode = selectedOption.data('warehousecode');
                var warehouseName = selectedOption.data('warehousename');
                $("#customerWarehouse").text(warehouseName + "-" + warehouseCode);
            }
            else {
                $("#rememberCustomer").prop("checked", false);
                $("#select-delivery-line").val(0);
                $('#selectAllCustomer').val(0);
            }
        });
    }

    function clearTablePackagesInputs() {
        const table = document.getElementById('table-packages');
        $("#NumberVirtualPackage").val(0);
        if (table) {

            const inputs = table.querySelectorAll('input');
            inputs.forEach(input => {
                if (input.name !== "ProductGroupTypeId") input.value = '';
            });

            const textareas = table.querySelectorAll('textarea');
            textareas.forEach(textarea => {
                textarea.value = '';
            });
            clearCheckbox();
        }
    }

    $("#packageNumber").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            abp.ui.setBusy($("body"));
            // appen to table woodenPackingTable
            let value = $(this).val();
            $("#woodenPackingTable").removeClass("d-none");
            $("#lstWoodenPacking tbody").append(`<tr><td class="woodenPackingPackageNumber">${value}</td><td><button class="btn btn-sm bg-danger remote-woodenPackage" style="color: red"><i class="fas fa-trash"></i></button></td></tr>`);
            abp.ui.clearBusy($("body"));
            $("#packageNumber").val('');
        }
    });

    $(document).on("click", ".remote-woodenPackage", function (ev, i) {
        $(this).closest('tr').remove();
    })

    $(document).find('.create-package').on('click', (ev, i) => {
        const action = ev.currentTarget.attributes['data-redirect'].value;
        createPackage(action)
    });

    $("#btnPrintStamp").on('click', (ev, i) => {
        createPackage("", "lo");
    });

    $("#btnPrintStamp").keyup(function (e) {
        if (e.which == 13) {
            createPackage("", "lo")
        }
    });

    $("#btnPrintStampTmdt").on('click', (ev, i) => {

        createPackage("", "tmdt")
    });

    $("#btnPrintStampTmdt").keyup(function (e) {
        if (e.which == 13) {
            createPackage("", "tmdt")
        }
    });

    $(".btn-add-same").on("click", function (ev, i) {
        createPackage("create_same")
    });

    $(".clone-multi-button").on("click", function (ev, i) {
        createPackage("create_multi");
    });

    disableControl(false);

    function printStamp(packageIds, stampType, reload = false) {
        const packages = packageIds.join(",");
        abp.ui.setBusy();

        const url = `/Packages/PrintStamp?ids=${encodeURIComponent(packages)}&stampType=${encodeURIComponent(stampType)}`;

        $.get(url)
            .done(function (html) {
                printFromHtml(html, reload);
                $('.TrackingNumber').focus();
            })
            .fail(function (xhr) {
                abp.message.error(xhr.responseText || l('SaveFailed'), l('Error'));
            })
            .always(function () {
                abp.ui.clearBusy();
            });
    }

    let printIframeServer;

    function printFromHtml(html, reload) {
        if (!printIframeServer) {
            printIframeServer = document.createElement('iframe');
            printIframeServer.style.position = "absolute";
            printIframeServer.style.width = "0px";
            printIframeServer.style.height = "0px";
            printIframeServer.style.border = "none";
            printIframeServer.style.visibility = "hidden";
            document.body.appendChild(printIframeServer);
        }

        const doc = printIframeServer.contentWindow.document;
        doc.open();
        doc.write(html);
        doc.close();

        printIframeServer.onload = null; // ensure no stale handler
        printIframeServer.contentWindow.onload = function () {
            printIframeServer.contentWindow.focus();
            printIframeServer.contentWindow.print();

            if (reload) {
                setTimeout(() => window.location.reload(), 100);
            }
            disableControl(false);
        };
    }

    let printIframe9710;

    function printLabel9710(reload) {
        if (!printIframe9710) {
            printIframe9710 = document.createElement('iframe');
            printIframe9710.style.position = "absolute";
            printIframe9710.style.width = "0px";
            printIframe9710.style.height = "0px";
            printIframe9710.style.border = "none";
            printIframe9710.style.visibility = "hidden";
            document.body.appendChild(printIframe9710);
        }

        const content = document.getElementById('label-container-9710').innerHTML;
        const doc = printIframe9710.contentWindow.document;

        const html = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>Print Label</title>
                <style>
                    @media print {
                        @page { size: auto; margin: 0; }
                        body { font-family: Arial, sans-serif; font-size: 12px; margin: 0; padding: 0; }
                        .label-container { width: 105mm; height: 148mm; border: 1px solid black; padding: 5px; box-sizing: border-box;page-break-before: always; }
                        .section { border: 1px solid black; padding: 5px; margin-bottom: 3px; }
                        .flex { display: flex; justify-content: space-between; }
                        .bold { font-weight: bold; }
                        .barcode { text-align: center; width: 100%; margin: 5px 0; }
                        .barcode img { width: 80%; height: 80px; }
                        .amount { font-size: 16px; font-weight: bold; text-align: center; margin-top: 5px; }
                        #packageCode9710 { font-size: 28px; }
                        #barcodeImage9710 { width: 100%; height: auto; }
                        #svgBarcode9710 { width: 100%; height: auto; }
                        .toCustomerName9710 { font-size: 22px; font-weight: 600; }
                        #priceAmount9710 { font-size: 13px; }
                        #waybillCode9710 { font-size: 18px; }
                        .footer { font-size: 10px; text-align: center; margin-top: 5px; }
                    }
                </style>
            </head>
            <body>${content}</body>
            </html>
            `;
        doc.open();
        doc.write(html);
        doc.close();
        printIframe9710.contentWindow.onload = function () {
            printIframe9710.contentWindow.focus();
            printIframe9710.contentWindow.print();
            disableControl(false)
        };
    }

    function disableControl(disable) {
        $("#btnPrintStamp").attr("disabled", disable);
        $(".btn-add-same").attr("disabled", disable);
        $(".btn-add-multi").attr("disabled", disable);
        $(".create-package").attr("disabled", disable);
    }
   
    function createPackage(action, stampType) {
        stampType = $("#select-delivery-line").val() == 2 ? "tmdt" : "lo";
        if (!$("#select-delivery-line").val() || $("#select-delivery-line").val() <= 0) {
            PlaySound('warning');
            abp.notify.error(l('Vui lòng chọn line vận chuyển'));


            return;
        }
        if (!$("#selectAllCustomer").val() || $("#selectAllCustomer").val() <= 0) {
            PlaySound('warning');
            abp.notify.error(l('Vui lòng chọn khách hàng'));

            return;
        }
        const packages = [];
        var child = [];
        $("#table-packages tbody tr").each(function (i, e) {
            var package = {};
            $(e).find("input, select, textarea").each((j, input) => {
                var type = $(input).attr("type");
                var tagName = $(input).prop("tagName").toLowerCase();

                if (type === "text" || type === "number" || type === "textarea" || type === "hidden" || tagName === "select") {
                    if ($(input).attr("name") === "Weight" || $(input).attr("name") === "Price") {
                        package[$(input).attr("name")] = convertToStandardNumber($(input).val());
                        // package[$(input).attr("name")] = $(input).val().replaceAll(',', '').replace('.', ',');
                    }
                    else if ($(input).attr("name") === "DomesticShippingFee") {
                        package[$(input).attr("name")] = formatNumberCur($(input).val(), localeName);
                    }

                    else {
                        package[$(input).attr("name")] = $(input).val() == "" ? null : $(input).val();
                    }
                }
                if (type == "checkbox") {
                    package[$(input).attr("name")] = $(input).is(":checked");
                }
                if (type == "radio" && $(input).is(":checked")) {

                    package[$(input).attr("name")] = $(input).val();
                }
            });

            package["ShippingLineId"] = $("#select-delivery-line").val();
            package["CustomerId"] = $("#selectAllCustomer").val();
            package["children"] = [];
            // get list WoodenPacking from table lstWoodenPacking tbody tr td.woodenPackingPackageNumber
            package["WoodenPacking"] = $("#lstWoodenPacking").find(".woodenPackingPackageNumber").map(function () {
                return $(this).text();
            }).get();
            if ($(e).attr("type") === "parent") {
                packages.push(package);
            } else {
                packages[packages.length - 1]["children"].push(package)
            }
        });

        const data = {
            packages: packages,
            customerId: $("#selectAllCustomer").val(),
            shippingLineId: $("#select-delivery-line").val(),
            numberPackage: $("#NumberVirtualPackage").val(),
            // get from query param

        };
        abp.ui.setBusy();
        disableControl(true);
        _packageService.createPackagesWithSingleOrder(data)
            .then(function (result, status, xhr) {
                if (xhr.status === 200 && result.success) {

                    abp.notify.info(l('SavedSuccessfully'));
                    PlaySound('success');
                    if (action === 'current') {
                        clearTablePackagesInputs();
                        // window.location.href = "/Packages/Create"
                    } else if (action === 'detail') {
                        result.data.id.map((value) => {
                            clearTablePackagesInputs();
                            window.location.href = `/Packages/Detail/${value}`;
                        })
                    } else if (action == 'create_same') {
                        if (result.data.childs || result.data.childs == 0) {
                            $("[name='TrackingNumber']").val(result.data.newTrackingNumber);
                            window.history.pushState({}, '', `/Packages/Create?parentTrackingNumber=${result.data.parentTrackingNumber}`);
                        }
                        clearCheckbox();
                        printStamp(result.data.id, stampType, false);
                    }
                    else if (action == 'create_multi') {
                        clearTablePackagesInputs();
                        printStamp(result.data.id, stampType, true);
                    }
                    else {
                        clearTablePackagesInputs();
                        printStamp(result.data.id, stampType, false);
                    }
                    if (!reCustomer || !reCustomer.customerId || reCustomer.customerId == 0) {
                        $('#selectAllCustomer').select2('val', 0);
                    }

                } else {
                    PlaySound('warning');
                    if (result && result.message) {
                        abp.message.error(result.message);
                    }
                    else {
                        abp.message.error(l('SaveFailed'), l('Error'));
                    }

                }
                disableControl(false);
            })
            .fail(function (error) {
                if (error.responseJSON && error.responseJSON.error) {
                    const validationErrors = error.responseJSON.error.details;
                    if (validationErrors && Array.isArray(validationErrors)) {
                        validationErrors.forEach(function (e) {
                            $(`[id='${e.propertyName}']`).addClass("is-invalid");
                        })
                        let errorMessage = validationErrors.map(e => `<span>${e.propertyName}: <b>${e.errorMessage}</b></span>`).join('<br/>');
                        abp.message.error(errorMessage, l('ValidationError'), { isHtml: true });
                    } else {
                        abp.message.error(error.responseJSON.error.message, l('Error'));
                    }
                } else {
                    abp.message.error(l('SaveFailed'), l('Error'));
                }
                disableControl(false);
            })
            .always(function () {
                abp.ui.clearBusy();
                disableControl(false);
            });
    }

    function clearCheckbox() {
        const table = document.getElementById('table-packages');
        $(table).find('.check-IsWoodenCrate').prop('checked', false);
        $(table).find('.check-IsShockproof').prop('checked', false);
        $(table).find('.check-IsDomesticShipping').prop('checked', false);
        var price = '0';

        $(table).find('.input-domesticShippingFee').val(price);
        $(table).find('.price-vnd').html(formatCurrency(price));
        $(table).find('.input-price').addClass('d-none');

        //  $(table).find('.input-price').hide();
        // $(table).find().removeClass("d-none");
    }

    $(document).on("change", ["input", "select"], function (e) {
        $(e.target).removeClass("is-invalid");
    });

    $(".check-IsDomesticShipping").on("change", function () {
        const isDomesticShipping = $(this).is(":checked");
        const divPrice = $(this).parent(".form-check").find('.input-price');
        var price = '0';
        divPrice.find('.input-domesticShippingFee').val(price);
        divPrice.siblings().find(".price-vnd").text(price);
        if (isDomesticShipping) {
            divPrice.removeClass("d-none");
        } else {
            divPrice.addClass("d-none");
        }
    });

    $(document).on("change", "[name='ProductNameCn']", function (el) {
        const cnName = $(this).val();
        if (cnName) {
            abp.services.app.dictionary.getDictionaryByCnName(cnName).done(function (data) {
                if (data)
                    $(el.target).closest("tr").find("[name='ProductNameVi']").val(data);
            });
        } else {
            $(el.target).closest("tr").find("[name='ProductNameVi']").val('');
        }
    })

    $(document).on("click", "#tableWaybill tbody tr", function () {
        const waybillCode = $(this).attr("data-id");
        const targetId = $(this).attr("target-id");
        $(`[id='${targetId}']`).val(waybillCode);
        $("#SelectWaybill").modal("hide");
    });

    function getPackageCreateByCustomer() {
        return abp.services.app.package.getNewPackageByCurrentUser().done(function (data) {
            const tableList = $("#table-new-package tbody");
            let totalWeight = 0;
            tableList.empty();
            data.forEach(function (package) {

                totalWeight += package.weight;
                const warehouseName = package.warehouseCode
                tableList.append(`<tr>
                        <td width="10"><input name="selectPackage" value="${package.id}" data-warehouse-id="${package.warehouseId}" type="checkbox" id="pk_${package.id}"></td>
                        <td for="pk_${package.id}"><a target="_blank" href="/Packages/Detail/${package.id}" class="text-primary font-weight-bold">${package.packageNumber}</a></td>
                        <td>${package.customerName}</td>
                        <td>${formatThousand(package.weight || 0)}kg</td>
                        <td>${(package.shippingLineShortString)}</td>
                        <td>${warehouseName}</td>
                        <td width="15">
                        <button class="btn btn-sm bg-danger remote-package" style="color: red"><i class="fas fa-trash"></i></button>
                        </td>
                    </tr>`);
            });
            $("#total-weight").text(formatThousand(totalWeight));
            $("#total-package").text(data.length);
        });
    }

    function removeQuickBagging(packageId) {
        var payload = {
            id: packageId,
            isQuickBagging: false
        }
        _packageService.removeQuickBagging(payload).done(function (result) {
            PlaySound('success');
            abp.notify.info(l('SuccessfullyDeleted'));
            getPackageCreateByCustomer();
        }).fail(function (error) {
            abp.notify.error("SavedError");
        });
    }

    let printIframe;

    function printLabel(reload =false) {
        // Reuse iframe if it already exists
        if (!printIframe) {
            printIframe = document.createElement('iframe');
            printIframe.style.position = "absolute";
            printIframe.style.width = "0px";
            printIframe.style.height = "0px";
            printIframe.style.border = "none";
            printIframe.style.visibility = "hidden";
            document.body.appendChild(printIframe);
        }

        const content = document.getElementById('labelContent').innerHTML;
        const doc = printIframe.contentWindow.document;

        const html = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>Print Label</title>
                <style>
                    @media print {
                        @page { size: auto; margin: 0mm; }
                        body { font-family: Arial, sans-serif; font-size: 14px; margin: 10px; }
                        .label-container { width: 74mm; height: 52mm; border: 0px solid black; padding: 5px; page-break-before: always; }
                        .header { display: flex; justify-content: space-between; font-weight: bold; }
                        .barcode { text-align: center; margin: 5px 0; }
                        .barcode-svg { width: 100%; }
                        .footer { display: flex; justify-content: space-between; font-weight: bold; margin-top: 5px; }
                        .customerCode {font-size: 28px}
                    }
                </style>
            </head>
            <body>${content}</body>
            </html>
            `;
        doc.open();
        doc.write(html);
        doc.close();
        printIframe.contentWindow.onload = function () {
            printIframe.contentWindow.focus();
            printIframe.contentWindow.print();
            if (reload) {
                setTimeout(() => window.location.reload(), 100);
            }
            disableControl(false)
        };
    }

    $(document).on("click", ".remote-package", function () {
        var packageId = $(this).closest("tr").find("[name='selectPackage']").attr("value");
        abp.message.confirm(
            abp.utils.formatString(l('AreYouSureWantToDelete'), ""),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    removeQuickBagging(packageId)
                }
            }
        );
    });

    $("[name='DomesticShippingFee']").on("change", function () {
        
        var value = $(this).val();
        var rate = $("[name='RMBRate']").val() * 1;

        var priceVn = value * rate;
        $(this).siblings().find(".price-vnd").text(formatCurrency(priceVn));
    });

    $("#table-new-package #check-all").on("click", function () {
        const isChecked = $(this).is(":checked");
        $("#table-new-package input[name='selectPackage']").prop("checked", isChecked);
        $("#quickCreateBag").prop("disabled", !isChecked || !isSameWarehouseChecked());
    });

    $(document).on("click", "#table-new-package input[name='selectPackage']", function () {
        const isSameWarehouse = isSameWarehouseChecked();
        $("#quickCreateBag").prop("disabled", !isSameWarehouse);
    });

    $("#quickCreateBag").on("click", function () {
        const listBagId = $("#table-new-package tbody input:checked").map(function () {
            return $(this).val();
        }).get();
        if (listBagId.length > 0) {
            const param = listBagId.join(",")
            // open in new tab
            window.open("/Bags/Create?packagesId=" + param, "_blank");
            // uncheck class selectPackage
            $("#table-new-package tbody input:checked").each(function () {
                $(this).prop("checked", false);
            });
        }
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

    $(document).on("keydown", "[name='TrackingNumber']", function (el) {
        if (el.key === "Enter" || el.keyCode === 13) {
            el.preventDefault(); // Prevent default form submission

            var $inputElement = $(this);

            // Gọi AJAX... hoặc các xử lý khác
            const value = $inputElement.val();
            if (value) {
                const tr = $inputElement.closest("tr");
                _packageService.getByWaybillNumber(value).done(function (response) {
                    if (response) {
                        console.log(response);
                        if (response.customerId && response.customerId > 0) {
                            // Nếu như đang khóa và khác với khách hàng đang chọn thì cảnh báo
                            if (reCustomer != null && reCustomer.customerId > 0 && reCustomer.customerId != response.customerId) {
                                abp.notify.error(l('PleaseCheckTheCustomer'));
                                PlaySound('warning');
                                return;
                            }
                            $('#selectAllCustomer').select2('val', [response.customerId]);
                        }

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
                        if (response.note && response.note.trim().length > 0) {
                            // Show error notification
                            abp.notify.error(response.note);
                            // Add red warning note below the input
                            if (tr.find('.note-warning').length === 0) {
                                // Find the parent div of the input
                                var parentDiv = tr.find("[name='TrackingNumber']").closest('div');
                                // Append the warning note after the parent div
                                parentDiv.after('<div class="input-group mb-2 order-note"><label class="note-warning text-danger">' + response.note + '</label></div>');

                            } else {
                                tr.find('.note-warning').text(response.note);
                            }
                        }
                        else {
                            var _orderNote = tr.find('.order-note');
                            if (_orderNote)
                                $(_orderNote).remove();

                        }

                    } else {
                        var _orderNote = tr.find('.order-note');
                        if (_orderNote)
                            $(_orderNote).remove();
                    }
                }).always(function () {
                });
            }
        }
    });

    
    $('#packing').on('change', function () {
        $('#wooden-crate-group').toggle($(this).is(':checked'));
    }).trigger('change');

    $('input[name="WoodenCrateType"]').on('change', function () {
        $('#sharedCrateSelectContainer').toggle($(this).val() == '2' && $(this).is(':checked'));
    }).filter(':checked').trigger('change');

    $(document).on('change', "#select-delivery-line", function () {
        const deliveryLine = $("#select-delivery-line").val();
        if (!!deliveryLine && deliveryLine != 2) {
            $("#product-group-type").removeClass("d-none")
            $("[name='ProductNameCn']").removeClass("red-placeholder").attr("placeholder", l('ProductNameCn'))
                .removeAttr("tabindex");
            $("[name='Quantity']").removeClass("red-placeholder").attr("placeholder", l('Quantity')).removeAttr("tabindex");
            $("[name='Price']").removeClass("red-placeholder").attr("placeholder", l('Price')).removeAttr("tabindex");
            $("#btnPrintStamp").attr("tabindex", 3).show();

            $("#btnPrintStampTmdt").removeAttr("tabindex").hide();
        }
        if (deliveryLine == 2) {
            $("#product-group-type").addClass("d-none");
            $("[name='ProductNameCn']").addClass("red-placeholder").attr("placeholder", (l('ProductNameCn') + " *")).attr("tabindex", 3)
            $("[name='Quantity']").addClass("red-placeholder").attr("placeholder", (l('Quantity') + " *")).attr("tabindex", 4)
            $("[name='Price']").addClass("red-placeholder").attr("placeholder", (l('Price') + " *")).attr("tabindex", 5)
            $("#btnPrintStampTmdt").attr("tabindex", 6).show();
            $("#btnPrintStamp").removeAttr("tabindex").hide();
        }

       RememberCustomerWithShippingLine();
    });

    updateRowId();
    getPackageCreateByCustomer();
    setInterval(function () {
        // kiểm tra xem có bất kỳ checkox name = selectPackage checked
        if ($("input[name='selectPackage']:checked").length < 1) {
            getPackageCreateByCustomer();
        }

    }, 3000);
    initValue();

})(jQuery);


function updateRowId() {
    var rows = $("#table-packages tbody tr");
    rows.each((index, row) => {
        $(row).find("input, textarea, select").each((i, input) => {
            const name = $(input).attr("name");
            if (name) {
                $(input).attr("id", `Packages[${index}].${name}`);
                $(input).next("label").attr("for", `Packages[${index}].${name}`);
            }
        })
    })
}

function AddMultiClonePackage(e) {
    $("#CloneMultiModal").modal("show");
}

function isSameWarehouseChecked() {
    const checkedInputs = $("#table-new-package tbody input:checked");
    if (checkedInputs.length === 0) return false;

    const firstWarehouseId = $(checkedInputs[0]).attr("data-warehouse-id");
    return checkedInputs.toArray().every(input => $(input).attr("data-warehouse-id") === firstWarehouseId);
}

function deletePackage(_this) {
    if ($("#table-packages tbody tr").length <= 1) return
    $(_this).closest("tr").remove();
    updateRowId();
}

$(document).ready(function () {
    // 新增：为所有具有 tabindex 属性的输入框绑定 keydown 事件
    $("[tabindex]").on("keydown", function (e) {
        if (e.key === "Enter" || e.keyCode === 13) {
            e.preventDefault(); // 阻止默认行为（如表单提交）
            const currentTabIndex = parseInt($(this).attr("tabindex"), 10);
            const nextInput = $("[tabindex]").filter(function () {
                return parseInt($(this).attr("tabindex"), 10) > currentTabIndex;
            }).first();
            if (nextInput.length > 0) {
                nextInput.focus();
            }
        }
    });
});
