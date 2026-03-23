(function ($) {
    var _dictionaryService = abp.services.app.dictionary,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DictionaryCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#DictionaryTable');

    var _$dictionarysTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: _dictionaryService.getAll,
            inputFilter: function () {
                return $('#DictionarySearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$dictionarysTable.draw(false)
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
                data: 'nameVi',
                sortable: false
            },
            {
                targets: 2,
                data: 'nameCn',
                sortable: false
            },
            {
                targets: 3,
                data: 'description',
                sortable: false
            },
            {
                targets: 4, // Assuming the action column is at index 4
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `<div class="btn-group">`,
                        `   <button type="button" class="btn btn-default dropdown-toggle dropdown-icon" data-toggle="dropdown" aria-expanded="false">`,
                        `       <span class="sr-only">Toggle Dropdown</span>`,
                        `   </button>`,
                        `   <div class="dropdown-menu">`,
                        `       <a class="dropdown-item edit-dictionary" data-dictionary-id="${row.id}" data-toggle="modal" data-target="#DictionaryEditModal">`,
                        `           <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        `       </a>`,
                        `       <a class="dropdown-item delete-dictionary" data-dictionary-id="${row.id}" data-dictionary-name="${row.nameVi}">`,
                        `           <i class="fas fa-trash"></i> ${l('Delete')}`,
                        `       </a>`,
                        `   </div>`,
                        `</div>`
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
        var dictionary = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _dictionaryService.create(dictionary).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$dictionarysTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-dictionary', function () {
        var dictionaryId = $(this).attr("data-dictionary-id");
        var dictionaryName = $(this).attr('data-dictionary-name');

        deleteDictionary(dictionaryId, dictionaryName);
    });

    function deleteDictionary(dictionaryId, dictionaryName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                dictionaryName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _dictionaryService.delete({
                        id: dictionaryId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$dictionarysTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-dictionary', function (e) {
        var dictionaryId = $(this).attr("data-dictionary-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Dictionary/EditModal?Id=' + dictionaryId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#DictionaryEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#DictionaryCreateModal"]', (e) => {
        $('.nav-tabs a[href="#dictionary-details"]').tab('show')
    });

    abp.event.on('dictionary.edited', (data) => {
        _$dictionarysTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$dictionarysTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$dictionarysTable.ajax.reload();
            return false;
        }
    });
})(jQuery);