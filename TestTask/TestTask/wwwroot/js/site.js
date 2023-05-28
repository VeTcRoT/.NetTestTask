$(document).ready(function () {
    $('.upload-form').submit(function (event) {
        event.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            url: '/Home/Upload',
            type: 'POST',
            data: formData,
            success: function (data) {
                if (data.success) {
                    location.reload();
                }
                else {
                    var fileErrorContainer = $('.file-errors')
                    showValidationErrors(fileErrorContainer, data.errors)
                }
            },
            cache: false,
            contentType: false,
            processData: false
        });
        
    });

    $('.editable').dblclick(function () {
        var field = $(this).data('field');
        var id = $(this).data('id');
        var input = $('<input type="text" />');
        var oldValue = 'Enter a value'
        input.val($(this).text());
        $(this).empty().append(input);
        input.focus();

        input.blur(function () {
            var value = input.val();
            $(this).parent().text(value == '' ? oldValue : value);
            updateEmployee(id, field, value, oldValue);
        });
    });

    function updateEmployee(id, field, value) {
        var url = '/Home/Update';
        deleteValidationErrors();
        $.post(url, { id: id, field: field, value: value }, function (data) {
            if (data.success) {
                console.log('Record updated successfully');
            } else {
                var errorContainer = $('.field-validation-error').filter(function () {
                    return $(this).prev().data('field') === field && $(this).prev().data('id') === id;
                });
                showValidationErrors(errorContainer, data.errors);
            }
        });
    }

    function showValidationErrors(errorContainer, errors) {
        errorContainer.empty();
        if (Array.isArray(errors)) {
            $.each(errors, function (index, error) {
                var errorMessage = $('<span class="text-danger"></span>').text(error);
                errorContainer.append(errorMessage);
            });
        } else {
            var errorMessage = $('<span class="text-danger"></span>').text(errors);
            errorContainer.append(errorMessage);
        }
    }

    $('a.delete-link').click(function (e) {
        e.preventDefault();
        deleteValidationErrors();
        var url = $(this).attr('href');
        if (confirm('Are you sure you want to delete this record?')) {
            $.post(url, function (data) {
                if (data.success) {
                    $(`a[href='${url}']`).parent().parent().remove();
                }
                else {
                    var errorContainer = $('.file-errors')
                    showValidationErrors(errorContainer, data.errors);
                }
            });
        }
    });

    function deleteValidationErrors() {
        $('.field-validation-error').empty();
    }

    $('th').click(function () {
        var table = $(this).parents('table').eq(0);
        var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()));
        this.asc = !this.asc;
        if (!this.asc) {
            rows = rows.reverse();
        }
        for (var i = 0; i < rows.length; i++) {
            table.append(rows[i]);
        }
    });

    $('#filter').keyup(function () {
        var rows = $('.table tbody tr');
        var val = $.trim($(this).val()).replace(/ +/g, ' ').toLowerCase();

        rows.show().filter(function () {
            var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
            return !~text.indexOf(val);
        }).hide();
    });

    function comparer(index) {
        if (index === 2) {
            return function (a, b) {
                var valA = getCellValue(a, index), valB = getCellValue(b, index);
                console.log(valA)
                valA = new Date(parseDate(valA));
                console.log(valA)
                valB = new Date(parseDate(valB));
                return valA - valB;
            };
        } else {
            return function (a, b) {
                var valA = getCellValue(a, index), valB = getCellValue(b, index);
                return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB);
            };
        }
    }

    function getCellValue(row, index) {
        return $(row).children('td').eq(index).text();
    }

    function parseDate(dateString) {
        var dateParts = dateString.split('.');
        return dateParts[2].trim() + '-' + dateParts[1].trim() + '-' + dateParts[0].trim();
    }
});