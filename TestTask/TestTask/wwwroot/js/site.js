$(document).ready(function () {

    $('.editable').dblclick(function () {
        var field = $(this).data('field');
        var id = $(this).data('id');
        var input = $('<input type="text" />');
        input.val($(this).text());
        $(this).empty().append(input);
        input.focus();

        input.blur(function () {
            var value = input.val();
            $(this).parent().text(value);
            updateEmployee(id, field, value);
        });
    });

    function updateEmployee(id, field, value) {
        var url = '/Home/Update';
        $.post(url, { Id: id, Field: field, Value: value }, function () {
            console.log('Record updated successfully');
        });
    }

    $('a.delete-link').click(function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (confirm('Are you sure you want to delete this record?')) {
            $.post(url, function () {
                location.reload();
            });
        }
    });

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

                valA = new Date(parseDate(valA));
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
        return dateParts[2] + '-' + dateParts[1] + '-' + dateParts[0];
    }
});