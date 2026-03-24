(function ($) {
    var _packageService = abp.services.app.package;
    var _bagService = abp.services.app.bag;
    var l = abp.localization.getSource('pbt');
    var _$table = $('#PackagesBagTable');
    var _bagId = $("[name='bagId']").val();

    $(".select2").select2({
        allowClear: true,
        theme: "bootstrap4",
        disabled: true
    });

    const warehouseStatusDescriptions = {
        "0": { text: l('MissingInformation'), color: 'red' },
        "1": { text: l('WarehouseStatus.InStock'), color: 'blue' },
        "2": { text: l('WarehouseStatus.OutOfStock'), color: 'green' }
    };
    const packageDeliveryStatusDescriptions = {
        "0": { text: l('MissingInfo'), color: 'danger' },
        "1": { text: l('Initiate'), color: 'secondary' },
        "2": { text: l('InTransit'), color: 'info' },
        "3": { text: l('WaitingForShipping'), color: 'warning' },
        "4": { text: l('Shipping'), color: 'primary' },
        "5": { text: l('InWarehouseVN'), color: 'info' },
        "6": { text: l('WaitingForDelivery'), color: 'warning' },
        "7": { text: l('DeliveryRequest'), color: 'info' },
        "8": { text: l('DeliveryInProgress'), color: 'info' },
        "9": { text: l('Delivered'), color: 'success' },
        "10": { text: l('Completed'), color: 'success' },
        "11": { text: l('WaitingForReturn'), color: 'warning' },
        "12": { text: l('Returning'), color: 'primary' },
        "13": { text: l('Returned'), color: 'success' },
        "14": { text: l('CustomerNotClaiming'), color: 'danger' },
        "15": { text: l('WaitingForClearance'), color: 'warning' },
        "16": { text: l('Clearance'), color: 'success' }
    };
    var _$packagePendingTable = _$table.DataTable({
        paging: false,
        serverSide: true,

        listAction: {
            ajaxFunction: function () {

                return _packageService.getAllPackagesByBagId(arguments[0]).done(function (value) {
                    $("#totalPackage").text(value.totalCount);

                });
            },
            inputFilter: function () {
                return { bagId: $("[name='bagId']").val() };
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: function () { _$packagePendingTable.draw(false); }
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
                data: 'packageNumber',
                sortable: false
            },
            {
                targets: 2,
                data: 'trackingNumber',
                sortable: false
            },
            {
                targets: 3,
                sortable: false,
                data: 'customerName',
            },
            {
                targets: 4,
                data: 'weight',
                sortable: false,
                className: 'text-right',
                render: function (data, type, row, meta) {
                    return data ? formatNumberThousand(data, 2) : '0';
                }
            },
            {
                targets: 5,
                data: 'volume',
                sortable: false
            },
            {
                targets: 6,
                data: 'note',
                sortable: false
            },
            {
                targets: 7,
                data: 'creationTimeFormat',
                sortable: false
            },
            {
                targets: 8,
                data: 'matchTime',
                sortable: false,
                render: function (data) {
                    return data ? formatDateToDDMMYYYYHHmm(data) : '';
                }
            },
            {
                targets: 9,
                data: 'deliveryTime',
                sortable: false
            },
            {
                targets: 10,
                data: "warehouseStatus",
                render: function (data, type, row, meta) {
                    var status = warehouseStatusDescriptions[data];
                    return '<span style="color: ' + status.color + '">' + status.text + '</span>';
                }
            },
            {
                targets: 11,
                data: 'shippingStatus',
                sortable: false,
                render: function (data, type, row, meta) {
                    var status = packageDeliveryStatusDescriptions[row.shippingStatus];
                    if (status) {
                        return '<span class="badge badge-' + status.color + '">' + status.text + '</span>';
                    }
                    return '<span class="badge badge-secondary">Không xác định</span>';
                }
            },
            {
                targets: 12,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: function (data, type, row, meta) {
                    return row.isRepresentForWeightCover ? '' : [
                        '   <a href="" type="button" class="btn btn-sm bg-secondary edit-package" data-id="' + row.id + '">',
                        '       <i class="fas fa-pencil-alt"></i> ',
                        '   </a>',
                        '<button type="button" class="btn btn-sm bg-danger delete-package" data-id="' + row.id + '" data-name="' + row.packageNumber + '">',
                        '<i class="fas fa-trash"></i> ',
                        '</button>'
                    ].join('');
                }
            }
        ],
        rowCallback: function (row, data, index) {
            // Kiểm tra nếu weight = 0
            if (data.isRepresentForWeightCover) {
                // Thêm class hoặc style để thay đổi màu sắc của hàng
                $(row).css('background-color', '#ffcccc'); // Màu nền đỏ nhạt
            }
        }
    });


    $(document).on('click', '.edit-package', function (e) {
        const packageId = $(this).attr("data-id");
        e.preventDefault();
        window.location.href = "/Packages/Edit/" + packageId;

    });

    // Bắt sự kiện khi DataTable vẽ xong
    _$table.on('draw.dt', function () {
        checkPackingClose();
    });

    $("#scanCode").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            abp.ui.setBusy($("body"));

            var bagId = $("[name='bagId']").val();
            var data = {
                PackageCode: e.target.value.trim(),
                BagId: bagId.trim()
            };
            _bagService.addPackageToBag(data).done(function (res) {
                if (res && res.status == 200) {
                    $("#packageWeight").val(res.data);
                    abp.notify.success(l(res.message));
                    PlayAudio('success');
                    _$packagePendingTable.ajax.reload();
                }
                else {
                    abp.message.error(l(res.message));
                    PlaySound('warning');
                    _$packagePendingTable.ajax.reload();
                }
            }).fail(function (error) {
                abp.message.error(error.message);
                PlaySound('warning');
            });
            abp.ui.clearBusy($("body"));
            $(this).val("");
        }
    });

    $("#packageWeight").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            abp.ui.setBusy($("body"));
            var isChecked = $("#packageCheck").prop('checked');
            var weightCover = $("#packageCheck").attr("data-value").replaceAll(",", ".");

            abp.services.app.bag.updateCoverWeight({
                id: _bagId,
                weight: e.target.value.replaceAll(",", "."),
                isWeightCover: isChecked,
                weightCover: weightCover
            }).done(function (data) {

                _$packagePendingTable.ajax.reload();
                checkPackingClose();
                abp.ui.clearBusy($("body"));
                // hiển thị thông báo
                abp.notify.success("Cập nhật trọng lượng bao thành công!");
            }).fail(function (error) {
                PlaySound('warning');
                abp.notify.error("Failed to update weight: " + error.message);
                abp.ui.clearBusy($("body"));
            });
        }
    });

    // lấy danh sách bao đã đóng
    function getBagClosed() {
        return abp.services.app.bag.getBagsClosed().done(function (data) {
            var list = $("#list-closed-bag");
            list.empty();
            for (var i = 0; i < data.length; i++) {
                var bag = data[i];
                var tempHtml = '<div class="list-group-item">' +
                    '<a href="/bags/bagging/' + bag.id + '"><h5 class="mb-1 text-primary">' + bag.bagCode + '</h5></a>' +
                    '<span class="mb-1 float-left">Cân nặng: <strong>' + (bag.weight || 0) + ' kg</strong></span>' +
                    '<small class="float-right">S.lg kiện: <strong>' + bag.totalPackage + '</strong></small>' +
                    '</div>';
                list.append(tempHtml);
            }
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Failed to load bag: " + error.message);
        });
    }

    function addPackage(packageCode) {
        var bagId = $("[name='bagId']").val();
        var data = {
            PackageCode: packageCode,
            BagId: bagId.trim()
        };
        return _bagService.addPackageToBag(data).done(function (res) {
            debugger;
            if (res && res.status == 200) {
                $("#packageWeight").val(res);
                abp.notify.success(res.message);
                PlayAudio('success');
            }
            else {
                abp.message.error(res.message);
                PlaySound('warning');
            }

        }).fail(function (error) {
            debugger;
            abp.message.error(error.message);
            PlaySound('warning');
        });
    }

    // Nếu cân nặng bao và trọng lương chênh lệch không quá 3kg thì đóng bao
    function checkPackingClose() {
        var bagWeight = parseFloat($("#packageWeight").val() || 0);
        var totalWeight = 0;
        _$packagePendingTable.rows().data().each(function (row) {
            totalWeight += parseFloat(row.weight) || 0;
        });
        var packageValue = $("#packageCheck").attr("data-value").replaceAll(",", ".");
        var hasPackage = $("#packageCheck").is(":checked");
        if (hasPackage) totalWeight += parseFloat(packageValue);
        if (Math.abs(bagWeight - totalWeight) <= 3 && bagWeight > 0) {
            $("#ending-packing").removeAttr("disabled");
        } else {
            $("#ending-packing").attr("disabled", "disabled");
        }
    }

    $("#packageCheck").on("change", function () {
        // Cache DOM elements
        var $packageWeight = $("#packageWeight");
        var $packageCheck = $("#packageCheck");

        // Get current weight and package value
        var totalWeightStr = $packageWeight.val().replaceAll(",", ".");
        var str = $packageCheck.attr("data-value");
        var packageValueStr = str ? str.replaceAll(",", ".") : "";

        // Validate inputs
        if (!totalWeightStr || !packageValueStr || isNaN(totalWeightStr) || isNaN(packageValueStr)) {
            console.error("Invalid input for package weight or value.");
            return;
        }

        // Parse values safely
        var totalWeight = parseFloat(totalWeightStr);
        var packageValue = parseFloat(packageValueStr);

        // Determine if checkbox is checked
        var isChecked = $(this).is(":checked");

        // Update total weight based on checkbox state
        totalWeight += isChecked ? packageValue : -packageValue;

        // Update the input field and trigger keyup event
        try {
            var e = $.Event('keyup');
            e.keyCode = 13;
            $packageWeight.val(totalWeight).trigger(e); // Ensure two decimal places for consistency
        } catch (error) {
            console.error("Error updating package weight:", error);
        }
    });

    function loadCustomer() {
        return abp.services.app.customer.getFull().done(function (data) {
            var customerSelect = $("select[name='Dto.CustomerId']");
            var value = customerSelect.attr("data-value");

            customerSelect.empty();
            customerSelect.append('<option value="">' + l('SelectCustomer') + '</option>');
            for (var i = 0; i < data.length; i++) {
                const customer = data[i];
                var selected = customer.id == value ? "selected" : "";
                customerSelect.append('<option ' + selected + ' value="' + customer.id + '">' + customer.username + '</option>');
            }
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Failed to load customer: " + error.message);
        });
    }

    $("#packageCheck").on("change", function () {
        checkPackingClose();
    });

    $("#editBag").on('click', function () {
        $("#editBag").addClass("d-none");
        $("#CancelBag, #saveBag").removeClass("d-none");
        $("#customer").removeAttr("disabled")
        $("#fmEditBag").find("input[type='text'], input[type='number'], select").removeAttr("readonly");
        $("#fmEditBag").find("input[type='checkbox']").removeAttr("disabled");
        $("#fmEditBag").find("textarea").removeAttr("readonly");
    });
    $("#CancelBag").on('click', function () {
        displayForm();
    });

    function displayForm() {
        $("#editBag").removeClass("d-none");
        $("#CancelBag, #saveBag").addClass("d-none");
        $("#customer").attr("disabled", "disabled")
        $("#fmEditBag").find("input[type='text'], input[type='number'], select").attr("readonly", "readonly");
        $("#fmEditBag").find("input[type='checkbox']").attr("disabled", "disabled");
    }

    $(document).on("click", "#ending-packing", function () {
        var bagId = $("[name='bagId']").val();
        abp.ui.setBusy($("body"));
        _bagService.bagging({ bagId: bagId }).done(function (result) {

            abp.ui.clearBusy($("body"));
            window.location.reload();
        });
    });

    $(document).on('click', '.delete-package', function () {
        var packageId = $(this).attr("data-id");
        var bagId = $("[name='bagId']").val();
        var packageName = $(this).attr('data-name');
        deletePackageFromBag(packageId, bagId, packageName);
    });

    function updatePackageBagIdToNull(packageId, bagId) {
        // Kiểm tra xem packageId có hợp lệ không
        if (!packageId) {
            PlaySound('warning');
            abp.notify.error("Package ID không hợp lệ.");
            return;
        }
        // Gọi dịch vụ để cập nhật BagId thành null
        _bagService.removePackageFromBag({
            packageId: packageId,
            bagId: bagId
        }
        ).done(function (res) {
            if (res.status == 200) {
                $("#packageWeight").val(res.data.weight);
                abp.notify.info(l(res.message));
                _$packagePendingTable.ajax.reload(); // Tải lại bảng để cập nhật dữ liệuư
                PlaySound('success');
            }
            else {
                abp.notify.error(l(res.message));
                PlaySound('warning');
            }
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Cập nhật thất bại: " + error.message);
        });
    }

    function deletePackageFromBag(packageId, bagId, packageName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                packageName),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    updatePackageBagIdToNull(packageId, bagId); // Gọi hàm cập nhật
                }
            }
        );
    }
 
    function submitEditBagForm() {

        const form = document.getElementById('fmEditBag');
        form.querySelectorAll('input[type="hidden"]').forEach(hiddenInput => {
            const name = hiddenInput.name;
            const checkbox = form.querySelector(`input[type="checkbox"][name="${name}"]`);
            if (checkbox && checkbox.checked) {
                hiddenInput.remove();
            }
        });
        var formData = new FormData(form);
        var bagData = {};
        formData.forEach(function (value, key, ddd) {

            bagData[key.split(".")[1]] = value;
        });
        bagData["id"] = _bagId;
        abp.ui.setBusy();
        abp.services.app.bag.updateBag(bagData).done(function () {
            abp.notify.success("Cập nhật bao thành công!");
        }).fail(function (error) {
            abp.notify.error("Có lỗi khi cập nhật bao : " + error.message);
        }).always(function () {
            displayForm();
            abp.ui.clearBusy();
        });
    }

    function cloneBag(bagID) {
        abp.ui.setBusy();
        abp.services.app.bag.createSimilarBag(bagID).done(function (data) {
            window.location.href = '/bags/bagging/' + data.id;
        }).fail(function (error) {
            abp.notify.error("Có lỗi khi tạo bao tương tự : " + error.message);
        }).always(function () {
            abp.ui.clearBusy();
        });
    }

    $("#createSimilarBag").on("click", function () {
        var bagId = $("[name='bagId']").val();
        cloneBag(bagId);
    });

    $('#printStamp').click(function () {
        var bagId = $("[name='bagId']").val();
        _bagService.getForStamp(bagId, true).done(function (data) {

            $("#bagNo").text(data.shippingType == 2 ? data.warehouseDestination.code + " - " + data.bagCode : data.bagCode);
            /*  $("#bagNo").text(data.bagCode);*/
            $("#createdDate").text(data.creationTimeFormat);
            $("#exportDate").text(data.creationTimeFormat);
            $("#fromName").text(data.warehouseCreate.name);
            /*  $("#toName").text(data.warehouseDestination.name + '-' + data.warehouseDestination.code + '  ' + data.receiver);*/
            $("#toName").text(data.receiver.toUpperCase());
            $("#packages").text(data.totalPackages);
            $("#fromNameMobile").text(data.warehouseCreate.phone);
            $("#fromNameAdd").text(data.warehouseCreate.address);
            $("#weight").text((data.weight || 0) + " (kg)");
            $("#volume").text((data.volumeStr || 0) + " (m2)");
            debugger;
            var tempPackages = '<tr>' +
                '<td class="bold" rowspan="2"><span id="productName">Name: ' + data.packagesDtos[0].productNameVi + '</span></td>' +
                '<td class="bold"><span id="quantity">Quantity</span></td>' +
                '</tr>' +
                '<tr>' +
                '<td class="bold"><span id="quantity">' + data.packagesDtos[0].quantity + '</span></td>' +
                '</tr>';
            $("#packageTemplate").empty().append(tempPackages);

            var $barcodeSvg = $("#barcodeImage");
            $barcodeSvg.empty(); // nếu cần
            JsBarcode($barcodeSvg[0], data.bagCode, {
                format: "CODE128",
                width: 3,
                height: 80,
                displayValue: false
            });

            printLabel();
        });
    });


    function printLabel() {
        const content = document.getElementById('labelContent').innerHTML;
        const iframe = document.createElement('iframe');
        iframe.style.position = 'absolute';
        iframe.style.width = '0px';
        iframe.style.height = '0px';
        iframe.style.border = 'none';
        iframe.style.visibility = 'hidden'; // an toàn hơn display: none
        document.body.appendChild(iframe);

        const doc = iframe.contentWindow.document;
        doc.open();
        doc.write('<html><head><title>Print</title><style>');
        doc.write(`
        @media print {
            @page { size: auto; margin: 0 }
            body {
                font-family: Arial, sans-serif;
                font-size: 13px;
                margin: 0;
                padding: 0;
            }
            .label-container {
                width: 105mm;
                height: 148mm;
                border: 1px solid black;
                padding: 5mm;
                box-sizing: border-box;
            }
            .header {
                display: flex;
                justify-content: space-between;
                align-items: top;
                border-bottom: 1px solid black;
                padding-bottom: 4px;
                margin-bottom: 4px;
            }
            .barcode {
                width: 70mm;
                height: 20mm;
                background: black;
                margin: 1mm 0;
            }
            table {
                width: 100%;
                border-collapse: collapse;
                text-align: center;
                font-size: 13px;
            }
            th, td {
                border: 1px dashed black;
                padding: 4px 2px;
            }
            .footer {
                display: flex;
                justify-content: space-between;
                font-weight: bold;
                font-size: 13px;
                margin-top: 4px;
            }
            #toName {
                font-size: 20px;
            }
            .footer div {
                height: 14mm;
                margin-top: 4px;
            }
        }
    `);
        doc.write('</style></head><body>');
        doc.write(content);
        doc.write('</body></html>');
        doc.close();

        let printed = false;

        function doPrint() {
            if (printed) return;
            printed = true;
            try {
                iframe.contentWindow.focus();
                iframe.contentWindow.print();
            } catch (e) {
                console.error('Print failed:', e);
            }
            setTimeout(() => {
                if (iframe && iframe.parentNode) {
                    document.body.removeChild(iframe);
                }
            }, 200);
        }
        iframe.onload = doPrint;
        setTimeout(doPrint, 500);
    }

    $("#saveBag").on('click', function () {
        submitEditBagForm();
    });

    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    });

    $(document).on("click", "#nav-bar-status .nav-link", function () {
        $(this).closest("#nav-bar-status").find(".nav-link").removeClass("active");
        $(this).addClass("active");
        $(this).prev().prop("checked", true);
        _$packagePendingTable.ajax.reload();
    });

    $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', function (e) {
        $('.nav-tabs a[href="#department-details"]').tab('show');
        loadProvince();
    });

    abp.event.on('department.edited', function (data) {
        _$packagePendingTable.ajax.reload();
    });

    $('.btn-search').on('click', function (e) {
        _$packagePendingTable.ajax.reload();
    });

    $('.txt-search').on('keypress', function (e) {
        if (e.which == 13) {
            _$packagePendingTable.ajax.reload();
            return false;
        }
    });

    $("#BagType").on('change', function (e) {
        const value = e.target.value;
        if (value == 1) {
            $("#customer").attr("readonly", false);
        } else {
            $("#customer").val("").attr("readonly", true);
        }
    })

    $("#isOtherFeature").on("change", function () {
        const isChecked = $(this).is(":checked");
        if (isChecked) {
            $("[name='Dto.otherReason']").removeClass("d-none")
        } else {
            $("[name='Dto.otherReason']").addClass("d-none").val("")
        }
    }).trigger("change");

    checkPackingClose();
    getBagClosed();
    loadCustomer();
})(jQuery);