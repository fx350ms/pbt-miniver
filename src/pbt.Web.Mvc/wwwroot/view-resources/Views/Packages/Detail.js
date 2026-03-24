(function ($) {
    var _bagService = abp.services.app.bag,
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#PackagesPendingTable');

    loadWarehouse();
     
    $(document).on('click', '.btn-remote-delivery-note', function (e) {
      
        var id = $(this).attr("data-id");
        var dnId = $(this).attr("data-delivery-note-id");
        var dnCode = $(this).attr("data-code");
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                dnCode),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _packageService.removeDeliveryNote(id, dnId).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        window.location.reload()
                    });
                }
            }
        );
    });

    $(document).on('click', '.delete-department', function () {
        var departmentId = $(this).attr("data-department-id");
        var departmentName = $(this).attr('data-department-name');

        deleteDepartments(departmentId, departmentName);
    });

    function deleteDepartments(departmentId, departmentName) {
        abp.message.confirm(abp.utils.formatString(l('AreYouSureWantToDelete'), departmentName), null, function (isConfirmed) {
            if (isConfirmed) {
                _warehouseService.delete({
                    id: departmentId
                }).done(function () {
                    abp.notify.info(l('SuccessfullyDeleted'));
                    _$departmentsTable.ajax.reload();
                });
            }
        });
    }

    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    })

    function loadWarehouse() {
        return abp.services.app.warehouse.getFull().done(function (data) {
            var dropdown = $("#WarehouseCode");
            dropdown.empty();
            dropdown.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, warehouse) {
                dropdown.append('<option value="' + warehouse.id + '">' + warehouse.name + '</option>');
            });
        }).fail(function (error) {
            abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }

    function loadPackageLogs() {
        var packageId = $("[name='packageId']").val();
        return abp.services.app.entityChangeLogger.getLogsByMultiEntityTypeName(['PACKAGE','PACKAGEDTO'], packageId).done(function (data) {

            var logsContainer = $("#package-logs");
            logsContainer.empty();
            data.forEach(function (log) {
                logsContainer.append(`
                 <div class="log-item">
                    <div class="log-content">
                        <div class="log-text"><strong>${log.actor}</strong> - ${log.description}</div>
                        <div class="log-time">${log.timestamp}</div>
                    </div>
                 </div>
            `);
            })

        }).fail(function (error) {
            abp.notify.error("Failed to load log: " + error.message);
        });
    }
 

    $('.btnPrintLabel').click(function () {
        var type = $(this).attr("data-type");
        var packageId = $("[name='packageId']").val();

        $("#labelContent").empty();
        abp.ui.setBusy("body");

        if (type === "real") {
            abp.services.app.package.getDetail(packageId, true).done(function (tem) {
                var temp = generateLabelTemplate({
                    isReal: true,
                    wh: "HN",
                    line: tem.shippingLineId == 1 ? "LO" : 'TMDT',
                    packageCode: tem.packageNumber,
                    customerCode: tem.customer && tem.customer.username ? tem.customer.username : "",
                    orderNumber: tem.trackingNumber,
                    waybillNumber: tem.waybillNumber,
                    weight: tem.weight + "kg",
                    warehouseCode: tem.vnWarehouse ? tem.vnWarehouse.code : ""
                });

                $("#labelContent").append(temp);
                generateBarcodes();
                printLabel();
                abp.ui.clearBusy("body");
            });
        }

        if (type === "fake") {
            abp.services.app.package.getPackageFakes(packageId).done(function (data) {
                data.forEach(function (tem) {
                    var temp = generateLabelTemplate({
                        isReal: false,
                        wh: "HN",
                        line: tem.shippingLineId == 1 ? "LO" : 'TMDT',
                        packageCode: tem.trackingNumber,
                        customerCode: tem.customerFake && tem.customerFake.fullName ? tem.customerFake.fullName : "",
                        orderNumber: tem.order && tem.order.orderNumber ? tem.order.orderNumber : "",
                        waybillNumber: tem.order && tem.order.waybillNumber ? tem.order.waybillNumber : "",
                        weight: tem.weight + "kg"
                    });

                    $("#labelContent").append(temp);
                });
                generateBarcodes();
                printLabel();
                abp.ui.clearBusy("body");
            });
        }
    });


    // 👉 Tạo template label DOM từ dữ liệu
    function generateLabelTemplate(data) {
        const $label = $(`
        <div class="label-container">
            <div class="header">
                <span><span class="whvn">${data.wh}</span>&nbsp;&nbsp;<span class="linevn">${data.line}</span></span>
                <span style="font-size: 28px" class="packageCode">${data.packageCode}</span>
            </div>
            <div class="barcode">
                <svg class="barcode-svg" data-code="${data.packageCode}"></svg>
            </div>
            <div class="footer" style="position: relative">
                <div>
                    <p class="customerCode">${data.customerCode}</p>
                    <p class="orderNumber">${data.orderNumber}</p>
                    <p class="waybillNumber">${data.waybillNumber}</p>
                    <p style="font-weight: bold;">pbtt</p>
                </div>
                <div>
                    <p class="weight">${data.weight}</p>
                </div>
                <span id="warehouseCode" style="position: absolute; right: 4px; bottom: 4px; font-size: 28px">${data.warehouseCode}</span>
            </div>
        </div>
    `);

        return $label;
    }

    // 👉 Dùng JsBarcode để render tất cả barcode SVG
    function generateBarcodes() {
        $(".barcode-svg").each(function () {
            const code = $(this).data("code");
            JsBarcode(this, code, {
                format: "CODE128", width: 2.3, height: 80, displayValue: false
            });
        });
    }


    let printIframe;

    function printLabel() {
        if (!printIframe) {
            printIframe = document.createElement('iframe');
            printIframe.style.position = 'absolute';
            printIframe.style.width = '0';
            printIframe.style.height = '0';
            printIframe.style.border = 'none';
            printIframe.style.visibility = 'hidden';
            document.body.appendChild(printIframe);
        }

        const doc = printIframe.contentWindow.document;
        const content = document.getElementById('labelContent').innerHTML;

        const html = `
    <!DOCTYPE html>
    <html>
    <head>
        <title>In tem</title>
        <style>
            @media print {
                @page { size: auto; margin: 0mm; }
                body { font-family: Arial, sans-serif; font-size: 14px; margin: 10px; }
                .label-container { width: 74mm; height: 52mm; padding: 5px; page-break-before: always; }
                .header { display: flex; justify-content: space-between; font-weight: bold; }
                .barcode { text-align: center; margin: 5px 0; }
                .barcode-svg{ width: 100%; }
                .footer { display: flex; justify-content: space-between; font-weight: bold; margin-top: 5px; }
                .customerCode {font-size: 28px}
                .orderNumber {padding-top: 0px; margin-top: 0; line-height: 0}
                .waybillNumber {font-size: 14px; padding-top: 0px; margin-top: 0;}
            }
        </style>
    </head>
    <body>
        ${content}
    </body>
    </html>
    `;

        doc.open();
        doc.write(html);
        doc.close();

        // In sau một nhịp render (tối ưu hơn so với onload vì iframe đã tồn tại)
        setTimeout(() => {
            try {
                printIframe.contentWindow.focus();
                printIframe.contentWindow.print();
            } catch (e) {
                console.error("Lỗi khi in:", e);
            }
        }, 50);
    }


    $('#btnPrintLabel9710').click(function () {
        var packageId = $("[name='packageId']").val();

        abp.ui.setBusy("body");

        abp.services.app.package.getDetail(packageId, true).done(function (tem) {
            // Đổ dữ liệu
            $("#sender9710").text(tem.fakeCompany || '');
            $("#to9710").text(tem.customerFake && tem.customerFake.fullName ? tem.customerFake.fullName : '');
            $("#sdt9710").text(tem.customerFake && tem.customerFake.phoneNumber ? tem.customerFake.phoneNumber : '');
            $("#add9710").text(tem.customerFake && tem.customerFake.address ? tem.customerFake.address : '');
            $("#packageCode9710").text(tem.packageNumber);
            $("#waybillCode9710").text('(' + tem.trackingNumber + ')');
            $("#waybillNumber9710").text(tem.waybillNumber);
            $("#orderCode9710").text(tem.order && tem.order.orderNumber ? tem.order.orderNumber : '');
            $("#quantity9710").text(tem.quantity);
            $("#packageName9710").text(tem.productNameVi);
            $("#toCustomerName9710").text(tem.customer && tem.customer.username ? tem.customer.username : '');
            $("#orderCreate9710").text(tem.order && tem.order.orderDateString ? tem.order.orderDateString : '');
            $("#priceAmount9710").text(tem.price >= 3000000 ? "***" : tem.priceStr);
            $("#weight9710").text(tem.weightString);
            $("#warehouseVn").text(!tem.vnWarehouse ? '' : tem.vnWarehouse.name + '-' + tem.vnWarehouse.code);
            $("#amount").text(tem.totalPrice.toLocaleString("vi-VN") + " VNĐ");

            var $barcodeContainer = $('#barcodeImage9710');
            $barcodeContainer.empty(); // reset
            var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            svg.setAttribute("id", "svgBarcode9710");
            svg.style.width = "100%";
            svg.style.height = "80px";
            $barcodeContainer.append(svg);

            JsBarcode(svg, tem.packageNumber, {
                format: "CODE128", width: 2.3, height: 80, displayValue: false
            });

            // In label
            printLabel9710();
            abp.ui.clearBusy("body");
        });
    });



    let printIframe9710;

    function printLabel9710() {
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
                .label-container { width: 105mm; height: 148mm; border: 1px solid black; padding: 5px; box-sizing: border-box; }
                .section { border: 1px solid black; padding: 5px; margin-bottom: 3px; }
                .flex { display: flex; justify-content: space-between; }
                .bold { font-weight: bold; }
                .barcode { text-align: center; width: 100%; margin: 5px 0; }
                .barcode img { width: 80%; height: 80px; }
                .amount { font-size: 16px; font-weight: bold; text-align: center; margin-top: 5px; }
                #packageCode9710 { font-size: 28px; }
                #barcodeImage9710 { width: 100%; height: auto; }
                #svgBarcode9710 { width: 100%; height: auto; }
                #toCustomerName9710 { font-size: 22px; font-weight: 600; }
                #priceAmount9710 { font-size: 13px; }
                #waybillCode9710 { font-size: 18px; }
                #waybillNumber9710 { font-size: 14px; }
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

        setTimeout(() => {
            const state = printIframe9710.contentWindow.document.readyState;
            if (state === 'complete') {
                printIframe9710.contentWindow.focus();
                printIframe9710.contentWindow.print();
            } else {
                setTimeout(() => {
                    printIframe9710.contentWindow.focus();
                    printIframe9710.contentWindow.print();
                }, 50);
            }
        }, 50);
    }



    $(document).on('click', '.edit-department', function (e) {
        var departmentId = $(this).attr("data-department-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Warehouses/EditModal?Id=' + departmentId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#DepartmentEditModal div.modal-content').html(content);
                loadProvince("#EditProvinceId").then(function () {
                    $("#EditProvinceId").trigger("change");
                });
            },
            error: function (e) {
            }
        });
    });

    $('#ProvinceId').change(function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here

                var districtDropdown = $('#DistrictId');
                districtDropdown.empty();
                // Thêm option mặc định
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    districtDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });


    $(document).on("change", "#EditProvinceId", function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                // code here

                var districtDropdown = $('#EditDistrictId');
                var selectedId = districtDropdown.attr("value");
                districtDropdown.empty();
                // Thêm option mặc định
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    var selected = selectedId == item.id ? "selected" : "";
                    districtDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                });

                districtDropdown.trigger("change");
            });
        } else {
            $('#EditDistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#EditWardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $('#DistrictId').change(function () {
        var districtId = $(this).val();
        if (districtId) {
            abp.services.app.ward.getFullByDistrict(districtId).done(function (data) {
                // code here

                var wardDropdown = $('#WardId');
                wardDropdown.empty();
                // Thêm option mặc định
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    wardDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $(document).on("change", "#EditDistrictId", function () {
        var districtId = $(this).val();
        if (districtId) {
            abp.services.app.ward.getFullByDistrict(districtId).done(function (data) {
                // code here

                var wardDropdown = $('#EditWardId');
                var selectedId = wardDropdown.attr("value");
                wardDropdown.empty();
                // Thêm option mặc định
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                // Duyệt qua mảng dữ liệu và thêm các option
                $.each(data, function (index, item) {
                    var selected = selectedId == item.id ? "selected" : "";
                    wardDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                });
            });
        } else {
            $('#DistrictId').html('<option value="">@L("SelectDistrict")</option>');
            $('#WardId').html('<option value="">@L("SelectWard")</option>');
        }
    });

    $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', function (e) {
        $('.nav-tabs a[href="#department-details"]').tab('show');
        loadProvince();
    });

    //abp.event.on('department.edited', (data) => {
    //    _$departmentsTable.ajax.reload();
    //});

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    //$('.btn-search').on('click', (e) => {
    //    _$departmentsTable.ajax.reload();
    //});

    //$('.txt-search').on('keypress', (e) => {
    //    if (e.which == 13) {
    //        _$departmentsTable.ajax.reload();
    //        return false;
    //    }
    //});

    loadPackageLogs();
})(jQuery);
