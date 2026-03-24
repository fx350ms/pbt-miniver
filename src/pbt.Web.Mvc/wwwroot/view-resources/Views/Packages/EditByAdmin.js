(function ($) {
    const _packageService = abp.services.app.package;
    var _shippingPartnerServices = abp.services.app.shippingPartner;
    _$form = $('#form-package'),
        _$saveButtons = $('.save-package');
    // Localization source
    const l = abp.localization.getSource('pbt');

    var autoNumericFields = {};

    $(document).find('.mask-number').each(function (c) {
    
        var fieldName = $(this).attr('name');
        var decimalPlaces = $(this).attr('decimal-places') || 0;
        if(fieldName)
        {
            autoNumericFields[fieldName] = new AutoNumeric(this, {
                currencySymbol: '',
                decimalCharacter: '.',
                digitGroupSeparator: ',',
                decimalPlaces: decimalPlaces,
                minimumValue: '0'
            });
        }
    });


    $(document).on('change', "#select-delivery-line", function () {
        const deliveryLine = $("#select-delivery-line").val();
        if (!!deliveryLine && deliveryLine != 2) {
          
            $("#lb-product-group-type").removeClass("d-none")
            $("#product-group-type").removeClass("d-none")
            // $("[name='ProductNameCn']").removeClass("red-placeholder").attr("placeholder", l('ProductNameCn')) ;
            // $("[name='Quantity']").removeClass("red-placeholder").attr("placeholder", l('Quantity'));
            // $("[name='Price']").removeClass("red-placeholder").attr("placeholder", l('Price'));
           
        }
        if (deliveryLine == 2) {
            $("#lb-product-group-type").addClass("d-none");
            $("#product-group-type").removeClass("d-none")
            // $("[name='ProductNameCn']").addClass("red-placeholder").attr("placeholder", (l('ProductNameCn') + " *"))
            // $("[name='Quantity']").addClass("red-placeholder").attr("placeholder", (l('Quantity') + " *"))
            // $("[name='Price']").addClass("red-placeholder").attr("placeholder", (l('Price') + " *"))
        }
    });

    const formatSelectDate = "DD/MM/YYYY HH:mm:ss";
    
    $('.date-select').daterangepicker({
        // startDate: moment().subtract(6, 'days').startOf('day'),
        "locale": {
            "format": formatSelectDate,
            "separator": " - ",
            "applyLabel": l('Apply'),
            "cancelLabel": l('Cancel'),
            "fromLabel": l('From'),
            "toLabel": l('To'),
            "customRangeLabel": l('Select'),
            "weekLabel": "W",
        },
        singleDatePicker: true,
        timePickerSeconds: true, // Cho phép chọn giây
        timePicker24Hour: true,
        autoUpdateInput: false,
        "cancelClass": "btn-danger",
        }
    );

    $('.date-select').on('apply.daterangepicker', function (ev, picker) {
        // Manually update the input value with the selected dates
        $(this).val(picker.startDate.format(formatSelectDate));
    });

    // Event handler for when the "Cancel" (or "Clear") button is clicked
    $('.date-select').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
    });

      
    $('#selectAllCustomer').select2({
        theme: "bootstrap4"
    });

    $('[name=rdCodeType]').on('change', function () {
        var selectedValue = $('input[name="rdCodeType"]:checked').val();
        _$hiddenCodeType.val(selectedValue);
    });

    $('[name=actionScan]').on('change', function () {
        var selectedValue = $('input[name="actionScan"]:checked').val();
        $('[name=Action]').val(selectedValue);
    });

    $(".un-match-package").on("click", function (e) {
        var packageId = $("[name='Id']").val();
        _packageService.unMatchOrder(packageId).done(function (result) {
            abp.notify.info(l('UnMatchSuccessfully'));
            // reload
            setTimeout(function () {
                window.location.reload();
            }, 2000)
        }).fail(function (error) {
            abp.notify.error(error);
        });
    })

    $('#packing').on('change', function () {
        $('#wooden-crate-group').toggle($(this).is(':checked'));
    }).trigger('change');

    $('input[name="WoodenCrateType"]').on('change', function () {
        if ($(this).val() == '2' && $(this).is(':checked')) {
            $('#sharedCrateSelectContainer').removeClass("d-none");
        } else if ($(this).val() == '1' && $(this).is(':checked')) {
            $('#sharedCrateSelectContainer').addClass('d-none');
            $('#sharedCrateSelect').val(null);
        } else {
            $('#sharedCrateSelectContainer').addClass("d-none");
        }
    }).filter(':checked').trigger('change');

    function savePackage(redirectTo) {

        if (!_$form.valid()) {
            return;
        }

        var packageData = _$form.serializeFormToObject();

        // numeric → int
        packageData.Length = packageData.Length ? parseInt(packageData.Length) : 0;
        packageData.Width = packageData.Width ? parseInt(packageData.Width) : 0;
        packageData.Height = packageData.Height ? parseInt(packageData.Height) : 0;
        packageData.Quantity = packageData.Quantity ? parseInt(packageData.Quantity) : 0;
        packageData.PriceCN = packageData.PriceCN ? parseFloat(packageData.PriceCN) : 0;
        packageData.CategoryId = packageData.CategoryId ? parseInt(packageData.CategoryId) : 0;
        packageData.ProductGroupTypeId = packageData.ProductGroupTypeId ? parseInt(packageData.ProductGroupTypeId) : 0;

        // boolean
        packageData.IsDomesticShipping = $('[name="IsDomesticShipping"]').prop('checked');
        packageData.IsShockproof = $('[name="IsShockproof"]').prop('checked');
        packageData.IsWoodenCrate = $('[name="IsWoodenCrate"]').prop('checked');
        packageData.IsInsured = $('[name="IsInsured"]').prop('checked');

        // decimal
        packageData.Weight = packageData.Weight ? parseFloat(packageData.Weight.replaceAll(",", ".")) : 0;
        packageData.DomesticShippingFee = packageData.DomesticShippingFee ? parseFloat(packageData.DomesticShippingFee) : 0;
        packageData.InsuranceValue = packageData.InsuranceValue ? parseFloat(packageData.InsuranceValue) : 0;

        // timeline fields
        packageData.ExportDateCN = packageData.ExportDateCN || null;
        packageData.ImportDateVN = packageData.ImportDateVN || null;
        packageData.ExportDateVN = packageData.ExportDateVN || null;

        // select fields
        packageData.ShippingLineId = parseInt(packageData.ShippingLineId);
        packageData.WarehouseStatus = parseInt(packageData.WarehouseStatus);
        packageData.ShippingStatus = parseInt(packageData.ShippingStatus);

        abp.ui.setBusy(_$form);

        _packageService.editPackageByAdmin(packageData).done(function (rs) {
            debugger;
            if (rs.success) {
                abp.notify.success("Cập nhật kiện hàng thành công");
                if (redirectTo) {
                    window.location.href = redirectTo;
                }
            } else {
                abp.notify.error(rs.message || "Có lỗi xảy ra");
            }
        }).always(function () {
            abp.ui.clearBusy(_$form);
        });
    }


    $('.btn-clear-value').on('click', function () {
        var target = $(this).attr('target');
        var targetInput = $(this).attr('target-date');
        if (target) {
            var targetValue = $(this).attr('target-value');
            if (targetValue) {
                $('[name="' + target + '"]').val(targetValue);
            }
            else {
                $('[name="' + target + '"]').val('');
                $('.' + targetInput).val('');
            }
        }
    });

    // Handle save button clicks
    _$saveButtons.on('click', function (e) {
        e.preventDefault();
        var redirectTo = $(this).data('redirect');
        savePackage(redirectTo);
    });

    // getShippingPartner();
    // loadCustomer();

    $(document).on("change", ["input", "select"], function (e) {
        $(e.target).removeClass("is-invalid");
    });


    $("[name='ProductNameCn']").on("change", function (el) {
        const cnName = $(this).val();
        if (cnName) {
            abp.services.app.dictionary.getDictionaryByCnName(cnName).done(function (data) {
                if (data)
                    $(el.target).closest("tr").find("[name='ProductNameVi']").val(data);
            })
        }
    })

    // on selectAllCustomer selected
    $(document).on("change", "#selectAllCustomer,#select-delivery-line", function () {
        loadBag();
    });

    
    function loadBag() {
        //
        if ($('#hidden-bag-id').val() == '') {
            $('.select-bag-group').removeClass('d-none'); // Hiển thị select để chọn bao mới
            $('.select-new-bag').select2({
                theme: 'bootstrap4',
                ajax: {
                    delay: 550, // wait 250 milliseconds before triggering the request
                    url: abp.appPath + 'api/services/app/Bag/getSelectableBagsForPackage',
                    type: "GET",
                    data: function (params) {
                        return {
                            q: params.term, // search term
                            shippingLine: $('#select-delivery-line').val(),
                            customerId: $('#selectAllCustomer').val(),
                        };
                    },
                    processResults: function (data) {
                        return {
                            results: data.result
                        };
                    },
                    dataType: 'json',
                }
            });
        }
    }


    $(document).on('click', '.unbag-package', function () {

        var packageId = $(this).data('package-id');

        // Gọi API để bỏ kiện hàng khỏi bao
        abp.ui.setBusy();
        abp.services.app.package.unBag(packageId).done(function () {
            abp.notify.info('Đã bỏ kiện hàng khỏi bao.');
            $('#hidden-bag-id').val(''); // Xóa giá trị bao hiện tại
            $('.bag-info').remove(); // Xóa nút "Bỏ khỏi bao"
            // Hiển thị select để chọn bao mới
          //  $('.select-bag-group').removeClass('d-none'); // Hiển thị select
            loadBag(); // Gọi hàm loadBag để tải danh sách bao mới
        }).fail(function (error) {
            abp.notify.error('Không thể bỏ kiện hàng khỏi bao.');
            console.error(error);
        }).always(function () {
            abp.ui.clearBusy();
        });
    });
      

    $("[name='DomesticShippingFee']").on("change", function () {
       
        var value = $(this).val();
        var rate = $("[name='RMBRate']").val() * 1;
        var priceVn = value * rate;
        $(this).siblings().find(".price-vnd").text(formatCurrency(priceVn));
    }).trigger("change")

    $("#local-ship").on("change", function () {

        const isDomesticShipping = $(this).is(":checked");
        const textBox = $(this).parent(".form-check").find(".input-price");
        if (isDomesticShipping) {
            textBox.removeClass("d-none");

        } else {
            textBox.addClass("d-none");
            textBox.val(0);
        }
    }).trigger("change");

    $("#table-new-package #check-all").on("click", function () {

        if ($(this).is(":checked")) {
            $("#table-new-package input[name='selectPackage']").prop("checked", true);
        } else {
            $("#table-new-package input[name='selectPackage']").prop("checked", false);
        }
    })

    $("#quickCreateBag").on("click", function () {
        const listBagId = $("#table-new-package tbody input:checked").map(function () {
            return $(this).val();
        }).get();
        if (listBagId.length > 0) {
            const param = listBagId.join(",")
            window.location.href = "/Bags/Create?packagesId=" + param;
        }
    })

    // tìm kiếm khi nhập input searchWaybill
    let searchTimeout;
    $(document).on("keyup", "#searchWaybill", function () {
        const value = $(this).val();
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            getListWaybill(value);
        }, 300); // Adjust the delay as needed
    });

    $("[name='IsInsured']").on("change", function () {
        const isDomesticShipping = $(this).is(":checked");
        const textBox = $(this).parent(".form-check").find("input").last();
        if (isDomesticShipping) {
            textBox.removeClass("d-none");
        } else {
            textBox.addClass("d-none");
        }
    });

    $(document).on("change", "[name='TrackingNumber']", function (el) {
        const value = $(this).val();
        const tr = $(this).closest("tr");
        abp.ui.setBusy();
        _packageService.getByWaybillNumber(value).done(function (response) {
            if (response) {
                tr.find("[name='IsInsured']").prop("checked", response.insurance > 0);
                if (response.insurance > 0) {
                    tr.find("[name='InsuranceValue']").removeClass("d-none");
                    tr.find("[name='InsuranceValue']").val(response.insurance);
                }
                tr.find("[name='IsWoodenCrate']").prop("checked", response.useWoodenPackaging);
                tr.find("[name='IsShockproof']").prop("checked", response.useShockproofPackaging);
                tr.find("[name='IsDomesticShipping']").prop("checked", response.useDomesticTransportation);
                if (response.useDomesticTransportation) {
                    tr.find("[name='DomesticShippingFee']").removeClass("d-none");
                }
            } else {
                // thông báo lỗi.
                PlaySound('warning');
                abp.notify.error("Không tìm thấy đơn hàng");
            }
        }).always(function () {
            abp.ui.clearBusy();
        });
    })

    $('input[type="radio"][name="AddressId"]').change(function () {
        var target = $(this).data('target');
        $('[data-type="address-id"]').each(function () {
            $(this).CardWidget('collapse');
        });

        $('div#' + target + '[data-type="address-id"]').CardWidget('expand');

        var value = $("input[name='AddressId']:checked").val();
        $('#AddressId').val(value);
    });
     
    $("#select-delivery-line").trigger("change").trigger("click")

})(jQuery);
 