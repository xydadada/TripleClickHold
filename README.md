# TripleClickHold

一个独立的 Windows 三倍点击保持器。短按一次会输出多次完整点击；长按时，前面的点击完成后保留最后一次按下，直到物理按键松开。

这是一个本地运行的桌面工具，不联网、不安装服务、不安装驱动，也不修改旧版 `SideButtonAutoClicker`。

## 功能

- 左键、右键分别启用或关闭。
- 点击次数可调为 1–20 次，默认 3 次。
- 间隔可使用固定值，也可在最小–最大毫秒范围内逐事件随机取值（0–100 ms）。
- 可选择长按时是否保持最后一次按下。
- Mouse4 / Mouse5（鼠标侧键）按下任意一个即可切换总开关；侧键是专用切换键，不执行浏览器前进/后退。
- 切换热键和紧急退出热键可在设置窗口中修改，默认分别为 F8 和 Ctrl+Alt+F11。
- 切换时在鼠标附近显示短暂状态提示；提示不激活窗口、不接收鼠标点击。
- 首次启动显示设置窗口；默认关闭，不会自动拦截鼠标输入。
- 设置保存到 `%LocalAppData%\\TripleClickHold\\settings.json`。

## 安全边界

程序使用 Windows 低级鼠标钩子来区分物理输入和模拟输出，因此需要管理员权限，以便与管理员权限运行的游戏处于同一输入权限级别。

钩子回调只解析事件并把命令放入队列；文件读写、界面更新和模拟输出都在其他线程完成。模拟事件带有专用标记，不会再次进入自己的物理输入路径。新的物理按下会取消尚未完成的旧点击序列，避免快速点击越积越多。

不要同时运行其他会拦截左右键的连点器。使用前请确认目标游戏或软件允许模拟输入；项目不保证任何特定游戏的兼容性。

## 使用

1. 从 [Releases](https://github.com/xydadada/TripleClickHold/releases/latest) 直接下载 `TripleClickHold-Setup-v1.1.0.exe` 安装包；也可以下载 `TripleClickHold-win-x64.zip` 免安装运行。
2. 安装包会创建桌面/开始菜单快捷方式和卸载项，并保留用户设置。首次启动会打开设置窗口，程序默认处于关闭状态。
3. 在设置窗口调整点击次数、间隔、左右键和热键，点击“保存并应用”。
4. 在游戏或目标软件中按 Mouse4/Mouse5 或切换热键开启/关闭。状态会在鼠标附近短暂显示。
5. 需要立即退出时使用设置里的退出热键，或从托盘菜单选择“退出”。

## 从源码构建

要求：Windows 10/11、.NET 8 SDK、x64 环境。

```powershell
dotnet restore TripleClickHold.csproj
dotnet build TripleClickHold.csproj -c Release -r win-x64
dotnet publish TripleClickHold.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

只做不拦截输入的离线检查：

```powershell
dotnet .\\bin\\Release\\net8.0-windows\\win-x64\\TripleClickHold.dll --self-check
```

预览设置窗口布局（同样不会安装钩子或发送输入）：

```powershell
dotnet .\\bin\\Release\\net8.0-windows\\win-x64\\TripleClickHold.dll `
  --render-settings .\\settings-preview.png
```

## 项目结构

- `MainForm.cs`：托盘、热键、唯一设置窗口和程序生命周期。
- `Settings.cs` / `SettingsForm.cs`：配置模型、持久化和设置界面。
- `MouseHookThread.cs`：专用低级鼠标钩子线程，包含侧键切换和左右键物理事件解析。
- `ClickWorker.cs`：独立输出线程；实现最新点击取消和按键释放保护。
- `InputPlan.cs` / `DelayChooser.cs`：点击序列和固定/随机间隔逻辑。
- `MouseOutput.cs` / `NativeMethods.cs`：Win32 输入输出边界。
- `StatusOverlayForm.cs`：不抢焦点、可穿透点击的状态提示。
- `SelfCheck.cs`：无输入离线检查。
- `docs/ARCHITECTURE.md`：线程边界、状态机和故障处理说明。
- `.github/workflows/build.yml`：Windows CI、离线检查和 ZIP 构建。

## 版本与反馈

版本记录见 [CHANGELOG.md](CHANGELOG.md)。提交问题时请提供 Windows 版本、程序版本、设置摘要和可复现步骤；不要上传 `%LocalAppData%\\TripleClickHold\\settings.json` 中可能包含的个人配置。

项目采用 MIT License，详见 [LICENSE](LICENSE)。
