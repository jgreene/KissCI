﻿$(function () {
    var timeoutSetting = 3000;
    var isRefreshing = false;

    var refreshGrid = function () {
        if (isRefreshing == true)
            return;

        var container = $('.project-view-table-container');
        var cat = container.find('.project-view-table').attr('data-category');
        
        var url = (function () {
            var root = '/project/grid';
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

    
    
    $('.project-view-table-container').on('click', '.force-button', function (e) {
        e.preventDefault();
        var projectName = $(this).parent().parent().attr('data-project-name');

        $.post('/project/force/?projectName=' + projectName, function (data) {
            refreshGrid();
        });
    });

    setInterval(function () {
        refreshGrid();
    }, timeoutSetting);
});