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
        console.log(id)
        console.log(field)
        console.log(value)
        $.post(url, { Id: id, Field: field, Value: value }, function () {
            console.log('Record updated successfully');
        });
    }
});