$(function () {
    var container = $("<div></div>")
        .addClass("fixed-right")
        .appendTo("body");

    var home = $('<div title="回到主页"><span class="pop">回到主页</span><a href="/"><i class="fa fa-home"></i></a></div>')
        .addClass("icon-home")
        .addClass("icon")
        .appendTo(container);

    var chat = $('<div title="在线咨询"><a href="javascript:void(0);"><span class="pop">在线咨询</span><i class="fa fa-comments-o"></i></a></div>')
        .addClass("icon icon-chat")
        .appendTo(container);

    var phone = $('<div title="联系我们"><span class="pop">400-066-0159</span><i class="fa fa-phone"></i></div>')
        .addClass("icon icon-phone")
        .appendTo(container);

    var weibo = $('<div title="@伊朵活泉VIPSKIN"><a href="http://weibo.com/u/5419774806" target="_blank"><span class="pop">@伊朵活泉VIPSKIN</span><i class="fa fa-weibo"></i></a></div>')
        .addClass("icon-weibo")
        .addClass("icon")
        .appendTo(container);

    var qq = $('<div title=""><i class="fa fa-qq"></i></div>')
        .addClass("icon icon-qq")
        .appendTo(container);

    var weixin = $('<div title=""><i class="fa fa-weixin"></i></div>')
        .addClass("icon icon-weixin")
        .appendTo(container);

    var scrollTop = $('<div title="回到顶部"><span class="pop">回到顶部</span><i class="fa fa-arrow-up"></i></div>')
        .addClass("icon icon-scroll-top icon-hide")
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