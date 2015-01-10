$(function () {
    var container = $("<div></div>")
        .addClass("fixed-right")
        .appendTo("body");

    var home = $('<div title="回到主页"><span class="pop">回到主页</span><a href="/"><i class="fa fa-home"></i></a></div>')
        .addClass("icon-home")
        .addClass("icon")
        .appendTo(container);

    var phone = $('<div title="联系我们"><span class="pop">400-066-0159</span><i class="fa fa-phone"></i></div>')
        .addClass("icon-phone")
        .addClass("icon")
        .appendTo(container);

    var weibo = $('<div title="@伊朵活泉VIPSKIN"><span class="pop">@伊朵活泉VIPSKIN</span><a href="http://weibo.com/u/5419774806" target="_blank"><i class="fa fa-weibo"></i></a></div>')
        .addClass("icon-weibo")
        .addClass("icon")
        .appendTo(container);

    var qq = $('<div title=""><i class="fa fa-qq"></i></div>')
        .addClass("icon-qq")
        .addClass("icon")
        .appendTo(container);

    var weixin = $('<div title=""><i class="fa fa-weixin"></i></div>')
        .addClass("icon icon-weixin")
        .appendTo(container);

    var scrollTop = $('<div title="回到顶部"><span class="pop">回到顶部</span><i class="fa fa-arrow-up"></i></div>')
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