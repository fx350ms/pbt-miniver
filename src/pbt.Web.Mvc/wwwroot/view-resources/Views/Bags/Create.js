(function ($) {
    var _bagService = abp.services.app.bag;
    var _shippingPartnerServices = abp.services.app.shippingPartner;

    l = abp.localization.getSource('pbt');
    _$form = $("#create-bag-form");
    $('.select2').select2({
        theme: "bootstrap4"
    });

    _$form.find('.btnSave').on('click', (e) => {
        const actionClose = e.currentTarget.attributes["data-close"].value;
        const close = $(e.target).attr('data-close') == "true";
        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }
        const data = _$form.serializeFormToObject();
        
        // Use vanilla JavaScript to improve performance and reduce dependency on jQuery
        const selectedPackageInputs = document.querySelectorAll("input.selected-package");
        data["selectedPackages"] = Array.from(selectedPackageInputs).map(input => {
            const value = input.value.trim(); // Trim whitespace to ensure clean values
            if (!value) {
                console.warn("Empty value detected for selected-package input:", input);
                return null; // Return null or handle empty values as needed
            }
            return value;
        }).filter(value => value !== null); // Remove any null values from the array
        
        // Handle the case where no selected packages exist
        if (data["selectedPackages"].length === 0) {
            console.warn("No selected packages found.");
        }
        abp.ui.setBusy();
        _bagService.createNew(data).done(function (value) {
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));   
            if (actionClose === "false") {
                window.location.href = "/Bags"
            } else {
                window.location.href = `/Bags/Bagging/${value.id}`
            }
        }).fail((error) => {
            if (error) {
                const validationErrors = error.validationErrors;
                if (validationErrors && Array.isArray(validationErrors)) {
                    validationErrors.forEach(function (e) {
                        $(`[id='${e.members[0]}']`).addClass("is-invalid");
                    })
                    //let errorMessage = validationErrors.map(e => `${e.propertyName}: ${e.errorMessage}`).join('');
                    //abp.message.error(errorMessage, l('ValidationError'));
                } else {
                    abp.message.error(error.message, l('Error'));
                }
            } else {
                abp.message.error(l('SaveFailed'), l('Error'));
            }
        })
            .always(function () {
                abp.ui.clearBusy();
            });
    });

    function getShippingPartner() {
        _shippingPartnerServices.getAllShippingPartnersByLocation(0).done(function (response) {
            if (response) {
                var selectShippingPartner = $("#shippingPartnerId");
                selectShippingPartner.append('<option value="">' + l('SelectShippingPartner') + '</option>');
                response.forEach(function (partner) {
                    selectShippingPartner.append(
                        `<option value="${partner.id}"> ${partner.name}</option>`
                    );
                });
            }
        })
    }

    async function loadWarehouse() {
        return await abp.services.app.warehouse.getFull().done(function (data) {
            const warehouseCreate = $("#warehouseCreateId");
            const warehouseDestination = $("#warehouseDestinationId");
            warehouseCreate.empty();
            warehouseDestination.empty();
            warehouseCreate.append('<option value="">' + l('SelectWarehouse') + '</option>');
            warehouseDestination.append('<option value="">' + l('SelectWarehouse') + '</option>');
            $.each(data, function (index, warehouse) {
                debugger;
                if(warehouse.countryId === 1){
                    const value = warehouseCreate.attr('data-value');
                    const selected = value == warehouse.id ? "selected" : "";
                    warehouseCreate.append('<option '+selected+' value="' + warehouse.id + '">' + warehouse.name + '</option>');
                }
                if (warehouse.countryId === 2) {
                    /// Viet nam
                    const value = warehouseDestination.attr('data-value');
                    const selected = value == warehouse.id ? "selected" : "";
                    warehouseDestination.append('<option '+selected+' value="' + warehouse.id + '">' + warehouse.name + '</option>');
                }
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load Tracking numer: " + error.message);
        });
    }
   
    async function loadCustomer() {
        return await abp.services.app.customer.getFull().done(function (data) {
            const customerSelect = $("select[name='CustomerId']");
            customerSelect.empty();
            customerSelect.append('<option value="">' + l('SelectCustomer') + '</option>');
            
            const defaultCustomerId = $("[name='BagType']:checked").val() == 2 ? '' : $("#defaultCustomerId").val();
            $.each(data, function (index, customer) {
                const selected = defaultCustomerId == customer.id ? "selected" : "";
                customerSelect.append('<option ' + selected + ' value="' + customer.id + '">' + customer.username + '</option>');
            });
        }).fail(function (error) {
            PlaySound('warning'); abp.notify.error("Failed to load customer: " + error.message);
        });
    }
    
    function checkCustomerSelectStatus() {
        // Cache the DOM elements for better performance
        const bagTypeChecked = $("[name='BagType']:checked");
        const customerIdSelect = $("select[name='CustomerId']");
    
        // Get the value of the checked radio button, defaulting to null if none is checked
        const customerSelected = bagTypeChecked.val() || null;
    
        // Validate and handle the logic based on the selected value
        if (customerSelected === "2") { // Ensure we compare strings since .val() returns a string
            $("select[name='CustomerId']").attr("disabled", "disabled").val(null).trigger('change').closest('.form-group').addClass("d-none");
        } else {
            loadCustomer().then(r => {
                $("select[name='CustomerId']").removeAttr("disabled").closest('.form-group').removeClass("d-none");
            });        }
    }
    
    $(".bagType").on("change", function (e) {
        if (e.target.value === "1") {
            loadCustomer().then(r => {
                $("select[name='CustomerId']").removeAttr("disabled").closest('.form-group').removeClass("d-none");
            });
        } else {
            $("select[name='CustomerId']").attr("disabled", "disabled").val(null).trigger('change').closest('.form-group').addClass("d-none");
        }
    });

    $('.btn-search').on('click', (e) => {
        _$departmentsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$departmentsTable.ajax.reload();
            return false;
        }
    });

    $("#table-packages tbody tr").on("click", function () {
        $("#table-packages tbody tr").removeClass("active")
        $(this).addClass("active")
    })
    
    $("#isOtherFeature").on("change", function () {
        const isChecked = $(this).is(":checked");
        if (isChecked) {
            $("[name='otherReason']").removeClass("d-none")
        } else {
            $("[name='otherReason']").addClass("d-none")
        }
    })
    loadWarehouse();
    getShippingPartner();
    loadCustomer();
    checkCustomerSelectStatus()
})(jQuery);
