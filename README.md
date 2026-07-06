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

命名规则：

```
原代码生成目标 + -split
```

已支持的 split 目标：

| 语言 | 目标 |
|:-----|:-----|
| C# | `cs-simple-json-split`, `cs-bin-split`, `cs-dotnet-json-split`, `cs-dotnet-bin-split` |
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

### 增加本地化的文件夹配置支持

在项目中。经常会出现语言表在协作的时候有冲突。这个时候就需要按照每个模块来做表格文件本身的分离，已经不是Sheet的分离的问题了。所以将本地化文件夹配置支持文件夹下的所有文件都识别为本地化文件。

#### 示例配置

- l10n.provider=`gameframex`

这里必须为 `gameframex` 否则会导致本地化文件识别失败。

- l10n.textFile.path=`./Excels/Tables/Localization/`

这里值必须为 文件夹路径 否则会导致本地化文件识别失败。

导出参数参考

```
--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key  --xargs l10n.textFile.path=./Excels/Tables/Localization/
```

### 增加自动导表的文件名称扩展识别

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
