(function ($) {
    var _exportService = abp.services.app.deliveryNote;
        l = abp.localization.getSource('pbt');
        _$modal = $('#DepartmentCreateModal');
        _$form = _$modal.find('form');

    loadItemDeliveryNote();
    
    // getBagToday();

    // _$departmentsTable.on('draw', function () {
    //     $("#totalPackage").text(_$departmentsTable.page.info().recordsTotal);
    // });

    _$form.find('.save-button').on('click', (e) => {

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
            // _$departmentsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });
      
    async function loadItemDeliveryNote() {
        var deliveryNoteId = $("[name='deliveryNoteId']").val();
        return await _exportService.getItemByDeliveryNote(deliveryNoteId).done(function (data) {
            $("#totalItems").text(data.bags.length + data.packages.length);
            var table = $("#PackagesPendingTable tbody");
            table.empty();
            $.each(data.bags, function (index, item) {
                var row = `
                    <tr id="row-${data.id}">
                        <td>${item.bagCode}
                            <input type="hidden" name="itemDelivery" value="${item.id}">
                        </td>
                        <td>${item.deliveryRequestOrderCode || "_"}</td>
                        <td>-</td>
                        <td>${item.address || "_"}</td>
                        <td>${item.weight || 0} kg</td>
                        <td>${item.volumePackages || "-"}</td>
                        <td>
                            <button class="btn btn-outline-secondary btn-sm btn-bag-detail" data-id="${item.id}" data-toggle="modal" data-target="#detailModal">📋</button>
                        </td>
                    </tr>
                `;
                table.append(row);
            });

            $.each(data.packages, function (index, item) {
                var row = `
                    <tr id="row-${data.id}">
                        <td>
                            ${item.packageNumber}
                            <input type="hidden" name="itemDelivery" value="${item.id}">
                        </td>
                        <td>${item.deliveryRequestOrderCode || "_"}</td>
                        <td>${item.orderCode}</td>
                        <td>${item.warehouseId}</td>
                        <td>${item.weight || 0} kg</td>
                        <td>${item.dimention} (cm3)</td>
                        <td></td>
                    </tr>
                `;
                table.append(row);
            });
            
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }
     
    
    $(document).on('click', '#btn-export', function () {
        // show confirm
        abp.ui.setBusy($("body"));
        var deliveryNoteId = $("[name='deliveryNoteId']").val();
        _exportService.updateStatusDeliveryNote({ id: deliveryNoteId, status: 1 }).done(function (data) {
            abp.ui.clearBusy($("body"));
            abp.notify.success("Xuất kho thành công");
            setTimeout(function () {
                window.location.reload();
            }, 2000)
        }).fail(function (error) {
            PlaySound('warning');
            abp.notify.error("Failed to export: " + error.message);
        });
    });

    $(document).on('click', '.edit-department', function (e) {
        var departmentId = $(this).attr("data-department-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Warehouses/EditModal?Id=' + departmentId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#DepartmentEditModal div.modal-content').html(content);

                loadProvince("#EditProvinceId").then(() => {
                    $("#EditProvinceId").trigger("change");
                });
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', '.btn-bag-detail', function () {
        const bagId = $(this).data('id');
        getPackageByBag(bagId);
    })

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
   
    $('.print-delivery-note').click(function () {

        var id = $("[name='deliveryNoteId']").val();
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
            temp.find("#deliveryNoteCode").text(deliveryNote.deliveryNoteCode);
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
                   
                    itemsBagsHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.bagCode} - ${item.weight} (kg) - ${item.totalPackages} (kiện)</span><br/>`;
                })
                temp.find(".product-bag").html(itemsBagsHtml);
            }
            if(items.packages.length > 0){
                itemsPackagesHtml += `<div style="font-weight: bold; margin: 2px">Kiện:</div>`;
                items.packages.forEach((item, index) => {
                    itemsPackagesHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.packageNumber} - <i>${item.trackingNumber}</i> - ${item.weight} (kg)</span><br/>`;
                })
                temp.find(".product-package").html(itemsPackagesHtml);
            }
            temp.find("#customerName").html(deliveryNote.receiver);
            temp.find("#customerRecevie").html(deliveryNote.receiver + '(' + deliveryNote.receiver + ')');
            temp.find("#notephoneNumber").html(`************`);
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
            }, 100);
        };
    }

    function loadDeliveryNoteLogs() {
        var deliveryNoteId = $("[name='deliveryNoteId']").val();
        return abp.services.app.entityChangeLogger.getLogsByMultiEntityTypeName(['DELIVERYNOTE','DELIVERYNOTEDTO'], deliveryNoteId).done(function (data) {
            const logsContainer = $("#deliveryNote-logs");
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
 

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });
    loadDeliveryNoteLogs();
   
})(jQuery);
