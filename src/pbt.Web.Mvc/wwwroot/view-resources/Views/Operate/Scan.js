(function ($) {
    var _barcode = abp.services.app.barCode,
        l = abp.localization.getSource('pbt'),
        _$form = $('#form-scan-code'),
        _$hiddenCodeType = $('#hiddenCodeType');

    $('[name=rdCodeType]').on('change', function () {
        var selectedValue = $('input[name="rdCodeType"]:checked').val();
        _$hiddenCodeType.val(selectedValue);
    });


    $('[name=actionScan]').on('change', function () {
        var selectedValue = $('input[name="actionScan"]:checked').val();
        $('[name=Action]').val(selectedValue);
    });

    $('#txtInputCodeScan').on('keypress', function (e) {
        console.log('keypress' +e.which);
        if (e.which === 13) { // Kiểm tra phím Enter
            e.preventDefault(); // Ngăn hành động mặc định
          
            var data = _$form.serializeFormToObject();
            _barcode.create(data).done(function () {
                _$form[0].reset();
                PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            }).always(function () {

            });
        }
    });
})(jQuery);
