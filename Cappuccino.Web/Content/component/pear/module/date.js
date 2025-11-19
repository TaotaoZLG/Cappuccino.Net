/*
* 日期时间扩展模块
*/
layui.define(['laydate', 'jquery'], function (exports) {
    "use strict";
    var $ = layui.jquery,
        laydate = layui.laydate;

    // 等待DOM加载完成后执行（确保能获取到页面元素）
    $(function () {
        // laydate 时间控件绑定（.select-time）
        if ($(".select-time").length > 0) {
            var startLayDate = laydate.render({
                elem: '#startTime',
                max: $('#endTime').val(),
                theme: 'default',
                type: $('#startTime').attr("data-type") || 'date',
                trigger: 'click',
                done: function (value, date) {
                    if (value !== '') {
                        endLayDate.config.min.year = date.year;
                        endLayDate.config.min.month = date.month - 1;
                        endLayDate.config.min.date = date.date;
                    } else {
                        endLayDate.config.min.year = '';
                        endLayDate.config.min.month = '';
                        endLayDate.config.min.date = '';
                    }
                }
            });
            var endLayDate = laydate.render({
                elem: '#endTime',
                min: $('#startTime').val(),
                theme: 'default',
                type: $('#endTime').attr("data-type") || 'date',
                trigger: 'click',
                done: function (value, date) {
                    if (value !== '') {
                        startLayDate.config.max.year = date.year;
                        startLayDate.config.max.month = date.month - 1;
                        startLayDate.config.max.date = date.date;
                    } else {
                        startLayDate.config.max.year = '2099';
                        startLayDate.config.max.month = '12';
                        startLayDate.config.max.date = '31';
                    }
                }
            });
        }

        // laydate time-input 时间控件绑定（.time-input）
        if ($(".time-input").length > 0) {
            $(".time-input").each(function (index, item) {
                var time = $(item);
                var type = time.attr("data-type") || 'date';
                var format = time.attr("data-format") || 'yyyy-MM-dd';
                var buttons = time.attr("data-btn") || 'clear|now|confirm';
                var callback = time.attr("data-callback") || {};
                var range = time.attr("data-range") || false;
                var newBtnArr = [];

                if (buttons) {
                    if (buttons.indexOf("|") > 0) {
                        var btnArr = buttons.split("|");
                        btnArr.forEach(btn => {
                            if (["clear", "now", "confirm"].includes(btn)) {
                                newBtnArr.push(btn);
                            }
                        });
                    } else if (["clear", "now", "confirm"].includes(buttons)) {
                        newBtnArr.push(buttons);
                    }
                } else {
                    newBtnArr = ['clear', 'now', 'confirm'];
                }

                laydate.render({
                    elem: item,
                    theme: 'default',
                    trigger: 'click',
                    type: type,
                    format: format,
                    btns: newBtnArr,
                    range: range,
                    done: function (value, data) {
                        if (typeof window[callback] === 'function') {
                            window[callback](value, data);
                        }
                    }
                });
            });
        }
    });

    // 导出模块（无需暴露方法，仅执行初始化）
    exports('date', {});
});