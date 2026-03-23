(function ($) {
    const _deliveryRequestServices = abp.services.app.deliveryRequest;
    l = abp.localization.getSource('pbt');
    _$table = $('#PackageTable');
    
    //var _$departmentsTable = _$table.DataTable({
    //    paging: true,
    //    serverSide: true,
    //    listAction: {
    //        ajaxFunction: _deliveryRequestServices.getPackages,
    //        inputFilter: function () {
    //            return $('#PackagesListForm').serializeFormToObject(true);
    //        }
    //    },
    //    buttons: [
    //        {
    //            name: 'refresh',
    //            text: '<i class="fas fa-redo-alt"></i>',
    //            action: () => _$departmentsTable.draw(false)
    //        }
    //    ],
    //    responsive: {
    //        details: {
    //            type: 'column'
    //        }
    //    },
    //    columnDefs: [
    //        {
    //            targets: 0,
    //            data: null,
    //            render: function (data, type, row, meta) {
    //                return meta.row + meta.settings._iDisplayStart + 1;
    //            }
    //        },
    //        //{
    //        //    targets: 1,
    //        //    data: null,
    //        //    sortable: false,
    //        //    defaultContent: '',
    //        //    render: (data, type, row, meta) => {
    //        //        return [
    //        //            `<div class="form-check form-check-inline">
    //        //                <input type="checkbox" class="form-check-input selected-order" value="${row.id}">
    //        //            </div>`
    //        //        ].join('');
    //        //    }
    //        //},
    //        {
    //            targets: 1,
    //            data: 'orderId',
    //            sortable: false
    //        },
    //        {
    //            targets: 2,
    //            data: 'packageNumber',
    //            sortable: false
    //        },
    //        {
    //            targets: 3,
    //            data: 'shippingStatusName',
    //            sortable: false
    //        },
    //        {
    //            targets: 4,
    //            data: 'weight',
    //            sortable: false
    //        },
    //        {
    //            targets: 5,
    //            data: 'volume',
    //            sortable: false
    //        },   
    //        {
    //            targets: 6,
    //            data: 'creationTimeFormat',
    //            sortable: false
    //        }
    //    ]
    //});

    async function loadCustomer() {
        return await abp.services.app.customer.getFull().done(function (data) {
            const customerSelect = $("select[name='CustomerId']");
            customerSelect.empty();
            customerSelect.append('<option value="">' + l('SelectCustomer') + '</option>');
            $.each(data, function (index, customer) {
                customerSelect.append('<option value="' + customer.id + '">' + customer.username + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load customer: " + error.message);
        });
    }

    $(document).on("click", "#createDeliveryRequest", function () {
        AddNewDeliveryRequest();
    })
    function AddNewDeliveryRequest() {
        var selectedOrder = [];
        $(".selected-order").each((i, element) => {
            if($(element).is(":checked")){
                selectedOrder.push($(element).val());
            }
        });
        if(selectedOrder.length == 0) {
            abp.notify.info(l('vui lòng chọn đơn hàng'));
            return;
        }
        if($("#select-customer-address").val() == -1) {
            abp.notify.info(l('vui lòng chọn địa chỉ giao hàng'));
            return;
        }
        const data = {
            orders: selectedOrder,
            shippingMethod: $("#ShippingMethod").val(),
            customerId: $("#CustomerId").val(),
            note: $("#Node").val(),
        };
        _deliveryRequestServices.createDeliveryRequest(data).done(function (response) {
            abp.notify.success("Tạo yêu cầu giao thành công.");
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Tạo yêu cầu giao thất bại: " + error.message);
        });
    }
    
    $("#checkAll").click((e) => {
        $(".selected-order").prop("checked", $(e.target).is(":checked"));
    })

    $(document).on("click", ".selected-order",(e) => {
        let amount = 0;
        let depositAmount = 0;
        $(".selected-order").each((i, element) => {
            if($(element).is(":checked")){
                amount += parseInt($(element).closest("tr").find(".totalCost").attr("data-value"));
                depositAmount += parseInt($(element).closest("tr").find(".amountDue").attr("data-value"));
            }
        });

        $("#totalAmount").text(formatCurrency(amount));
        $("#totalDeposit").text(formatCurrency(depositAmount));
    })
    
    $("#cancelDeliveryRequest").click(() => {
        var id = $("[name='deliveryRequestId']").val();
        var code = $("#deliveryRequestCode").text();
        cancelDeliveryRequest(id, code);
    })

    function cancelDeliveryRequest(id, deliveryRequestCode) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToCancel'),
                deliveryRequestCode),
            "Hủy yêu cầu giao",
            (isConfirmed) => {
                if (isConfirmed) {
                    _deliveryRequestServices.cancelDeliveryRequest(id).done((result) => {
                        if (result && result.success) {
                            abp.notify.info(result.message || l('SuccessfullyDeleted'));
                            PlaySound('success');
                            window.location.href = "/DeliveryRequest";
                        } else {
                            abp.notify.error(result.message || l('FailedToDelete'));
                            PlaySound('warning');
                        }
                    });
                }
            }
        );
    }
    loadCustomer().then(r => {});
})(jQuery);

function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN', {style: 'currency', currency: 'VND'}).replace('VND', '₫');
}
function formatThousand(amount) {
    return amount.toLocaleString('vi-VN');
}
