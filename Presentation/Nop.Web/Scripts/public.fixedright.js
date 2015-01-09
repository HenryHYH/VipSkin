$(function () {
    var container = $("<div></div>")
        .addClass("fixed-right")
        .appendTo("body");

    $('<div title="回到主页"><i class="fa fa-home"></i></div>')
       .addClass("icon-home")
       .addClass("icon")
       .click(function () {
           window.location.href = "/";
       }).appendTo(container);

    var scrollTop = $('<div title="回到顶部"><i class="fa fa-arrow-up"></i></div>')
        .addClass("icon-scroll-top")
        .addClass("icon")
        .addClass("icon-hide")
        .click(function () {
            $("html, body").animate({ scrollTop: 0 }, "slow");
        }).appendTo(container);

    $(document).scroll(function () {
        if ($(this).scrollTop() <= 100)
            scrollTop.fadeOut("fast");
        else
            scrollTop.fadeIn("fast");
    });
});