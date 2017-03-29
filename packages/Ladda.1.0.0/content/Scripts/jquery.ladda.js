(function ($) {
    $.fn.laddaClick = function (clickCallback) {
        /// <summary>动态加载按钮</summary>
        /// <param name="clickCallback">点击按钮回调事件，返回false会停止按钮动画</param>

        var laddaObj = $(this).ladda();
        laddaObj.click(function () {
            laddaObj.ladda('start');
            var result = clickCallback.apply($(this), $(this));
            if (result != undefined && result == false) {
                laddaObj.ladda('stop');
            }
            return false;
        });

        return laddaObj;
    };
})(jQuery);