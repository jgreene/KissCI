$(function () {

    var refresh = function () {
        $.get('/configuration/table', function (result) {
            $('.configuration-container').html(result);
        });
    };

    var getKeyValue = function (item) {
        var elem = $(item).parents('.keyPair');
        var key = elem.find('.key').val();
        var value = elem.find('.value').val();

        return { key: key, value: value };

    };

    var remove = function (key, func) {
        $.post('/configuration/remove?key=' + key, func);
    }

    var save = function (key, value, func) {
        $.post('/configuration/save?key=' + key + '&value=' + value, func);
    };

    var persistOn = function (buttonClass) {
        $('.configuration-container').on('click', buttonClass, function (e) {
            e.preventDefault();

            var item = getKeyValue(this);

            save(item.key, item.value, function (res) {
                refresh();
            });
        });
    };

    persistOn('.add-button');
    persistOn('.update-button');

    $('.configuration-container').on('click', '.remove-button', function (e) {
        e.preventDefault();

        var item = getKeyValue(this);

        remove(item.key, function (res) {
            refresh();
        });
    });


});