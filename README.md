# NewDesktop
这是一个基于WPF的桌面重写计划，是个桌面整理软件加美化软件，计划在最大限度保留原生体验的前提下加点东西。
![image](https://github.com/Yeilintong/NewDesktop/blob/main/Image/YL1.png)
# 文件结构
```
NewDesktop/
├── Behaviors/                # 自定义交互行为
│   └── DragBehavior.cs       # 盒子拖拽行为实现类
├── Models/                   # 数据模型层
│   ├── Box.cs               
│   ├── Icon.cs              
│   └── PositionedObject.cs  
├── ViewModels/               # 视图模型层
│   ├── BoxModel.cs          
│   ├── IconModel.cs         
│   └── MainViewModel.cs     
├── Views/                    # 视图层
│   ├── BoxView.xaml          # 盒子控件
│   ├── IconView.xaml         # 图标控件  
│   └── UserControl1.xaml     # [临时文件]
├── App.xaml                  # 应用入口
└── MainWindow.xaml.cs        # 窗口扩展逻辑（桌面嵌入建议暂时禁用）
```
# 计划表

| 功能           | 进度   | 备注                   |
| ------------ | ---- | -------------------- |
| 整理盒子控件       | 95%  | 盒子重写完成，多选功能施工中            |
| 整理盒子拖动改变位置功能 | 100% | 已经可用                 |
| 整理盒子拖动改变大小功能 | 100% | 已经可用                 |
| 图标控件         | 50%  | 文字等需添加               |
| 图标控件拖动改变位置功能 | 90%  | 已经可用，但画布拖动受透明度影响，待解决 |
| 布局保存加载功能     | 50%  | 施工中                  |
| 桌面文件监视功能     | 0%   |                      |
