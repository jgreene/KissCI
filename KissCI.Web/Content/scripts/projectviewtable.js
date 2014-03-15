$(function () {
    var timeoutSetting = 3000;
    var isRefreshing = false;

    var refreshGrid = function () {
        if (isRefreshing == true)
            return;

        var container = $('.project-view-table-container');
        var cat = container.find('.project-view-table').attr('data-category');
        
        var url = (function () {
            var root = '/category/grid';
            if (cat !== '') {
                return root + '?categoryName=' + cat;
            }

            return root;
        })();

        isRefreshing = true;
        container.load(encodeURI(url), function () {
            isRefreshing = false;
        });
    };

    var bindProjectButtons = function (names) {
        _.each(names, function (name) {
            var container = $('.project-view-table-container');
            container.on('click', '.' + name + '-button', function (e) {
                e.preventDefault();
                var projectName = $(this).parents('.project-row').data('project-name');

                if (!confirm("Are you sure you want to execute " + name + " for " + projectName + "?"))
                    return;

                $.post('/project/' + projectName + '/' + name, function (data) {
                    refreshGrid();
                });
            });
        });
    };

    var bindCommandButtons = function () {
        var container = $('.project-view-table-container');
        container.on('click', '.command-button', function (e) {
            e.preventDefault();
            
            var projectName = $(this).parents('.project-row').data('project-name');
            var commandName = $(this).data('command-name');

            if (!confirm("Are you sure you want to execute " + commandName + " for " + projectName + "?"))
                return;

            $.post('/project/' + projectName + '/run/' + commandName, function(data) {
                refreshGrid();
            });
        });
    };

    bindProjectButtons(['stop', 'cancel', 'start']);
    bindCommandButtons();

    setInterval(function () {
        refreshGrid();
    }, timeoutSetting);
});