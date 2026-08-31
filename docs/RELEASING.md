# Releasing

1. 更新 `TripleClickHold.csproj` 的版本号和 `CHANGELOG.md`。
2. 在 Windows 上运行构建和离线检查：

   ```powershell
   dotnet build -c Release -r win-x64
   dotnet .\\bin\\Release\\net8.0-windows\\win-x64\\TripleClickHold.dll --self-check
   ```

3. 发布单文件：

   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true `
     -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
   ```

4. 将发布目录压缩为 `TripleClickHold-win-x64.zip`，并记录 SHA-256。
5. 使用 `installer/TripleClickHold.iss` 编译 `TripleClickHold-Setup-v{version}.exe`。安装包是 per-user 安装，不需要安装服务；卸载时保留用户配置。
6. 通过 GitHub Actions 或 GitHub Release 发布 ZIP、安装包和校验文件；不要把用户配置、调试截图和临时目录放进压缩包。
