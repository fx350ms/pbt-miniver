(function ($) {
    var _departmentService = abp.services.app.shippingPartner,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#DepartmentCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#DepartmentsTable');
   
    var _$departmentsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
       
        listAction: {
            ajaxFunction: _departmentService.getAll,
            inputFilter: function () {
                return $('#DepartmentSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$departmentsTable.draw(false)
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
                data: 'name',
                sortable: false
            },
            {
                targets: 2,
                data: 'code',
                sortable: false
            },
            {
                targets: 3,
                data: 'phone',
                sortable: false
            },
            {
                targets: 4,
                data: 'email',
                sortable: false
            },
            {
                targets: 5,
                data: 'address',
                sortable: false
            },
            {
                targets: 6,
                data: 'maxAmount',
                sortable: false
            },
            {
                targets: 7,
                data: 'maxWeigth',
                sortable: false
            },
            {
                targets: 8,
                data: null,
                sortable: false,
                width: 20,
                defaultContent: '',
                render: (data, type, row, meta) => {
                return [
                    `   <div class="btn-group">`,
                    `       <button type="button" class="btn btn-sm btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">`,
                    `       </button>`,
                    `       <div class="dropdown-menu">`,
                    `           <a class="dropdown-item edit-department" href="#" data-department-id="${row.id}" data-toggle="modal" data-target="#DepartmentEditModal">`,
                    `               <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                    `           </a>`,
                    `           <a class="dropdown-item delete-department" href="#" data-department-id="${row.id}" data-department-name="${row.name}">`,
                    `               <i class="fas fa-trash"></i> ${l('Delete')}`,
                    `           </a>`,
                    `       </div>`,
                    `   </div>`
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
        var department = _$form.serializeFormToObject();
        department.Page = department.Name.substring(0, 1);
        abp.ui.setBusy(_$modal);
        _departmentService.create(department).done(function () {
         
            _$modal.modal('hide');
            _$form[0].reset();
            abp.notify.info(l('SavedSuccessfully'));
            PlayAudio('success', function () {
                _$departmentsTable.ajax.reload();
            });

          
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-department', function () {
        var departmentId = $(this).attr("data-department-id");
        var departmentName = $(this).attr('data-department-name');

        deleteDepartments(departmentId, departmentName);
    });

    function deleteDepartments(departmentId, departmentName) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                departmentName),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _departmentService.delete({
                        id: departmentId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$departmentsTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-department', function (e) {
        var departmentId = $(this).attr("data-department-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'ShippingPartners/EditModal?Id=' + departmentId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                $('#DepartmentEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#DepartmentsCreateModal"]', (e) => {
        $('.nav-tabs a[href="#department-details"]').tab('show')
    });

    abp.event.on('department.edited', (data) => {
        _$departmentsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
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
})(jQuery);
