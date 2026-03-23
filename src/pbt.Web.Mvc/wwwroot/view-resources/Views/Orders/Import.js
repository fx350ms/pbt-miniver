(function ($) {
    var _orderService = abp.services.app.order,
        l = abp.localization.getSource('pbt'),
        _$form = $('#ImportForm');

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        // Tạo FormData để gửi dữ liệu và tệp
        var formData = new FormData();
        var files = $("input[name='FileContent']")[0].files;

        if (files && files.length > 0) {
            formData.append('Attachments', files[0]);
        }

        abp.ui.setBusy(_$form);
        $.ajax({
            url: '/api/services/app/Order/Import',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                abp.notify.info(l('SavedSuccessfully'));
                if (response) {
                    // Hiển thị kết quả import
                    $('#import-result').show();
                    $('#total-orders').text(response.result.total);
                    $('#success-count').text(response.result.successCount);
                    $('#failed-count').text(response.result.failedCount);

                    if (response.result.failedCount > 0 && response.result.messages.length > 0) {
                        $('#error-messages').show();
                        var $errorList = $('#error-list');
                        $errorList.empty();
                        response.result.messages.forEach(function (message) {
                            $errorList.append('<li>' + message + '</li>');
                        });
                    } else {
                        $('#error-messages').hide();
                    }
                }
            },
            error: function (xhr) {
                abp.message.error(l('ErrorOccurred'));
            },
            complete: function () {
                abp.ui.clearBusy(_$form);
            }
        });
    });
})(jQuery);
