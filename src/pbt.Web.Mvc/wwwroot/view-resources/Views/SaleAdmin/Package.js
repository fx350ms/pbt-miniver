(function ($) {

    var _customerService = abp.services.app.customer,
        _packageService = abp.services.app.package,
        l = abp.localization.getSource('pbt'),
        _$packageDiv = $('#package-list'),
        _$form = _$packageDiv.find('form'),
        _$packageTable = $('#PackageTable');

    const shippingLines = {
        1: { text: 'Lô', color: 'blue' },
        2: { text: 'TMĐT', color: 'purple' },
    };

    var packagesTable = _$packageTable.DataTable({
        paging: true,
        serverSide: true,
        initComplete: function (settings, json, a, b) {

        },
        listAction: {
            ajaxFunction: _packageService.getListForSaleView,
            inputFilter: function () {
                return _$form.serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => packagesTable.draw(false)
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
                data: 'customerName',
                sortable: false,
                width: 140,
            },
            {
                targets: 2,
                data: 'waybillNumber',
                sortable: false,
                width: 160,
                render: (data, type, row, meta) => {
                    if (row.parentId && row.parentId > 0 && row.waybillNumber) {
                        if (isAdminOrSaleAdmin) {
                            return `<a href="/Orders/Detail/${row.orderId}">${row.trackingNumber} - (${row.waybillNumber})</a>`;
                        }
                        else {
                            return `${row.trackingNumber} - (${row.waybillNumber})`;
                        }

                    }
                    else {
                        if (isAdminOrSaleAdmin) {
                            return `<a href="/Orders/Detail/${row.id}">${row.trackingNumber}</a>`;
                        }
                        else {
                            return `${row.trackingNumber}`;
                        }

                    }
                }
            },

            {
                targets: 3,
                data: 'exportDate',
                sortable: false,
                render: (data, type, row, meta) => {
                    return data ? moment(data).format('DD/MM/YYYY') : ''; // Ngày xuất kho TQ
                }
            },
            {
                targets: 4,
                data: 'importDate',
                sortable: false,
                render: (data, type, row, meta) => {
                    return data ? moment(data).format('DD/MM/YYYY') : ''; // Ngày xuất kho TQ
                }
            },
            {
                targets: 5,
                data: 'bagNumber',
                sortable: false,
                render: (data) => {
                    return data || ''; // Mã bao
                }
            },
            {
                targets: 6,
                data: 'weight',
                sortable: false,
                className: 'text-right',
                render: (data) => {
                    return data ? `${data} kg` : ''; // Cân nặng tịnh (Kg)
                }
            },
            {
                targets: 7,
                data: 'volume',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return row.volumeStr;
                }
            },
            {
                targets: 8,
                data: 'shippingPartner',
                sortable: false,
                className: 'text-right'

            },
            {
                targets: 9,
                data: 'totalFee',
                sortable: false,
                className: 'text-right',
                render: (data) => {
                    return data ? `${data.toLocaleString()} đ` : ''; // Phí ship nội địa
                }
            },
            {
                targets: 10,
                data: 'domesticShippingFee',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? `${data.toLocaleString()} đ` : ''; // Phí ship nội địa
                }
            },
            {
                targets: 11,
                data: 'woodenPackagingFee',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? `${data.toLocaleString()} đ` : ''; // Phí đóng gỗ
                }
            },
            {
                targets: 12,
                data: 'shockproofFee',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {

                    return data ? `${data.toLocaleString()} đ` : ''; // Phí đóng gỗ
                }
            },
            {
                targets: 13,
                data: 'insuranceFee',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? `${data.toLocaleString()} đ` : ''; // Phí đóng gỗ
                }
            },
            {
                targets: 14,
                data: 'totalPrice',
                sortable: false,
                className: 'text-right',
                render: (data, type, row, meta) => {
                    return data ? `${data.toLocaleString()} đ` : ''; // Phí đóng gỗ
                }
            },
            {
                targets: 15,
                data: 'shippingLineId',
                sortable: false,
                render: (data, type, row, meta) => {

                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const line = shippingLines[row.shippingLineId];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${line ? line.color : 'black'};">${line ? line.text : ''}</strong>`;

                }
            },
            {
                targets: 16,
                data: 'note',
                sortable: false,

            },
            {
                targets: 17,
                data: 'packageNumber',
                sortable: false,
                render: (data, type, row, meta) => {
                    if (isAdminOrSaleAdmin) {
                        return `<a href="/Packages/Finance/${row.id}">${row.packageNumber}</a>`;
                    }
                    else {
                        return `${row.packageNumber}`;
                    }
                }
            }
        ]
    });
    $('.btn-search').on('click', (e) => {
        packagesTable.ajax.reload();
        return false;
    });
})(jQuery);
