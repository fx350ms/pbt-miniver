(function ($) {
    var _CustomerFakeService = abp.services.app.customerFake,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#CustomerFakeCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#CustomerFakesTable');
   
    var _$CustomerFakesTable = _$table.DataTable({
        paging: true,
        serverSide: true,
         select : true,
        listAction: {
            ajaxFunction: _CustomerFakeService.getAll,
            inputFilter: function () {
                return $('#CustomerFakeSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$CustomerFakesTable.draw(false)
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
                data: 'fullName',
                sortable: false
            },
            {
                targets: 2,
                data: 'address',
                sortable: false
            },
         
            {
                targets: 3,
                data: null,
                sortable: false,
                width: 300,
                defaultContent: '',
                render: (data, type, row, meta) => {

                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-CustomerFake" data-CustomerFake-id="${row.id}" data-toggle="modal" data-target="#CustomerFakeEditModal">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </button>',
                      
                        
                        `   <button type="button" class="btn btn-sm bg-danger delete-CustomerFake" data-CustomerFake-id="${row.id}" data-CustomerFake-name="${row.name}">`,
                        `       <i class="fas fa-trash"></i> ${l('Delete')}`,
                        '   </button>'
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
        var CustomerFake = _$form.serializeFormToObject();
     
        abp.ui.setBusy(_$modal);
        _CustomerFakeService.create(CustomerFake).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$CustomerFakesTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-CustomerFake', function () {
        var CustomerFakeId = $(this).attr("data-CustomerFake-id");
        var CustomerFakeName = $(this).attr('data-CustomerFake-name');

        deleteCustomerFakes(CustomerFakeId, CustomerFakeName);
    });

    function deleteCustomerFakes(CustomerFakeId, CustomerFakeName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                CustomerFakeName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _CustomerFakeService.delete({
                        id: CustomerFakeId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$CustomerFakesTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-CustomerFake', function (e) {
        var CustomerFakeId = $(this).attr("data-CustomerFake-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'CustomerFakes/EditModal?Id=' + CustomerFakeId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#CustomerFakeEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#CustomerFakesCreateModal"]', (e) => {
        $('.nav-tabs a[href="#CustomerFake-details"]').tab('show')
    });

    abp.event.on('CustomerFake.edited', (data) => {
        _$CustomerFakesTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$CustomerFakesTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$CustomerFakesTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
