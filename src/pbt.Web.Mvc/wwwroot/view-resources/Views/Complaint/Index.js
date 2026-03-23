(function ($) {
    var _complaintService = abp.services.app.complaint,
        l = abp.localization.getSource('pbt'),
        _$modal = $('#ComplaintCreateModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ComplaintsTable');


    const statusDescription = {
        1: { text: 'Chờ tiếp nhận', color: 'blue' },
        2: { text: 'Đang giải quyết', color: 'purple' },
        3: { text: 'Đã hoàn tiền', color: 'green' },
        4: { text: 'Từ chối', color: 'red' },

    };

    $('#input-date-range').daterangepicker(
        {
            locale: { cancelLabel: 'Clear' }
        }
    ).val('');
    var _$complaintsTable = _$table.DataTable({
        paging: true,
        serverSide: true,

        listAction: {
            ajaxFunction: _complaintService.getAll,
            inputFilter: function () {
                return $('#ComplaintSearchForm').serializeFormToObject(true);
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$complaintsTable.draw(false)
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
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                targets: 2,
                data: 'creationTimeString', 
            },
            {
                targets: 3,
                data: 'complaintCode',
                sortable: false,
                render: (data, type, row, meta) => {
                    return 'pbt-' + row.id;
                }
            },
            {
                targets: 4,
                data: 'orderCode',
                sortable: false
            },
            {
                targets:5,
                data: 'status',
                sortable: false,
                render: (data, type, row, meta) => {
                    const status = statusDescription[row.status];
                    // Trả về mô tả với màu sắc được áp dụng
                    return `<strong style="color: ${status ? status.color : 'black'};">${status ? status.text : 'Chưa xác định'}</strong>`;
                }
            },
            {
                targets: 6,
                data: 'resolution',
                sortable: false
            },
            {
                targets: 7,
                data: 'refundAmount',
                sortable: false
            },
            {
                targets: 8,
                data: 'reason',
                sortable: false
            },
            {
                targets: 9,
                data: 'refundAmountExpect',
                sortable: false,
                className : 'text-right'
            },
            {
                targets: 10,
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
                        `       <a class="dropdown-item view-complaint" data-complaint-id="${row.id}" data-toggle="modal" data-target="#ComplaintViewModal">`,
                        `           <i class="fas fa-eye"></i> ${l('View')}`,
                        `       </a>`,
                        `       <a class="dropdown-item edit-complaint" data-complaint-id="${row.id}" data-toggle="modal" data-target="#ComplaintEditModal">`,
                        `           <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        `       </a>`,
                        `       <a class="dropdown-item delete-complaint" data-complaint-id="${row.id}" data-complaint-name="${row.name}">`,
                        `           <i class="fas fa-trash"></i> ${l('Delete')}`,
                        `       </a>`,
                        `   </div>`,
                        `</div>`
                    ].join('');
                }
            }
        ]
    });

    _$modal.find('.save-button').on('click', (e) => {
        e.preventDefault();
        if (!_$form.valid()) {
            return;
        }

        var formData = new FormData();
        var files = $("input[name='Images']")[0].files;

        // Thêm tệp hình ảnh vào FormData
        for (var i = 0; i < files.length; i++) {
            formData.append("Images", files[i]);
        }

        var complaint = _$form.serializeFormToObject();
        
        abp.ui.setBusy(_$modal);
        _complaintService.create(complaint).done(function () {
            _$modal.modal('hide');
            _$form[0].reset();
            PlaySound('success'); abp.notify.info(l('SavedSuccessfully'));
            _$complaintsTable.ajax.reload();
        }).always(function () {
            abp.ui.clearBusy(_$modal);
        });
    });

    $(document).on('click', '.delete-complaint', function () {
        var complaintId = $(this).attr("data-complaint-id");
        var complaintCode = $(this).attr('data-complaint-code');

        deleteComplaints(complaintId, complaintCode);
    });

    function deleteComplaints(complaintId, complaintCode) {
        abp.message.confirm(
            abp.utils.formatString(
                l('AreYouSureWantToDelete'),
                complaintCode),
            null,
            (isConfirmed) => {
                if (isConfirmed) {
                    _complaintService.delete({
                        id: complaintId
                    }).done(() => {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        _$complaintsTable.ajax.reload();
                    });
                }
            }
        );
    }

    $(document).on('click', '.edit-complaint', function (e) {
        var complaintId = $(this).attr("data-complaint-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Complaint/EditModal?Id=' + complaintId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ComplaintEditModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    $(document).on('click', 'a[data-target="#ComplaintCreateModal"]', (e) => {

        /*  $('.nav-tabs a[href="#complaint-details"]').tab('show');*/
        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Complaint/Create',
            type: 'GET',
            dataType: 'html',
            success: function (content) {
                $('#ComplaintCreateModal div.modal-content').html(content);
            },
            error: function (e) {
            }
        });
    });

    abp.event.on('complaint.edited', (data) => {
        _$complaintsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$complaintsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$complaintsTable.ajax.reload();
            return false;
        }
    });
})(jQuery);
