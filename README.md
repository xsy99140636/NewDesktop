<div align="center">
  <a>
    <img alt="" width="700"heigth="142"src="">
  </a>
  <h1>NewDesktop</h1>
  <p>
    这是一个基于WPF的Win桌面重写计划。
  </p>
  <p>
    是个桌面整理软件加美化软件，计划在最大限度保留原生体验的前提下加点东西。
  </p>

  <a>
    <img alt="Static Badge" src="https://img.shields.io/badge/.NET-9.0-512BD4?link=https%3A%2F%2Fdotnet.microsoft.com%2F">
  </a>
</div>

# NewDesktop
这是一个基于WPF的桌面重写计划，是个桌面整理软件加美化软件，计划在最大限度保留原生体验的前提下加点东西。
![image](https://github.com/Yeilintong/NewDesktop/blob/main/Image/YL3.png)
![image](https://github.com/Yeilintong/NewDesktop/blob/main/Image/BS.png)
# 文件结构
```
NewDesktop/
├── Behaviors/                                # 自定义交互行为
│   ├── DoubleClickBehavior.cs                # 双击交互行为（打开/选择操作）
│   └── DragBehavior.cs                       # 拖放功能实现（支持Canvas内元素拖拽和边缘吸附）
├── Models/                                   # 数据模型
│   ├── Box.cs                                # 盒子模型
│   ├── Icon.cs                               # 图标模型
│   └── PositionedObject.cs                   # 基类
├── Services/                                 # 服务层
│   └── SaveLoadService.cs                    # 保存加载
├── Shell/                                    # 系统集成模块
│   ├── ContextMenu.cs                        # 调用原生Windows Shell右键菜单（文件/文件夹）
│   └── DesktopAttacher.cs                    # 窗口桌面附着器（将应用嵌入桌面层）
├── Styles/                                   # 样式资源
│   ├── Dictionary.xaml                       # 全局资源字典（滚动条样式）
│   └── ListViewStyle.xaml                    # ListView样式
├── ViewModels/                               # 视图模型
│   ├── BoxModel.cs                           # 盒子视图模型
│   ├── IconModel.cs                          # 图标视图模型
│   └── MainViewModel.cs                      # 主视图模型
├── Views/                                    # 视图层
│   ├── Common/                               # 通用控件库
│   │   └── ColorPickerUserControl.xaml.cs    # 颜色选择器控件
│   ├── BoxView.xaml                          # 盒子控件
│   │   └── BoxView.xaml.cs                   # 盒子控件后台逻辑
│   ├── IconView.xaml                         # 图标控件
│   │   └── IconView.xaml.cs                  # 图标控件后台逻辑
│   └── SettingsPage/                         # 设置功能控件视图
│       ├── BoxSettingsPage.xaml              # 盒子属性设置页
│       │   └── BoxSettingsPage.xaml.cs
│       ├── HomeSettingsPage.xaml             # 主界面设置页
│       │   └── HomeSettingsPage.xaml.cs
│       ├── ss.xaml                           # [待命名] 临时设置页
│       │   └── ss.xaml.cs
│       └── SaveSettingsPage.xaml             # 布局保存规则设置页
│           └── SaveSettingsPage.xaml.cs
├── App.xaml                                  # 应用入口
│   └── App.xaml.cs
├── MainWindow.xaml                           # 主窗口（承载桌面画布）
│   └── MainWindow.xaml.cs                    # 窗口扩展逻辑
└── SettingsWindow.xaml                       # 独立设置窗口
    └── SettingsWindow.xaml.cs                # 设置窗口逻辑（导航框架管理）
```
# 计划表

| 功能           | 进度   | 备注                   |
| ------------ | ---- | -------------------- |
| 整理盒子控件       | 95%  | 盒子重写完成，多选功能施工中            |
| 整理盒子拖动改变位置功能 | 100% | 已经可用                 |
| 整理盒子拖动改变大小功能 | 100% | 已经可用                 |
| 图标控件         | 50%  | 文字等需添加               |
| 图标控件拖动改变位置功能 | 90%  | 已经可用，支持多个同时拖动，但画布拖动受透明度影响，待解决 |
| 布局保存加载功能     | 100%  | 已经可用                  |
| 桌面文件监视功能     | 0%   |                      |
