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

            ajaxFunction: _deliveryNoteService.getMyDeliveryNotesFilter,
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
