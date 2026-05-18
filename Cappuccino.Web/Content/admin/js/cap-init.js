// 等待DOM加载完成后执行（确保能获取到页面元素）
$(function () {
    layui.define(['laydate', 'element'], function (exports) {
        "use strict";
        var laydate = layui.laydate;
        var element = layui.element;

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

/**
* 获取当前激活Tab中的iframe window对象
* @param {string} tabContainerSelector - Tab容器选择器（默认layui默认Tab容器）
* @returns {Window|null} 成功返回iframe window，失败返回null
*/
function getActiveTabIframeWindow(tabContainerSelector = '.layui-tab') {
    try {
        // 1. 【精准匹配你的项目结构】定位 Pear Admin 激活的Tab面板
        const $activePanel = $('.pear-tab .layui-tab-item.layui-show');
        console.log('【调试】激活的Tab面板元素：', $activePanel);

        if (!$activePanel.length) {
            console.warn('未找到激活的Tab面板');
            return null;
        }

        // 2. 查找面板内的 iframe（你的结构里只有1个，精准获取）
        const $iframe = $activePanel.find('iframe');
        console.log('【调试】找到的iframe元素：', $iframe);

        if (!$iframe.length) {
            console.warn('激活面板中没有iframe');
            return null;
        }

        // 3. 获取原生iframe对象 + contentWindow
        const iframeDom = $iframe[0];
        const iframeWindow = iframeDom.contentWindow;
        console.log('【调试】iframe contentWindow：', iframeWindow);

        if (!iframeWindow) {
            console.warn('iframe 未加载完成，无法获取window');
            return null;
        }

        console.log('✅ 成功获取激活Tab的iframe Window');
        return iframeWindow;

    } catch (error) {
        console.error('❌ 获取失败：', error);
        // 跨域是最常见的禁止访问原因
        if (error.message?.includes('cross-origin') || error.message?.includes('权限')) {
            console.error('💥 跨域限制！浏览器禁止访问不同源的iframe contentWindow');
        }
        return null;
    }
}

/**
 * 密码规则范围验证
 * @param {int} chrtype - 1:纯数字，2:纯字母，3:字母数字，4:字母数字特殊符号
 * @param {string} password - 待验证的密码字符串
 * @returns 
 */
function checkpwd(chrtype, password) {
    if (chrtype == 1) {
        if (!cap.numValid(password) || !/^.{6,12}$/.test(password)) {
            return "密码只能为0-9数字，且长度6-12位";
        }
    } else if (chrtype == 2) {
        if (!cap.enValid(password) || !/^.{6,12}$/.test(password)) {
            return "密码只能为a-z和A-Z字母，且长度6-12位";
        }
    } else if (chrtype == 3) {
        if (!cap.enNumValid(password) || !/^.{8,18}$/.test(password)) {
            return "密码必须包含字母以及数字，且长度8-18位";
        }
    } else if (chrtype == 4) {
        if (!cap.charValid(password) || !/^.{8,18}$/.test(password)) {
            return "密码必须包含字母、数字、以及特殊符号<font color='red'>~!@#$%^&*()-=_+</font>，且长度8-18位";
        }
    }
    return "未知密码规则";
}