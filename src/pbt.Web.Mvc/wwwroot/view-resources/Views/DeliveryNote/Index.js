(function ($) {
    var _deliveryNoteService = abp.services.app.deliveryNote,
        l = abp.localization.getSource('pbt'),

        _$table = $('#DeliveryNoteTable');
    const statusDescriptions = {
        0: { text: 'Nháp', class: 'badge badge-info' },
        1: { text: 'Đã xuất', class: 'badge badge-success' },
        2: { text: 'Đã hủy', class: 'badge badge-secondary' }
    };

    $('.select2').select2({
        theme: "bootstrap4", width: "100%"
    });


    var _$deliveryNoteTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {

            ajaxFunction: _deliveryNoteService.getDeliveryNotesFilter,
            inputFilter: function () {
                return $('#PackageSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$deliveryNoteTable.draw(false)
            }
        ],
        lengthMenu: [25, 50],
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
                data: 'deliveryNoteCode',
                sortable: false,
                render: function (data, type, row) {
                        var html = `<a target="_blank" href="/DeliveryNote/Detail/${row.id}"> ${data} </a>`
                        return html;
                }
            },
            {
                targets: 2,
                data: 'status',
                sortable: false,
                render: function (data, type, row) {

                    const status = statusDescriptions[row.status];
                    // Trả về mô tả với màu sắc được áp dụng
                    var html = `<strong class="${status ? status.class : 'black'}">${status ? status.text : 'Chưa xác định'}</strong> `;
                    return html;
                }
            },
            {
                targets: 3,
                data: 'creationTimeFormat',
                sortable: false,
                width: 200,
                render: (data, type, row, meta) => {
 
                    // Ngày tạo
                    const creation = formatDateToDDMMYYYYHHmm(row.creationTime) || '';
                    // Ngày xuất kho TQ
                    const exportDate = row.exportTime ? formatDateToDDMMYYYYHHmm(row.exportTime) : '';
                    
                    return [
                        `<div>${l('CreationTime')}: <strong> ${creation}</strong> </div>`,
                        `<div>${l('ExportDate')}: <strong> ${exportDate}</strong> </div>`,
                    ].join('');
                }
            },
             
            {
                targets: 4,
                data: 'note',
                width: 200,
                sortable: false
            },
            // Customer name
            {
                targets: 5,
                data: '',
                sortable: false,
                render: function (data, type, row) {
                    return row.customer ? `<span class="text-bold">${row.customer.username || ''}</span>` : ''; // Hiển thị tên khách hàng
                }

            },
            {
                targets: 6,
                data: 'receiver',
                sortable: false
            },


            // Address
            {
                targets: 7,
                data: 'recipientAddress',
                width: 200,
                sortable: false,

            },
            // Shipping partner
            {
                targets: 8,
                data: '',
                sortable: false,
                render: function (data, type, row) {
                    return row.shippingPartner ? '' + row.shippingPartner.name + '' : ''; // Hiển thị tên đối tác vận chuyển
                }
            },

            {
                targets: 9,
                data: 'totalWeight',
                className: 'text-right',
                sortable: false,
                render: function (data, type, row) {
                    return `${formatThousand(data || '0') }kg`;
                }
            },
            {
                targets: 10,
                data: 'deliveryFee',
                width: 150,
                sortable: false,
                render: function (data, type, row) {
                    let badgeHtml = '';
                    if (row.deliveryFeeReason === 1) {
                        badgeHtml = '<span class="badge badge-success">Không thu phí</span>';
                    } else if (row.deliveryFeeReason === 2) {
                        badgeHtml = '<span class="badge badge-danger">Thu lại phí</span>';
                    }
                    return `  ${badgeHtml}  `;
                }
            },
            {
                targets: 11,
                data: 'deliveryFee',
                width: 120,
                className: 'text-right',
                sortable: false,
                render: function (data, type, row) {
                  
                    return `    ${formatThousand(data || '0') }`;
                }
            },
            {
                targets: 12,
                data: 'shippingFee',
                className: 'text-right',
                sortable: false,
                render: function (data, type, row) {
                    return formatThousand(data || '0');
                }
            },

            {
                targets: 13,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    var cancelItemHtml = row.status==0  ? 
                    `<a target="_blank" class="dropdown-item btn-cancel-deliverynote bg-danger"  type="button" data-id="${row.id}">
                    <i class="fas fa-times"></i> ${l('Cancel')}
                    </a>` : '' ;

                    return [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,
                        `       <a target="_blank" class="dropdown-item bg-primary" href="/DeliveryNote/detail/${row.id}" type="button" data-department-id="${row.id}">`,
                        `           <i class="fas fa-eye"></i> ${l('Detail')}`,
                        `       </a>`,
                        cancelItemHtml,
                        `       <a class="dropdown-item print-temp bg-success"   type="button" data-id="${row.id}">`,
                        `           <i class="fas fa-print"></i> ${l('PrintTemp')}`,
                        `       </a>`,
                      
                        `   </div>`,
                        `</div>`
                    ].join('');
                }
            }
        ]
    });


    $('.date-range').daterangepicker({
        // startDate: moment().subtract(6, 'days').startOf('day'),
        "locale": {
            "format": "DD/MM/YYYY HH:mm:ss",
            "separator": " - ",
            "applyLabel": l('Apply'),
            "cancelLabel": l('Cancel'),
            "fromLabel": l('From'),
            "toLabel": l('To'),
            "customRangeLabel": l('Select'),
            "weekLabel": "W",

        },
        timePickerSeconds: true, // Cho phép chọn giây
        timePicker24Hour: true,
        //  autoUpdateInput: true,
        "cancelClass": "btn-danger",
        "timePicker": true,
    }
    );
    $('.date-range').on('apply.daterangepicker', function (ev, picker) {
        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format("DD/MM/YYYY HH:mm:ss"));
        $('.end-date.' + target).val(picker.endDate.format("DD/MM/YYYY HH:mm:ss"));
        $(this).val(picker.startDate.format("DD/MM/YYYY HH:mm:ss") + ' - ' + picker.endDate.format("DD/MM/YYYY HH:mm:ss"));
    });

    $('.date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        var target = $(this).attr('target');
        $('.start-date.' + target).val('');
        $('.end-date.' + target).val('');
    });

    $('.date-range').each(function () {

        var picker = $(this).data('daterangepicker'); // Lấy đối tượng daterangepicker
        var target = $(this).attr('target'); // Lấy 'target' từ input .date-range

        if (picker) {
            // Gán giá trị mặc định cho các trường .start-date và .end-date
            $('.start-date.' + target).val('');
            $('.end-date.' + target).val('');

            // Cập nhật giá trị hiển thị trên input .date-range chính
            $(this).val('');
        }
    });


    $('.btn-clear-date').on('click', function () {
        var target = $(this).attr('target');
        var targetInput = $(this).attr('target-date');
        if (target) {
            $('#' + target).val('');
            $('.' + targetInput).val('');
        }
    });

    $(document).on("click", ".nav-bag", function (event) {
        var type = $(this).attr("data-type");
        $("[name='FilterType']").val(type);
        $(".nav-bag").removeClass("active");
        $(this).addClass("active")
        _$deliveryNoteTable.ajax.reload();
    })


    $(document).on('click', '.print-temp', function () {
        var id = $(this).attr("data-id");
        printStamp(id);
    });

    $(document).on('click', '.btn-cancel-deliverynote', function () {
        var id = $(this).attr("data-id");
        abp.message.confirm(
            l('DeliveryNoteConfirmCancelMessage'),
            undefined,
            (isConfirmed) => {
                if (isConfirmed) {
                    _deliveryNoteService.cancelDeliveryNote(id ).then(() => {
                        abp.notify.success(l('SuccessfullyCanceled'));
                        _$deliveryNoteTable.ajax.reload();
                    });
                }
            }
        );
    });

    function printStamp(deliveryNoteId) {

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
                    itemsBagsHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.bagCode} - ${item.weight} (kg) - ${item.totalPackages} (kiện)</span><br/>`;
                })
                temp.find(".product-bag").html(itemsBagsHtml);
            }
            if (items.packages.length > 0) {
                itemsPackagesHtml += `<div style="font-weight: bold; margin: 2px">Kiện:</div>`;
                items.packages.forEach((item, index) => {
                    itemsPackagesHtml += `<span style="margin-right: 4px"><b>${index + 1}</b>. ${item.packageNumber} - <i>${item.trackingNumber}</i> - ${item.weight} (kg)</span><br/>`;
                })
                temp.find(".product-package").html(itemsPackagesHtml);
            }

            temp.find("#customerName").html(deliveryNote.receiver);
            temp.find("#customerRecevie").html(deliveryNote.receiver + '(' + deliveryNote.receiver + ')');
            temp.find("#notephoneNumber").html(`********${deliveryNote.recipientPhoneNumber ? deliveryNote.recipientPhoneNumber.slice(-2) : "20"}`);
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
                window.location.href = '/DeliveryNote';
            }, 100);
        };
    }



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


    $(document).on('click', 'a[data-target="#DepartmentCreateModal"]', (e) => {
        $('.nav-tabs a[href="#department-details"]').tab('show')
        loadProvince();
    });

    abp.event.on('department.edited', (data) => {
        _$deliveryNoteTable.ajax.reload();
    });


    $('.btn-search').on('click', (e) => {
        _$deliveryNoteTable.ajax.reload();
        return false;
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$deliveryNoteTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
