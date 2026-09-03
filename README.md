# Luban

![icon](docs/images/logo.png)

[![license](http://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT) ![star](https://img.shields.io/github/stars/focus-creative-games/luban?style=flat-square)

luban是一个强大、易用、优雅、稳定的游戏配置解决方案。它设计目标为满足从小型到超大型游戏项目的简单到复杂的游戏配置工作流需求。

luban可以处理丰富的文件类型，支持主流的语言，可以生成多种导出格式，支持丰富的数据检验功能，具有良好的跨平台能力，并且生成极快。
luban有清晰优雅的生成管线设计，支持良好的模块化和插件化，方便开发者进行二次开发。开发者很容易就能将luban适配到自己的配置格式，定制出满足项目要求的强大的配置工具。

luban标准化了游戏配置开发工作流，可以极大提升策划和程序的工作效率。

## 核心特性

- 丰富的源数据格式。支持excel族(csv,xls,xlsx,xlsm)、json、xml、yaml、lua等
- 丰富的导出格式。 支持生成binary、json、bson、xml、lua、yaml等格式数据
- 增强的excel格式。可以简洁地配置出像简单列表、子结构、结构列表，以及任意复杂的深层次的嵌套结构
- 完备的类型系统。不仅能表达常见的规范行列表，由于**支持OOP类型继承**，能灵活优雅表达行为树、技能、剧情、副本之类复杂GamePlay数据
- 支持多种的语言。内置支持生成c#、java、go、cpp、lua、python、typescript、rust、php、erlang 等语言代码，同时还能通过protobuf之类消息方案支持其他语言
- 支持主流的消息方案。 protobuf(schema + binary + json)、flatbuffers(schema + json)、msgpack(binary)
- 强大的数据校验能力。ref引用检查、path资源路径、range范围检查等等
- 完善的本地化支持
- 支持所有主流的游戏引擎和平台。支持Unity、Unreal、Cocos2x、Godot、微信小游戏等
- 良好的跨平台能力。能在Win,Linux,Mac平台良好运行。
- 支持所有主流的热更新方案。hybridclr、ilruntime、{x,t,s}lua、puerts等
- 清晰优雅的生成管线，很容易在luban基础上进行二次开发，定制出适合自己项目风格的配置工具。

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

#### English

A set of split code generation targets is added without touching the original ones — each new target is the original target name plus a `-split` suffix.

Split targets break the generated code into finer-grained files: definition information goes into Def files designed for reading, review, and AI context, while parsing, loading, and other runtime code stays in Impl or runtime files. When you only care about a single config type, read its Def file instead of the full schema file.

**When to use**

- **AI-assisted development**: feed Def files to the model as context — they contain only field and type definitions without deserialization code, so they are small and have a high signal-to-noise ratio.
- **Code review and daily lookup**: open the Def file of a table to check its fields instead of locating it inside a thousands-of-lines full schema file.
- **Fewer merge conflicts**: schema changes concentrate in Def files and no longer interleave with runtime code changes, keeping diffs focused.
- **Non-invasive coexistence**: original non-split targets are unchanged; both kinds can be selected per export target.

**How to use**

The target naming rule is `original code target + -split`. Switch `--codeTarget` in your export command to the split variant and keep everything else unchanged:

```text
dotnet Luban.dll --target client --dataTarget bin --codeTarget cs-bin-split --xargs outputDataDir=./Output/Data --xargs outputCodeDir=./Output/Code --conf ./luban.conf
```

Go split targets additionally require a module name:

```text
--xargs lubanGoModule=github.com/gameframex/config
```

Split output file naming per language (example: table `ItemConfig` under namespace `Tables`):

| Language | Definition files (Def) | Runtime files (Impl) |
|:---------|:-----------------------|:---------------------|
| C# | `Tables/ItemConfig.Def.cs` | `Tables/ItemConfig.Impl.cs` (no merged `ItemConfig.cs` anymore) |
| Go | `Tables.ItemConfig.def.go` | `Tables.ItemConfig.impl.go` |
| Rust | `cfg/src/Tables.ItemConfig.Def.rs`, `Tables.TbItemConfig.Def.rs`, `Tables.Def.rs` | `cfg/src/tables.rs`, `cfg/src/lib.rs` |
| C++ | `Tables.ItemConfig.Def.h` | `schema.h`, `Tables.ItemConfig.Impl.cpp` |
| Java | `Tables/ItemConfigDef.java` (companion definition class) | `Tables/ItemConfig.java` |
| TypeScript | `Tables.ItemConfig.def.ts`, `Tables.def.ts` | `schema.ts` |
| Lua | `Tables.ItemConfig.def.lua` | runtime schema file |
| PHP / GDScript | `Tables.ItemConfig.def.php` / `Tables.ItemConfig.def.gd` | runtime schema file |

Def files contain definition information only (fields, types, constant declarations) — no deserialization, construction, or loading code; this constraint is asserted by the acceptance script.

Supported split targets:

| Language | Targets |
|:---------|:--------|
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

The split rules keep consistent semantics across languages:

- C#, Go, and C++ emit Def/Impl style files to separate definition context from runtime implementation.
- Java cannot split one public class across files, so it emits the runtime class plus an `XxxDef` companion definition class.
- TypeScript, Python, Lua, PHP, and GDScript keep the runtime schema file and additionally emit one `*.def.*` definition file per type.

Local acceptance script:

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

#### English

Two additional C# code generation targets, `cs-litjson` and `cs-litjson-split`, are added while `cs-simple-json` / `cs-simple-json-split` (which keep emitting SimpleJSON-based code) remain unchanged. The new targets emit deserialization code based on the GameFrameX LitJson package (namespace `GameFrameX.LitJSON.Runtime`, main classes `JsonData` / `JsonMapper`). Fully backward compatible — pick the target matching the JSON library already present in your runtime.

| Target | Original counterpart | Runtime dependency | Output |
|:-------|:---------------------|:-------------------|:-------|
| `cs-litjson` | `cs-simple-json` | `GameFrameX.LitJSON.Runtime` | single file, definition and runtime code merged |
| `cs-litjson-split` | `cs-simple-json-split` | `GameFrameX.LitJSON.Runtime` | split output, definitions in Def files, runtime code in Impl files |

**When to use**

- Your runtime (especially Unity hot-update layers) already standardizes on GameFrameX LitJson, and you want config loading to share that dependency instead of pulling in SimpleJSON as well.
- C# projects that want split granularity (definition / runtime separation) with LitJson deserialization.

**How to use**

```text
--codeTarget cs-litjson        # single merged file
--codeTarget cs-litjson-split  # Def / Impl separated output
```

Prerequisite: the runtime project references the GameFrameX LitJson package (namespace `GameFrameX.LitJSON.Runtime`, main classes `JsonData` / `JsonMapper`). The generated code depends only on that package, not on SimpleJSON; the targets do not affect the `cs-simple-json` family, so you can pick per export target.

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

#### English

In real projects, localization tables often run into conflicts during collaboration. The fix is to split the table files themselves per module — it is no longer just a matter of splitting sheets. The localization folder support makes every recognized file inside the configured folders load as a localization table.

- `l10n.provider=gameframex` — required. Any other value makes localization file detection fail.
- `l10n.textFile.path=<folder>` — required. The value must be a folder path, otherwise localization file detection fails. The option can be repeated to load multiple folders:

```text
--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key
--xargs l10n.textFile.path=./Excels/Tables/LocalizationModuleA/
--xargs l10n.textFile.path=./Excels/Tables/LocalizationModuleB/
```

- Repeated `--xargs` options with the same name no longer raise a duplicate error; values are loaded in command-line order.
- Other non-list options take the value of their first occurrence.
- Each folder is scanned non-recursively (top-level files only); configure sub-directories as separate entries.
- A folder that does not exist is reported in the log and skipped; the remaining folders still load.
- The same key appearing in different files is reported as a duplicate in the log.

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
--xargs l10n.inline.outputPath=/Users/mac/Documents/UnityWorks/X/Config/Excels/Local/Generated
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

`l10n.inline.*` 生成的 `.xlsx` 文件**不会**被自动删除或覆盖：

- **现有 xlsx 已存在的 key**：现有 cell 内容保持不变（用户翻译优先）。inline 不会覆盖任何已有 row。
- **业务表新增的 key**：以 inline 写入的默认 value 追加到文件末尾。
- **业务表删除的 key**：文件里保留（不删）。
- **用户手工加的 row**：保留。
- **多语言列**：保留所有现有列；如 `l10n.inline.languageFieldName` 指定的列在现有文件里缺失，则在 metadata 中扩展该列。
- **行顺序**：现有行严格保持原顺序；新追加的行追加到末尾。
- **Excel 样式**：合并时会整体重写 xlsx 文件，单元格文本内容保留，但样式信息（颜色、列宽、批注等）不会保留。

简单说：**只追加，不删、不覆盖、不重排**。这意味着第一次跑 inline 后，应该把生成目录当作翻译入口；后续每次跑只会把新出现的 key 追加进去，已翻译的内容始终保留。如果业务表里某个 key 的 inline 默认值变了，已翻译过的文件不会被刷新 —— 要让 inline 默认值变更生效，需要手动调整。

生成文件使用现有 Luban 表格式，保持原有元数据表头布局，数据行从 B 列开始填充（A 列为空），表头语言列为 `key | zh_CN`，文件扩展名为 `.xlsx`。外部人工本地化表优先于 inline value；inline key 在不同表中出现时 value 必须一致，合法空 value 会被保留。

#### English

Plain scalar `text` fields support a one-column or two-column layout without any special field markers:

```text
header row: ... | name |        | description |        | id
type row:   ... | text |        | text        |        | int
data row:   ... | key  | value  | desc_key    | value  | 1
```

A `text` column gets an inline value column only when both the field name and the type of the following column are empty; otherwise `text` occupies a single column and its value equals its key. A table may contain multiple `text` fields, but only plain scalars are supported — `list<text>`, arrays, maps, and multi-row text do not support inline values.

When explicitly enabled, Luban generates standalone localization XLSX files per business table after the business data is loaded. When no Provider is configured, it only generates the files without replacing the keys in the business data:

```text
--xargs l10n.inline.enabled=true
--xargs l10n.inline.languageFieldName=zh_CN
--xargs l10n.inline.outputPath=Localization/Generated
--xargs l10n.inline.outputFileNameFormat=L-Localization-c-{table}.xlsx
```

`l10n.inline.outputPath` is the output directory of the generated `.xlsx` files. Both relative and absolute paths are supported; relative paths are resolved against the current working directory of the Luban process.

`l10n.inline.outputFileNameFormat` is the file name template of the generated `.xlsx` files and defaults to `{table}.xlsx`. Supported placeholders (all sanitized for file name safety):

- `{table}`: full business table name.
- `{source}`: source xlsx file name without extension; falls back to the table name when no source exists.
- `{sourceDir}`: last segment of the directory containing the source xlsx.
- `{rawName}`: second segment of the source file name split by `-`/`_` (consistent with GameFrameXTableImporter's rawTableName).
- `{comment}`: third and later segments of the source file name, joined by `-`.
- `{sheet}`: sheet name inside the source xlsx (distinguishes sub-tables of one file); empty when the source has no sheet concept.

One table may come from multiple source files or from multiple sheets of the same file (sub-tables). File names are computed per source group, and groups the template does not distinguish are merged into the same output file: a template containing `{source}`/`{sheet}` splits by sub-table, while a `{table}`-only template keeps one file per table. Use the following to follow the GameFrameX naming convention (`L-Localization-c-{namespace}.{tableName}-{comment}.xlsx`):

```text
--xargs l10n.inline.outputFileNameFormat=L-Localization-c-{sourceDir}.{rawName}-{comment}.xlsx
```

Input `Tables/D-ItemConfig-道具表-道具.xlsx` → output `L-Localization-c-Tables.ItemConfig-道具表-道具.xlsx`.

##### Merge semantics on re-generation

The `.xlsx` files generated by `l10n.inline.*` are never deleted or overwritten:

- **Keys already present in the xlsx**: existing cell content is kept unchanged (user translations win). Inline never overwrites an existing row.
- **Keys newly added to business tables**: appended to the end of the file with the inline default value.
- **Keys removed from business tables**: kept in the file (not deleted).
- **Rows added manually by users**: preserved.
- **Language columns**: all existing columns are preserved; if the column named by `l10n.inline.languageFieldName` is missing in an existing file, it is extended in the metadata.
- **Row order**: existing rows keep their exact order; new rows are appended at the end.
- **Excel styles**: merging rewrites the whole xlsx; cell text content is preserved, but styling (colors, column widths, comments, etc.) is not.

In short: **append-only — no deletion, no overwrite, no reordering**. Treat the generated directory as the translation entry point after the first run: subsequent runs only append newly appearing keys, and translated content is always kept. If the inline default value of a key changes in the business table, already-generated files are not refreshed — apply such changes manually.

Generated files use the standard Luban table format, keep the original metadata header layout, fill data rows starting from column B (column A stays empty), use `key | zh_CN` as the header language columns, and use the `.xlsx` extension. External hand-maintained localization tables take precedence over inline values; an inline key must carry the same value across tables; valid empty values are preserved.

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

#### English

Tables are registered automatically through file name conventions: the table schema is defined inside the xlsx itself, and the file name carries the sort prefix, table name, export group, and comment. Together with the directory layout this also derives the namespace — no per-table XML declarations needed.

**When to use**

- **Schema-free table registration**: adding a config table only takes a new xlsx file named by convention; there is no XML definition to maintain in parallel.
- **Directory as namespace**: organize tables into per-module folders and get the generated code namespaces for free.
- **Multi-file sub-tables**: split one table's rows across several files; they are merged back into a single table by file name.
- **Group-filtered export**: the group segment in the file name (e.g. `s` for client, `c` for server) lets one table repository serve different targets with different subsets.

**Required export parameter**

```text
--xargs tableImporter.name=gameframex
```

**Naming format**

`[any letter]-[table name]-[group]-[comment].xlsx`

- The file name starts with `any letter-`.
- The table name segment is English without spaces; underscores are allowed.
- The group segment must be one of the defined groups (`s` or `c`) and is optional.
- The comment segment can be any length. Only the content between the first and second `-` is used, with `Tb` prefixed, as the final table name.

**Examples**

Table names:

- `L-Localization.xlsx` => `Tb`Localization
- `C-Achievement-成就表.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA-BBB.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA-BBB-CCC.xlsx` => `Tb`Achievement

Group names:

- `C-Achievement-s-成就表.xlsx` => `Tb`Achievement, exported only when the current export target is `s`
- `C-Achievement-c-成就表-AAA.xlsx` => `Tb`Achievement, exported only when the current export target is `c`
- `C-Achievement-s-成就表-AAA-BBB.xlsx` => `Tb`Achievement, exported only when the current export target is `s`
- `C-Achievement-c-成就表-AAA-BBB-CCC.xlsx` => `Tb`Achievement, exported only when the current export target is `c`

**Advanced options and behavior details**

Besides `tableImporter.name=gameframex`, the following optional parameters are supported (all have defaults):

| Option | Default | Description |
|:-------|:--------|:------------|
| `tableImporter.filePattern` | `([a-zA-Z0-9]-.+)` | File name regex; the first capture group feeds the table name |
| `tableImporter.tableNamespaceFormat` | `{0}` | Table namespace format |
| `tableImporter.tableNameFormat` | `Tb{0}` | Table name format; `Tb` prefix by default |
| `tableImporter.valueTypeNameFormat` | `{0}` | Value type name format |

- The data directory is scanned recursively for `xlsx` / `xls` / `xlsm` / `csv`; ignored files and configured exclude paths are skipped automatically.
- **Directory as namespace**: the relative directory path is converted to `.`-separated form, split by `-` / `_`, and the first segment becomes the namespace.
- **Multi-file merge (sub-tables)**: files sharing the same namespace and table name are merged into one table's input.
- A table name (second segment) containing Chinese characters fails the build.
- The group segment only takes effect when it matches a group defined in the configuration, and the table is exported only when the current target includes that group.

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

#### English

A new `autoExtend` capability is added: when an enum is marked as auto-extended, the build scans all table data referencing that enum, collects values not declared in the definition, and adds them as formal enum items. There is no need to enumerate all values in the schema up front — values grow naturally with the data, and the generated enum code contains both predefined and auto-collected items.

**When to use**

- Enum values keep growing with business data and you do not want to maintain the enum definition every time.
- The "name" of a value matters more than its number (recommended together with json/string exports).

**Configuration**

XML schema (dedicated attribute):

```xml
<enum name="Color" auto_extend="true">
  <var name="RED" value="0" />
  <var name="GREEN" value="1" />
</enum>
```

Excel schema (reuses the existing `tags` column; no new column needed): write `auto_extend` or `auto_extend=1` into the `tags` column of the enum's row in `__enums__` to enable it.

**Behavior**

- Collection runs after type compilation and before code generation and formal data loading; the generated enum code includes the auto-collected items.
- Undefined name values (e.g. `PURPLE`) get new int values assigned in lexicographic order (incrementing from `max(predefined items) + 1`).
- Undefined numeric values (e.g. `42`) keep the literal value and are named `AUTO_VALUE_42`.
- Results are fully deterministic for the same input data (independent of loading order).
- Auto-collected items are tagged `auto=1`, and the details of new items are printed to the log.

**Notes**

- The int values of auto-collected items depend on the current data set — **adding or removing data may change the assigned values**. Binary exports (int-encoded) need extra care; json/string exports (name-encoded) are unaffected.
- Enums without `auto_extend` behave exactly as before; undefined values still raise errors under the original logic.

## Docker

Luban 提供 Docker 镜像，版本 tag 与 Git tag 保持一致，使用无 `v` 前缀的 SemVer 格式，例如 `3.12.0`。

镜像地址：

| Registry | Image |
|:---------|:------|
| Docker Hub | `docker.io/gameframex/gameframex-luban:<version>` |
| GitHub Container Registry | `ghcr.io/gameframex/gameframex-luban:<version>` |
| Aliyun Container Registry | `<ALIYUN_REGISTRY_URL>/<ALIYUN_NAMESPACE>/gameframex-luban:<version>` |

示例：

```bash
docker run --rm docker.io/gameframex/gameframex-luban:3.12.0 --help
```

在当前项目目录中运行 Luban 时，可以挂载工作目录：

```bash
docker run --rm -v "$PWD:/work" -w /work docker.io/gameframex/gameframex-luban:3.12.0 --help
```

发布镜像只提供精确版本 tag，不提供 `latest`、`3`、`3.12` 等浮动 tag。

## 文档

- [官方文档](https://luban.doc.code-philosophy.com/)
- [快速上手](https://luban.doc.code-philosophy.com/docs/beginner/quickstart)
- **示例项目** ([github](https://github.com/focus-creative-games/luban_examples)) ([gitee](https://gitee.com/focus-creative-games/luban_examples))

## 支持与联系

- QQ群: 692890842 （Luban开发交流群）
- discord: https://discord.gg/dGY4zzGMJ4
- 邮箱: luban@code-philosophy.com

## license

Luban is licensed under the [MIT](https://github.com/focus-creative-games/luban/blob/main/LICENSE) license
