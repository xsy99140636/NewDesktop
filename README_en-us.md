> [!WARNING]
> 此项目现处在最早期阶段！\
> Early-stage development in progress!

<div align="center">
  <a>
    <img alt="" width="700"heigth="142"src="">
  </a>
  <h1>NewDesktop</h1>
  <p>
    This is a Windows desktop rewrite project based on WPF.
  </p>
  <p>
    A desktop organization and beautification software that plans to add features while retaining the native experience to the greatest extent.
  </p>

[中文](./README.md) | [English](./README_en-us.md)

  <a>
    <img alt="Static Badge" src="https://img.shields.io/badge/.NET-9.0-512BD4">
  </a>
  <a>
    <img alt="Static Badge" src="https://img.shields.io/badge/WPF-%E5%BA%94%E7%94%A8%E7%A8%8B%E5%BA%8F-0078D4">
  </a>
</div>

# Preview
<div align="center">
    <img src="./Image/YL3.png" alt="Image1" width="30%" align="center">
    <img src="./Image/BS.png" alt="Image2" width="30%" align="center">
</div>

# Features
## Icon Operations

1. **Icon Dragging**:  
   - Supports dragging single/multiple icons for repositioning  
   - Allows dragging icons to system file managers or web-based file managers  
   *(Note: This feature is under active development and may contain unresolved issues)*

2. **Right-click Context Menu**:  
   - Triggers the native Windows 10 context menu on icons  
   *(Windows 11 context menu support is pending implementation)*

3. **Double-click/File Drop**:  
   - Double-click icons to execute default actions  
   - When dragging files onto icons:  
     - For `.exe`/`.lnk` icons: Opens files with the corresponding application  
     - For folder icons: *Functionality in development*  

## Box Operations

1. **Box Context Menu**:  
   Right-click in the box content area to access:  
   - Create New Box  
   - Remove Box  
   - Auto-box  
   - Settings  

2. **Box Creation**:  
   - Create via settings or context menu  

3. **Box Dragging**:  
   - Drag boxes with edge snapping for precise positioning  

4. **Box Resizing**:  
   - Drag borders/corners to adjust dimensions  
   - Hold `Ctrl` for grid-aligned resizing  

5. **Collapse/Expand**:  
   - Double-click box header to toggle state  
   - Enable "Auto-box" to automatically expand/collapse based on mouse hover  

# File Structure
```
NewDesktop/
├── Behaviors/ # Custom interaction behaviors
│ ├── ContextMenuBehavior.cs
│ ├── DoubleClickBehavior.cs # Double-click interaction behavior (open/select operation)
│ ├── DragBehavior.cs # Drag-drop implementation (supports in-canvas element dragging and edge snapping)
│ └── IconDragDrop.cs
├── Models/ # Data models
│ ├── Box.cs # Box model
│ ├── Icon.cs # Icon model
│ └── PositionedObject.cs # Base class
├── Services/ # Service layer
│ └── SaveLoadService.cs # Save/load service
├── Shell/ # System integration module
│ ├── ContextMenu.cs # Invokes native Windows Shell context menu (files/folders)
│ └── DesktopAttacher.cs # Desktop window attacher (embeds application to desktop layer)
├── Styles/ # Style resources
│ ├── Dictionary.xaml # Global resource dictionary (scrollbar style)
│ └── ListViewStyle.xaml # ListView style
├── ViewModels/ # View models
│ ├── BoxModel.cs # Box view model
│ ├── IconModel.cs # Icon view model
│ └── MainViewModel.cs # Main view model
├── Views/ # View layer
│ ├── Common/ # Common control library
│ │ └── ColorPickerUserControl.xaml.cs # Color picker control
│ ├── BoxView.xaml # Box control
│ │ └── BoxView.xaml.cs # Box control backend logic
│ ├── IconView.xaml # Icon control
│ │ └── IconView.xaml.cs # Icon control backend logic
│ └── SettingsPage/ # Settings feature control views
│ ├── BoxSettingsPage.xaml # Box property settings page
│ │ └── BoxSettingsPage.xaml.cs
│ ├── HomeSettingsPage.xaml # Main interface settings page
│ │ └── HomeSettingsPage.xaml.cs
│ ├── ss.xaml # [Temporary name] Temporary settings page
│ │ └── ss.xaml.cs
│ └── SaveSettingsPage.xaml # Layout save rules settings page
│ └── SaveSettingsPage.xaml.cs
├── App.xaml # Application entry
│ └── App.xaml.cs
├── MainWindow.xaml # Main window (desktop canvas host)
│ └── MainWindow.xaml.cs # Window extension logic
└── SettingsWindow.xaml # Independent settings window
└── SettingsWindow.xaml.cs # Settings window logic (navigation framework management)
```
