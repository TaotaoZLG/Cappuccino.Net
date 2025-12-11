; (function (window) {
    "use strict";

    // 存放字典数据（顶层窗口共享）
    var dataDict = {};
    // 标记字典是否加载完成（仅用于提示，非异步控制）
    var isDictLoaded = false;

    /**
     * 初始化字典数据（AJAX请求后端同步接口）
     */
    function initDataDict() {
        $.ajax({
            url: '/System/SysDict/GetDataDictList',
            type: 'get',
            dataType: 'json',
            success: function (response) {
                if (response.code === 0 && response.data) {
                    // 直接赋值后端返回数据，结构完全对齐
                    dataDict = response.data;
                    isDictLoaded = true;
                    //console.log('字典数据加载完成（后端同步接口）');
                } else {
                    console.error('字典数据加载失败:', response.msg || '未知错误');
                }
            },
            error: function (xhr, status, error) {
                console.error('字典请求异常:', status + ' - ' + error);
            }
        });
    }

    /**
     * 获取指定类型的字典列表（同步返回，需确保字典已加载）
     * @param {string} typeCode 字典类型编码（如"user_sex"）
     * @returns {Array} 字典项数组（后端Dicts结构）
     */
    function getDataDict(typeCode) {
        // 提示未加载完成（非阻塞，仅友好提示）
        if (!isDictLoaded) {
            console.warn('字典尚未加载完成，可能返回空数据');
        }
        // 1. 检查字典类型是否存在 2. 取Dicts数组（后端返回的字典项集合）3. 返回副本避免修改源数据
        return dataDict[typeCode] ? [...dataDict[typeCode].dicts] : [];
    }

    /**
     * 回显字典值（同步返回，修正字段匹配）
     * @param {string} typeCode 字典类型编码
     * @param {string|number} dictKey 字典项匹配键（对应后端Value）
     * @returns {string} 显示文本或带样式HTML
     */
    function getDataDictValue(typeCode, dictKey) {
        // 提示未加载完成
        if (!isDictLoaded) {
            console.warn('字典尚未加载完成，可能返回空值');
            return '';
        }

        // 1. 检查字典类型是否存在 2. 检查Dicts数组是否存在
        if (!dataDict[typeCode] || !dataDict[typeCode].dicts) {
            console.warn(`字典类型不存在或无数据: ${typeCode}`);
            return '';
        }

        // 循环后端返回的Dicts数组
        var dicts = dataDict[typeCode].dicts;
        for (let i = 0; i < dicts.length; i++) {
            var item = dicts[i];
            if (item.value == dictKey) {
                // 样式类逻辑（后端若未来添加ListClass字段可直接兼容）
                if (item.class) {
                    return `<span class="layui-btn layui-btn-xs layui-btn-${item.class}">${item.label}</span>`;
                } else {
                    return item.label;
                }
            }
        }

        // 未找到匹配项
        console.warn(`字典项不存在: 类型=${typeCode}, 键=${dictKey}`);
        return '';
    }

    // 页面加载时自动初始化字典
    initDataDict();

    // 挂载到顶层窗口，支持top调用
    window.top.initDataDict = initDataDict;
    window.top.getDataDict = getDataDict;
    window.top.getDataDictValue = getDataDictValue;

})(window);