(function ($) {
    var _deliveryNoteService = abp.services.app.deliveryNote;
    var _bagService = abp.services.app.bag;
    var _packageService = abp.services.app.package;
    var _shippingPartnerServices = abp.services.app.shippingPartner;
    var _deliveryRequest = abp.services.app.deliveryRequest;
    l = abp.localization.getSource('pbt');
    var _$form = $("#form-delivery-note");
    var _$formScanCode = $("#form-scan-code");

    let currentAmountNumberic = new AutoNumeric('#currentAmount', {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0'
    });
    let financialAmountNumberic = new AutoNumeric('#financialAmount', {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0'
    });
    let maxDebtNumberic = new AutoNumeric('#maxDebt', {
        currencySymbol: '',
        decimalCharacter: '.',
        digitGroupSeparator: ',',
        decimalPlaces: 0,
        minimumValue: '0'
    });
    //new AutoNumeric('#financialAmount', {
    //    currencySymbol: '',
    //    decimalCharacter: '.',
    //    digitGroupSeparator: ',',   
    //    decimalPlaces: 2,
    //    minimumValue: '0'
    //});


    // lấy data từ query param deliveryRequestId, customerId
    var deliveryRequestIdParam = new URLSearchParams(window.location.search).get("deliveryRequestId");
    var customerIdParam = new URLSearchParams(window.location.search).get("customerId");

    $('#customer-select').on('change', function () { $('#customer-select').trigger({ type: 'select2:select' }); });

    $('#customer-select').select2()
        .addClass('form-control')
        .on('select2:select', function (e) {
            var selectedOption = $(this).find('option:selected');
            var customerId = selectedOption.val();
            var customerName = selectedOption.text();

            if (customerId && customerId > 0) {
               
                var currentdebt = selectedOption.data('currentdebt');
                var currentamount = selectedOption.data('currentamount');
                var maxDebt = selectedOption.data('maxdebt');
                $("#ReceiptName").val(selectedOption.text());

                currentAmountNumberic.set(FormatNumberToUpload(currentamount));//.val(FormatNumberToDisplay(currentamount));
                financialAmountNumberic.set(FormatNumberToUpload(currentdebt));
                maxDebtNumberic.set(FormatNumberToUpload(maxDebt));
                abp.ui.setBusy($("body"));

                var tabId = `customer-tab-${customerId}`;
                var contentId = `customer-content-${customerId}`;

                // Kiểm tra nếu tab đã tồn tại
                if ($(`#${tabId}`).length === 0) {
                    // Thêm tab mới
                    $('#delivery-note-tabs').append(`
                    <li class="nav-item">
                        <a class="nav-link" id="${tabId}" data-toggle="pill" href="#${contentId}" role="tab" aria-controls="${contentId}" aria-selected="false">
                            ${customerName}
                             <button type="button" class="close-tab" data-tab-id="customer-tab-${customerId}" data-content-id="customer-content-${customerId}" aria-label="Close">
                             <i class="fas fa-times"></i>
                            </button>
                        </a>
                    </li>
                `);

                    // Thêm content mới
                    $('#delivery-note-tabs-content').append(`
                    <div class="tab-pane fade customer-content" id="${contentId}" role="tabpanel" aria-labelledby="${tabId}">
                        <div id="partial-${customerId}"></div>
                    </div>
                `);

                    $.ajax({
                        url: '/DeliveryNote/LoadDeliveryNoteItem?customerId=' + customerId,
                        type: "GET",
                        dataType: "html",
                        success: function (html) {
                            $(`#partial-${customerId}`).html(html);
                            LoadDeliveryNoteByCustomerId(customerId);

                        },
                        error: function () {
                            abp.notify.error("Failed to load DeliveryNoteItem: " + error.message);
                        }
                    });
                }

                // Active tab và content của khách hàng
                $(`#${tabId}`).tab('show');


                abp.ui.clearBusy($("body"));
            }
        });

    $(document).on('click', '.close-tab', function (e) {
        e.preventDefault();

        // Lấy ID của tab và content liên quan
        var tabId = $(this).data('tab-id');
        var contentId = $(this).data('content-id');

        // Xóa tab và content
        $(`#${tabId}`).closest('li').remove(); // Xóa tab
        $(`#${contentId}`).remove(); // Xóa content

        // Kích hoạt tab đầu tiên nếu có
        var firstTab = $('#delivery-note-tabs .nav-link').first();
        if (firstTab.length > 0) {
            firstTab.tab('show');
        }
    });

    function deleteDeliveryNote(deliveryNoteId, deliveryRequestIdParam) {
        abp.message.confirm(
            abp.utils.formatString(
                "Tạo phiếu xuất theo yêu cầu giao đã chọn. phiếu xuất tạm sẽ bị xóa ?",
                ""),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _deliveryNoteService.deleteDeliveryNote(
                        deliveryNoteId, deliveryRequestIdParam
                    ).done(() => {
                        abp.notify.info("Tạo phiếu xuất từ yêu cầu giao thành công");
                        window.history.pushState({}, '', '/DeliveryNote/Create');
                        // trigger select customer
                        $("#delivery-note-tabs").empty();
                        $('#customer-select').val(customerIdParam).trigger('change');
                        $('#customer-select').trigger({ type: 'select2:select' });
                        deliveryRequestIdParam = null;
                        customerIdParam = null;
                    });
                }
            }
        );
    }

    function LoadDeliveryNoteByCustomerId(customerId) {
        var contentId = `#customer-content-${customerId}`;
        if ($(contentId).length > 0) {
            $(contentId).find('.mask-number').each(function () {
                const val = $(this).val().replaceAll(',', '').replaceAll('.', ',');
                $(this).val(val);
                new AutoNumeric(this, {
                    currencySymbol: '',
                    decimalCharacter: ',',
                    digitGroupSeparator: '.',
                    decimalPlaces: 0,
                    minimumValue: '0'
                });
            });
            $(contentId).find('.mask-number-2').each(function () {
                const val = $(this).val().replaceAll(',', '').replaceAll('.', ',');
                $(this).val(val);
                new AutoNumeric(this, {
                    currencySymbol: '',
                    decimalCharacter: ',',
                    digitGroupSeparator: '.',
                    decimalPlaces: 2,
                    minimumValue: '0'
                });
            });
            // $(contentId).find('.mask-number-2').each(function () {
            //     new AutoNumeric(this, {
            //         currencySymbol: '',
            //         decimalCharacter: '.',
            //         digitGroupSeparator: ',',
            //         decimalPlaces: 2,
            //         minimumValue: '0'           // Giá trị tối thiểu có thể nhập
            //     });
            // });
            getBagByCustomer(contentId, customerId);
            var deliveryNoteId = $(contentId).find('[name="Id"]').val();
            if (deliveryNoteId && deliveryNoteId > 0) {
                loadDataReadyForDelivery(contentId, deliveryNoteId);

                //if (customerIdParam && deliveryRequestIdParam){
                //    deleteDeliveryNote(deliveryNoteId, deliveryRequestIdParam);
                //}
            }
            updateDeliveryFeeControls(contentId);
        }
    }

    function getDeliveryRequest() {
        return false;

        _deliveryRequest.getNewDeliveryRequest().done(function (response) {
            if (response && response.items) {
                var selectShippingPartner = $("#customer-list");
                response.items.forEach(function (deliveryRequest) {
                    selectShippingPartner.append(
                        `<div class="customer-item" data-id="${deliveryRequest.customer.id}">
                                <p class="customer-detail">Tên KH: <strong>${deliveryRequest.customer.fullName}</strong></p>
                                <p class="customer-detail">Mã YCG: <strong>${deliveryRequest.requestCode || "_"}</strong></p>
                                <p class="customer-detail">Số kiện/KG: <strong>${deliveryRequest.totalPackage}/${deliveryRequest.weight}</strong></p>
                        </div>`
                    );
                });
                selectShippingPartner.find('.mask-number').each(function () {
                    new AutoNumeric(this, {
                        currencySymbol: '',
                        decimalCharacter: ',',
                        digitGroupSeparator: '.',
                        decimalPlaces: 0,
                        minimumValue: '0'
                    });
                });

            }
        })
    }


    //new AutoNumeric('.mask-number', {
    //    currencySymbol: '',     // Ký hiệu tiền tệ
    //    decimalCharacter: ',',      // Dấu phân cách thập phân
    //    digitGroupSeparator: '.',   // Dấu phân cách hàng nghìn
    //    decimalPlaces: 2,           // Số chữ số sau dấu thập phân
    //    minimumValue: '0'           // Giá trị tối thiểu có thể nhập
    //});
    async function getBagByCustomer(contentId, customerId) {
        const tableItemRequest = $(contentId).find(".bagRequest tbody");
        tableItemRequest.empty();
        var totalWeight = 0;
        await abp.services.app.bag.getBagForCreateDeliveryNoteByCustomerId(customerId).done(function (data) {
            data.forEach(function (bag) {
                tableItemRequest.append(
                    `<tr id="row-${bag.id}" data-type="bag">
                        <td><input type="checkbox" item-id="${bag.id}" item-type="bag"/></td>
                        <td class="itemCode">${bag.bagCode}</td>
                        <td>${bag.deliveryRequestOrderCode || '-'}</td>
                        <td>${formatNumberThousand(bag.weight) || 0} kg</td >
                        <td>${bag.note}</td>
                        <td>
                            <button class="btn btn-outline-secondary btn-sm btn-bag-detail" data-id="${bag.id}" data-toggle="modal" onclick="return false;" data-target="#detailModal">📋</button>
                        </td>
                    </tr>`
                );
                totalWeight += bag.weight;
            });

            $(contentId).find(".numberBag").text(data.length);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });

        await abp.services.app.package.getPackageByCustomer(customerId).done(function (data) {
            data.forEach(function (package) {
                tableItemRequest.append(
                    `<tr id="row-${package.id}" data-type="package">
                         <td><input type="checkbox" item-id="${package.id}" item-type="package"/></td>
                        <td class="itemCode">${package.packageNumber}</td>
                        <td>${package.deliveryRequestOrderCode || '-'}</td>
                        <td>${formatNumberThousand(package.weight) || 0} kg</td>
                        <td>${package.dimention || '0'} (cm3)</td>
                        <td></td>
                    </tr>`
                );
                totalWeight += package.weight;
            });
            $(contentId).find(".numberPackage").text(data.length);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });

        $(contentId).find('.totalWeight').text(formatThousand(totalWeight));
    }

    async function loadDataReadyForDelivery(contentId, deliveryNoteId) {
        const tableItemRequest = $(contentId).find(".tableReadyDelivery tbody");
        tableItemRequest.empty();
        var totalWeight = 0;
        var totalBags = 0;
        var totalPackages = 0;

        await abp.services.app.bag.getByDeliveryNoteId(deliveryNoteId).done(function (data) {

            data.forEach(function (bag) {
                tableItemRequest.append(
                    `<tr id="row-${bag.id}" data-type="bag">
                        <td class="itemCode" data-id="${bag.id}" btn-bag-detail data-toggle="modal" data-target="#detailModal" >${bag.bagCode}</td>
                        <td>${bag.deliveryNoteCode || '-'}</td>
                        <td>${formatNumberThousand(bag.weight) || 0} kg</td>
                        <td>${bag.note}</td>
                        <td>
                            <button class="btn btn-sm btn-danger btn-remove" data-type="bag" delivery-note-id="${deliveryNoteId}" data-customer-id="${bag.customerId}" data-id="${bag.id}" data-name="${bag.bagCode}"   ><i class="fas fa-times"></i></button>
                        </td>
                    </tr>`
                );
                totalWeight += bag.weight;
            });
            totalBags = data.length;
            //   $(contentId).find(".numberBag").text(data.length);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });

        await abp.services.app.package.getByDeliveryNoteId(deliveryNoteId).done(function (data) {
            data.forEach(function (package) {
                tableItemRequest.append(
                    `<tr id="row-${package.id}" data-type="package">
                        
                        <td class="itemCode">${package.packageNumber}</td>
                        <td>${package.deliveryNoteCode || '-'}</td>
                        <td>${formatNumberThousand(package.weight) || 0} kg</td>
                        <td>${package.volume || '0'} (cm3)</td>
                         <td>
                            <button class="btn btn-sm btn-danger btn-remove" data-type="package" delivery-note-id="${deliveryNoteId}"  data-customer-id="${package.customerId}" data-id="${package.id}" data-name="${package.packageNumber}"    ><i class="fas fa-times"></i></button>
                        </td>
                    </tr>`
                );
                totalWeight += package.weight;
            });
            //$(contentId).find(".numberPackage").text(data.length);
            totalPackages = data.length;
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });
        var totalHtml = `<strong>${(totalBags + totalPackages)}</strong> bản ghi - <strong>${totalBags}</strong> bao - <strong>${totalPackages}</strong> kiện - <strong>${formatThousand(totalWeight)}</strong> KG`;
        $(contentId).find(".label-total").html(totalHtml);

    }


    function ScanCode(code) {
        if (code) {

            var customerId = _$formScanCode.find('[name="CustomerId"]').val();
            var lockCustomer = _$formScanCode.find('[name="LockCustomer"]').prop('checked');
            //var customerId = _$formScanCode.find('[name="CustomerId"]').val();
            _deliveryNoteService.scanCode({ code: code, lockCustomer: lockCustomer, customerId: customerId }).done(function (response) {
                if (!response || response.status == 400) {
                    PlaySound('warning');
                    abp.notify.error(response.message);
                    return;
                }

                else if (response && response.status == 401) {
                    // Thông báo lỗi kiện này không phải của Customer đang chọn 
                    PlaySound('007');
                    abp.notify.error("Mã kiện này không phải của khách hàng đang chọn. ");
                    return;
                }

                // Hiển thị panel thông tin của phiếu xuất
                if (response && response.status == 200 && response.data) {

                    var data = response.data;
                    PlaySound('export');
                    abp.notify.success(l('SavedSuccessfully'));
                    if (data.customerId) {
                        _$formScanCode.find('[name="CustomerId"]').val(data.customerId);
                        var contentId = `#customer-content-${data.customerId}`;
                        if (data.customerId != customerId) {
                            // Gán lại giá trị của select sau đó load lại các thông tin của khách hàng
                            $('#customer-select').val(data.customerId).trigger('change');
                            $('#customer-select').trigger({ type: 'select2:select' });
                        }

                        getBagByCustomer(contentId, data.customerId);
                        if (data.deliveryNoteId) {
                            loadDataReadyForDelivery(contentId, data.deliveryNoteId);
                            LoadDeliveryNoteSummary(contentId, data.deliveryNoteId);
                        }

                        $(contentId).find('.mask-number').maskNumber({ integer: false, thousands: '.', decimal: "," });
                    }

                } else {
                    PlaySound('warning');
                    abp.notify.error("Không tìm thấy kiện/bao hàng với mã: " + code);
                }
            });
        }
    }

    function LoadDeliveryNoteSummary(contentId, deliveryNoteId) {
        _deliveryNoteService.get({ id: deliveryNoteId }).done(function (data) {
            if (data) {
                //$(contentId).find('[name="ShippingFee"]').val(formatThousand(data.shippingFee || 0)).attr("value", data.shippingFee);
                //$(contentId).find('[name="TotalWeight"]').val(formatThousand(data.totalWeight || 0)).attr("value", data.totalWeight);
                //$(contentId).find('[name="ShippingFee"]').attr("value", data.shippingFee);
                //$(contentId).find('[name="TotalWeight"]').attr("value", data.totalWeight);
                let $content = $(contentId);
                var $shippingFeeInput = $content.find('[name="ShippingFee"]');
                var $totalWeightInput = $content.find('[name="TotalWeight"]');

                AutoNumeric.set($shippingFeeInput[0], data.shippingFee || 0);
                AutoNumeric.set($totalWeightInput[0], data.totalWeight || 0);
                //AutoNumeric.set('[name="ShippingFee"]', data.shippingFee || 0);
                //AutoNumeric.set('[name="TotalWeight"]', data.totalWeight || 0);
            }
        }).fail(function (error) {
        });
    }

    function updateDeliveryFeeControls(contentId) {
        let $content = $(contentId);
        $content.on('change', 'select[name="DeliveryFeeReason"]', function () {
             
            var $feeInput = $content.find('[name="DeliveryFee"]');
            var $shippingPartner = $content.find('[name="ShippingPartnerId"]');

            // find selected reason (works for radio group or select inside the content)
            var $reasonChecked = $content.find('input[name="DeliveryFeeReason"]:checked');
            var reasonVal = $reasonChecked.length ? $reasonChecked.val() : $content.find('select[name="DeliveryFeeReason"]').val();

            // default enable
            if ($feeInput.length) $feeInput.prop('disabled', false);
            if ($shippingPartner.length) $shippingPartner.prop('disabled', false);

            if (!reasonVal) return;

            var r = parseInt(reasonVal, 10);

            if (r === 3) { // khách tự đến lấy => fee = 0, fee disabled, partner disabled
                if ($feeInput.length) {
                    // set numeric if AutoNumeric instance exists, else set value
                    try {
                        AutoNumeric.set($feeInput[0], 0);
                    } catch (e) {
                        $feeInput.val(0);
                    }
                    $feeInput.prop('disabled', true);
                }
                if ($shippingPartner.length) {
                    $shippingPartner.val('-1');
                    $shippingPartner.prop('disabled', true);
                }
            } else if (r === 4) { // giao cho đối tác => partner selectable, fee = 0 and disabled
                if ($feeInput.length) {
                    try {
                        AutoNumeric.set($feeInput[0], 0);
                    } catch (e) {
                        $feeInput.val(0);
                    }
                   
                    $feeInput.prop('disabled', true);
                }
                if ($shippingPartner.length) {
                    $shippingPartner.prop('disabled', false);
                }
            } else {
                // other reasons: enable fee input; keep shipping partner enabled (or adjust if you want partner only for r===4)
                if ($feeInput.length) $feeInput.prop('disabled', false);
                if ($shippingPartner.length) $shippingPartner.prop('disabled', false);
            }
        });
         
    }


    $('.btn-save-and-export').on('click', function () {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouChecked'),
                ''),
            l('PleaseConfirm'),
            (isConfirmed) => {
                if (isConfirmed) {
                    saveSingleDeliveryNote();
                }
            }
        );
    });

    $(document).on('click', '.btn-create-quick-delivery-note', function () {
        var customerId = $(this).attr('data-customer-id');
        if (!customerId) {
            abp.notify.error("Không tìm thấy khách hàng.");
            return;
        }

        // Tìm bảng chứa các checkbox của khách hàng
        var className = `.table-customer-${customerId}`;
        var selectedItems = $(className).find('input[type="checkbox"]:checked'); // Lấy danh sách checkbox được chọn

        if (selectedItems.length === 0) {
            abp.notify.error("Vui lòng chọn ít nhất một bao hoặc kiện.");
            return;
        }

        // Tạo danh sách chứa Id và type
        var bagIds = [];
        var packageIds = [];
        selectedItems.each(function () {
            var itemId = $(this).attr('item-id'); // Lấy Id của item
            var itemType = $(this).attr('item-type'); // Lấy type của item (bag/package)
            if (itemType === 'package') {
                packageIds.push(itemId);
            }
            if (itemType === 'bag') {
                bagIds.push(itemId);
            }
        });
        const url = `/DeliveryNote/CreateQuickDeliveryNote?customerId=${customerId}&bagIds=${bagIds.join(',')}&packageIds=${packageIds.join(',')}`;
        // Mở tab mới với URL đã tạo
        window.open(url);
        console.log("Danh sách được chọn:", selectedData);

    });

    $("#save-delivery-note").on("click", function (e) {
        saveSingleDeliveryNote();
    });

    function saveSingleDeliveryNote() {
        // Tìm tab đang active
        var activeTab = $('#delivery-note-tabs .nav-link.active').attr('href'); // Lấy ID của tab đang active
        var activeContent = $(activeTab); // Lấy nội dung của tab đang active

        if (!activeContent || activeContent.length === 0) {
            abp.notify.error("Không tìm thấy tab đang active.");
            return;
        }
        // Tạo object để lưu dữ liệu
        var data = {};
        // Lấy tất cả các thẻ có thuộc tính name trong tab đang active
        activeContent.find('[name]').each(function () {
            var name = $(this).attr('name');
            var value = $(this).val();

            // Nếu là checkbox, lấy trạng thái checked
            if ($(this).is(':checkbox')) {
                value = $(this).is(':checked');
            }

            // Nếu là radio, chỉ lấy radio được chọn
            if ($(this).is(':radio') && !$(this).is(':checked')) {
                return; // Bỏ qua radio không được chọn
            }

            // Gán giá trị vào object
            data[name] = value;
        });

        if (data.Id == 0) {
            data.Id = $('#' + data.CustomerId + '-Id').val();
        }
        data['ShippingFee'] = FormatNumberToUpload(data['ShippingFee']);
        data['DeliveryFee'] = FormatNumberToUpload(data['DeliveryFee']);
        data['TotalWeight'] = FormatNumberToUpload(data['TotalWeight']);

        // Gửi dữ liệu qua API
        abp.services.app.deliveryNote.saveWithTransactions(data).done(function (response) {
            console.log(response);
            abp.notify.success("Tạo phiếu xuất kho thành công.");
            printStamp(response.id);
        }).fail(function (error) {
            if (error) {
                const validationErrors = error.validationErrors;
                if (validationErrors) {
                    validationErrors.forEach(function (validationError) {
                        const field = validationError.members[0];
                        const message = validationError.message;
                        const inputField = activeContent.find(`[name="${field}"]`);
                        inputField.addClass("is-invalid");
                        inputField.siblings(".invalid-feedback").text(message);
                    });
                } else {
                    abp.message.error(error.message);
                }
            } else {
                abp.message.error("Có lỗi xảy ra!");
            }
        });
    }

    $(document).on('click', '.btn-remove', function () {
        var dataId = $(this).attr("data-id");
        var dataCode = $(this).attr('data-name');
        var dataType = $(this).attr('data-type');
        var customerId = $(this).attr('data-customer-id');
        var deliveryNoteId = $(this).attr('delivery-note-id');
        var contentDiv = $(this).closest('.customer-content');
        var contentId = "#" + contentDiv.attr('id');
        deleteBag(dataId, dataCode, dataType, contentId, customerId, deliveryNoteId);

        return false;
    });

    function deleteBag(dataId, dataCode, dataType, contentId, customerId, deliveryNoteId) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                dataCode),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _deliveryNoteService.removeItem({ itemId: dataId, itemType: dataType }).done((result) => {

                        if (result.status == 200) {
                            PlaySound('success');
                            abp.notify.info(l('SuccessfullyDeleted'));
                            getBagByCustomer(contentId, customerId);
                            loadDataReadyForDelivery(contentId, deliveryNoteId);

                            var data = result.deliveryNote;

                            if (data) {
                                // $(contentId).find('[name="TotalWeight"]').val(formatThousand(data.totalWeight) || 0);
                                // $(contentId).find('[name="ShippingFee"]').val(formatThousand(data.shippingFee) || 0);
                                AutoNumeric.set('[name="ShippingFee"]', data.shippingFee || 0);
                                AutoNumeric.set('[name="TotalWeight"]', data.totalWeight || 0);
                                $(contentId).find('[name="ShippingFee"]').attr("value", data.shippingFee);
                                $(contentId).find('[name="TotalWeight"]').attr("value", data.totalWeight);
                            }
                            $(contentId).find('.mask-number').maskNumber({
                                integer: false,
                                thousands: '.',
                                decimal: ",",
                            });
                        }
                        else {
                            abp.notify.error(result.message);
                        }
                    });
                }
            }
        );
    }

    $(document).on('click', '.btn-bag-detail', function () {
        const bagId = $(this).data('id');
        getPackageByBag(bagId);
    });

    function getPackageByBag(bagId) {
        return abp.services.app.package.getPackageByBag(bagId).done(function (data) {
            var tableBagRequest = $("#package-list tbody");
            tableBagRequest.empty();
            var stt = 1;
            data.forEach(function (package) {
                tableBagRequest.append(
                    `<tr>
                        <td>${stt}</td>
                        <td>${package.packageNumber}</td>
                        <td>${package.deliveryRequestOrderId}</td>
                        <td>${package.weight || 0}kg</td>
                        <td>-</td>
                    </tr>`
                );
                stt++;
            });

        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load packages: " + error.message);
        });
    }


    $("#scanCode").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {

            abp.ui.setBusy($("body"));
            ScanCode(e.target.value);
            abp.ui.clearBusy($("body"));

            $("#scanCode").val('');
        }
    });


    function printStamp(deliveryNoteId) {

        if (deliveryNoteId == 0) {
        }
        var id = deliveryNoteId;
        var temp = $(
            `
            <div class="container">
            <div class="header">
              <div class="code" >
              <span style="font-size: 12px; display: block" id="deliveryNoteCode"></span>
                <span  style="font-size: 10px; display: block" id="dateNow"></span>
              </div>
            </div>
        
            <div class="title">
              <h5 style="margin-bottom: 0; line-height: 0">PHIẾU XUẤT KHO</h5>
              <p style="font-size: 13px; ">(Xuất cho khách hàng)</p>
            </div>
        
            <div class="info-section">
              <div class="left">
                <div class="list bold" style="font-size: 12px">DANH SÁCH XUẤT</div>
                <div class="product product-package">
                    
                </div>
                <div class="product product-bag">
                    
                </div>
                <hr>
              </div>
              <div class="right">
                <table class="info">
                  <tr>
                      <td>Họ tên khách hàng:</td>
                      <td><div class="font-bold" id="customerName"></div></td>
                  </tr>
                  <tr>
                      <td>Người nhận:</td>
                      <td><div class="font-bold" id="customerRecevie"></div></td>
                      <td>SDT: <div class="font-bold" id="notephoneNumber"></div> </td>
                  </tr>
                  <tr>
                      <td>Địa chỉ người nhận:</td>
                      <td colspan="2"><div class="font-bold" id="noteCustomerAddress"></div></td>
                  </tr>
                  <tr><td>Số lượng xuất:</td>
                  <td>
                    <div class="font-bold" id="numberNoteExport"></div>
                  </td>
                  <td>Tổng cân nặng:<span class="font-bold" id="totalNoteWeight"></span></td></tr>
                  <tr><td>Số dư tài chính (VND):</td>
                  <td colspan="2"><span class="font-bold" id="amountCustomerCurrent"></span></td></tr>
                  <tr><td>Đại lý:</td>
                  <td colspan="2"><span class="font-bold">pttt</span></td></tr>
                  <tr><td>Ghi chú:</td>
                  <td colspan="2"><span class="font-bold" id="deliverNote"></span></td></tr>
              
                </table>
                    <hr>
                    <div class="note">
                      <p>- Một số mã sản phẩm có thể bị ẩn do danh sách quá dài.</p>
                      <p>- Quý khách vui lòng kiểm tra số lượng và tình trạng kiện hàng trước khi ký nhận. Chúng tôi không tiếp nhận các khiếu nại không nhận được mã kiện có trong phiếu xuất kho này sau khi Quý khách đã ký nhận.</p>
                      <p>- Tiền thu COD được cập nhật vào tài khoản của Quý khách trong vòng 24 giờ.</p>
                      <p>- Chúng tôi không giải quyết khiếu nại được tạo sau 24h kể từ khi Khách hàng nhận hàng thành công.</p>
                      <p>- Chúng tôi không chịu bất kỳ trách nhiệm nào về hư hỏng hàng hóa (bao gồm việc mất hàng, móp méo, vỡ hóng...) sau khi giao hàng cho Nhà xe/Chành xe, Người nhận hộ do Khách hàng yêu cầu.</p>
                    </div>
              </div>
            </div>
            <div class="footer">
              <div>
                <div>Người lập phiếu</div>
                <br>
                <div class="name" id="nameCreate"></div>
              </div>
              <div>
                <div>Người nhận hàng</div>
                <br>
                <div class="name" id="nameRecevie"></div>
              </div>
            </div>
          </div>    
        `);
        $("#labelContent").empty();
        abp.ui.setBusy("body");

        Promise.all([
            _deliveryNoteService.getDeliveryNoteById(id)
            , _deliveryNoteService.getItemByDeliveryNote(id)
        ]).then((data) => {
            const deliveryNote = data[0].dto;
            const items = data[1];
            temp.find("#deliveryNoteCode").text(deliveryNote ? deliveryNote.deliveryNoteCode : "_");
            const date = new Date();
            const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
            const formattedDate = date.toLocaleDateString('vi-VN', options);
            const day = formattedDate.split('/')[0];
            const month = formattedDate.split('/')[1];
            const year = formattedDate.split('/')[2];
            temp.find("#dateNow").text(`Ngày ${day} tháng ${month} năm ${year}`);

            var itemsPackagesHtml = ``;
            var itemsBagsHtml = ``;
            if (items.bags.length > 0) {
                itemsBagsHtml += `<div style="font-weight: bold; margin: 2px">Bao:</div>`;
                items.bags.forEach((item, index) => {

                    itemsBagsHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.bagCode} - ${item.weight} (kg) - ${item.totalPackages} (kiện)</span><br />`;
                })
                temp.find(".product-bag").html(itemsBagsHtml);
            }
            if (items.packages.length > 0) {
                itemsPackagesHtml += `<div style="font-weight: bold; margin: 2px">Kiện:</div>`;
                items.packages.forEach((item, index) => {
                    itemsPackagesHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.packageNumber} - <i>${item.trackingNumber}</i> - ${item.weight} (kg)</span><br />`;
                })
                temp.find(".product-package").html(itemsPackagesHtml);
            }

            temp.find("#customerName").html(deliveryNote.receiver);
            temp.find("#customerRecevie").html(deliveryNote.receiver + '(' + deliveryNote.receiver + ')');
            temp.find("#notephoneNumber").html(`********${deliveryNote.recipientPhoneNumber.slice(-2)}`);
            temp.find("#noteCustomerAddress").html(deliveryNote.recipientAddress);
            temp.find("#nameCreate").html(deliveryNote.creatorName);
            temp.find("#nameRecevie").html(deliveryNote.receiver);
            var numberNoteExportHtml = '';
            if (items.bags.length > 0) {
                numberNoteExportHtml += `${items.bags.length} bao`
            }
            if (items.packages.length > 0) {
                numberNoteExportHtml += `${items.packages.length} kiện`
            }
            temp.find("#numberNoteExport").html(numberNoteExportHtml);
            temp.find("#totalNoteWeight").html(`${deliveryNote.totalWeight} (kg)`);
            // gán và format số tiền việt nam đồng
            var amount = deliveryNote.financialNegativePart;
            var formattedAmount = amount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
            temp.find("#amountCustomerCurrent").html(formattedAmount + " VND");
            temp.find("#deliverNote").html(deliveryNote.note);

            console.log(deliveryNote);
            console.log(items);

            $("#labelContent").append(temp.clone());
            printLabel();
            abp.ui.clearBusy("body");
        })


    };

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
        doc.write(`@page {
      size: A5 ;
          height: 100%;
        width: 100%;
      margin: 0 10px;
    }
    .font-bold{
        font-weight: bold;
    }
    .container {
      width: 100%;
      box-sizing: border-box;
    }
    .product{
        font-size: 10px;
    }
    .header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 10px;
    }

    .title {
      text-align: center;
      line-height: 1.2;
    }

    .title h2, .title h3 {
      margin: 0;
    }

    .info-section {
      display: flex;
      justify-content: space-between;
      margin-top: 20px;
    }

    .left, .right {
      width: 48%;
    }

    .right table {
      width: 100%;
    }

    .right td {
      padding: 2px 0;
    }

    .bold {
      font-weight: bold;
    }

    .list {
      margin-top: 10px;
    }

    .note {
      font-size: 10px;
      margin-top: 15px;
      font-size: 9px;
    }

    .footer {
      display: flex;
      justify-content: space-between;
      text-align: center;
      margin-top: 30px;
      font-size: 12px;
    }

    .footer div {
      width: 45%;
    }

    .footer .name {
      margin-top: 50px;
      font-weight: bold;
    }
    .info{
        font-size: 9px;
    }

    hr {
      margin-top: 20px;
    }`);
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

    getDeliveryRequest();

    // set value  $('#customer-select') and triger change
    if (customerIdParam)
        $('#customer-select').val(customerIdParam).trigger('change');

})(jQuery);
