> [!WARNING]
> 此项目现处在最早期阶段！\
> Early-stage development in progress!

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

  [中文](./README.md) | [English](./README_en-us.md)

  <a>
    <img alt="Static Badge" src="https://img.shields.io/badge/.NET-9.0-512BD4">
  </a>
  <a>
    <img alt="Static Badge" src="https://img.shields.io/badge/WPF-%E5%BA%94%E7%94%A8%E7%A8%8B%E5%BA%8F-0078D4">
  </a>
</div>

# 预览
<div align="center">
    <img src="./Image/YL3.png" alt="图片1" width="30%" align="center">
    <img src="./Image/BS.png" alt="图片2" width="30%" align="center">
</div>

# 功能
## 图标操作

1. **图标拖动**:  
   - 支持单个/多个图标拖拽重新定位
   - 允许将图标拖入系统文件管理器或网页端文件管理器  
   (注意：此功能仍在开发中，可能存在部分问题)

2. **右键上下文菜单**:  
   - 在图标上触发原生 Windows 10 上下文菜单  
   (Windows 11 上下文菜单支持待实现)

3. **双击/文件拖放**:  
   - 双击图标执行默认操作
   - 将文件拖放到图标上时：  
     - 对于 `.exe`/`.lnk` 图标：将用该软件打开文件
     - 对于文件夹图标：功能开发中

## 盒子操作

1. **盒子上下文菜单**:  
   在盒子内容区域右键可进行以下操作：  
   - 新建盒子
   - 移除盒子
   - 自动盒子
   - 设置

2. **盒子创建**:  
   - 通过设置或上下文菜单创建

3. **盒子拖拽**:  
   - 拖动盒子时有边缘吸附定位

4. **盒子尺寸调整**:  
   - 拖拽边框/边角调整尺寸
   - 按住 `Ctrl` 键可按网格对齐调整

5. **折叠/展开**:  
   - 双击盒子头部切换状态
   - 启用“自动盒子”后，将根据鼠标是否悬停自动展开/收起


# 文件结构
```
NewDesktop/
├── Behaviors/                                # 自定义交互行为
│   ├── ContextMenuBehavior.cs
│   ├── DoubleClickBehavior.cs                # 双击交互行为（打开/选择操作）
│   ├── DragBehavior.cs                       # 拖放功能实现（支持Canvas内元素拖拽和边缘吸附）
│   └── IconDragDrop.cs
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
