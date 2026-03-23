(function ($) {
    var _complaintService = abp.services.app.complaint,
        l = abp.localization.getSource('pbt'),
        _$form = $('#complaintCreateForm');
    $('[name=OrderId]').on('change', function () {
        var selectedOrderId = $(this).val(); // Lấy giá trị của OrderId
        var selectedOrderNumber = $(this).find("option:selected").text(); // Lấy tên (OrderNumber) từ option được chọn

        // Kiểm tra nếu không chọn OrderId nào (giá trị rỗng)
        if (!selectedOrderId) {
            $('[name=OrderCode]').val(''); // Đặt OrderCode về rỗng
        } else {
            $('[name=OrderCode]').val(selectedOrderNumber); // Gán giá trị OrderNumber vào OrderCode
        }
    });
    $('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
         

        // Tạo FormData để gửi dữ liệu và tệp
        var formData = new FormData();
        var files = $("input[name='Images']")[0].files;

        // Thêm tệp vào FormData
        for (var i = 0; i < files.length; i++) {
            formData.append('Images', files[i]);
        }

        // Lấy các dữ liệu từ form và thêm vào FormData
        var complaint = _$form.serializeFormToObject();
        for (var key in complaint) {
            formData.append(key, complaint[key]);
        }

        $.ajax({
            url: abp.appPath + 'api/services/app/complaint/CreateWithImages',
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function () {
                PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
                // Reset form hoặc đóng modal
                window.location.href = '/Complaint';
            },
            error: function () {
                PlaySound('warning'); abp.notify.error(l('SaveFailed'));
            },
            complete: function () {
                abp.ui.clearBusy(_$form);
            }
        });
    });


    $('.select2').select2({
    }).addClass('form-control');
})(jQuery);
