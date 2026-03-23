(function ($) {
    var _configurationSettingService = abp.services.app.configurationSetting,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#ConfigurationSettingCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ConfigurationSettingTable');

    var _$configurationSettingsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _configurationSettingService.getAll,
            inputFilter: function () {
                return $('#ConfigurationSettingSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$configurationSettingsTable.draw(false)
            }
        ],
        columnDefs: [
            {
                targets: 0,
                className: 'control',
                defaultContent: '',
            },
            {
                targets: 1,
                data: 'key',
                sortable: false,
                render: (data, type, row, meta) => {
                    return [
                        `<a type="button" class=" edit-configuration-setting" data-configuration-setting-id="${row.id}" data-toggle="modal" data-target="#ConfigurationSettingEditModal" title="${l('Edit')}">`,
                        data,
                        `</a>`].join('');
                }
            },
            {
                targets: 2,
                data: 'value',
                sortable: false
            },
            {
                targets: 3,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        ` <div class="btn-group"> `,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,
                        `       <a type="button" class="dropdown-item bg-secondary edit-configuration-setting" data-configuration-setting-id="${row.id}" data-toggle="modal" data-target="#ConfigurationSettingEditModal" title="${l('Edit')}">`,
                        `           <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        `       </a>`,
                        `       <a type="button" class="dropdown-item bg-danger delete-configuration-setting" data-configuration-setting-id="${row.id}" data-configuration-setting-key="${row.key}" title="${l('Delete')}">`,
                        `           <i class="fas fa-trash"></i> ${l('Delete')}`,
                        `       </a>`,
                        `   </div>`,
                        ` </div>`
                    ].join('');
                }
            }
        ]
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }
        var configurationSetting = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _configurationSettingService.createWithCheckExist(configurationSetting).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));

            _$configurationSettingsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-configuration-setting', function () {
        var configurationSettingId = $(this).attr("data-configuration-setting-id");
        var configurationSettingKey = $(this).attr('data-configuration-setting-key');

        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                configurationSettingKey),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _configurationSettingService.delete({
                        id: configurationSettingId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$configurationSettingsTable.ajax.reload();
                    });
                }
            }
        );
    });

    $(document).on('click', '.edit-configuration-setting', function (e) {
        var configurationSettingId = $(this).attr("data-configuration-setting-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'ConfigurationSetting/EditModal?Id=' + configurationSettingId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ConfigurationSettingEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$configurationSettingsTable.ajax.reload();
    });

    $('#SearchKey').on('keypress', (e) => {
        if (e.which == 13) {
            _$configurationSettingsTable.ajax.reload();
            return false;
        }
    });

    abp.event.on('configurationSetting.edited', (data) => {
        _$configurationSettingsTable.ajax.reload();
    });

})(jQuery);