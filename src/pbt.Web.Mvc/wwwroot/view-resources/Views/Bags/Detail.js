(function ($) {
    var _bagService = abp.services.app.bag,
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#PackagesPendingTable');

    let warehouseStatusDescriptions = {
        "0": { text: l('MissingInformation'), color: 'red' },
        "1": { text: l('WarehouseStatus.InStock'), color: 'blue' },
        "2": { text: l('WarehouseStatus.OutOfStock'), color: 'green' }
    };

    let packageDeliveryStatusDescriptions = {
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
    var _$departmentsTable = _$table.DataTable({
        paging: false,
        serverSide: true,

        listAction: {
            ajaxFunction: function () {
                return _packageService.getAllPackagesByBagIdForBagDetail(arguments[0]).done(function (value) {
                    $("#totalBag").text(value.totalCount);
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
                action: function () { _$departmentsTable.draw(false); }
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
                sortable: false,
                render: function (data, type, row) {
                    return '<a  target="_blank" href="/Packages/Detail/' + row.id + '">' + data + '</a>';
                }
            },
            {
                targets: 2,
                data: 'trackingNumber',
                sortable: false
            },
            {
                targets: 3,
                data: 'customerName',
                sortable: false,
                className: 'text-right'
            },
            {
                targets: 4,
                data: 'weight',
                sortable: false,
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
                sortable: false,
                data: 'currentWarehouseName'
            },
            {
                targets: 9,
                data: 'exportDate',
                sortable: false,
                render: function (data, type, row, meta) {
                    return row.exportDate ? formatDateToDDMMYYYYHHmmss(row.exportDate) : '';
                }
           
            },
            {
                targets: 10,
                sortable: false,
                data:"warehouseStatus",
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
                width: 80,
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

    loadWarehouse();

    _$departmentsTable.on('draw', function () {
        $("#totalPackage").text(_$departmentsTable.page.info().recordsTotal);
    });

    _$form.find('.save-button').on('click', function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var department = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _warehouseService.create(department).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$departmentsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-package', function () {
        var departmentId = $(this).attr("data-id");
        var departmentName = $(this).attr('data-name');
        deleteDepartments(departmentId, departmentName);
    });

    function deleteDepartments(departmentId, departmentName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                departmentName),
            null,
            function (isConfirmed) {
                if (isConfirmed) {
                    const bagId = $("[name='bagId']").val();
                    updatePackageBagIdToNull(departmentId, bagId)
                }
            }
        );
    }
    function updatePackageBagIdToNull(packageId, bagId) {
        // Kiểm tra xem packageId có hợp lệ không
        if (!packageId) {
            PlaySound('warning');
            abp.notify.error("Package ID không hợp lệ.");
            return;
        }
        // Gọi dịch vụ để cập nhật BagId thành null
        _packageService.unBag(packageId).done(function (data) {
            window.location.reload();
            abp.notify.info("Cập nhật thành công!");
            _$departmentsTable.ajax.reload(); // Tải lại bảng để cập nhật dữ liệu
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Cập nhật thất bại: " + error.message);
        });
    }

    $("#ClearSearch").on("click", function () {
        $("#PackageSearchForm").find("input, select").val("");
    });

    function loadWarehouse() {
        return abp.services.app.warehouse.getFull().done(function (data) {
            var dropdown = $("#WarehouseCode");
            dropdown.empty();
            dropdown.append('<option value="">' + l('SelectWarehouse') + '</option>');
            for (var i = 0; i < data.length; i++) {
                var warehouse = data[i];
                dropdown.append('<option value="' + warehouse.id + '">' + warehouse.name + '</option>');
            }
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }

    function getBagToday() {
        return abp.services.app.bag.getBagsToday().done(function (data) {
            var list = $("#list-bag-today");
            var currentBagId = $("[name='bagId']").val();
            list.empty();
            for (var i = 0; i < data.length; i++) {
                var bag = data[i];
                list.append('<a style="color: black; cursor: pointer" href="/bags/detail/' + bag.id + '"><li class="list-group-item ' + (currentBagId == bag.id ? 'active' : '') + '">' + bag.bagCode + '</li ></a >');
            }
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }

    $('#printStamp').click(function () {
        var bagId = $("[name='bagId']").val();
        _bagService.getForStamp(bagId, true).done(function (data) {
            
            $("#bagNo").text(data.shippingType == 2 ? data.warehouseDestination.code + " - " + data.bagCode : data.bagCode);
            $("#createdDate").text(data.creationTimeFormat);
            $("#exportDate").text(data.creationTimeFormat);
            $("#fromName").text(data.warehouseCreate.name);
            $("#toLocation").text(data.warehouseDestination.name);
            //$("#toName").text(data.warehouseDestination.name + '-'+ data.warehouseDestination.code + '  ' + data.receiver);
            $("#toName").text(data.receiver.toUpperCase());
            $("#packages").text(data.totalPackages);
            $("#fromNameMobile").text(data.warehouseCreate.phone);
            $("#fromNameAdd").text(data.warehouseCreate.address);
            $("#weight").text((data.weight || 0) + " (kg)");
            $("#volume").text((data.volumeStr || 0) + " (m2)");
            var tempPackages = "";
            tempPackages = '<tr>' +
                '<td class="bold"><span id="productName">' + data.packagesDtos[0].productNameVi + '</span></td>' +
                '<td class="bold"><span id="quantity">' + data.packagesDtos[0].quantity + '</span></td>' +
                '</tr>';
            $("#packageTemplate").empty().append(tempPackages);
            
            var $barcodeSvg = $("#barcodeImage");
            $barcodeSvg.empty();
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
        doc.write('@media print { ' +
            '@page { ' +
            '    size: auto;' +
            '    margin: 0' +
            '}' +
            'body { ' +
            '    font-family: Arial, sans-serif; ' +
            '    font-size: 13px; /* 调整字体大小以适应 A8 尺寸 */' +
            '    margin: 0;' +
            '    padding: 0;' +
            '}' +
            '.label-container { ' +
            '    width: 105mm; /* A8 宽度 */' +
            '    height: 148mm; /* A8 高度 */' +
            '    border: 1px solid black; ' +
            '    padding: 5mm;' +
            '    box-sizing: border-box;' +
            '}' +
            '.header { ' +
            '    display: flex; ' +
            '    justify-content: space-between; ' +
            '    align-items: top; ' +
            '    border-bottom: 1px solid black; ' +
            '    padding-bottom: 4px; ' +
            '    margin-bottom: 4px;' +
            '}' +
            '.barcode { ' +
            '    width: 70mm; /* 调整条形码宽度 */' +
            '    height: 20mm; /* 调整条形码高度 */' +
            '    background: black; ' +
            '    margin: 1mm 0;' +
            '}' +
            'table { ' +
            '    width: 100%; ' +
            '    border-collapse: collapse; ' +
            '    text-align: center; ' +
            '    font-size: 13px; /* 调整表格字体大小 */' +
            '}' +
            'th, td { ' +
            '    border: 1px dashed black; ' +
            '    padding: 4px 2px; ' +
            '}' +
            '.footer { ' +
            '    display: flex; ' +
            '    justify-content: space-between; ' +
            '    font-weight: bold;' +
            '    font-size: 13px;' +
            '    margin-top: 4px;' +
            '}' +
            '#toName{' +
            '    font-size: 20px;' +
            '}' +
            '.footer div { ' +
            '    height: 14mm;  ' +
            '    margin-top: 4px;' +
            '}' +
            '}');
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

    function loadBagLogs() {
        var bagId = $("[name='bagId']").val();
        return abp.services.app.entityChangeLogger.getLogsByMultiEntityTypeName(['BAG', 'BAGDTO'], bagId).done(function (data) {
            var logsContainer = $("#bag-logs");
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

    $(document).on('click', '.edit-package', function (e) {
        const packageId = $(this).attr("data-id");
        e.preventDefault();
        window.location.href = "/Packages/Edit/" + packageId;
       
    });

    $('#ProvinceId').change(function () {
        var provinceId = $(this).val();
        if (provinceId) {
            abp.services.app.district.getFullByProvince(provinceId).done(function (data) {
                var districtDropdown = $('#DistrictId');
                districtDropdown.empty();
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    districtDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                }
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
                var districtDropdown = $('#EditDistrictId');
                var selectedId = districtDropdown.attr("value");
                districtDropdown.empty();
                districtDropdown.append('<option value="">' + l('SelectDistrict') + '</option>');
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    var selected = selectedId == item.id ? "selected" : "";
                    districtDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                }

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
                var wardDropdown = $('#WardId');
                wardDropdown.empty();
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    wardDropdown.append('<option value="' + item.id + '">' + item.name + '</option>');
                }
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
                var wardDropdown = $('#EditWardId');
                var selectedId = wardDropdown.attr("value");
                wardDropdown.empty();
                wardDropdown.append('<option value="">' + l('SelectWard') + '</option>');
                for (var i = 0; i < data.length; i++) {
                    var item = data[i];
                    var selected = selectedId == item.id ? "selected" : "";
                    wardDropdown.append('<option ' + selected + ' value="' + item.id + '">' + item.name + '</option>');
                }
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

    abp.event.on('department.edited', function (data) {
        _$departmentsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', function () {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', function () {
        _$form.clearForm();
    });

    $('.btn-search').on('click', function (e) {
        _$departmentsTable.ajax.reload();
    });
 

    $(document).on('click', '.btn-delete', function () {
        var bagId = $(this).attr("data-id");
        var bagCode = $(this).attr('data-name');
        deleteBag(bagId, bagCode);
    });

    function deleteBag(bagId, bagCode) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                bagCode),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _bagService.deleteBag(bagId).done((result) => {
                        if (result.success) {
                            abp.notify.info(l('SuccessfullyDeleted'));
                            window.location.href = '/Bags';
                        }
                        else {
                            abp.notify.error(result.message);
                        }
                    });
                }
            }
        );
    }

    $('.txt-search').on('keypress', function (e) {
        if (e.which == 13) {
            _$departmentsTable.ajax.reload();
            return false;
        }
    });
    loadBagLogs();

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
        cloneBag(bagId);
    });
})(jQuery);