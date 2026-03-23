(function ($) {
    var _barcode = abp.services.app.barCode,
        _operateServices = abp.services.app.operate,
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-scan-code'),
        _$hiddenCodeType = $('#hiddenCodeType'),
        _$table = $('#BarcodeTable'),
        _$tb_form = $('#CreateBarCodeSearchForm')


    const codeTypeDescriptions = {
        1: { text: l('Package'), color: 'blue' },
        2: { text: l('Bag'), color: 'purple' },
        4: { text: l('Waybill'), color: 'green' },
    };


    var _$scanCodeTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: function () {

                return _barcode.getAllOnCreateView(arguments[0]).done(function (value) {
                    $('#totalRecords').html(value.totalCount);
                });
            },

            inputFilter: function () {

                var data = {};
                data.onlyMyCode = $('#hiddenMyCode').val();
                return data;
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$scanCodeTable.draw(false)
            }
        ],
        lengthMenu: [25, 50, 100],
        language: {
            "info": l('Display') + " _START_ - _END_ | " + l('Total') + " _TOTAL_ " + l('Record'),
            "lengthMenu": l('Display') + " _MENU_ " + l('Record'),
            "emptyTable": l('EmptyTable'),
            "zeroRecords": l('ZeroRecords')
        },

        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                width: 10,
                render: function (data, type, row, meta) {
                    return `<input type="checkbox" data-check="row"  data-row-id="${row.id}"/>`;
                }
            },
            {
                targets: 1,
                data: null,
                width: 50,
                className: 'text-right',
                sortable: false,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 2,
                width: 180,
                data: 'scanCode',
                sortable: false
            },
            {
                targets: 3,
                width: 180,
                data: 'customerName',
                sortable: false
            },
            {
                targets: 4,
                data: 'content',

                sortable: false,
                render: function (data, type, row) {
                    return data ? '<p class="text-danger">' + data + '</p>' : ''; // Hiển thị nội dung hoặc thông báo nếu không có nội dung  
                }
            },
            {
                targets: 5,
                data: 'creationTime',
                sortable: false,
                width: 120,
                render: function (data, type, row) {
                    return formatDateToDDMMYYYYHHmm(data);
                }
            },
            {
                targets: 6,
                data: 'codeType',
                sortable: false,
                width: 120,
                render: (data, type, row, meta) => {

                    // Lấy mô tả và màu sắc của orderStatus từ đối tượng ánh xạ
                    const codeType = codeTypeDescriptions[data];

                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${codeType ? codeType.color : 'black'};">${codeType ? codeType.text : l('Unknown')}</strong>`;

                }
            },
            {
                targets: 7,
                data: 'action',
                width: 100,
                sortable: false,
                render: function (data, type, row) {
                    return data == 1 ? l('In') : l('Out');
                }
            },
            {
                targets: 8,
                width: 100,
                data: 'creatorUserName',
                sortable: false
            }

        ]
    });


    $(document).on('click', '[data-check=all]', function () {
        // Kiểm tra nếu checkbox "select-all" được check hay chưa
        var isChecked = $(this).prop('checked');

        // Cập nhật trạng thái của tất cả các checkbox có data-check="customer"
        $('input[data-check=row]').prop('checked', isChecked);
        $('.btn-delete').prop('disabled', !isChecked);
    });

    $(document).on('change', 'input[data-check=row]', function () {
        // Nếu có checkbox "customer" nào không được check, bỏ check "select-all"
        if ($('input[data-check=row]:not(:checked)').length > 0) {
            $('input[data-check=all]').prop('checked', false);
        } else {
            // Nếu tất cả checkbox "customer" đều được check, thì check "select-all"
            $('input[data-check=all]').prop('checked', true);
        }
        // Kiểm tra nếu có ít nhất một checkbox được check
        if ($('input[data-check=row]:checked').length > 0) {
            $('.btn-delete').prop('disabled', false);
        } else {
            $('.btn-delete').prop('disabled', true);
        }

    });


    $('#txtInputCodeScan').on('keypress', function (e) {
        if (e.which === 13) { // Kiểm tra phím Enter
            e.preventDefault(); // Ngăn hành động mặc định
            $('#txt-order-note').html('');
            const data = _$form.serializeFormToObject();
            _operateServices.updatePackageShippingStatus(data).done(function (result, data, xhr) {

                if (xhr.status !== 200) {
                    abp.notify.error(result.message || l('BadRequestError'));
                    PlaySound('warning');
                    return;
                }

                if (result && result.success) {
                    if (result.statusCode == 200) {
                        abp.notify.info(l('SavedSuccessfully'));
                        PlaySound('success');
                    }
                    else if (result.statusCode == 205) {

                        abp.notify.info(l('SavedSuccessfully'));
                        PlaySound('note');
                        $('#txt-order-note').html(result.message);
                    }
                    else {
                        abp.notify.error(result.message || l('BadRequestError'));
                        PlaySound('warning');
                        return;
                    }
                }

                else {
                    if (result && result.message) 
                    {
                        abp.notify.error(result.message || l('BadRequestError'));
                        PlaySound('warning');
                        return;
                    }
                    else {
                        abp.notify.error(l('BadRequestError'));
                        PlaySound('warning');
                        return;
                    }
                }

                $('#txtInputCodeScan').val('');
                _$scanCodeTable.ajax.reload();

            }).fail(function (jqXHR) {

                abp.notify.error(l('ErrorOccurred'));
                PlaySound('warning');
            });
            $('#txtInputCodeScan').val('');
        }
    });
    $('#OnlyCurrentUser').on('change', function () {

        // Kiểm tra trạng thái của checkbox
        var isChecked = $(this).is(':checked');

        // Gán giá trị true/false cho #hiddenMyCode
        $('#hiddenMyCode').val(isChecked);

        // Reload lại bảng dữ liệu với giá trị mới
        _$scanCodeTable.ajax.reload();
    });
    $('.btn-delete-all').on('click', function (e) {

        var dataRowIds = $('input[data-check=row]:checked').map(function () {
            return $(this).data('row-id');
        }).get();

        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDeleteAllOfYourCode'),
                ''),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _barcode.deleteAllByCurrentUser().done((data) => {

                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$scanCodeTable.ajax.reload();
                    });
                }
            }
        );
    });

    $('.btn-delete').on('click', function (e) {

        var dataRowIds = $('input[data-check=row]:checked').map(function () {
            return $(this).data('row-id');
        }).get();

        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                ''),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _barcode.deleteByIds(
                        dataRowIds
                    ).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$scanCodeTable.ajax.reload();
                    });
                }
            }
        );
    });
})(jQuery);
