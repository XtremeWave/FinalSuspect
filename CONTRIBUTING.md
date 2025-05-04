## 贡献指南

### 前置条件

您的工作环境上需要安装下列内容:
- .NET 6
- Git
- (可选但推荐) Visual Studio / Jetbrains Rider / Visual Studio Code / Mono Develop

### 提交贡献

将FinalSuspect克隆至本地:
```bash
git clone https://github.com/XtremeWave/FinalSuspect.git
```
> [!TIP]
> 若您复刻了自己的仓库,则需要将`XtremeWave`替换为您的GitHub用户名。

使用此命令提交并推送:
```bash
git add --all
git commit -m "您的提交信息"
git push origin FinalSus
```
> [!Warning]
> `--all`参数将添加所有更改过的文件。\
> 这可能会方便开发者一次性提交很多文件,但也可能提交不必要的文件(如构建缓存,IDE项目设置文件)。\
> 请在提交时忽略他们或在后续删除。

#### 本地构建FinalSuspect

```bash
dotnet restore
dotnet build
```
