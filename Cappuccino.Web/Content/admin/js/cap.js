// 添加到页面的window对象上面，在页面中用ys.进行访问
; window.cap = {};
(function ($, ys) {
    "use strict";
    $.extend(ys, {
        // url
        openDialog: function (option) {
            // 如果是移动端，就使用自适应大小弹窗
            if (ys.isMobile()) {
                option.width = 'auto';
                option.height = 'auto';
            }
            else {
                if (!option.height) {
                    option.height = ($(window).height() - 50) + 'px';
                }
            }
            var _option = $.extend({
                type: 2,
                title: '',
                width: '800px',
                height: "750px",
                content: '',
                maxmin: true,
                shade: 0.4,
                btn: null,
                callback: null,
                shadeClose: false,
                fix: false,
                closeBtn: 1
            }, option);
            top.layer.open({
                type: _option.type, // 2表示content的值为url，1表示content的值为html
                area: [_option.width, _option.height],
                maxmin: _option.maxmin,
                shade: _option.shade,
                title: _option.title,
                content: _option.content,
                btn: _option.btn,
                shadeClose: _option.shadeClose, // 弹层外区域关闭     
                fix: _option.fix,
                closeBtn: _option.closeBtn,  // 1表示带关闭，0表示不带
                yes: _option.callback,
                cancel: function (index) {
                    return true;
                }
            });
        },
        // html
        openDialogContent: function (option) {
            // 如果是移动端，就使用自适应大小弹窗
            if (ys.isMobile()) {
                option.width = 'auto';
                option.height = 'auto';
            }
            else {
                if (!option.height) {
                    option.height = ($(window).height() - 50) + 'px';
                }
            }
            var _option = $.extend({
                type: 1,
                title: false,
                width: '800px',
                height: "750px",
                content: '',
                maxmin: false,
                shade: 0.4,
                btn: null,
                callback: null,
                shadeClose: true,
                fix: true,
                closeBtn: 1
            }, option);
            top.layer.open({
                type: _option.type, // 2表示content的值为url，1表示content的值为html
                area: [_option.width, _option.height],
                maxmin: _option.maxmin,
                shade: _option.shade,
                title: _option.title,
                content: _option.content,
                btn: _option.btn,
                shadeClose: _option.shadeClose, // 弹层外区域关闭
                fix: _option.fix,
                closeBtn: _option.closeBtn,  // 1表示带关闭，0表示不带
                yes: _option.callback,
                cancel: function (index) {
                    return true;
                }
            });
        },
        // 弹出层全屏
        openFull: function (option) {
            // 如果是移动端，就使用自适应大小弹窗
            if (ys.isMobile()) {
                option.width = 'auto';
                option.height = 'auto';
            }
            else {
                if (!option.height) {
                    option.height = ($(window).height() - 50) + 'px';
                }
            }
            var _option = $.extend({
                type: 2,
                title: false,
                width: '800px',
                content: '',
                maxmin: true,
                shade: 0.3,
                btn: null,
                callback: function (index, layero) {
                    var iframeWin = layero.find('iframe')[0];
                    iframeWin.contentWindow.submitHandler(index, layero);
                },
                // 弹层外区域关闭
                shadeClose: true,
                fix: false,  //是否固定
                closeBtn: 1
            }, option);
            var index = top.layer.open({
                type: _option.type, // 2表示content的值为url，1表示content的值为html
                area: [_option.width, _option.height],
                maxmin: _option.maxmin,
                shade: _option.shade,
                title: _option.title,
                content: _option.content,
                btn: _option.btn,
                shadeClose: _option.shadeClose, // 弹层外区域关闭
                fix: _option.fix,
                closeBtn: _option.closeBtn,  // 1表示带关闭，0表示不带
                yes: _option.callback,
                cancel: function (index) {
                    return true;
                },
                success: function () {
                    $(':focus').blur();
                }
            });
            top.layer.full(index);
        },
        // 新选卡页方式打开
        openTab: function (url, title, isRefresh) {
            createMenuItem(url, title, isRefresh);
        },
        // 右侧弹出窗口打开
        popupRight: function (title, url, width) {
            width = ys.isEmpty(width) ? 150 : width;
            if (top.location !== self.location) {
                if ($(top.window).outerWidth() < 400) {
                    width = 50;
                }
            }
            top.layer.open({
                type: 2,
                offset: 'r',
                anim: 'slideLeft',
                move: false,
                title: title,
                shade: 0.3,
                shadeClose: true,
                area: [($(window).outerWidth() - width) + 'px', '100%'],
                content: url
            });
        },
        // 关闭窗体
        close: function (index) {
            if (ys.isNullOrEmpty(index)) {
                var index = parent.layer.getFrameIndex(window.name);
                parent.layer.close(index);
            } else {
                top.layer.close(index);
            }
        },
        // 关闭全部窗体
        closeAll: function () {
            top.layer.closeAll();
        },
        closeDialog: function () {
            var index = parent.layer.getFrameIndex(window.name);
            parent.layer.close(index);
        },
        // 消息提示
        msg: function (content) {
            top.layer.msg(content);
        },
        // 警告消息
        msgWarning: function (content) {
            top.layer.msg(content, { icon: 0, time: 1000, shift: 5 });
        },
        // 成功消息
        msgSuccess: function (content) {
            if (ys.isNullOrEmpty(content)) {
                content = "操作成功";
            }
            top.layer.msg(content, { icon: 1, time: 1000, shift: 5 });
        },
        // 错误消息
        msgError: function (content) {
            if (ys.isNullOrEmpty(content)) {
                content = "操作失败";
            }
            top.layer.msg(content, { icon: 2, time: 3000, shift: 5 });
        },
        // 警告提示
        alertWarning: function (content) {
            top.layer.alert(content, {
                icon: 0,
                title: "系统提示",
                btn: ['确认'],
                btnclass: ['btn btn-primary'],
            });
        },
        // 成功提示
        alertSuccess: function (content) {
            top.layer.alert(content, {
                icon: 1,
                title: "系统提示",
                btn: ['确认'],
                btnclass: ['btn btn-primary'],
            });
        },
        // 错误提示
        alertError: function (content) {
            top.layer.alert(content, {
                icon: 2,
                title: "系统提示",
                btn: ['确认'],
                btnclass: ['btn btn-primary'],
            });
        },
        // 确认窗体
        confirm: function (content, callback) {
            top.layer.confirm(content, {
                icon: 3,
                title: "系统提示",
                btn: ['确认', '取消'],
                btnclass: ['btn btn-primary', 'btn btn-danger'],
            }, function (index) {
                top.layer.close(index);
                callback(true);
            });
        },
        // 消息提示，重新加载页面
        msgReload: function (msg, type) {
            top.layer.msg(msg, {
                icon: 1,
                time: 500,
                shade: [0.1, '#8F8F8F']
            },
                function () {
                    ys.reload();
                });
        },
        // 获取iframe页的DOM
        getChildFrame: function (index) {
            if (ys.isEmpty(index)) {
                var index = parent.layer.getFrameIndex(window.name);
                return parent.layer.getChildFrame('body', index);
            } else {
                return top.layer.getChildFrame('body', index);
            }
        },
        // 打开遮罩层
        showLoading: function (message) {
            $.blockUI({ message: '<div class="loaderbox"><div class="loading-activity"></div> ' + message + '</div>', css: { border: "none", backgroundColor: 'transparent' } });
        },
        // 关闭遮罩层
        closeLoading: function () {
            setTimeout(function () { $.unblockUI(); }, 50);
        },
        // 重新加载
        reload: function () {
            parent.location.reload();
        },
        // 获取选中行id
        getIds: function (row) {
            var ids = '';
            $.each(row, function (i, obj) {
                if (i == 0) {
                    ids = obj.Id;
                }
                else {
                    ids += "," + obj.Id;
                }
            });
            return ids;
        },
        // 判断是否选择编辑行
        checkRowEdit: function (row) {
            if (row.length == 0) {
                ys.msgError("您没有选择任何行！");
            } else if (row.length > 1) {
                ys.msgError("您的选择大于1行！");
            } else if (row.length == 1) {
                return true;
            }
            return false;
        },
        // 判断是否选择删除行
        checkRowDelete: function (row) {
            if (row.length == 0) {
                ys.msgError("您没有选择任何行！");
            } else if (row.length > 0) {
                return true;
            }
            return false;
        },
        // 判断是否有选中行 zlg
        checkRowData: function (row) {
            if (row.length == 0) {
                ys.msgError("您没有选择任何行！");
            } else if (row.length > 0) {
                return true;
            }
            return false;
        },

        ajax: function (option) {
            var opt = $.extend({
                url: option.url,
                async: true,
                type: "get",
                data: option.data || {},
                dataType: option.dataType || "json",
                error: function (xhr, status, obj) { ys.alertError("系统出错了"); },
                success: function (rdata) {
                    ys.msgSuccess();
                },
                beforeSend: function (xhr) {
                    ys.showLoading("正在处理中...");
                },
                complete: function (xhr, status) {
                    ys.closeLoading();
                }
            }, option);

            if (ys.isNullOrEmpty(opt.url)) {
                ys.alertError("url 参数不能为空");
                return;
            }
            $.ajax({
                url: opt.url,
                async: opt.async,
                type: opt.type,
                data: opt.data,
                dataType: opt.dataType,
                error: opt.error,
                success: opt.success,
                beforeSend: opt.beforeSend,
                complete: opt.complete,
            });
        },
        //上传文件
        ajaxUploadFile: function (option) {
            var opt = $.extend({
                url: option.url,
                data: option.data || {},
                error: function (xhr, status, obj) {
                    console.log(xhr);
                    console.log(status);
                    console.log(obj);
                    ys.alertError("系统出错了");
                },
                success: function (rdata) {
                    ys.msgSuccess();
                },
                beforeSend: function (xhr) {
                    ys.showLoading("正在处理中...");
                },
                complete: function (xhr, status) {
                    ys.closeLoading();
                }
            }, option);

            if (ys.isNullOrEmpty(opt.url)) {
                ys.alertError("url 参数不能为空");
                return;
            }
            if (ys.isNullOrEmpty(opt.data)) {
                ys.alertError("data 参数不能为空");
                return;
            }
            $.ajax({
                url: opt.url,
                data: opt.data,
                type: "post",
                processData: false,
                contentType: false,
                error: opt.error,
                success: opt.success,
                beforeSend: opt.beforeSend,
                complete: opt.complete
            })
        },
        // 导出数据
        exportExcel: function (url, postData) {
            ys.ajax({
                url: url,
                type: "post",
                data: postData,
                success: function (obj) {
                    if (obj.Code == 1) {
                        window.location.href = ctx + "File/DownloadFile?filePath=" + obj.Data + "&delete=1";
                    }
                    else {
                        ys.msgError(obj.Message);
                    }
                },
                beforeSend: function (xhr) {
                    ys.showLoading("正在导出数据，请稍后...");
                }
            });
        },
        request: function (name) {
            var params = decodeURI(window.location.search);
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
            var r = params.substr(1).match(reg);
            if (r != null) {
                return unescape(r[2]);
            }
            return null;
        },
        getHttpFileName: function (url) {
            if (url == null || url == '') {
                return url;
            }
            var i = url.lastIndexOf('/');
            if (i > 0) {
                return url.substring(i + 1);
            }
            return url;
        },
        getFileNameWithoutExtension: function (fileName) {
            if (fileName == null || fileName == '') {
                return fileName;
            }
            var i = fileName.indexOf('.');
            if (i > 0) {
                return fileName.substring(0, i);
            }
            return fileName;
        },
        changeURLParam: function (url, arg, arg_val) {
            var pattern = arg + '=([^&]*)';
            var replaceText = arg + '=' + arg_val;
            if (url.match(pattern)) {
                var tmp = '/(' + arg + '=)([^&]*)/gi';
                tmp = url.replace(eval(tmp), replaceText);
                return tmp;
            } else {
                if (url.match('[\?]')) {
                    var arr = url.split('#');
                    if (arr.length > 1) {
                        return arr[0] + '&' + replaceText + '#' + arr[1];
                    }
                    else {
                        return url + '&' + replaceText;
                    }
                } else {
                    return url + '?' + replaceText;
                }
            }
        },
        // 判断字符串是否为空
        isEmpty: function (value) {
            if (value == null || this.trim(value) == "" || value == undefined || value == "undefined") {
                return true;
            }
            return false;
        },
        // 判断一个字符串是否为非空串
        isNotEmpty: function (value) {
            return !ys.isEmpty(value);
        },
        // 是否为空串
        isNullOrEmpty: function (obj) {
            if ((typeof (obj) == "string" && obj == "") || obj == null || obj == undefined) {
                return true;
            }
            else {
                return false;
            }
        },
        // 判断对象是否为空
        isObjectEmpty: function (obj) {
            for (let key in obj) {
                if (obj.hasOwnProperty(key)) {
                    return false; // 如果对象有任何属性，则返回false
                }
            }
            return true; // 如果对象没有任何属性，则返回true
        },
        // 空对象转字符串
        nullToStr: function (value) {
            if (ys.isNullOrEmpty(value)) {
                return "-";
            }
            return value;
        },
        // 过滤字符串"null"/空值为null
        filterNullField: function (obj) {
            // 场景1：处理单个值（字符串/undefined/null）
            if (obj === null || obj === undefined || obj === "null" || obj === "" || obj === "undefined") {
                return null;
            }

            // 场景2：非对象类型（数字、布尔等），直接返回原值
            if (typeof obj !== 'object' || obj === null) {
                return obj;
            }

            // 场景3：处理对象（遍历所有字段）
            for (let key in obj) {
                if (obj.hasOwnProperty(key)) {
                    // 匹配无效值："null"、空字符串、"undefined"、undefined、null
                    if (obj[key] === null || obj[key] === undefined || obj[key] === "null" || obj[key] === "" || obj[key] === "undefined") {
                        obj[key] = null;
                    }
                }
            }
            return obj;
        },
        // 是否显示数据 为空默认为显示
        visible: function (value) {
            if (ys.isEmpty(value) || value == true) {
                return true;
            }
            return false;
        },
        // 空格截取
        trim: function (value) {
            if (value == null) {
                return "";
            }
            return value.toString().replace(/(^\s*)|(\s*$)|\r|\n/g, "");
        },
        // 比较两个字符串（大小写敏感）
        equals: function (str, that) {
            return str == that;
        },
        // 比较两个字符串（大小写不敏感）
        equalsIgnoreCase: function (str, that) {
            return String(str).toUpperCase() === String(that).toUpperCase();
        },
        // 将字符串按指定字符分割
        split: function (str, sep, maxLen) {
            if ($.common.isEmpty(str)) {
                return null;
            }
            var value = String(str).split(sep);
            return maxLen ? value.slice(0, maxLen - 1) : value;
        },
        // 字符串格式化(%s )
        sprintf: function (str) {
            var args = arguments, flag = true, i = 1;
            str = str.replace(/%s/g, function () {
                var arg = args[i++];
                if (typeof arg === 'undefined') {
                    flag = false;
                    return '';
                }
                return arg == null ? '' : arg;
            });
            return flag ? str : '';
        },
        // 日期格式化 时间戳  -> yyyy-MM-dd HH-mm-ss
        dateFormat: function (date, format) {
            var that = this;
            if (that.isEmpty(date)) return "";
            if (!date) return;
            if (!format) format = "yyyy-MM-dd";
            switch (typeof date) {
                case "string":
                    date = new Date(date.replace(/-/g, "/"));
                    break;
                case "number":
                    date = new Date(date);
                    break;
            }
            if (!date instanceof Date) return;
            var dict = {
                "yyyy": date.getFullYear(),
                "M": date.getMonth() + 1,
                "d": date.getDate(),
                "H": date.getHours(),
                "m": date.getMinutes(),
                "s": date.getSeconds(),
                "MM": ("" + (date.getMonth() + 101)).substr(1),
                "dd": ("" + (date.getDate() + 100)).substr(1),
                "HH": ("" + (date.getHours() + 100)).substr(1),
                "mm": ("" + (date.getMinutes() + 100)).substr(1),
                "ss": ("" + (date.getSeconds() + 100)).substr(1)
            };
            return format.replace(/(yyyy|MM?|dd?|HH?|ss?|mm?)/g, function () {
                return dict[arguments[0]];
            });
        },
        // 指定随机数返回
        random: function (min, max) {
            return Math.floor((Math.random() * max) + min);
        },
        // 数组去重
        uniqueFn: function (array) {
            var result = [];
            var hashObj = {};
            for (var i = 0; i < array.length; i++) {
                if (!hashObj[array[i]]) {
                    hashObj[array[i]] = true;
                    result.push(array[i]);
                }
            }
            return result;
        },
        // 数组中的所有元素放入一个字符串
        join: function (array, separator) {
            if (ys.isEmpty(array)) {
                return null;
            }
            return array.join(separator);
        },
        // 获取form下所有的字段并转换为json对象
        formToJSON: function (formId) {
            var json = {};
            $.each($("#" + formId).serializeArray(), function (i, field) {
                if (json[field.name]) {
                    json[field.name] += ("," + field.value);
                } else {
                    json[field.name] = field.value;
                }
            });
            return json;
        },
        // 数据字典转下拉框
        dictToSelect: function (datas, value, name) {
            var actions = [];
            actions.push(ys.sprintf("<select class='form-control' name='%s'>", name));
            $.each(datas, function (index, dict) {
                actions.push(ys.sprintf("<option value='%s'", dict.dictValue));
                if (dict.dictValue == ('' + value)) {
                    actions.push(' selected');
                }
                actions.push(ys.sprintf(">%s</option>", dict.dictLabel));
            });
            actions.push('</select>');
            return actions.join('');
        },
        // 获取obj对象长度
        getLength: function (obj) {
            var count = 0;
            for (var i in obj) {
                if (obj.hasOwnProperty(i)) {
                    count++;
                }
            }
            return count;
        },

        // Html.Raw()方法会提示语法错误，所以用这个函数包装一下
        getJson: function (value) {
            return value;
        },
        getGuid: function () {
            var guid = "";
            for (var i = 1; i <= 32; i++) {
                var n = Math.floor(Math.random() * 16.0).toString(16);
                guid += n;
                if ((i == 8) || (i == 12) || (i == 16) || (i == 20)) guid += "-";
            }
            return guid;
        },
        getValueByKey: function (json, key) {
            var value = "";
            $.each(json, function (i, obj) {
                if (obj.Key == key) {
                    value = obj.Value;
                }
            });
            return value;
        },
        getLastValue: function (str) {
            if (!ys.isNullOrEmpty(str)) {
                var arr = str.toString().split(',');
                return arr[arr.length - 1];
            }
            return '';
        },
        // 状态显示样式初始化
        formatStatus: function (datas, value) {
            if (ys.isEmpty(datas) || ys.isEmpty(value)) {
                return '';
            }
            var actions = [];
            $.each(datas, function (i, e) {
                if (e.Key == value) {
                    switch (value) {
                        case 0: {
                            return actions.push('<span class="badge badge-warning">' + e.Value + '</span>');
                        };
                        case 1: {
                            return actions.push('<span class="badge badge-primary">' + e.Value + '</span>');
                        };
                    }
                }
            });
            return actions.join('');
        },
        // 回显数据字典
        selectDictLabel: function (datas, value) {
            if (ys.isEmpty(datas) || ys.isEmpty(value)) {
                return '';
            }
            var actions = [];
            $.each(datas, function (index, dict) {
                if (dict.Key == ('' + value)) {
                    var listClass = ys.equals("default", dict.ListClass) || ys.isEmpty(dict.ListClass) ? "" : "badge badge-" + dict.ListClass;
                    var cssClass = ys.isNotEmpty(dict.cssClass) ? dict.cssClass : listClass;
                    actions.push(ys.sprintf("<span class='%s'>%s</span>", cssClass, dict.DictValue));
                    return false;
                }
            });
            if (actions.length === 0) {
                actions.push(ys.sprintf("<span>%s</span>", value))
            }
            return actions.join('');
        },
        // 格式为 yyyy-MM-dd HH:mm:ss
        formatDate: function (v, format) {
            if (!v) return "";
            if (!format) format = "yyyy-MM-dd";
            var d = v;
            if (typeof v === 'string') {
                if (v.indexOf("/Date(") > -1)
                    d = new Date(parseInt(v.replace("/Date(", "").replace(")/", ""), 10));
                else
                    d = new Date(Date.parse(v.replace(/-/g, "/").replace("T", " ").split(".")[0]));
            }
            var o = {
                "M+": d.getMonth() + 1,  //month
                "d+": d.getDate(),       //day
                "H+": d.getHours(),      //hour
                "m+": d.getMinutes(),    //minute
                "s+": d.getSeconds(),    //second
                "q+": Math.floor((d.getMonth() + 3) / 3),  //quarter
                "S": d.getMilliseconds() //millisecondjsonca4
            };
            if (/(y+)/.test(format)) {
                format = format.replace(RegExp.$1, (d.getFullYear() + "").substr(4 - RegExp.$1.length));
            }
            for (var k in o) {
                if (new RegExp("(" + k + ")").test(format)) {
                    format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
                }
            }
            return format;
        },
        // 获取n天前的日期
        getDaysAgo: function (days) {
            if (!days) days = 0;
            var date = new Date();
            date.setDate(date.getDate() - days);
            return date;
        },
        trimStart: function (rawStr, c) {
            if (c == null || c == '') {
                var str = rawStr.replace(/^s*/, '');
                return str;
            }
            else {
                var rg = new RegExp('^' + c + '*');
                var str = rawStr.replace(rg, '');
                return str;
            }
        },
        trimEnd: function (rawStr, c) {
            if (c == null || c == "") {
                var rg = /s/;
                var i = rawStr.length;
                while (rg.test(rawStr.charAt(--i)));
                return rawStr.slice(0, i + 1);
            }
            else {
                var rg = new RegExp(c);
                var i = rawStr.length;
                while (rg.test(rawStr.charAt(--i)));
                return rawStr.slice(0, i + 1);
            }
        },
        toString: function (value) {
            if (value == null) {
                return '';
            }
            return value.toString();
        },
        openLink: function (href, target) {
            var a = document.createElement('a')
            if (target) {
                a.target = target;
            }
            else {
                a.target = '_blank';
            }
            a.href = href;
            a.click();
        },
        recursion: function (obj, id, destArr, key, parentKey) {
            if (!key) {
                key = "id";
            }
            if (!parentKey) {
                parentKey = "parentId";
            }
            for (var item in obj) {
                if (obj[item][key] == id) {
                    destArr.push(obj[item]);
                    return ys.recursion(obj, obj[item][parentKey], destArr, key, parentKey);
                }
            }
        },
        // 判断移动端
        isMobile: function () {
            return navigator.userAgent.match(/(Android|iPhone|SymbianOS|Windows Phone|iPad|iPod)/i);
        },
        // 数字正则表达式，只能为0-9数字
        numValid: function (text) {
            var patten = new RegExp(/^[0-9]+$/);
            return patten.test(text);
        },
        // 英文正则表达式，只能为a-z和A-Z字母
        enValid: function (text) {
            var patten = new RegExp(/^[a-zA-Z]+$/);
            return patten.test(text);
        },
        // 英文、数字正则表达式，必须包含（字母，数字）
        enNumValid: function (text) {
            var patten = new RegExp(/^(?=.*[a-zA-Z]+)(?=.*[0-9]+)[a-zA-Z0-9]+$/);
            return patten.test(text);
        },
        // 英文、数字、特殊字符正则表达式，必须包含（字母，数字，特殊字符!@#$%^&*()-=_+）
        charValid: function (text) {
            var patten = new RegExp(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[~!@#\$%\^&\*\(\)\-=_\+])[A-Za-z\d~!@#\$%\^&\*\(\)\-=_\+]{6,}$/);
            return patten.test(text);
        },
        // 隐藏/显示搜索栏 zlg
        toggleSearch: function () {
            if ($('#searchDiv').css('display') == "none") {
                $('#searchDiv').slideDown('1000');
                $('#btnOpenSearch').html('<i class="fa fa-search"></i> 关闭搜索');
            } else {
                $('#searchDiv').slideUp('1000');
                $('#btnOpenSearch').html('<i class="fa fa-search"></i> 开启搜索');
            }
        },
        // 列超出指定长度浮动提示 target（copy单击复制文本 open弹窗打开文本）
        tooltip: function (value, length, target) {
            var _length = ys.isNullOrEmpty(length) ? 20 : length;
            var _text = "";
            var _value = ys.toString(value);
            var _target = ys.isNullOrEmpty(target) ? 'copy' : target;
            if (_value.length > _length) {
                _text = _value.substr(0, _length) + "...";
                _value = _value.replace(/\'/g, "&apos;");
                _value = _value.replace(/\"/g, "&quot;");
                var actions = [];
                actions.push(ys.sprintf('<input style="opacity: 0;position: absolute;width:5px;z-index:-1" type="text" value="%s"/>', _value));
                actions.push(ys.sprintf('<a href="###" class="tooltip-show" data-toggle="tooltip" data-target="%s" title="%s">%s</a>', _target, _value, _text));
                return actions.join('');
            } else {
                _text = _value;
                return _text;
            }
        },
        // 下拉按钮切换
        dropdownToggle: function (value) {
            var actions = [];
            actions.push('<div class="btn-group">');
            actions.push('<button type="button" class="btn btn-xs dropdown-toggle" data-toggle="dropdown" aria-expanded="false">');
            actions.push('<i class="fa fa-cog"></i>&nbsp;<span class="fa fa-chevron-down"></span></button>');
            actions.push('<ul class="dropdown-menu">');
            actions.push(value.replace(/<a/g, "<li><a").replace(/<\/a>/g, "</a></li>"));
            actions.push('</ul>');
            actions.push('</div>');
            return actions.join('');
        },
        // 图片预览
        imageView: function (value, height, width, target) {
            if ($.common.isEmpty(width)) {
                width = 'auto';
            }
            if ($.common.isEmpty(height)) {
                height = 'auto';
            }
            // blank or self
            var _target = $.common.isEmpty(target) ? 'self' : target;
            if ($.common.isNotEmpty(value)) {
                return $.common.sprintf("<img class='img-circle img-xs' data-height='%s' data-width='%s' data-target='%s' src='%s'/>", height, width, _target, value);
            } else {
                return $.common.nullToStr(value);
            }
        },
        // 操作封装处理
        operate: {
            // 删除信息
            remove: function (id, url) {
                ys.confirm('确定删除该条数据吗？', function () {
                    ys.ajax({
                        url: url + '?ids=' + id,
                        type: 'post',
                        success: function (obj) {
                            if (obj.Code == 1) {
                                ys.msgSuccess(obj.Message);
                                ys.searchGrid();
                            }
                            else {
                                ys.msgError(obj.Message);
                            }
                        }
                    });
                });
            },
            // 删除树结构
            removeTree: function (id, url) {
                ys.confirm('确定删除该条数据吗？', function () {
                    ys.ajax({
                        url: url + '?ids=' + id,
                        type: 'post',
                        success: function (obj) {
                            if (obj.Code == 1) {
                                ys.msgSuccess(obj.Message);
                                ys.searchTreeGrid();
                            }
                            else {
                                ys.msgError(obj.Message);
                            }
                        }
                    });
                });
            },
            // 编辑
            edit: function (id, url) {
                ys.openDialog({
                    title: '编辑',
                    content: url + '?id=' + id,
                    callback: function (index, layero) {
                        var iframeWin = layero.find('iframe')[0];
                        iframeWin.contentWindow.saveForm(index, layero);
                    }
                });
            },
            // 全屏编辑
            editFull: function (id, url) {
                ys.openFull({
                    title: '编辑',
                    content: url + '?id=' + id,
                    callback: function (index, layero) {
                        var iframeWin = layero.find('iframe')[0];
                        iframeWin.contentWindow.saveForm(index, layero);
                    }
                });
            },
            view: function (id, url, width) {
                var url = url + '?id=' + id;
                ys.popupRight("信息详情", url, width);
            },
        }
    });
})(window.jQuery, window.cap);