# DynamicWin

**Dynamic Island utility for Windows**

DynamicWin là ứng dụng tiện ích dành cho Windows, lấy cảm hứng từ Dynamic Island trên iPhone. Ứng dụng tập trung vào một giao diện nhỏ gọn, hiện đại, có khả năng mở rộng để hiển thị thông tin hệ thống, điều khiển media, phím tắt và các tiện ích khác.

> **Project Status:** In Development

---

## Technology Stack

* **Language:** C#
* **Framework:** WPF
* **Platform:** Windows
* **Runtime:** .NET
* **UI:** XAML
* **Windows Integration:** Win32 / Windows API

---

## Planned Architecture

```text
DynamicWin/
│
├── Core/
│   ├── DynamicCore.cs
│   ├── WindowManager.cs
│   ├── WidgetManager.cs
│   └── AnimationManager.cs
│
├── UI/
│   │
│   ├── Core/
│   │   ├── DynamicIsland.xaml
│   │   └── DynamicIsland.xaml.cs
│   │
│   ├── Widgets/
│   │   ├── Battery/
│   │   │   ├── BatteryWidget.xaml
│   │   │   └── BatteryWidget.xaml.cs
│   │   │
│   │   ├── System/
│   │   │   ├── SystemWidget.xaml
│   │   │   └── SystemWidget.xaml.cs
│   │   │
│   │   ├── Media/
│   │   │   ├── MediaWidget.xaml
│   │   │   └── MediaWidget.xaml.cs
│   │   │
│   │   ├── Network/
│   │   │   ├── NetworkWidget.xaml
│   │   │   └── NetworkWidget.xaml.cs
│   │   │
│   │   ├── Timer/
│   │   │   ├── TimerWidget.xaml
│   │   │   └── TimerWidget.xaml.cs
│   │   │
│   │   └── Shortcuts/
│   │       ├── ShortcutWidget.xaml
│   │       └── ShortcutWidget.xaml.cs
│   │
│   └── Components/
│       ├── DWButton.xaml
│       ├── DWSlider.xaml
│       ├── DWProgressBar.xaml
│       └── DWIcon.xaml
│
├── Services/
│   ├── BatteryService.cs
│   ├── SystemMonitorService.cs
│   ├── MediaService.cs
│   ├── NetworkService.cs
│   ├── NotificationService.cs
│   ├── ClipboardService.cs
│   └── ShortcutService.cs
│
├── Windows/
│   ├── WindowsApi.cs
│   ├── WindowHelper.cs
│   └── NativeMethods.cs
│
├── Utils/
│   ├── Animator.cs
│   ├── Easing.cs
│   ├── SettingsManager.cs
│   └── Logger.cs
│
├── Resources/
│   ├── Icons/
│   ├── Images/
│   └── Fonts/
│
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
│
├── DynamicWin.csproj
└── README.md
```

---

## Architecture Overview

### Core

Chứa các thành phần điều phối chính của DynamicWin.

* `DynamicCore` — quản lý trạng thái và hoạt động chính của Dynamic Island.
* `WindowManager` — quản lý vị trí, kích thước, trạng thái cửa sổ.
* `WidgetManager` — quản lý các widget.
* `AnimationManager` — quản lý animation và transition.

### UI

Chứa toàn bộ giao diện WPF.

#### Core UI

Dynamic Island chính của ứng dụng.

```text
DynamicIsland
       │
       ├── Collapsed State
       │
       └── Expanded State
              │
              ├── Widgets
              └── Quick Actions
```

#### Widgets

Các tiện ích được thiết kế độc lập để có thể thêm hoặc loại bỏ mà không ảnh hưởng đến Core.

Ví dụ:

* Battery
* CPU / RAM / GPU
* Network
* Media
* Timer
* Shortcuts
* Notifications
* Clipboard

#### Components

Các thành phần giao diện dùng lại nhiều nơi:

* Button
* Slider
* Progress Bar
* Icon
* Animation components

---

## Services

Services chịu trách nhiệm lấy dữ liệu hoặc giao tiếp với hệ thống Windows.

```text
Windows
   │
   ├── BatteryService
   ├── SystemMonitorService
   ├── MediaService
   ├── NetworkService
   ├── NotificationService
   └── ClipboardService
           │
           ▼
       WidgetManager
           │
           ▼
           UI
```

Điều này giúp UI không phải trực tiếp xử lý Windows API hoặc logic hệ thống.

---

## Windows

Chứa phần tích hợp với Windows API.

Các chức năng dự kiến:

