# Luban

![icon](docs/images/logo.png)

[![license](http://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT) ![star](https://img.shields.io/github/stars/focus-creative-games/luban?style=flat-square) ![release](https://img.shields.io/github/v/release/GameFrameX/luban?style=flat-square)

**中文** | [English](./README_EN.md)

luban是一个强大、易用、优雅、稳定的游戏配置解决方案。它设计目标为满足从小型到超大型游戏项目的简单到复杂的游戏配置工作流需求。

luban可以处理丰富的文件类型，支持主流的语言，可以生成多种导出格式，支持丰富的数据检验功能，具有良好的跨平台能力，并且生成极快。
luban有清晰优雅的生成管线设计，支持良好的模块化和插件化，方便开发者进行二次开发。开发者很容易就能将luban适配到自己的配置格式，定制出满足项目要求的强大的配置工具。

luban标准化了游戏配置开发工作流，可以极大提升策划和程序的工作效率。

自 **5.0.0** 起，Luban 面向 AI Agent / IDE 提供 **AI Native** 能力：官方 Skills、机器可读 schema 与报错、`Luban.Agent` CLI，以及 MCP Server，让加表、查 schema、校验与生成可以稳定地交给 Agent 完成。

## 核心特性

- 丰富的源数据格式。支持excel族(csv,tsv,xls,xlsx,xlsm)、json、xml、yaml、lua等
- 丰富的导出格式。 支持生成binary、json、bson、xml、lua、yaml等格式数据
- 增强的excel格式。可以简洁地配置出像简单列表、子结构、结构列表，以及任意复杂的深层次的嵌套结构
- 完备的类型系统。不仅能表达常见的规范行列表，由于**支持OOP类型继承**，能灵活优雅表达行为树、技能、剧情、副本之类复杂GamePlay数据
- 支持多种的语言。内置支持生成c#、java、go、cpp、lua、python、javascript、typescript、rust、php、erlang、godot 等语言代码，同时还能通过protobuf之类消息方案支持其他语言
- 支持主流的消息方案。 protobuf(schema + binary + json)、flatbuffers(schema + json)、msgpack(binary)
- 强大的数据校验能力。ref引用检查、path资源路径、range范围检查等等
- 完善的本地化支持
- **AI Native（5.0.0+）**。官方 Agent Skills、`-c schema-json`、`--errorFormat json`、`Luban.Agent`（校验 / 查表 / describe）、`Luban.Mcp`（IDE 工具调用）
- 支持所有主流的游戏引擎和平台。支持Unity、Unreal、Cocos2x、Godot、微信小游戏等
- 良好的跨平台能力。能在Win,Linux,Mac平台良好运行。
- 支持所有主流的热更新方案。hybridclr、ilruntime、{x,t,s}lua、puerts等
- 生成的代码未调用任何反射接口，兼容[Obfuz](https://github.com/focus-creative-games/obfuz)、Obfuscator、Confuser、.Net Refactor等常见代码混淆和加固工具。
- 清晰优雅的生成管线，很容易在luban基础上进行二次开发，定制出适合自己项目风格的配置工具。

## GameFrameX 分支说明

本仓库是 [focus-creative-games/luban](https://github.com/focus-creative-games/luban) 的 GameFrameX 分支，已合并官方 **v5.1.0**：吸收官方 v4 / v5 的基座重构与 AI 能力，同时保留下列 GameFrameX 功能与修复。分支相关问题请提交到 [GameFrameX/luban Issues](https://github.com/GameFrameX/luban/issues)。

### 与官方版本的功能差异

| 功能 | 说明 |
|:-----|:-----|
| split 代码生成目标 | 原目标名追加 `-split`，定义与运行时代码分文件输出，见[说明](#增加-split-代码生成目标) |
| `cs-litjson` / `cs-litjson-split` | 基于 GameFrameX LitJson 的 C# 代码生成目标，见[说明](#增加-cs-litjson--cs-litjson-split-代码生成目标) |
| C# 运行时形态 | `cs-bin`、`cs-dotnet-bin`、`cs-dotnet-json`、`cs-simple-json`、`cs-litjson` 及其 `-split` 变体生成 GameFrameX 运行时形态：表类继承 `BaseDataTable<T>`，`Tables.Init(ConfigComponent)` 后通过 `LoadAsync` 异步加载；`cs-code`、`cs-editor-json`、`cs-newtonsoft-json` 不使用该形态 |
| 文件名自动导表 | `tableImporter.name=gameframex`，按文件名与目录推导表名、分组与命名空间，见[说明](#增加自动导表的文件名称扩展识别) |
| 导表排除目录 | `--excludePath`，见[说明](#增加导表排除目录参数---excludepath) |
| 本地化文件夹 | `l10n.provider=gameframex`，`l10n.textFile.path` 支持多个文件夹，见[说明](#增加本地化的文件夹配置支持) |
| text 内联本地化 | `l10n.inline.*`，见[说明](#支持-text-内联本地化) |
| 枚举自动收集 | `auto_extend`，见[说明](#增加枚举自动收集autoextend能力) |
| 同名 `--xargs` 多值 | 同名参数可重复出现，列表语义的参数按出现顺序取全部值 |
| 目标框架 | 统一为 `net10.0`（官方为 `net8.0`） |
| `Luban.GUI` | 基于 Avalonia 的桌面导表工具（`src/Luban.GUI`） |
| Docker 镜像 | 见 [Docker](#docker) |

### 自定义修复在合并 v5.1.0 后的状态

| 修复 | 原提交 | 合并后状态 |
|:-----|:-------|:-----------|
| l10n：`TranslateText` 按继承关系生成 `virtual` / `override` | `24774645` | 保留 |
| l10n：内联本地化 xlsx 增量合并、无变化时不重写、源语言 value 变化时同步 | `a8fd260c`、`b2a3e7b7`、`421349aa` | 保留 |
| cs-dotnet-json：表类自持 `_loadFunc`，不再链基类 JSON 构造 | `e1f39884` | 保留 |
| cs-dotnet-json：非空 ref 字段取值改用 `TryGet` | `27b318b8` | 保留 |
| cs-dotnet-bin：list / one 表 `LoadAsync` 补 `override`；map 表重载前清空 `LongDataMaps` | `ff138c33` | 保留 |
| cs-bin-split：`bean.def` 模板移除多余的 `using SimpleJSON` | `95b08b0b` | 保留 |
| table 模板 list / one 模式生成重复字段与构造函数 | `92aea61d` | 保留 |
| 读取字节使用 `ReadExactly` 避免短读 | `1e2333f3` | 保留 |
| 整型字段支持 `0x` 十六进制字面量 | `dd520088` | 由官方实现取代（官方 `42e5a380`，`LoadDataUtil.TryParseExcelIntFromNumberOrConstAlias`） |
| `map<int, _>` 的 key 支持 `0x` 十六进制字面量 | `a93ab40f` | 保留（官方未覆盖 map key 解析路径） |
| Bean 全名与字段同名时编译错误、`[*items,,,*items]` 多行标记、末列合并单元格越界 | `368c5344`、`d5cff3d4`、`d6c7a87c`、`534dd6c0` | 官方已有等价修复，以官方实现为准 |

## AI 支持

官方 v5 的 AI Native 能力已随 v5.1.0 合入本分支：

| 能力 | 入口 | 用途 |
|:-----|:-----|:-----|
| Agent Skills | [`ai/skills/`](./ai/skills) | 加表、填表、schema 设计、校验器、生成排错、运行时加载 6 个技能 |
| schema-json | `-c schema-json` | 输出机器可读的 `schema.json` |
| JSON 诊断 | `--errorFormat json` | 失败时向 stderr 输出结构化 JSON 诊断 |
| Luban.Agent | [`src/Luban.Agent`](./src/Luban.Agent/README.md) | `capabilities` / `list-tables` / `describe` / `schema` / `validate`，stdout 输出单个 JSON 结果 |
| Luban.Mcp | [`src/Luban.Mcp`](./src/Luban.Mcp/README.md) | stdio MCP Server，供 IDE / Agent 调用 |

### 构建

本分支的 AI 工具以三个独立输出目录发布（Docker 镜像只包含主程序 `Luban`）：

```bash
dotnet build src/Luban/Luban.csproj -c Release -o <out>/Luban
dotnet build src/Luban.Agent/Luban.Agent.csproj -c Release -o <out>/Luban.Agent
dotnet build src/Luban.Mcp/Luban.Mcp.csproj -c Release -o <out>/Luban.Mcp
```

### 启动 MCP Server

在 MCP 客户端配置中注册（`mcpServers` 格式，`<out>` 换成实际输出目录）：

```json
{
  "mcpServers": {
    "luban": {
      "command": "dotnet",
      "args": ["<out>/Luban.Mcp/Luban.Mcp.dll"],
      "env": {
        "LUBAN_AGENT_DLL": "<out>/Luban.Agent/Luban.Agent.dll",
        "LUBAN_DLL": "<out>/Luban/Luban.dll",
        "LUBAN_DOC": "<luban-doc>/docs"
      }
    }
  }
}
```

- 必须设置 `LUBAN_AGENT_DLL` 与 `LUBAN_DLL`：未设置时 Luban.Mcp 只回退查找源码树内的 `bin/Release|Debug/net8.0`，本分支目标框架为 `net10.0`，回退路径不会命中。
- `LUBAN_DOC` 可选，指向 luban-doc 仓库的 `docs` 目录，供 `search_docs` 使用。
- 暴露的工具：`list_tables` / `get_schema` / `describe` / `validate` / `generate` / `search_docs`，参数见 [`src/Luban.Mcp/README.md`](./src/Luban.Mcp/README.md)。

### 使用技能

把 `ai/skills/` 下的技能目录复制到 Agent 的技能目录（如 `.cursor/skills/`、`~/.claude/skills/`），详见 [`ai/README.md`](./ai/README.md)。

### 命令行

```bash
# 导出机器可读 schema（schema.json 输出到 outputCodeDir）
dotnet <out>/Luban/Luban.dll --conf luban.conf -t all -c schema-json -d json -x outputCodeDir=<codeDir> -x outputDataDir=<dataDir>

# 失败时以 JSON 输出诊断（写入 stderr）
dotnet <out>/Luban/Luban.dll --conf luban.conf -t all -d json --errorFormat json -x outputDataDir=<dataDir>

# 只校验、不输出文件
dotnet <out>/Luban.Agent/Luban.Agent.dll validate --conf luban.conf -t all
```

文档：[AI 支持总览](https://www.datable.cn/docs/ai/overview) · [Skills](https://www.datable.cn/docs/ai/skills) · [Agent CLI](https://www.datable.cn/docs/ai/agent-cli) · [MCP](https://www.datable.cn/docs/ai/mcp) · [schema-json](https://www.datable.cn/docs/ai/schema-json)

## 修改和增加

### 增加 split 代码生成目标

在不破坏原有代码生成目标的前提下，新增一组以原目标名称追加 `-split` 后缀的代码生成目标。

split 目标用于将生成代码拆分为更细粒度的文件：定义信息输出到面向阅读、审查和 AI 上下文使用的 Def 文件中，解析、加载和运行时代码保留在 Impl 或运行时文件中。这样在只关注单个配置类型时，可以读取对应的 Def 文件，而不需要读取完整的全量 schema 文件。

#### 使用场景

- **AI 辅助开发**：把 Def 文件作为 AI 上下文提供给模型，只含字段与类型定义、不含反序列化实现，token 占用小、信噪比高。
- **代码审查与日常查阅**：只想确认某个配置表的字段含义时，直接打开对应的 Def 文件，不必在几千行的全量 schema 文件里定位。
- **降低合并冲突**：schema 调整集中在 Def 文件，与运行时代码的改动互不干扰，diff 更聚焦。
- **无侵入共存**：原有非 split 目标行为完全不变，两种目标可按导出 target 分别选择。

#### 使用方式

目标命名规则为「原代码生成目标 + `-split`」，把导出命令中的 `--codeTarget` 换成对应 split 目标即可，其余参数不变：

```text
dotnet Luban.dll --target client --dataTarget bin --codeTarget cs-bin-split --xargs outputDataDir=./Output/Data --xargs outputCodeDir=./Output/Code --conf ./luban.conf
```

Go 语言的 split 目标需要额外指定模块名：

```text
--xargs lubanGoModule=github.com/gameframex/config
```

各语言 split 输出的文件命名（以 `Tables` 命名空间下的 `ItemConfig` 表为例）：

| 语言 | 定义文件（Def） | 运行时文件（Impl） |
|:-----|:---------------|:------------------|
| C# | `Tables/ItemConfig.Def.cs` | `Tables/ItemConfig.Impl.cs`（不再生成合并的 `ItemConfig.cs`） |
| Go | `Tables.ItemConfig.def.go` | `Tables.ItemConfig.impl.go` |
| Rust | `cfg/src/Tables.ItemConfig.Def.rs`、`Tables.TbItemConfig.Def.rs`、`Tables.Def.rs` | `cfg/src/tables.rs`、`cfg/src/lib.rs` |
| C++ | `Tables.ItemConfig.Def.h` | `schema.h`、`Tables.ItemConfig.Impl.cpp` |
| Java | `Tables/ItemConfigDef.java`（定义伴生类） | `Tables/ItemConfig.java` |
| TypeScript | `Tables.ItemConfig.def.ts`、`Tables.def.ts` | `schema.ts` |
| Lua | `Tables.ItemConfig.def.lua` | 运行时 schema 文件 |
| PHP / GDScript | `Tables.ItemConfig.def.php` / `Tables.ItemConfig.def.gd` | 运行时 schema 文件 |

Def 文件只包含定义信息（字段、类型、常量声明），不含反序列化、构造、加载等运行时代码；该约束由验收脚本断言保证。

已支持的 split 目标：

| 语言 | 目标 |
|:-----|:-----|
| C# | `cs-simple-json-split`, `cs-litjson-split`, `cs-bin-split`, `cs-dotnet-json-split`, `cs-dotnet-bin-split` |
| Go | `go-json-split`, `go-bin-split` |
| Rust | `rust-json-split`, `rust-bin-split` |
| C++ | `cpp-rawptr-bin-split`, `cpp-sharedptr-bin-split` |
| Java | `java-json-split`, `java-bin-split` |
| TypeScript | `typescript-json-split`, `typescript-bin-split`, `typescript-protobuf-split` |
| Python | `python-json-split` |
| Lua | `lua-bin-split`, `lua-lua-split` |
| PHP | `php-json-split` |
| GDScript | `gdscript-json-split` |

拆分规则按语言能力保持一致语义：

- C#、Go、C++ 输出 Def/Impl 风格文件，便于区分定义上下文和运行时实现。
- Java 由于不支持跨文件拆分同一个 public class，输出运行时类和 `XxxDef` 定义伴生类。
- TypeScript、Python、Lua、PHP、GDScript 保留运行时 schema 文件，同时额外输出每个类型对应的 `*.def.*` 定义文件。

本地验收脚本：

```
BASE=/tmp/luban-split-code-targets-acceptance scripts/accept-split-code-targets.sh
```

### 增加 cs-litjson / cs-litjson-split 代码生成目标

在保留 `cs-simple-json` / `cs-simple-json-split`（继续输出基于 SimpleJSON 的代码）不变的前提下，新增 `cs-litjson` 与 `cs-litjson-split` 两个独立的 C# 代码生成目标，输出基于 GameFrameX LitJson 包（命名空间 `GameFrameX.LitJSON.Runtime`，主类 `JsonData` / `JsonMapper`）的反序列化代码。向后兼容，原有 `cs-simple-json` 系列目标行为不变，按运行时已引入的 JSON 库选择对应目标即可。

| 目标 | 对应的原目标 | 运行时依赖 | 输出形式 |
|:-----|:------------|:----------|:---------|
| `cs-litjson` | `cs-simple-json` | `GameFrameX.LitJSON.Runtime` | 单文件，定义与运行时代码合并输出 |
| `cs-litjson-split` | `cs-simple-json-split` | `GameFrameX.LitJSON.Runtime` | split 输出，定义信息在 Def 文件，运行时代码在 Impl 文件 |

#### 使用场景

- 运行时（尤其是 Unity 热更新层）已统一使用 GameFrameX LitJson 作为 JSON 库，希望配置加载与项目其余部分共用同一套 JSON 依赖，不再同时引入 SimpleJSON。
- 需要 split 粒度（定义 / 运行时分离）且使用 LitJson 反序列化的 C# 工程。

#### 使用方式

```text
--codeTarget cs-litjson        # 单文件输出，定义与运行时合并
--codeTarget cs-litjson-split  # Def / Impl 分离输出
```

前提：运行时工程已引入 GameFrameX LitJson 包（命名空间 `GameFrameX.LitJSON.Runtime`，主类 `JsonData` / `JsonMapper`）。生成代码只依赖该包，不依赖 SimpleJSON；与 `cs-simple-json` 系列目标互不影响，可按导出 target 分别选择。`cs-litjson-split` 已包含在上文 split 目标列表的 C# 行中。

### 增加本地化的文件夹配置支持

在项目中。经常会出现语言表在协作的时候有冲突。这个时候就需要按照每个模块来做表格文件本身的分离，已经不是Sheet的分离的问题了。所以将本地化文件夹配置支持文件夹下的所有文件都识别为本地化文件。

#### 示例配置

- l10n.provider=`gameframex`

这里必须为 `gameframex` 否则会导致本地化文件识别失败。

- l10n.textFile.path=`./Excels/Tables/Localization/`

这里值必须为 文件夹路径 否则会导致本地化文件识别失败。该参数支持重复指定多个文件夹。

导出参数参考

```
--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key  --xargs l10n.textFile.path=./Excels/Tables/Localization/
```

#### 多文件夹加载（支持重复参数）

`l10n.textFile.path` 支持重复指定多个文件夹，用于按模块拆分本地化表、避免协作冲突：

```text
--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key
--xargs l10n.textFile.path=./Excels/Tables/LocalizationModuleA/
--xargs l10n.textFile.path=./Excels/Tables/LocalizationModuleB/
```

- 同名 `--xargs` 参数可以重复出现，不再报 duplicate 错误；值按命令行出现顺序依次加载。
- 非列表语义的其它参数以第一次出现的值为准。
- 每个文件夹只扫描顶层文件，不递归子目录；需要加载子目录时单独再配置一行。
- 文件夹路径不存在时记录错误日志并跳过，不影响其它文件夹的加载。
- 不同文件中出现相同 key 时记录重复错误日志。

### 支持 text 内联本地化

普通标量 `text` 支持一列或两列写法，不需要增加特殊字段标记：

```text
字段名行：... | name |        | description |        | id
类型行：  ... | text |        | text        |        | int
数据行：  ... | key  | 中文值  | desc_key    | 描述值  | 1
```

只有当 `text` 后一列的字段名和类型同时为空时，才将其识别为 value 列；否则 `text` 只占一列。单列 `text` 的 value 等于 key。一个表中可以有多个 `text` 字段，但只支持普通标量，不支持 `list<text>`、数组、map 或多行 text 的内联 value。

显式开启后，Luban 会在业务数据加载完成后按业务表完整名称生成独立的本地化 XLSX 文件；未配置 Provider 时只生成文件，不替换业务数据中的 key：

```text
--xargs l10n.inline.enabled=true
--xargs l10n.inline.languageFieldName=zh_CN
--xargs l10n.inline.outputPath=Localization/Generated
--xargs l10n.inline.outputFileNameFormat=L-Localization-c-{table}.xlsx
```

`l10n.inline.outputPath` 用于指定自动生成的本地化 `.xlsx` 文件目录，支持相对路径和绝对路径。相对路径以 Luban 进程的当前工作目录为基准，例如：

```text
--xargs l10n.inline.outputPath=/path/to/Config/Excels/Local/Generated
```

`l10n.inline.outputFileNameFormat` 用于指定自动生成的本地化 `.xlsx` 文件名模板，默认 `{table}.xlsx`。模板支持占位符（均会做文件名安全清洗）：
- `{table}`：业务表完整名。
- `{source}`：源 xlsx 文件名（去扩展名）。
- `{sourceDir}`：源 xlsx 所在目录的最后一段。
- `{rawName}`：源文件名按 `-`/`_` 拆出的第二段（与 GameFrameXTableImporter 的 rawTableName 一致）。
- `{comment}`：源文件名拆出的第三段及之后，用 `-` 拼接。
- `{sheet}`：源 xlsx 内的 sheet 名（同一文件多 sheet 分表时区分用）；无 sheet 概念时为空。

同一个表可能来自多个源文件或同一文件的多个 sheet（分表）。按 source 部件分组后计算文件名，模板未区分的组自动合并到同一输出文件：模板包含 `{source}`/`{sheet}` 等区分性占位符时按分表拆分，只含 `{table}` 时保持一表一文件。缺 source 时 `{source}` 退化到 `{table}`。需要输出文件名按 GameFrameX 命名约定（如 `L-Localization-c-{namespace}.{表名}-{中文注释}.xlsx`）时使用：

```text
--xargs l10n.inline.outputFileNameFormat=L-Localization-c-{sourceDir}.{rawName}-{comment}.xlsx
```

输入 `Tables/D-ItemConfig-道具表-道具.xlsx` → 输出 `L-Localization-c-Tables.ItemConfig-道具表-道具.xlsx`。

#### 重新生成时的合并语义

`l10n.inline.*` 生成的 `.xlsx` 文件**不会**被删除或整表覆盖，而是增量合并：

- **现有 xlsx 已存在的 key**：`l10n.inline.languageFieldName` 指定的源语言列以业务表为准，value 变化时同步更新；其他语言列（译者填写的翻译）保持不变。
- **业务表新增的 key**：以 inline 写入的默认 value 追加到文件末尾。
- **业务表删除的 key**：文件里保留（不删）。
- **用户手工加的 row**：保留。
- **多语言列**：保留所有现有列；如 `l10n.inline.languageFieldName` 指定的列在现有文件里缺失，则在 metadata 中扩展该列。
- **行顺序**：现有行严格保持原顺序；新追加的行追加到末尾。
- **文件重写**：只有出现新 key、新语言列或源语言 value 变化时才重写 xlsx，内容无变化时文件保持不动；重写时单元格文本与 `##` 注释行保留，但样式信息（颜色、列宽、批注等）不会保留。

简单说：**只追加，不删、不重排；源语言列跟随业务表，其他语言列不覆盖**。第一次跑 inline 后，应该把生成目录当作翻译入口；后续每次跑只会追加新出现的 key 并同步源语言 value，已翻译的其他语言内容始终保留。

生成文件使用现有 Luban 表格式，保持原有元数据表头布局，数据行从 B 列开始填充（A 列为空），表头语言列为 `key | zh_CN`，文件扩展名为 `.xlsx`。外部人工本地化表优先于 inline value；inline key 在不同表中出现时 value 必须一致，合法空 value 会被保留。

### 增加自动导表的文件名称扩展识别

通过文件名约定自动注册配置表：表结构直接写在 xlsx 内，文件名携带排序前缀、表名、导出组与注释信息，配合目录结构自动推导命名空间，无需在 XML 中逐表声明。

#### 使用场景

- **免 schema 声明**：新增配置表时只需按命名规范新建 xlsx 文件，不必同步维护 XML 定义。
- **目录即命名空间**：按模块分目录组织表格，自动映射为生成代码的命名空间。
- **多文件分表协作**：同一张表的数据拆到多个文件，按文件名自动合并为一张表。
- **按组过滤导出**：通过文件名中的组名（如 `s` 客户端 / `c` 服务端），同一套表仓按导出 target 输出不同子集。

#### 导出参数(必须配置)

```
--xargs tableImporter.name=gameframex
```

#### 说明

格式 [任意字母]-[导出的表名称]-[导出的组名]-[表名称注释].xlsx

表格以任意字母-开头。

中间部分的表名称为英文且不能有空格可以有下划线

导出的组名称必须是定义的`s`、 `c` 之一，可选

后面表名称注释可以接任意长度。程序只取第一个`-` 和第二个`-` 之间的内容加上 `Tb` 为最终表名称。

#### 示例

##### 导出的表名称

L-Localization.xlsx => `Tb`Localization

C-Achievement-成就表.xlsx => `Tb`Achievement

C-Achievement-成就表-AAA.xlsx => `Tb`Achievement

C-Achievement-成就表-AAA-BBB.xlsx => `Tb`Achievement

C-Achievement-成就表-AAA-BBB-CCC.xlsx => `Tb`Achievement

##### 导出的组表名称

C-Achievement-s-成就表.xlsx => `Tb`Achievement, 当前导出目标为 `s` 时才会导出

C-Achievement-c-成就表-AAA.xlsx => `Tb`Achievement, 当前导出目标为 `c` 时才会导出

C-Achievement-s-成就表-AAA-BBB.xlsx => `Tb`Achievement, 当前导出目标为 `s` 时才会导出

C-Achievement-c-成就表-AAA-BBB-CCC.xlsx => `Tb`Achievement, 当前导出目标为 `c` 时才会导出

#### 进阶配置与行为细节

除 `tableImporter.name=gameframex` 外，还支持以下可选参数（均有默认值）：

| 参数 | 默认值 | 说明 |
|:-----|:-------|:-----|
| `tableImporter.filePattern` | `([a-zA-Z0-9]-.+)` | 文件名匹配正则，第一个捕获组作为表名来源 |
| `tableImporter.tableNamespaceFormat` | `{0}` | 表命名空间格式 |
| `tableImporter.tableNameFormat` | `Tb{0}` | 表名格式，默认加 `Tb` 前缀 |
| `tableImporter.valueTypeNameFormat` | `{0}` | 表值类型名格式 |

- 递归扫描数据目录下所有 `xlsx` / `xls` / `xlsm` / `csv`，自动忽略约定规则的文件与配置的排除路径。
- **目录即命名空间**：表所在相对目录路径转为 `.` 分隔后按 `-` / `_` 拆分，取第一段作为命名空间。
- **多文件合并（分表）**：命名空间与表名都相同的多个文件，合并为同一张表的数据输入。
- 表名（第二段）包含中文时构建直接报错。
- 组名（第三段）只有命中配置中已定义的组时才生效，且仅当当前导出 target 包含该组时才会导出该表。

### 增加导表排除目录参数 `--excludePath`

配合文件名自动导表（`tableImporter.name=gameframex`）使用，扫描数据目录时跳过指定目录下的表格文件，可重复指定：

```text
--excludePath Local --excludePath Test
```

- 按「相对数据目录的路径」做前缀匹配，不区分大小写；`Local` 也会匹配 `LocalExtra/`。
- 参数中的 `/` 会被统一转换为 `\`。Windows 下可写多级目录（如 `Local\UI\`）；macOS / Linux 下扫描得到的相对路径使用 `/`，只有不带分隔符的首层目录名（如 `Local`）能命中。

### 增加枚举自动收集（autoExtend）能力

新增 `autoExtend` 能力：当枚举标记为自动扩展时，构建过程会自动扫描所有引用该枚举的表数据，把未在定义中声明的值收集起来，作为正式枚举项加入。这样无需在 schema 中预先枚举所有取值，值随数据自然生长，最终生成的枚举代码会包含全部（预定义 + 自动收集）的枚举项。

#### 适用场景

- 枚举取值随业务数据持续增长，不希望每次都回头维护枚举定义。
- 取值的"名字"比"数字"更重要（建议配合 json/字符串导出使用）。

#### 配置方式

XML schema（专用属性）：

```xml
<enum name="Color" auto_extend="true">
  <var name="RED" value="0" />
  <var name="GREEN" value="1" />
</enum>
```

Excel schema（复用已有的 `tags` 列，无需新增列）：在 `__enums__` 中对应枚举行的 `tags` 列写入 `auto_extend` 或 `auto_extend=1` 即可启用。

#### 行为说明

- 收集阶段在类型编译之后、代码生成与正式数据加载之前执行，生成的枚举代码会包含自动收集到的项。
- 名字型未定义值（如 `PURPLE`）按字典序排序后顺序分配新的 int 值（`max(预定义项) + 1` 递增）。
- 数字型未定义值（如 `42`）以 `AUTO_VALUE_42` 为名、保留该字面值。
- 同一份输入数据下结果完全确定（与加载顺序无关）。
- 自动收集的项会带 `auto=1` 标记，并在日志中输出新增项明细。

#### 注意事项

- 自动收集项的 int 值由"当前数据集"决定，**数据增删可能导致已分配值变化**：binary 导出（按 int 编码）需特别留意，json/字符串导出（按名字编码）则不受影响。
- 未启用 `auto_extend` 的枚举行为完全不变，遇到未定义值仍按原逻辑报错。

## Docker

Luban 提供 Docker 镜像，版本 tag 与 Git tag 保持一致，使用无 `v` 前缀的 SemVer 格式，例如 `3.13.1`；可用版本见 [Releases](https://github.com/GameFrameX/luban/releases)。镜像只包含主程序 `Luban`（`Luban.dll`），不含 `Luban.Agent` 与 `Luban.Mcp`。

镜像地址：

| Registry | Image |
|:---------|:------|
| Docker Hub | `docker.io/gameframex/gameframex-luban:<version>` |
| GitHub Container Registry | `ghcr.io/gameframex/gameframex-luban:<version>` |
| Aliyun Container Registry | `<ALIYUN_REGISTRY_URL>/<ALIYUN_NAMESPACE>/gameframex-luban:<version>` |

示例：

```bash
docker run --rm docker.io/gameframex/gameframex-luban:<version> --help
```

在当前项目目录中运行 Luban 时，可以挂载工作目录：

```bash
docker run --rm -v "$PWD:/work" -w /work docker.io/gameframex/gameframex-luban:<version> --help
```

发布镜像只提供精确版本 tag，不提供 `latest`、`5`、`5.1` 等浮动 tag。

## 文档

- [官方文档](https://www.datable.cn/docs/intro)
- [快速上手](https://www.datable.cn/docs/guide/install)
- [AI 支持（Skills / Agent / MCP / schema-json）](https://www.datable.cn/docs/ai/overview)
- [5.x 新特性（相对 4.x）](https://www.datable.cn/docs/other/whats-new)
- **示例项目** ([github](https://github.com/focus-creative-games/luban_examples)) ([gitee](https://gitee.com/focus-creative-games/luban_examples))

仓库内 AI 资源见 [`ai/`](./ai/README.md)。

## 开发者：运行测试

主仓自带 xUnit 集成测试（不依赖外部 `luban_examples`）：

```bash
dotnet test src/Luban.Tests/Luban.Tests.csproj
```

- 夹具与期望输出：`tests/fixtures/`、`tests/golden/`
- 刷新 golden（改导出行为后）：`pwsh tests/scripts/update-goldens.ps1` 或 `bash tests/scripts/update-goldens.sh`
- 也可在跑测试时设置环境变量 `LUBAN_UPDATE_GOLDEN=1` 就地更新 golden

## 支持与联系

- QQ群: 692890842 （Luban开发交流群）
- discord: <https://discord.gg/dGY4zzGMJ4>
- 邮箱: <luban@code-philosophy.com>

## license

Luban is licensed under the [MIT](https://github.com/focus-creative-games/luban/blob/main/LICENSE) license
