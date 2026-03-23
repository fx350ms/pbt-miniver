(function ($) {

    _barcode = abp.services.app.barCode,
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-scan-code'),
        _$hiddenCodeType = $('#hiddenCodeType'),
        _$table = $('#CodeTable'),
        _$tb_form = $('#BarCodeSearchForm')

    const codeTypeDescriptions = {
        1: { text: l('Package'), color: 'blue' },
        2: { text: l('Bag'), color: 'purple' },
        4: { text: l('Waybill'), color: 'green' },
    };

    const warehouseFlags = {
        2: { name: 'Kho TQ', flag: '<i class="famfamfam-flags cn" title="Vietnamese"></i>' },
        1: { name: 'Kho VN', flag: '<i class="mr-2 famfamfam-flags vn"></i>' }
    };


    var _$scanCodeTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
          //  ajaxFunction: _barcode.getData,
            ajaxFunction:    _barcode.getData ,

            inputFilter: function () {
                return _$tb_form.serializeFormToObject(true);
            }
        },
        
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$scanCodeTable.draw(false)
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
                data: null,
                width: 80,
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 1,
                data: 'scanCode'
            },
            {
                targets: 2,
                data: 'customerName'
            },
            {
                targets: 3,
                data: 'creationTime',
                render: function (data) {
                    return moment(data).format('YYYY-MM-DD HH:mm:ss');
                }
            },
            {
                targets: 4,
                data: 'codeType',
                render: function (data) {
                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const codeType = codeTypeDescriptions[data];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${codeType ? codeType.color : 'black'};">${codeType ? codeType.text : l('Unknown')}</strong>`;
                }
            },

            {
                targets: 5,
                data: 'action',
                render: function (data) {
                    return data === 1 ? 'Nhập' : 'Xuất';
                }
            },
            {
                targets: 6,
                data: 'sourceWarehouseName'
            },

            {
                targets: 7,
                data: 'creatorUserName'
            }

        ]
    });

    async function loadWarehouse() {
        return await abp.services.app.warehouse.getFull().done(function (data) {
            const dropdown = $("[name='Warehouse']");
            dropdown.empty();
            dropdown.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, warehouse) {
                dropdown.append('<option value="' + warehouse.id + '">' + warehouse.name + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load warehouse: " + error.message);
        });
    }
    loadWarehouse();
    $('.btn-search').on('click', (e) => {
        _$scanCodeTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$scanCodeTable.ajax.reload();
            return false;
        }
    });

    $('.btn-export').on('click', function () {
        // Lấy dữ liệu từ form tìm kiếm
        var filterData = $('#BarCodeSearchForm').serializeFormToObject();
        var maxResultCount = _$scanCodeTable.ajax.params().length;
        filterData.MaxResultCount = maxResultCount || 1000;
        var url = toQueryString(filterData);
        // Gọi API ExportExcel

        // gọi đến url Bags/ExportExcel?+url
        window.location.href = '/Operate/Download?' + url;
        abp.ui.clearBusy();
        // gọi đến url Bags/ExportExcel?+url

    });

    $('.select2').select2({
        theme: "bootstrap4", width: "100%"
    });


    // Initialize date range picker
    $('.date-range').daterangepicker({
        "timePicker": true,
        "timePicker24Hour": true,
        "timePickerSeconds": true,
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
        autoUpdateInput: false,
        "cancelClass": "btn-danger"
    });
    $('.date-range').on('apply.daterangepicker', function (ev, picker) {

        var target = $(this).attr('target');
        $('.start-date.' + target).val(picker.startDate.format('DD/MM/YYYY HH:mm:ss'));
        $('.end-date.' + target).val(picker.endDate.format('DD/MM/YYYY HH:mm:ss'));
        $(this).val(picker.startDate.format('DD/MM/YYYY HH:mm:ss') + ' - ' + picker.endDate.format('DD/MM/YYYY HH:mm:ss'));
    });
    $('.date-range').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        var target = $(this).attr('target');
        $('.start-date.' + target).val('');
        $('.end-date.' + target).val('');
    });

})(jQuery);
