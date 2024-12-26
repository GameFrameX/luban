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

格式 #[任意字母]-[导出的表名称]-[表名称注释].xlsx

表格以`#` 可以接任意字母开头。

中间部分的表名称为英文且不能有空格可以有下划线

后面表名称注释可以接任意长度。程序只取第一个`-` 和第二个`-` 之间的内容加上 `Tb` 为最终表名称。

#### 示例

#L-Localization.xlsx => `Tb`Localization

#C-Achievement-成就表.xlsx => `Tb`Achievement

#C-Achievement-成就表-AAA.xlsx => `Tb`Achievement

#C-Achievement-成就表-AAA-BBB.xlsx => `Tb`Achievement

#C-Achievement-成就表-AAA-BBB-CCC.xlsx => `Tb`Achievement

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