* Transparent Window
* Always on Top
* Window positioning
* Multi-monitor support
* Global hotkey
* Mouse interaction
* Windows notifications
* Media control
* System integration

---

## Interaction Concept

DynamicWin sẽ có hai trạng thái chính:

### Collapsed

Dynamic Island ở trạng thái nhỏ gọn.

```text
              ┌─────────────┐
              │  DynamicWin │
              └─────────────┘
```

### Expanded

Khi người dùng di chuột hoặc tương tác, Dynamic Island mở rộng để hiển thị các widget.

```text
        ┌───────────────────────────────┐
        │  ......................       │
        └───────────────────────────────┘
```

Các widget có thể xuất hiện bằng animation và biến mất khi người dùng không còn tương tác.

---

## Dynamic Core

Một trong những ý tưởng chính của DynamicWin là **Dynamic Core**.

Thay vì luôn hiển thị một panel lớn, ứng dụng sẽ ưu tiên một giao diện nhỏ gọn ở trạng thái bình thường.

Khi người dùng tương tác:

```text
             Hover
               ↓
        ┌─────────────┐
        │ Dynamic Core │
        └─────────────┘
               ↓
        Expand Animation
               ↓
┌───────────────────────────────────┐
│    Battery │  Network │    Media  │
│   Timer    │  Actions │  Settings │
└───────────────────────────────────┘
```

Các icon/widget có thể được triển khai với animation riêng và có khả năng thu hồi trở lại Dynamic Core.

---

## Planned Features

### Core

* [ ] Dynamic Island Core
* [ ] Transparent window
* [ ] Rounded UI
* [ ] Always-on-top
* [ ] Hover interaction
* [ ] Expand / Collapse animation
* [ ] Multi-monitor support

### System

* [ ] Battery monitor
* [ ] CPU monitor
* [ ] RAM monitor
* [ ] GPU monitor
* [ ] Temperature monitor
* [ ] Network monitor
* [ ] Disk usage

### Media

* [ ] Play / Pause
* [ ] Next / Previous
* [ ] Volume control
* [ ] Media information
* [ ] Audio visualizer

### Productivity

* [ ] Timer
* [ ] Stopwatch
* [ ] Pomodoro
* [ ] Clipboard
* [ ] Quick shortcuts
* [ ] Screenshot

### Windows Integration

* [ ] Windows notifications
* [ ] Global hotkey
* [ ] Startup with Windows
* [ ] System tray
* [ ] Wi-Fi
* [ ] Bluetooth
* [ ] Volume
* [ ] Brightness

### Customization

* [ ] Theme
* [ ] Animation settings
* [ ] Widget management
* [ ] Shortcut customization
* [ ] Position customization
* [ ] User settings

---

## Development Roadmap

### Phase 1 — Core

* [ ] Create WPF application
* [ ] Transparent window
* [ ] Dynamic Island shape
* [ ] Rounded corners
* [ ] Always-on-top
* [ ] Position at top of screen

### Phase 2 — Interaction

* [ ] Hover detection
* [ ] Expand animation
* [ ] Collapse animation
* [ ] Widget system
* [ ] Animation manager

### Phase 3 — System Widgets

* [ ] Battery
* [ ] CPU
* [ ] RAM
* [ ] Network
* [ ] Clock
* [ ] Volume

### Phase 4 — Media

* [ ] Media control
* [ ] Spotify support
* [ ] Media information
* [ ] Audio visualizer

### Phase 5 — Productivity

* [ ] Timer
* [ ] Pomodoro
* [ ] Clipboard
* [ ] Quick actions
* [ ] Application shortcuts

### Phase 6 — Windows Integration

* [ ] Windows notifications
* [ ] Global hotkey
* [ ] Startup
* [ ] System tray
* [ ] Windows API integration

### Phase 7 — Customization & Polish

* [ ] Settings
* [ ] Themes
* [ ] Custom widgets
* [ ] Animation tuning
* [ ] Performance optimization
* [ ] Installer
* [ ] Release build

---

## Project Goal

DynamicWin hướng tới một **Windows Dynamic Island nhỏ gọn, hiện đại và có tính tương tác cao**, trong đó người dùng có thể truy cập nhanh các thông tin và chức năng thường xuyên sử dụng mà không cần mở các ứng dụng riêng biệt.

Kiến trúc được thiết kế theo hướng **Core + Widget + Service**, giúp dự án có thể tiếp tục mở rộng thêm nhiều chức năng mà không phải thay đổi toàn bộ hệ thống.
