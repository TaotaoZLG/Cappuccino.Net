layui.define(['jquery', 'popup'], function (exports) {
    var $ = layui.jquery;
    var popup = layui.popup;

    var common = {
        /**
         * 是否前后端分离
         */
        isFrontendBackendSeparate: false,
        /**
         * 服务器地址
         */
        baseUrl: "http://localhost:8080",
        /**
         * ajax()函数二次封装
         * @param {string} url 接口地址（相对于baseUrl）
         * @param {string} type 请求类型（get/post，默认为get）
         * @param {object} params 请求参数（默认为空对象）
         * @param {boolean} load 是否显示加载动画（默认为true）
         * @param {boolean} async 是否异步请求（默认为true）
         * @returns {*|never|{always, promise, state, then}}
         */
        ajax: function (url, type, params, load, async) {
            var deferred = $.Deferred();
            var loadIndex;
            $.ajax({
                url: common.isFrontendBackendSeparate ? common.baseUrl + url : url,
                type: type || "get",
                data: params || {},
                dataType: "json",
                async: async || true,
                beforeSend: function () {
                    if (load) {
                        loadIndex = layer.load(0, { shade: false });
                    }
                },
                success: function (response) {
                    if (response.status == "1") {
                        // 业务正常
                        deferred.resolve(response)
                    } else if (response.status == "2") {
                        popup.warming(response.message, function () { window.top.location ="/Account/Login" })
                    } else {
                        // 业务异常
                        popup.warming(response.message)
                        deferred.reject("common.ajax warn: " + response.message);
                    }
                },
                complete: function () {
                    if (load) {
                        layer.close(loadIndex);
                    }
                },
                error: function () {
                    layer.close(loadIndex);
                    popup.failure("服务器错误")
                    deferred.reject("common.ajax error: 服务器错误");
                }
            });
            return deferred.promise();
        },
    };
    //输出接口
    exports('common', common);
});