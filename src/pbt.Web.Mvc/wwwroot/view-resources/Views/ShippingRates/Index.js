(function ($) {
    var _shippingRateGroupService = abp.services.app.shippingRateGroup,
        _shippingRateCustomerService = abp.services.app.shippingRateCustomer,
        _shippingRateService = abp.services.app.shippingRate,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#ShippingRateCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ShippingRatesTable');
    var _$shippingRatesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _shippingRateGroupService.getData,
            inputFilter: function () {
                return $('#ShippingRateSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$shippingRatesTable.draw(false)
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
                width: 40,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 1,
                width: 80,
                data: 'id'
            },
            {
                targets: 2,
                data: 'name',
                sortable: false,
                render: function (data, type, row, meta) {
                    //if (data) {
                    //    return '<strong>' + data +'</strong> <span class="badge badge-success">Mặc định</span>';
                    //}
                    return '<strong>' + data + '</strong>';
                }
            },
            {
                targets: 3,
                data: 'isActived',
                width: 180,
                sortable: false,
                render: function (data, type, row, meta) {
                    return data ? '<span class="badge badge-success">Có</span >' : '<span class="badge badge-danger"> Không</span >';
                }
            },
            {
                targets: 4,
                data: 'isDefaultForCustomer',
                width: 180,
                sortable: false,
                render: function (data, type, row, meta) {
                    return data ? '<span class="badge badge-success" > Mặc định</span >' : '';
                }
            },

            {
                targets: 5,
                data: 'note',
                sortable: false
            },
            {
                targets: 6,
                data: null,
                sortable: false,
                width: 20,
                render: (data, type, row, meta) => {

                    let actions = [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        ` </button>`,
                        ` <div class="dropdown-menu" style="">`,
                        `       <a type="button" class="dropdown-item  bg-primary  edit-shipping-rate"   data-toggle="modal" data-target="#ShippingRateEditModal" data-id="${row.id}" title="Điều chỉnh bảng giá">`,
                        `           <i class="fas fa-edit"></i> Điều chỉnh bảng giá`,
                        `       </a>`,
                        `       <a class="dropdown-item  bg-success   set-as-default"  type="button"  data-id="${row.id}" title="Chọn làm mặc định">`,
                        `           <i class="fas fa-star"></i> Chọn làm mặc định`,
                        `       </a>`,
                        `       <a class="dropdown-item assign-customers bg-default  "  type="button"   data-toggle="modal" data-target="#ShippingRateAssignCustomersModal" data-id="${row.id}"  title="Chọn khách hàng">`,
                        `           <i class="fas fa-users"></i> Chọn khách hàng`,
                        `       </a>`,

                        `       <a class="dropdown-item bg-danger btn-delete"  type="button"  data-id="${row.id}" data-text="${row.name}" title="Xóa">`,
                        `           <i class="fas fa-trash"></i> Xóa`,
                        `       </a>`,
                        `</div>`,
                        `</div>`
                    ];


                    return actions.join('');
                }
            }
        ]
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }
        var shippingRate = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _shippingRateGroupService.createWithResetDefault(shippingRate).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            _$shippingRatesTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.edit-shipping-rate', function (e) {

        var id = $(this).attr("data-id");
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'ShippingRate/ConfigureTiersModal?Id=' + id,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#ShippingRateEditModal div.modal-content').html(content);

                document.querySelectorAll("#ShippingRateEditModal .mask-num-price").forEach(input => {
                    input.addEventListener("blur", (e) => {
                        e.target.value = formatNumber(e.target.value);
                    });
                    input.addEventListener("focus", (e) => {
                        e.target.value = e.target.value.replace(/,/g, "");
                    });
                });
            },
            error: function (e) {

            }
        });
    });

    $('.btn-search').on('click', (e) => {
        _$shippingRatesTable.ajax.reload();
    });
    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$shippingRatesTable.ajax.reload();
            return false;
        }
    });

    $(document).on('click', '.select-all-customer', function () {
        // Kiểm tra nếu checkbox "select-all" được check hay chưa
        var isChecked = $(this).prop('checked');

        // Cập nhật trạng thái của tất cả các checkbox có data-check="customer"
        $('input.customer-checkbox').prop('checked', isChecked);

    });

    $(document).on('click', '.assign-customers', function (e) {
        var id = $(this).attr("data-id");
        $('#ShippingRateGroupId').val(id);
        e.preventDefault();
        abp.ui.setBusy();

        _shippingRateCustomerService.getCustomerIdsForShippingRate(id).done(function (result) {
            $('.select-customer-ids').val(result).trigger('change');
            abp.ui.clearBusy();
        });
        abp.ui.clearBusy();

    });

    $('.select-customer-ids').select2(
        {
            tags: true,
            closeOnSelect: false,
            tokenSeparators: [',']
        }
    );

 
    $(document).on('click', '.btn-delete', function (e) {

        var id = $(this).attr("data-id");
        var text = $(this).attr("data-text");
        deleteGroup(id, text);
    });

    function deleteGroup(id, text) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                text),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _shippingRateGroupService.delete({
                        id: id
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$shippingRatesTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.add-group', function (e) {

        // Lấy thông tin từ các dropdown
        var $lineDropdown = $('#select-line');
        var $fromWarehouseDropdown = $('#select-from-warehouse');
        var $toWarehouseDropdown = $('#select-to-warehouse');

        var shippingLine = $lineDropdown.val(); // Value của Line
        var shippingLineText = $lineDropdown.find('option:selected').text(); // Text của Line

        var warehouseFromId = $fromWarehouseDropdown.val(); // Value của Kho nguồn
        var warehouseFromName = $fromWarehouseDropdown.find('option:selected').text(); // Text của Kho nguồn

        var warehouseToId = $toWarehouseDropdown.val(); // Value của Kho đích
        var warehouseToName = $toWarehouseDropdown.find('option:selected').text(); // Text của Kho đích

        // Validate selections
        if (shippingLine === "-1") {
            abp.notify.error('Vui lòng chọn loại vận chuyển!');
            return;
        }
        if (warehouseFromId === "-1") {
            abp.notify.error('Vui lòng chọn kho nguồn!');
            return;
        }
        if (warehouseToId === "-1") {
            abp.notify.error('Vui lòng chọn kho đích!');
            return;
        }

        // Kiểm tra xem nhóm đã tồn tại chưa
        var isDuplicate = false;
        $('.rate-item').each(function () {
            var line = $(this).data('line');
            var from = $(this).data('from');
            var to = $(this).data('to');

            if (line == shippingLine && from == warehouseFromId && to == warehouseToId) {
                isDuplicate = true;
                return false; // Thoát khỏi vòng lặp
            }
        });

        if (isDuplicate) {
            abp.notify.error('Nhóm này đã tồn tại!');
            return;
        }

        // Gửi AJAX để lấy nội dung của nhóm mới
        abp.ajax({
            url: abp.appPath + 'ShippingRate/RateTierContentItem',
            type: 'GET',
            dataType: 'html',
            data: {
                fromId: warehouseFromId,
                fromName: warehouseFromName,
                toId: warehouseToId,
                toName: warehouseToName,
                line: shippingLine,
                lineName: shippingLineText
            },
            success: function (content) {
                // Thêm nội dung mới vào .rate-list
                $('.rate-list').append(content);
                document.querySelectorAll(".rate-list .mask-num-price").forEach(input => {
                    input.addEventListener("blur", (e) => {
                        e.target.value = formatNumber(e.target.value);
                    });
                    input.addEventListener("focus", (e) => {
                        e.target.value = e.target.value.replace(/,/g, "");
                    });
                });
                abp.notify.info('Nhóm đã được thêm thành công!');
            },
            error: function () {
                abp.notify.error('Có lỗi xảy ra khi thêm nhóm!');
            }
        });
    });

    $(document).on('click', '.add-tier-row', function (e) {
        e.preventDefault();

        // Get the current row
        var $currentRow = $(this).closest('tr');

        // Clone the current row starting from the second column
        var $newRow = $currentRow.clone();
        //  $newRow.find('td:first').remove(); // Remove the first column (td) from the cloned row

        // Change the last button in the new row to a "remove" button
        $newRow.find('.add-tier-row')
            .removeClass('add-tier-row btn-success')
            .addClass('remove-tier-row btn-danger')
            .attr('title', 'Xóa hàng')
            .html('<i class="fa fa-trash"></i>');
        $newRow.attr('data-id', 0);
        // Reset the values in the input fields of the new row
        // $newRow.find('input').val('');

        // Insert the new row immediately after the current row
        $currentRow.after($newRow);

        // Update the rowspan of the first column
        //var $firstColumn = $currentRow.find('td:first');
        //var currentRowspan = parseInt($firstColumn.attr('rowspan')) || 1;
        //$firstColumn.attr('rowspan', currentRowspan + 1);
    });

    $(document).on('click', '.remove-tier-row', function (e) {
        e.preventDefault();

        // Get the current row
        var $currentRow = $(this).closest('tr');

        // Get the row-data-id of the current row
        var rowDataId = $currentRow.attr('row-data-id');

        // Find the <td> element with the matching row-data-id
        var $firstColumn = $('td[row-data-id="' + rowDataId + '"]');

        // Remove the current row
        $currentRow.remove();
    });


    $(document).on('click', '.remove-tier-data', function (e) {
        e.preventDefault();

        // Get the current row
        var currentDataId = $(this).data('id');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                ''),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _shippingRateService.delete({
                        id: currentDataId
                    }).done(() => {

                        // Tìm đến thẻ div gần nhất có class là rate-item sau đó xóa đi
                        $(this).closest('.rate-item').remove();

                        abp.notify.info(l('SuccessfullyDeleted'));
                    });
                }
            }
        );

    });

    $(document).on('click', '.set-as-default', function (e) {
        e.preventDefault();

        // Get the current row
        var currentDataId = $(this).data('id');
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToSetAsDefault'),
                ''),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _shippingRateGroupService.setAsDefault(currentDataId).done(() => {
                        _$shippingRatesTable.ajax.reload();
                        abp.notify.info(l('SuccessfullyDeleted'));
                    });
                }
            }
        );

    });

    $(document).on('click', '.save-customer-button', function () {

        var shippingRateGroupId = $('#ShippingRateGroupId').val();
        var selectedCustomerIds = $('.select-customer-ids').val();
        if (selectedCustomerIds && selectedCustomerIds.length > 0) {
            abp.ui.setBusy();
            _shippingRateCustomerService.assignCustomersToShippingRate(shippingRateGroupId, selectedCustomerIds).done(function () {
                abp.notify.info(l("SavedSuccessfully"));
                abp.ui.clearBusy();
                $('#ShippingRateAssignCustomersModal').modal('hide');
            }).always(function () {
                abp.ui.clearBusy();
            });
        }
    });

    function formatNumber(val) {
        val = val.replace(/\D/g, "");
        return val.replace(/\B(?=(\d{3})+(?!\d))/g, ","); 
    }

})(jQuery);