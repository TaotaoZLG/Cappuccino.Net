layui.define(['table', 'jquery', 'element', 'form', 'tab', 'menu', 'frame', 'fullscreen', 'colorpicker'],
    function (exports) {
        "use strict";

        var $ = layui.jquery,
            form = layui.form,
            element = layui.element,
            pearTab = layui.tab,
            pearMenu = layui.menu,
            pearFrame = layui.frame,
            fullscreen = layui.fullscreen,
            colorpicker = layui.colorpicker;

        var bodyFrame;
        var sideMenu;
        var bodyTab;
        var config;

        var pearAdmin = new function () {
            this.render = function (initConfig) {
                if (initConfig != undefined) {
                    applyConfig(initConfig);
                } else {
                    readConfig().then(function (param) {
                        applyConfig(param);
                    });
                }
            }

            this.logoRender = function (param) {
                $(".layui-logo .logo").attr("src", param.logo.image);
                $(".layui-logo .title").html(param.logo.title);
            }

            this.menuRender = function (param) {
                sideMenu = pearMenu.render({
                    elem: 'sideMenu',
                    async: param.menu.async != undefined ? param.menu.async : true,
                    theme: "dark-theme",
                    height: '100%',
                    control: param.menu.control ? 'control' : false, // control 
                    defaultMenu: 0,
                    accordion: param.menu.accordion,
                    url: param.menu.data,
                    parseData: false,
                    change: function () {
                        compatible();
                    },
                    done: function () {
                        sideMenu.selectItem(param.menu.select);
                    }
                });
            }

            this.bodyRender = function (param) {
                if (param.tab.muiltTab) {
                    bodyTab = pearTab.render({
                        elem: 'content',
                        roll: true,
                        tool: true,
                        width: '100%',
                        height: '100%',
                        index: 0,
                        tabMax: param.tab.tabMax,
                        closeEvent: function (id) {
                            sideMenu.selectItem(id);
                        },
                        data: [{
                            id: param.tab.index.id,
                            url: param.tab.index.href,
                            title: param.tab.index.title,
                            close: false
                        }]
                    });
                    bodyTab.click(function (id) {
                        if (!param.tab.keepState) {
                            bodyTab.refresh(false);
                        }
                        bodyTab.positionTab();
                        sideMenu.selectItem(id);
                    })
                    sideMenu.click(function (dom, data) {
                        bodyTab.addTabOnly({
                            id: data.menuId,
                            title: data.menuTitle,
                            url: data.menuUrl,
                            icon: data.menuIcon,
                            close: true
                        }, 300, function () { });
                        compatible();
                    })
                } else {
                    bodyFrame = pearFrame.render({
                        elem: 'content',
                        title: '工作空间 / 首页',
                        url: param.tab.index.href,
                        width: '100%',
                        height: '100%'
                    });
                    sideMenu.click(function (dom, data) {
                        bodyFrame.changePage(data.menuUrl, data.menuPath, true);
                        compatible()
                    })
                }
            }

            this.keepLoad = function (param) {
                compatible()
                setTimeout(function () {
                    $(".loader-main").fadeOut(100);
                }, param.other.keepLoad)
            }

            this.themeRender = function (option) {
                if (option.theme.allowCustom == false) {
                    $('[pear-event="theme"]').remove();
                }
                var colorId = localStorage.getItem("theme-color");
                var menu = localStorage.getItem("theme-menu");
                var color = getColorById(colorId);
                if (menu == "null") {
                    menu = option.theme.defaultMenu;
                } else {
                    if (option.theme.allowCustom == false) {
                        menu = option.theme.defaultMenu;
                    }
                }
                localStorage.setItem("theme-color", color.id);
                localStorage.setItem("theme-menu", menu);
                this.colorSet(color.color);
                this.menuSkin(menu);
            }

            this.menuSkin = function (theme) {
                $(".pear-admin").removeClass("light-theme");
                $(".pear-admin").removeClass("dark-theme");
                $(".pear-admin").addClass(theme);
            }

            // 根据传入的颜色值动态生成CSS样式，并应用到页面中，实现主题色、选中色、LOGO背景色、头部栏背景色等的自定义配色功能
            this.colorSet = function (color) {
                var style = '';
                // 自 定 义 菜 单 配 色
                style +=
                    '.light-theme .pear-nav-tree .layui-this a:hover,.light-theme .pear-nav-tree .layui-this,.light-theme .pear-nav-tree .layui-this a,.pear-nav-tree .layui-this a,.pear-nav-tree .layui-this{background-color: ' +
                    color + '!important;}';

                // 自定义 Logo 标题演示
                style +=
                    '.pear-admin .layui-logo .title{color:' +
                    color + '!important;}';

                // 自 定 义 标 签 配 色
                style += '.pear-frame-title .dot,.pear-tab .layui-this .pear-tab-active{background-color: ' + color +
                    '!important;}';

                // 自 定 义 快 捷 菜 单
                style += '.bottom-nav li a:hover{background-color:' +
                    color + '!important;}';

                // 自 定 义 顶 部 配 色
                style += '.pear-admin .layui-header .layui-nav .layui-nav-bar{background-color: ' + color + '!important;}'

                // 自 定 义 加 载 配 色
                style += '.ball-loader>span,.signal-loader>span {background-color: ' + color + '!important;}';

                // 自 定 义 顶 部 配 色
                style += '.layui-header .layui-nav-child .layui-this a{background-color:' + color +
                    '!important;color:white!important;}';

                // 自 定 义 加 载 配 色
                style += '#preloader{background-color:' + color + '!important;}';

                // 自 定 义 样 式 选 择 边 框 配 色
                style +=
                    '.pearone-color .color-content li.layui-this:after, .pearone-color .color-content li:hover:after {border: ' +
                    color + ' 2px solid!important;}';

                style += '.layui-nav .layui-nav-child dd.layui-this a, .layui-nav-child dd.layui-this{background-color:' + color +
                    '!important}';

                style += '.pear-social-entrance {background-color:' + color + '!important}';
                style += '.pear-admin .pe-collaspe {background-color:' + color + '!important}';
                if ($("iframe").contents().find("#customTheme").length > 0) {
                    $("iframe").contents().find("#customTheme").remove();
                }
                $("#pearadmin-bg-color").html(style);
            }

            this.addNewTab = function (option) {
                if (!bodyTab || typeof bodyTab.addTabOnly !== 'function') {
                    console.warn('选项卡组件未初始化');
                    return false;
                }
                if (!option || !option.url) {
                    console.warn('URL不能为空');
                    return false;
                }
                bodyTab.addTabOnly({
                    id: option.id || "tab_" + Date.now(),
                    title: option.title || "新选项卡",
                    url: option.url,
                    icon: option.icon || "",
                    close: option.close !== undefined ? option.close : true
                }, 300, function () { });
                compatible();
            };
        };

        // Helper: 统一刷新/初始化自定义配色面板的 colorpicker，并把改变写入 localStorage 与样式
        function refreshCustomColorPickers(mainColor, logoColor, headerColor) {
            // 优先使用传入值，否则读取 localStorage，最后回退默认
            mainColor = (typeof mainColor !== "undefined" && mainColor !== null) ? mainColor : (localStorage.getItem('custom-main-color') || '#20222A');
            logoColor = (typeof logoColor !== "undefined") ? logoColor : (localStorage.getItem('custom-logo-color') || '');
            headerColor = (typeof headerColor !== "undefined") ? headerColor : (localStorage.getItem('custom-header-color') || '');

            // 主题色 colorpicker（立即同步 localStorage 并触发样式更新）
            colorpicker.render({
                elem: '[data-name="main"]',
                color: mainColor,
                done: function (c) {
                    localStorage.setItem('custom-main-color', c);
                    updateCustomThemeStyle();
                }
            });

            // LOGO 背景色 colorpicker
            colorpicker.render({
                elem: '[data-name="logo"]',
                color: logoColor,
                done: function (c) {
                    // 允许清空颜色（用空字符串表示）：
                    localStorage.setItem('custom-logo-color', c || '');
                    updateCustomThemeStyle();
                }
            });

            // 头部栏背景色 colorpicker
            colorpicker.render({
                elem: '[data-name="header"]',
                color: headerColor,
                done: function (c) {
                    localStorage.setItem('custom-header-color', c || '');
                    updateCustomThemeStyle();
                }
            });
        }

        $("body").on("click", "[pear-event=collaspe]", function () {
            sideMenu.collaspe();
            if ($(".pear-admin").is(".pear-mini")) {
                $(".layui-icon-spread-left").addClass("layui-icon-shrink-right")
                $(".layui-icon-spread-left").removeClass("layui-icon-spread-left")
                $(".pear-admin").removeClass("pear-mini");
            } else {
                $(".layui-icon-shrink-right").addClass("layui-icon-spread-left")
                $(".layui-icon-shrink-right").removeClass("layui-icon-shrink-right")
                $(".pear-admin").addClass("pear-mini");
            }
        });

        $("body").on("click", "[pear-event=fullScreen]", function () {
            var document = fullscreen.isFullscreen();
            if (document) {
                $(this).removeClass("layui-icon-screen-restore");
                fullscreen.fullClose()
            } else {
                $(this).addClass('layui-icon-screen-restore');
                fullscreen.fullScreen();
            }
        });

        $("body").on("click", "[pear-event=refresh]", function (e) {
            var $refreshIcon = $(this);
            // 判断当前是多标签模式 还是 单页模式，执行对应刷新
            var currentComponent = null;

            // 点击后立即显示加载动画
            $refreshIcon.removeClass("layui-icon-refresh-1").addClass("layui-anim layui-anim-rotate layui-anim-loop layui-icon-loading");

            // 确定当前应该使用的组件
            if (typeof bodyTab !== 'undefined' && bodyTab) {
                currentComponent = bodyTab;
            } else if (typeof bodyFrame !== 'undefined' && bodyFrame) {
                currentComponent = bodyFrame;
            }

            currentComponent.refresh(100, function () {
                // 刷新完成后的回调
                $refreshIcon.addClass("layui-icon-refresh-1").removeClass("layui-anim layui-anim-rotate layui-anim-loop layui-icon-loading");
            });
        });

        $("body").on("click", 'a[menu-url]', function () {
            if (config.tab.muiltTab) {
                var $this = $(this);
                var menuUrl = $this.attr("menu-url");
                var tabId = $this.attr("menu-id");

                // 安全检查：确保URL存在
                if (!menuUrl || menuUrl.trim() === "") {
                    console.warn("缺少menu-url属性，无法打开页面");
                    return;
                }
                if (!tabId || tabId.trim() === "") {
                    console.warn("缺少menu-id属性，无法打开页面");
                    return;
                }
                var tabTitle = $this.attr("menu-title") || "新选项卡";

                bodyTab.addTabOnly({
                    id: tabId,
                    title: tabTitle,
                    url: menuUrl,
                    icon: "",
                    close: true
                }, 300, function () { });
                compatible();
            } else {
                bodyFrame.changePage(menuUrl, "", true);
            }
        });

        $("body").on("click", "[pear-event=theme]", function () {
            var bgColorHtml = '';
            $.each(config.theme.colors, function (index, item) {
                // 初始索引项添加选中类（layui-this）
                var activeClass = index === config.theme.initColorIndex ? 'layui-this' : '';
                // 拼接单个风格项
                bgColorHtml +=
                    '<li class="' + activeClass + '" data-select-bgcolor="' + item.theme + '" data-index="' + index + '" >' +
                    '<a href="javascript:;" data-skin="skin-blue" style="" class="clearfix full-opacity-hover">' +
                    '<div><span style="display:block; width: 20%; float: left; height: 12px; background: ' + item.topLeft + ';"></span><span style="display:block; width: 80%; float: left; height: 12px; background: ' + item.topRight + ';"></span></div>' +
                    '<div><span style="display:block; width: 20%; float: left; height: 40px; background: ' + item.bottomLeft + ';"></span><span style="display:block; width: 80%; float: left; height: 40px; background: ' + item.bottomRight + ';"></span></div>' +
                    '</a>' +
                    '</li>';
            });

            var html =
                '<div class="pearone-color">\n' +
                '<div class="color-title">整体风格</div>\n' +
                '<div class="color-content">\n' +
                '<ul>\n' + bgColorHtml + '</ul>\n' +
                '</div>\n' +
                '</div>';

            layer.open({
                type: 1,
                offset: 'r',
                area: ['340px', '100%'],
                title: false,
                shade: 0.1,
                closeBtn: 0,
                shadeClose: false,
                anim: -1,
                skin: 'layer-anim-right',
                move: false,
                content: html + buildColorHtml() + buildLinkHtml() + buildCustomColorHtml(),
                success: function (layero, index) {
                    form.render();

                    var color = localStorage.getItem("theme-color");
                    var menu = localStorage.getItem("theme-menu");

                    if (color != "null") {
                        $(".select-color-item").removeClass("layui-icon")
                            .removeClass("layui-icon-ok");
                        $("*[color-id='" + color + "']").addClass("layui-icon")
                            .addClass("layui-icon-ok");
                    }
                    if (menu != "null") {
                        $("*[data-select-bgcolor]").removeClass("layui-this");
                        $("[data-select-bgcolor='" + menu + "']").addClass("layui-this");
                    }
                    $('#layui-layer-shade' + index).click(function () {
                        var $layero = $('#layui-layer' + index);
                        $layero.animate({
                            left: $layero.offset().left + $layero.width()
                        }, 200, function () {
                            layer.close(index);
                        });
                    });

                    // 初始化并同步自定义配色控件（将读取 localStorage 并渲染 colorpicker）——解决问题 1、2、3 的回显与同步
                    refreshCustomColorPickers();

                    // 恢复默认主题事件
                    $('.set-default-theme .layui-btn').click(function () {
                        layer.alert('确定要恢复默认主题吗？', function (index) {
                            // 清空自定义颜色缓存
                            localStorage.removeItem('custom-main-color');
                            localStorage.removeItem('custom-selected-color');
                            localStorage.removeItem('custom-logo-color');
                            localStorage.removeItem('custom-header-color');

                            // 恢复主题色默认值
                            var defaultColor = getColorById(config.theme.defaultColor);
                            localStorage.setItem("theme-color", defaultColor.id);
                            pearAdmin.colorSet(defaultColor.color);

                            // 重新渲染样式
                            updateCustomThemeStyle();
                            // 重新刷新 colorpicker 显示（回显默认）
                            refreshCustomColorPickers();

                            layer.close(index);
                        });
                    });
                }
            });
        });

        $('body').on('click', '[data-select-bgcolor]', function () {
            var theme = $(this).attr('data-select-bgcolor');
            var index = $(this).attr('data-index'); // 读取风格下标
            $('[data-select-bgcolor]').removeClass("layui-this");
            $(this).addClass("layui-this");
            localStorage.setItem("theme-menu", theme);
            pearAdmin.menuSkin(theme);

            // 联动更新自定义配色
            var colorItem = config.theme.colors[index];

            // 获取主题色
            var colorId = localStorage.getItem("theme-color");
            var color = getColorById(colorId);

            // 按映射规则赋值（可自定义调整）
            var customMain = color.color; // 主题色 → topLeft
            var customLogo = colorItem.topLeft; // LOGO背景 → topLeft
            var customHeader = colorItem.topRight; // 头部背景 → topRight

            // 1. 更新本地缓存
            localStorage.setItem('custom-main-color', customMain);
            localStorage.setItem('custom-logo-color', customLogo);
            localStorage.setItem('custom-header-color', customHeader);

            // 2. 刷新 colorpicker 回显并同步样式（解决问题 1）
            refreshCustomColorPickers(customMain, customLogo, customHeader);

            // 3. 不传参数，自动读取localStorage所有配色值更新（确保样式被写入）
            updateCustomThemeStyle();
        });

        $('body').on('click', '.select-color-item', function () {
            $(".select-color-item").removeClass("layui-icon").removeClass("layui-icon-ok");
            $(this).addClass("layui-icon").addClass("layui-icon-ok");
            var colorId = $(".select-color-item.layui-icon-ok").attr("color-id");
            localStorage.setItem("theme-color", colorId);
            var color = getColorById(colorId);
            pearAdmin.colorSet(color.color);

            // 同步主题色到自定义配色面板
            if (color && color.color) {
                // 立即写入 custom-main-color 并更新样式（解决问题 3）
                localStorage.setItem('custom-main-color', color.color);
                updateCustomThemeStyle();

                // 重新渲染并回显到自定义配色面板
                refreshCustomColorPickers(color.color);
            }
        });

        // 监听全屏状态变化，自动切换图标
        document.addEventListener('fullscreenchange', function () {
            var isFull = fullscreen.isFullscreen();
            var $btn = $("[pear-event=fullScreen]");
            if (isFull) {
                $btn.addClass('layui-icon-screen-restore');
            } else {
                $btn.removeClass('layui-icon-screen-restore');
            }
        });

        function readConfig() {
            var defer = $.Deferred();
            $.getJSON("/pear.config.json?fresh=" + Math.random(), function (result) {
                defer.resolve(result)
            });
            return defer.promise();
        }

        function applyConfig(param) {
            config = param;
            pearAdmin.logoRender(param);
            pearAdmin.menuRender(param);
            pearAdmin.bodyRender(param);
            pearAdmin.themeRender(param);
            pearAdmin.keepLoad(param);
        }

        function getColorById(id) {
            var color;
            var flag = false;
            $.each(config.colors, function (i, value) {
                if (value.id == id) {
                    color = value;
                    flag = true;
                }
            })
            if (flag == false || config.theme.allowCustom == false) {
                $.each(config.colors, function (i, value) {
                    if (value.id == config.theme.defaultColor) {
                        color = value;
                    }
                })
            }
            return color;
        }

        function buildLinkHtml() {
            var links = "";
            if (!config.links) {
                return links;
            }
            $.each(config.links, function (i, value) {
                links += '<a class="more-menu-item" href="' + value.href + '" target="_blank">' +
                    '<i class="' + value.icon + '" style="font-size: 19px;"></i> ' + value.title +
                    '</a>'
            })
            return '<div class="more-menu-list">' + links + '</div>';
        }

        function buildColorHtml() {
            var colors = "";
            $.each(config.colors, function (i, value) {
                colors += "<span class='select-color-item' color-id='" + value.id + "' style='background-color:" + value.color +
                    ";'></span>";
            })

            return "<div class='select-color'><div class='select-color-title'>主题色</div><div class='select-color-content'>" +
                colors + "</div></div>"
        }

        // 构建自定义配色HTML结构，包含主题色、选中色、LOGO背景色、头部栏背景色的颜色选择器，以及恢复默认主题按钮
        function buildCustomColorHtml() {
            return `
				<div class="custom-color">
					<div class="custom-color-title">自定义配色</div>
					<div class='custom-color-content'>
						<ul class="pear-setTheme-custom">
							<li>主题色 
								<div class="set-custom-theme layui-inline" data-name="main" lay-options="{color: '#20222A'}">
									<div class="layui-unselect layui-colorpicker">  
										<span>    
											<span class="layui-colorpicker-trigger-span" lay-type="" style="background: #20222A">      
												<i class="layui-icon layui-colorpicker-trigger-i layui-icon-down"></i>    
											</span>  
										</span>
									</div>
								</div>
							</li> 
							<li>LOGO 背景色 
								<div class="set-custom-theme layui-inline" data-name="logo" lay-options="{color: ''}">
									<div class="layui-unselect layui-colorpicker">  
										<span>    
											<span class="layui-colorpicker-trigger-span" lay-type="" style="">      
												<i class="layui-icon layui-colorpicker-trigger-i layui-icon-close"></i>    
											</span>  
										</span>
									</div>
								</div>
							</li> 
							<li>头部栏背景色 
								<div class="set-custom-theme layui-inline" data-name="header" lay-options="{color: ''}">
									<div class="layui-unselect layui-colorpicker">  
										<span>    
											<span class="layui-colorpicker-trigger-span" lay-type="" style="">      
												<i class="layui-icon layui-colorpicker-trigger-i layui-icon-close"></i>    
											</span>  
										</span>
									</div>
								</div>
							</li> 
							<li>恢复默认主题 
								<div class="set-default-theme">
									<button class="layui-btn layui-btn-primary">
										<i class="layui-icon layui-icon-refresh-1"></i>
									</button>
								</div>
							</li> 
						</ul>
					</div>
				</div>
			`;
        }

        function updateCustomThemeStyle(color) {
            // 读取自定义颜色（无则用默认值）
            var mainColor = localStorage.getItem('custom-main-color') || '#20222A';
            var logoColor = localStorage.getItem('custom-logo-color') || '';
            var headerColor = localStorage.getItem('custom-header-color') || '';

            // 基础自定义样式（复用原有colorSet逻辑 + 新增自定义项）
            var customStyle = '';

            // 主题色
            if (mainColor) {
                customStyle +=
                    '.light-theme .pear-nav-tree .layui-this a:hover,.light-theme .pear-nav-tree .layui-this,.light-theme .pear-nav-tree .layui-this a,.pear-nav-tree .layui-this a,.pear-nav-tree .layui-this{background-color: ' +
                    mainColor + '!important;}';
                customStyle +=
                    '.pear-admin .layui-logo .title{color:' +
                    mainColor + '!important;}';
                customStyle += '.pear-frame-title .dot,.pear-tab .layui-this .pear-tab-active{background-color: ' + mainColor +
                    '!important;}';
                customStyle += '.bottom-nav li a:hover{background-color:' +
                    mainColor + '!important;}';
                customStyle += '.pear-admin .layui-header .layui-nav .layui-nav-bar{background-color: ' + mainColor + '!important;}';
                customStyle += '.ball-loader>span,.signal-loader>span {background-color: ' + mainColor + '!important;}';
                customStyle += '.layui-header .layui-nav-child .layui-this a{background-color:' + mainColor +
                    '!important;color:white!important;}';
                customStyle += '#preloader{background-color:' + mainColor + '!important;}';
                customStyle +=
                    '.pearone-color .color-content li.layui-this:after, .pearone-color .color-content li:hover:after {border: ' +
                    mainColor + ' 2px solid!important;}';
                customStyle += '.layui-nav .layui-nav-child dd.layui-this a, .layui-nav-child dd.layui-this{background-color:' + mainColor +
                    '!important}';
                customStyle += '.pear-social-entrance {background-color:' + mainColor + '!important}';
                customStyle += '.pear-admin .pe-collaspe {background-color:' + mainColor + '!important}';
            }

            // LOGO背景色
            if (logoColor) {
                customStyle += '.pear-admin .layui-logo{background-color:' + logoColor + '!important;}';
            }

            // 头部栏背景色
            if (headerColor) {
                customStyle += '.pear-admin .layui-header{background-color:' + headerColor + '!important;}';
            }

            // 同步iframe内样式
            if ($("iframe").contents().find("#customTheme").length > 0) {
                $("iframe").contents().find("#customTheme").remove();
            }

            // 写入样式（覆盖原有自定义主题样式）
            $("#pearadmin-bg-color").html(customStyle);
        }

        function compatible() {
            if ($(window).width() <= 768) {
                sideMenu.collaspe();
                if ($(".pear-admin").is(".pear-mini")) {
                    $(".layui-icon-spread-left").addClass("layui-icon-shrink-right")
                    $(".layui-icon-spread-left").removeClass("layui-icon-spread-left")
                    $(".pear-admin").removeClass("pear-mini");
                } else {
                    $(".layui-icon-shrink-right").addClass("layui-icon-spread-left")
                    $(".layui-icon-shrink-right").removeClass("layui-icon-shrink-right")
                    $(".pear-admin").addClass("pear-mini");
                }
            }
        }

        function screenFun(num) {
            num = num || 1;
            num = num * 1;
            var docElm = document.documentElement;
            switch (num) {
                case 1:
                    if (docElm.requestFullscreen) {
                        docElm.requestFullscreen();
                    } else if (docElm.mozRequestFullScreen) {
                        docElm.mozRequestFullScreen();
                    } else if (docElm.webkitRequestFullScreen) {
                        docElm.webkitRequestFullScreen();
                    } else if (docElm.msRequestFullscreen) {
                        docElm.msRequestFullscreen();
                    }
                    break;
                case 2:
                    if (document.exitFullscreen) {
                        document.exitFullscreen();
                    } else if (document.mozCancelFullScreen) {
                        document.mozCancelFullScreen();
                    } else if (document.webkitCancelFullScreen) {
                        document.webkitCancelFullScreen();
                    } else if (document.msExitFullscreen) {
                        document.msExitFullscreen();
                    }
                    break;
            }
            return new Promise(function (res, rej) {
                res("返回值");
            });
        }

        exports('admin', pearAdmin);
    }
)
