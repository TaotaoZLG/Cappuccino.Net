// 等待DOM加载完成后执行（确保能获取到页面元素）
$(function () {
    layui.define(['laydate'], function (exports) {
        "use strict";
        var laydate = layui.laydate;

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
});

/** 密码规则范围验证 */
function checkpwd(chrtype, password) {
    if (chrtype == 1) {
        if (!ys.numValid(password) || !/^.{6,12}$/.test(password)) {
            return "密码只能为0-9数字，且长度6-12位";
        }
    } else if (chrtype == 2) {
        if (!ys.enValid(password) || !/^.{6,12}$/.test(password)) {
            return "密码只能为a-z和A-Z字母，且长度6-12位";
        }
    } else if (chrtype == 3) {
        if (!ys.enNumValid(password) || !/^.{8,18}$/.test(password)) {
            return "密码必须包含字母以及数字，且长度8-18位";
        }
    } else if (chrtype == 4) {
        if (!ys.charValid(password) || !/^.{8,18}$/.test(password)) {
            return "密码必须包含字母、数字、以及特殊符号<font color='red'>~!@#$%^&*()-=_+</font>，且长度8-18位";
        }
    }
}