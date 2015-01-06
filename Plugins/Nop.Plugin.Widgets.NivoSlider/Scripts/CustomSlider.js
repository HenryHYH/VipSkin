function CustomSlider(slider) {
    var images = slider.find(".slider-wrapper"),
        firstImg = slider.find(".slider-wrapper:first"),
        intervalId = null;

    var controlPanel = $('<div class="control-panel"></div>').appendTo(slider);
    for (var i = 0, iMax = images.length; i < iMax; i++) {
        var node = $('<div class="node"></div>').appendTo(controlPanel);
        if (i == 0) {
            node.addClass("active");
        }
    }

    slider.delegate(".node", "click", function () {
        var currentNode = $(this);
        currentNode.addClass("active").siblings().removeClass("active");

        currentImg.hide();
        currentImg = slider.find(".slider-wrapper:eq(" + currentNode.index() + ")");
        currentImg.fadeIn("slow");
    });

    var currentImg = firstImg.show();

    function Change() {
        currentImg.hide();

        var index = currentImg.index();
        var nextIndex = index + 1;

        if (images.length == nextIndex) {
            currentImg = firstImg;
            nextIndex = 0;
        }
        else {
            currentImg = currentImg.next(".slider-wrapper");
        }

        slider.find(".node:eq(" + nextIndex + ")").addClass("active").siblings().removeClass("active");

        currentImg.fadeIn("slow");

    }

    function Start() {
        intervalId = setInterval(Change, 3000);
    }

    function Stop() {
        clearInterval(intervalId);
    }

    slider.mouseover(function () {
        Stop();
    }).mouseout(function () {
        Start();
    });

    Start();
}