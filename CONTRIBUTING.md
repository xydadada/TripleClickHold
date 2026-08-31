# Contributing

感谢贡献。请先在 issue 中说明要解决的问题或要加入的行为，再提交 pull request。

## 开发约定

- 目标框架为 .NET 8，运行平台为 Windows x64。
- 不要在钩子回调中做文件读写、阻塞等待、界面操作或直接生成模拟输入。
- 不要提交 `bin/`、`obj/`、发布目录、用户配置或临时截图。
- 新的输入行为必须有无输入单元/离线检查，并说明无法自动验证的真实游戏兼容性边界。

## 本地检查

```powershell
dotnet restore TripleClickHold.csproj
dotnet build TripleClickHold.csproj -c Release -r win-x64
dotnet .\\bin\\Release\\net8.0-windows\\win-x64\\TripleClickHold.dll --self-check
```

请在 pull request 中描述改动、验证命令和可能影响的输入行为。
