# TripleClickHold

这是独立的三倍点击保持器，不修改 `SideButtonAutoClicker`。

- 左右键均使用“两次完整点击 + 第三次按下保持”的状态机。
- F8 切换；Ctrl+Alt+F11 紧急退出；启动默认关闭。
- 设置窗口由 `SettingsForm` 提供，配置通过 `SettingsStore` 保存在 `%LocalAppData%\\TripleClickHold\\settings.json`；设置变更不得在钩子回调中做文件或界面操作。
- 钩子回调只解析并排队物理事件；模拟输出带专用标记并由独立线程发送。
- `--self-check` 只能运行无输入检查；不要用真实桌面点击做自动化测试。
