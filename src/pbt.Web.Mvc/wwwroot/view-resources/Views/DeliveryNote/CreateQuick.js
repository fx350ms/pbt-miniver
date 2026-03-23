(function ($) {
    var _deliveryNoteService = abp.services.app.deliveryNote;
    var _bagService = abp.services.app.bag;
    var _packageService = abp.services.app.package;
    var _shippingPartnerServices = abp.services.app.shippingPartner;
    var _deliveryRequest = abp.services.app.deliveryRequest;

    l = abp.localization.getSource('pbt');
    var _$form = $("#form-delivery-note");


    async function loadBagsAndPackages(customerId) {
        try {
            // Lấy danh sách ID bao và kiện đã có trong bảng #table-delivery-note-items
            var excludedBagIds = [];
            var excludedPackageIds = [];

            $('#table-delivery-note-items tbody tr').each(function () {
                var itemId = $(this).data('id');
                var itemType = $(this).data('type');

                if (itemType === 1) {
                    excludedBagIds.push(itemId); // Bao
                } else if (itemType === 2) {
                    excludedPackageIds.push(itemId); // Kiện
                }
            });
          
            let bagsHtml = '';
            let packagesHtml = '';
            // Gọi service để lấy danh sách bao và kiện đã về kho VN
            _bagService.getBagsInVietnamWarehouse(
                customerId,
                excludedBagIds.join(',')
            ).done(function (bags) {
               
                bags.forEach(function (bag) {
                    bagsHtml += `
                    <tr data-id="${bag.id}" data-type="1">
                        <td class="item-code">${bag.bagCode}</td>
                        <td class="item-weight">${bag.weight}</td>
                        <td>BAO</td>
                        <td>
                            <button class="btn btn-sm bg-primary btn-add-item"><i class="fas fa-plus"></i></button>
                        </td>
                    </tr>
                `;
                });
                $('#table-bags-packages-pending tbody').html(bagsHtml);
            });

            _packageService.getPackagesInVietnamWarehouse(
                customerId,
                excludedPackageIds.join(',')
            ).done(function (packages) {
                packages.forEach(function (package) {
                    packagesHtml += `
                    <tr data-id="${package.id}" data-type="2">
                        <td class="item-code">${package.packageNumber}</td>
                        <td class="item-weight">${package.weight}</td>
                        <td>KIỆN</td>
                        <td>
                            <button class="btn btn-sm bg-primary btn-add-item"><i class="fas fa-plus"></i></button>
                        </td>
                    </tr>
                `;
                });

                $('#table-bags-packages-pending tbody').append(packagesHtml);
            });

        } catch (error) {
            console.error("Có lỗi xảy ra khi tải danh sách bao và kiện:", error);
            abp.notify.error("Không thể tải danh sách bao và kiện.");
        }
    }

    // Gọi hàm khi trang được tải
    $(document).ready(function () {
        const customerId = $('input[name="CustomerId"]').val();
        loadBagsAndPackages(customerId);
    });


    $(document).on('click', '.btn-add-item', function () {
        var row = $(this).closest('tr'); // Lấy dòng hiện tại
        var itemId = row.data('id'); // Lấy ID của item
        var itemType = row.data('type'); // Lấy loại của item (bag/package)
        var itemCode = row.find('.item-code').text(); // Lấy mã bao/kiện
        var itemWeight = row.find('.item-weight').text(); // Lấy cân nặng

        // Thêm item vào bảng #table-delivery-note-items
        $('#table-delivery-note-items tbody').append(`
        <tr data-id="${itemId}" data-type="${itemType}">
            <td>${itemCode}</td>
            <td>${itemWeight}</td>
            <td>${itemType === 1 ? 'BAO' : 'KIỆN'}</td>
            <td>
                <button class="btn btn-sm bg-danger btn-remove-item"><i class="fas fa-trash"></i></button>
            </td>
        </tr>
    `);

        // Xóa item khỏi bảng #table-bags-packages-pending
        row.remove();
    });

    $(document).on('click', '.btn-remove-item', function () {
        var row = $(this).closest('tr'); // Lấy dòng hiện tại
        var itemId = row.data('id'); // Lấy ID của item
        var itemType = row.data('type'); // Lấy loại của item (bag/package)
        var itemCode = row.find('td:first').text(); // Lấy mã bao/kiện
        var itemWeight = row.find('td:nth-child(2)').text(); // Lấy cân nặng

        // Thêm item vào bảng #table-bags-packages-pending
        $('#table-bags-packages-pending tbody').append(`
        <tr data-id="${itemId}" data-type="${itemType}">
            <td class="item-code">${itemCode}</td>
            <td class="item-weight">${itemWeight}</td>
            <td>${itemType === 1 ? 'BAO' : 'KIỆN'}</td>
            <td>
                <button class="btn btn-sm bg-primary btn-add-item"><i class="fas fa-plus"></i></button>
            </td>
        </tr>
    `);

        // Xóa item khỏi bảng #table-delivery-note-items
        row.remove();
    });
    // Xử lý sự kiện khi chọn khách hàng
    $(document).on('click', '.btn-save-and-export', async function () {
        try {
            const customerId = $('input[name="CustomerId"]').val();
            const receiver = $('input[name="Receiver"]').val();
            const recipientPhoneNumber = $('input[name="RecipientPhoneNumber"]').val();
            const recipientAddress = $('input[name="RecipientAddress"]').val();
            const note = $('input[name="Note"]').val();
            const totalWeight = parseFloat($('input[name="TotalWeight"]').val()) || 0;
            const deliveryFee = parseFloat($('input[name="DeliveryFee"]').val()) || 0;
            const deliveryFeeReason = parseInt($('select[name="DeliveryFeeReason"]').val());
            const shippingPartnerId = parseInt($('select[name="ShippingPartnerId"]').val()) || null;

            // Lấy danh sách bao và kiện từ bảng #table-delivery-note-items
            const items = [];
            $('#table-delivery-note-items tbody tr').each(function () {
                const itemId = $(this).data('id');
                const itemType = $(this).data('type');
                items.push({ id: itemId, type: itemType });
            });

            if (items.length === 0) {
                abp.notify.error("Vui lòng thêm bao hoặc kiện vào phiếu xuất.");
                return;
            }

            // Gọi API tạo phiếu xuất nhanh
            abp.ui.setBusy();
            const response = await abp.services.app.deliveryNote.createQuickDeliveryNote({
                customerId,
                receiver,
                recipientPhoneNumber,
                recipientAddress,
                note,
                totalWeight,
                deliveryFee,
                deliveryFeeReason,
                shippingPartnerId,
                items
            });

            abp.notify.success("Tạo phiếu xuất nhanh thành công.");
            window.location.href = `/DeliveryNote/Detail/${response.id}`;
        } catch (error) {
            console.error("Có lỗi xảy ra:", error);
            abp.notify.error("Không thể tạo phiếu xuất nhanh.");
        } finally {
            abp.ui.clearBusy();
        }
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

                    itemsBagsHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.bagCode} - ${item.weight} (kg) - ${item.totalPackages} (kiện)</span>`;
                })
                temp.find(".product-bag").html(itemsBagsHtml);
            }
            if (items.packages.length > 0) {
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


    $('.mask-number').maskNumber({ integer: true, thousands: '.' });

})(jQuery);
