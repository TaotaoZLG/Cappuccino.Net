; (function (window) {
    "use strict";
    layui.use('jquery', function () {
        var $ = layui.$;

        // 存放字典数据（顶层窗口共享）
        var dataDict = {};

        // 标记字典是否加载完成（仅用于提示，非异步控制）
        var isDictLoaded = false;

        /**
         * 初始化字典数据（AJAX请求后端同步接口）
         */
        function initDataDict() {
            return $.ajax({
                url: '/SystemManage/SysDict/GetDataDictList',
                type: 'get',
                dataType: 'json',
                success: function (response) {
                    if (response.status === 1 && response.data && Array.isArray(response.data)) {
                        // 将数组转换为对象，遍历数组中的每一项
                        response.data.forEach(function (item) {
                            // 将 dictCode 作为键，整个对象作为值存入 dataDict
                            dataDict[item.dictCode] = item;
                        });

                        isDictLoaded = true;
                        // console.log('字典数据加载完成');
                    } else {
                        console.error('字典数据加载失败:', response.message || '数据格式错误');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('字典请求异常:', status + ' - ' + error);
                }
            });
        }

        /**
         * 获取指定类型的字典列表（同步返回）
         * @param {string} dictCode 字典类型编码
         * @returns {Array} 字典项数组
         */
        function getDataDict(dictCode) {
            if (!isDictLoaded) {
                console.warn('字典尚未加载完成，可能返回空数据');
            }

            // 逻辑保持不变：检查是否存在，存在则返回 dictInfo 的副本
            return dataDict[dictCode] ? [...dataDict[dictCode].dictInfo] : [];
        }

        /**
         * 回显字典值
         * @param {string} dictCode 字典类型编码
         * @param {string|number} dictKey 字典项匹配键
         * @param {boolean} [isClass=true] 是否返回带样式的HTML
         * @returns {string} 显示文本或带样式HTML
         */
        function getDataDictValue(dictCode, dictKey, isClass = true) {
            if (!isDictLoaded) {
                console.warn('字典尚未加载完成，可能返回空值');
                return '';
            }

            if (!dataDict[dictCode] || !dataDict[dictCode].dictInfo) {
                console.warn(`字典类型不存在或无数据: ${dictCode}`);
                return '';
            }

            var dicts = dataDict[dictCode].dictInfo;
            for (let i = 0; i < dicts.length; i++) {
                var item = dicts[i];
                if (item.value == dictKey) {
                    if (item.class && isClass) {
                        return `<span class="layui-btn layui-btn-xs layui-btn-${item.class}">${item.name}</span>`;
                    } else {
                        return item.name;
                    }
                }
            }

            console.warn(`字典项不存在: 类型=${dictCode}, 键=${dictKey}`);
            return '';
        }

        // 页面加载时自动初始化
        initDataDict();

        // 挂载到顶层窗口
        window.top.initDataDict = initDataDict;
        window.top.getDataDict = getDataDict;
        window.top.getDataDictValue = getDataDictValue;

    });
})(window);