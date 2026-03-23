(function ($) {
    var _exportService = abp.services.app.deliveryNote;
    var _departmentService = abp.services.app.bag;
    var _shippingPartnerServices = abp.services.app.shippingPartner;
    var _deliveryRequest = abp.services.app.deliveryRequest;

    l = abp.localization.getSource('pbt');
    _$form = $("#create-bag-form");

    _$form.find('.btnSave').on('click', (e) => {
        const close = $(e.target).attr('data-close') == "true";
        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }
        const data = _$form.serializeFormToObject();
        data["IsClosed"] = close;
        abp.ui.setBusy();
        _departmentService.create(data).done(function () {
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            window.location.href = "/bags"
        }).always(function () {
            abp.ui.clearBusy();
        });
    });

    function getDeliveryRequest() {
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
            }
        })
    }

    function getShippingPartner() {
        
        _shippingPartnerServices.getAllShippingPartnersByLocation(1).done(function (response) {
            if (response) {
                var selectShippingPartner = $("#shippingPartner");
                selectShippingPartner.append('<option value="">' + l('SelectShippingPartner') + '</option>');
                response.forEach(function (partner) {
                    selectShippingPartner.append(
                        `<option value="${partner.id}">(${partner.code})${partner.name}</option>`
                    );
                });
            }
        })
    }

    $(document).on("click", "#customer-list .customer-item", function (ev) {
        debugger;
        var customerId = $(ev.currentTarget).attr("data-id");
        getCustomerData(customerId);
        getBagByCustomer(customerId);
        getShippingPartner();
        $("#select-customer").addClass("d-none");
        $(".customer-data").removeClass("d-none");
        $("#customer-select").val(customerId);
    })

    async function getBagByCustomer(customerId) {
        const tableItemRequest = $("#bagRequest tbody");
        tableItemRequest.empty();
        var totalWeight = 0;
        await abp.services.app.bag.getBagByCustomer(customerId).done(function (data) {
            data.forEach(function (bag) {
                tableItemRequest.append(
                    `<tr id="row-${bag.id}" data-type="bag">
                        <td><input type="checkbox"></td>
                        <td class="itemCode">${bag.bagCode}</td>
                        <td>${bag.deliveryRequestOrderCode}</td>
                        <td>${bag.weight || 0} kg</td>
                        <td>${bag.note}</td>
                        <td>
                            <button class="btn btn-outline-secondary btn-sm btn-bag-detail" data-id="${bag.id}" data-toggle="modal" data-target="#detailModal">📋</button>
                        </td>
                    </tr>`
                );
                totalWeight += bag.weight;
            });
            $("#numberBag").text(data.length);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });

        await abp.services.app.package.getPackageByCustomer(customerId).done(function (data) {
            data.forEach(function (package) {
                tableItemRequest.append(
                    `<tr id="row-${package.id}" data-type="package">
                        <td>
                        <input type="checkbox">
                        </td>
                        <td class="itemCode">${package.packageNumber}</td>
                        <td>${package.deliveryRequestOrderCode}</td>
                        <td>${package.weight || 0} kg</td>
                        <td>${package.dimention} (cm3)</td>
                        <td></td>
                    </tr>`
                );
                totalWeight += package.weight;
            });
            $("#numberPackage").text(data.length);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });
        $("#totalWeight").text(totalWeight);
        $("#totalWeightInput").val(totalWeight);
    }
    function ScanCode(code) {
        if (code) {
            _exportService.scanCode(code).done(function (data) {
                // Hiển thị panel thông tin của phiếu xuất
                debugger;
                if (data && data.result) {
                    
                } else {
                    PlaySound('warning');
                    abp.notify.error("Không tìm thấy kiện/bao hàng với mã: " + code);
                }
            });
        }
    }
    function addItemToDeliveryNote(itemCode, itemType) {
        // kiểm tra xem itemCode đã tồn tại trong cột ts có class .itemCode trong bảng #tableReadyDelivery hay chưa.
        if ($("#tableReadyDelivery tbody").find(`.itemCode:contains('${itemCode}')`).length > 0) {
            PlaySound('warning');
            abp.notify.error("Mã kiện/bao hàng đã tồn tại trong danh sách xuất kho.");
            abp.ui.clearBusy($("body"));
            $("#scanCode").val("");
            return false;
        }
       
        // kiểm tra xem kiện, bao có nằm trong danh sách sẵn sàng giao không 
        if ($("#bagRequest tbody").find(`.itemCode:contains('${itemCode}')`).length === 0) {
            PlaySound('warning');
            abp.notify.error("Mã kiện/bao hàng không phải của khách hàng đã chọn.");
            abp.ui.clearBusy($("body"));
            $("#scanCode").val("");
            return false;
        }
        
        if (itemType === "bag") {
            return abp.services.app.bag.getBagDeliveryRequestByCode(itemCode).done(function (data) {
                if(data == null){
                    PlaySound('warning');
                    abp.notify.error("Bao hàng không tồn tại hoặc đã được thêm vào phiếu xuất kho.");
                }
                const tableBagRequest = $("#tableReadyDelivery tbody");
                tableBagRequest.append(
                    `<tr id="row-${data.id}"  data-type="bag">
                            <td>
                            <input type="checkbox">
                            <input type="hidden" name="itemDelivery" value="${data.id}">
                            </td>
                            <td class="itemCode">${data.bagCode}</td>
                            <td>-</td>
                            <td>${data.weight || 0} kg</td>
                            <td>${data.note}</td>
                            <td>
                                <button class="btn btn-outline-secondary btn-sm btn-bag-detail" data-id="${data.id}" data-toggle="modal" data-target="#detailModal">📋</button>
                            </td>
                        </tr>`
                );
                $("#bagRequest tbody").find("#row-" + data.id).remove();
                var fee = data.packagesDtos.reduce(function (total, item) {
                    return total + item.domesticShippingFee;
                }, 0);
                $("#shippingFee").val(fee);
                PlaySound('success');
                abp.notify.info("Thêm bao hàng thành công.");
            }).fail(function (error) {
                PlaySound('warning');
                abp.notify.error("Có lỗi: " + error.message);
                return;
            });
        }

        if (itemType === "package") {
            return abp.services.app.package.getPackageByCode(itemCode).done(function (data) {
                if (data == null) {
                    // show message error
                    PlaySound('warning');
                    abp.notify.error("Không tìm thấy kiện/bao hàng");
                    return false;
                }
                const tableBagRequest = $("#tableReadyDelivery tbody");
                tableBagRequest.append(
                    `<tr data-type="package">
                        <td>
                        <input type="checkbox">
                        <input type="hidden" name="itemDelivery" value="${data.id}">
                        </td>
                        <td class="itemCode">${data.packageNumber}</td>
                        <td>${data.deliveryRequestOrderCode || "_"}</td>
                        <td>${data.weight || 0} kg</td>
                        <td>-</td>
                        <td>
                        </td>
                    </tr>`
                );
                $("#bagRequest tbody").find("#row-" + data.id).remove();
                $("#shippingFee").val(data.domesticShippingFee);
                PlaySound('success');
                abp.notify.info("Thêm kiện hàng thành công.");
            }).fail(function (error) {
                PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
            });
        }
    }

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

    $("#save-delivery-note").on("click", function (e) {
        saveDeliveryNote()
    })
    $("#save-exprot-delivery-note").on("click", function (e) {
        saveDeliveryNote(true)
    })

    function saveDeliveryNote(ep = false) {
        var data = {
            status: ep ? 1 : 0,
            customerId: $("#customer-select").val(),
            Receiver: $("#receiver").val(),
            ShippingFee: $("#shippingFee").val(),
            FinancialNegativePart: $("#financialAmount").val().replaceAll(".",""),
            Cod: "0",
            length: $("#Length").val(),
            width: $("#Width").val(),
            height: $("#Height").val(),
            deliveryFee: $("#deliveryFee").val(),
            note: $("#notes").val(),
            RecipientPhoneNumber: $("#recipientPhone").val(),
            RecipientAddress: $("#recipientAddress").val(),
            ItemType: $("[name='scanCodeType']:checked").val(),
            DeliveryFeeReason: $("#deliveryFeeReason").val(),
            ShippingPartnerId: $("#shippingPartner").val(),
            TotalWeight: $("#totalWeightInput").val() || 0,
            itemBags: [],
            itemPackages: []
        }
 
        $("#tableReadyDelivery tbody tr").each(function (i, el) {
            const type = $(el).attr("data-type");
            if (type === "bag") {
                data.itemBags.push($(el).find("[name='itemDelivery']").val());
            }
            if (type === "package") {
                data.itemPackages.push($(el).find("[name='itemDelivery']").val());
            }
        })

        abp.services.app.deliveryNote.createWithTransactions(data).done(function (data) {
            abp.notify.success("Tạo phiếu xuất kho thành công.");
            if(ep){
                printStamp(data.id);
            }else{
                window.location.href = '/DeliveryNote';
            }
        }).fail(function (error) {
            if (error) {
                const validationErrors = error.validationErrors;
                if (validationErrors) {
                    validationErrors.forEach(function (validationError) {
                        const field = validationError.members[0];
                        const message = validationError.message;
                        const inputField = $("#" + field);
                        inputField.addClass("is-invalid");
                        inputField.siblings(".invalid-feedback").text(message);
                    });
                } else {
                    abp.message.error(error.message);
                }
            } else {
                abp.message.error("Có lỗi xảy ra!");
            }
            //PlaySound('warning'); abp.notify.error("Failed to create delivery note: " + error.message);
        });

        console.log(data);
    }


    async function loadCustomer() {
        return await abp.services.app.customer.getFull().done(function (data) {
            const customerSelect = $("select[name='CustomerId']");
            customerSelect.empty();
            customerSelect.append('<option value="">' + l('SelectCustomer') + '</option>');
            $.each(data, function (index, customer) {
                customerSelect.append('<option value="' + customer.id + '">' + customer.fullName + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load customer: " + error.message);
        });
    }

    $(document).on('click', '.btn-bag-detail', function () {
        const bagId = $(this).data('id');
        getPackageByBag(bagId);
    })

    $("#customer-select").change((e) => {
        debugger;
        var selectCustomer = e.target.value;
        if (!!selectCustomer) {
            getCustomerData(selectCustomer);
            getBagByCustomer(selectCustomer);
            getShippingPartner();
            $("#select-customer").addClass("d-none");
            $(".customer-data").removeClass("d-none");
        } else {
            $("#select-customer").removeClass("d-none");
            $(".customer-data").addClass("d-none");
        }
    })

    // $("#save-delivery-note").click(() => {
    //     window.location.href = "/Export/Detail/1"
    // })

    function getCustomerData(selectCustomer) {
        return abp.services.app.customer.getByCustomerId(selectCustomer).done(function (data) {
            $("#receiver").val(data.fullName);
            $("#customerName").text(data.fullName);
            $("#customerName").text(data.phoneNumber);
            $("#currentAmount").text(formatNumberThousand(data.currentAmount) + ' đ');
            $("#receiver").text(data.fullName);
            $("#recipientPhone").val(data.phoneNumber);
            // $("#recipientAddress").val(data.address);
            $("#financialAmount").val(formatNumberThousand(data.currentDebt));
            console.log(data);
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load customer: " + error.message);
        });
    }

    function printStamp(deliveryNoteId){
        var id = deliveryNoteId || $("[name='deliveryNoteId']").val();
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
                  <tr><td>Phần âm tài chính (VND):</td>
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
            _exportService.getDeliveryNoteById(id)
            , _exportService.getItemByDeliveryNote(id)
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
            if(items.bags.length > 0){
                itemsBagsHtml += `<div style="font-weight: bold; margin: 2px">Bao:</div>`;
                items.bags.forEach((item, index) => {
                    
                    itemsBagsHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.bagCode} - ${item.weight} (kg) - ${item.totalPackages} (kiện)</span>`;
                })
                temp.find(".product-bag").html(itemsBagsHtml);
            }
            if(items.packages.length > 0){
                itemsPackagesHtml += `<div style="font-weight: bold; margin: 2px">Kiện:</div>`;
                items.packages.forEach((item, index) => {
                    itemsPackagesHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.packageNumber} - <i>${item.trackingNumber}</i> - ${item.weight} (kg)</span>`;
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
            if(items.bags.length > 0) {
                numberNoteExportHtml += `${items.bags.length} bao`
            }
            if(items.packages.length > 0) {
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
            // temp.find('.whvn').text("HN");
            // temp.find('.linevn').text(tem.shippingLineId == 1 ? "LO" : 'TMDT');
            // temp.find('#customerCode').text(tem.customer ? tem.customer.fullName : '');
            // temp.find('#packageCode').text(tem.packageNumber);
            // temp.find('#orderNumber').text(tem.trackingNumber);
            // temp.find('#weight').text(tem.weight + "kg");
            // temp.find('#barcodeImage').attr('src', 'https://bwipjs-api.metafloor.com/?bcid=code128&text=' + tem.packageNumber);
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

        iframe.contentWindow.onload = function(){
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
            setTimeout(function () {
                document.body.removeChild(iframe);
                window.location.href = '/DeliveryNote';
            }, 100);
        };
    }

    $("#scanCode").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            
            abp.ui.setBusy($("body"));
            ScanCode(e.target.value);
            abp.ui.clearBusy($("body"));
          ////  const type = $("[name='scanCodeType']:checked").val();
          //  addItemToDeliveryNote(  e.target.value).then(function () {
          //  }).always(function () {
          //      abp.ui.clearBusy($("body"));
          //  });
            $("#scanCode").val('');
        }
    });

    $(".bagType").on("change", function (e) {
        if (e.target.value == "2") {
            loadCustomer().then(r => {
                $("select[name='CustomerId']").removeAttr("disabled");
            });
        } else {
            $("select[name='CustomerId']").attr("disabled", "disabled").val("");
        }
    })

    $('.btn-search').on('click', (e) => {
        _$departmentsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$departmentsTable.ajax.reload();
            return false;
        }
    });

    $("#table-packages tbody tr").on("click", function () {
        $("#table-packages tbody tr").removeClass("active")
        $(this).addClass("active")
    })
    $('.select2').select2({
        theme: "bootstrap4"
    });
    if ($("#customer-select").val()) {

        $("#customer-select").trigger("change");
    }
    getDeliveryRequest();


    $('.mask-number').maskNumber({ integer: true, thousands: '.' });
    // loadCustomer();
})(jQuery);
