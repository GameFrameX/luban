## [5.1.0] - 2026-10-25

### Bug Fixes

* 修复最后一个标题头列为合并列时，读取从下行起的行数据时有可能最后几个单元的数据并不存在，此时ExcelStream按照[fromIndex, toIndex]访问单元格数据发生越界的bug
* 修复当标题行最后一列包含合并单元格时标题行的列范围可能超出其他数据行的列范围，遍历其他行的`[FromIndex, ToIndex]`范围单元格时索引越界的bug
* 修复 `[*items,,,*items]` 格式的多列限定标题头没有处理`*xxx`多行数据标记的bug (#243)
* 修复为生成cs-xx目标生成的Bean反序列化代码中当Bean全名与当前类的某个字段同名时出现编译错误的bug
* 内联本地化 xlsx 无变化时不再重写
* 内联本地化 value 变化时同步更新生成表
* bean.def 模板移除多余的 using SimpleJSON
* 非空 ref 字段取值改用 TryGet
* TranslateText 按继承关系生成 virtual/override
* list/one 表 LoadAsync 补 override；map 表补 LongDataMaps.Clear 防重载残留
* 表类自持 _loadFunc，不再链基类 JSON 构造
* 修复为生成cs-xx目标生成的Bean反序列化代码中当Bean全名与当前类的某个字段同名时出现编译错误的bug
* 修复如果表记录被映射到外部类型时，Table代码仍然对记录resolveRef引发的编译错误，因为外部类型上并没有ResolveRef字段
* FileUtil::GetExtensionWithoutDot处理没有extension文件时发生异常的bug
* 修复当table的valueType为值类型时，生成的c#代码有编译错误的bug
* 修复luban.conf中target的groups中配置的group名不存在时没有抛出清晰错误信息的问题
* 修复非数据单元薄（A1单元不以##开关）的A1单元格不是字符串类型时抛出异常的bug
* 修复当标题行最后一列包含合并单元格时标题行的列范围可能超出其他数据行的列范围，遍历其他行的`[FromIndex, ToIndex]`范围单元格时索引越界的bug
* 当namespace中包含保留字或者关键字时抛出错误
* 修复 `[*items,,,*items]` 格式的多列限定标题头没有处理`*xxx`多行数据标记的bug (#243)
* 修复SimpleJSON命名空间改为Luban.SimpleJson后未同步修改cs-editor-json目标模板的bug
* fix LiteDataSource::ReadMulti stopped at first record then rise a exception because expected reading end.
* index fields should be exported (#253)
* throw not exported exception when index field isnot exported and host bean is exprted(#254)
* show correct sheet name when raise exception in ExcelRowColumnDataSource::ReadMulti
* throw exception when parse a excel cell which is error value `#Error`
* don't throw InvalidExcelDataException exception when error cells occur at empty or comment row
* DefaultTableImporter uses wrong fileNamePattern Regex leading to ignore import files which contains `_`
* RefTypeVisitor doesn't export type of not exported fields
* update doc links after refactoring doc
* set bean.Comment with table.Comment if bean.ReadSchemaFromFile is true
* fix generated code of gdscript-json run failed when a table hasn't namespace because Tables field name can't same to field type name
* 表类自持 _loadFunc，不再链基类 JSON 构造 (#18)
* 恢复 C3 合并中丢失的两处 fork 行为

### Documentation

* 补充 cs-litjson / cs-litjson-split 代码生成目标说明
* 补充 text 字段内联本地化用法
* 补充各特性的中英文使用说明
* 补充技能本机注册与 MCP 接入说明
* 修正 README 工具名为 snake_case 并补充 generate 用法
* 同步中英文 README 并补充分支差异与 AI 支持说明

### Features

* 支持 text 字段内联本地化
* 支持同名 option 多值
* textFilePath 支持多个目录
* 内联本地化 xlsx 改为增量合并
* 整型字段支持 0x 前缀十六进制字面量
* Map<int, _> 类型 key 字段支持 0x 前缀十六进制字面量
* 支持常量别名
* 新增javascript-json和javascript-bin支持
* add comments for bean for code target go-json and go-bin
* 优化lua代码中enum类型的类型信息，由integer改为enum类型全名
* l10n.textFile.path accepts argument same to table.inputFiles
* FileUtil.IsIgnoreFile check ignore prefix char in file since max common path with baseDir
* 恢复对流式多行结构的支持(#262)
* add code target gdscript-bin and support nested container (#260)
* 支持集合字段索引及多索引列表表查询 (#261)
* 支持tsv格式数据源 (#263)
* standardize errors with LubanException and localize messages
* add in-repo Luban integration test suite with fixtures and CI
* support table variant
* track schema definition source file and sheet
* 合并官方 luban v5.1.0 基座重构并保留 GameFrameX 自定义修复
* 新增 GameFrameX 自定义修复回归测试

### Miscellaneous Tasks

* bump version to v4.8.0
* bump Scriban from vulnerable v7.1.0 to v7.2.1
* bump Luban version to 4.9.0
* change Cell ToString format
* bump MessagePack version to v3.1.7
* bump Luban version to v4.10.0
* bump Luban version to v4.10.1
* bump Scriban to v7.2.5
* bump Luban version to v4.10.2
* bump Luban version to v4.11.0
* bump version to v4.12.0
* replace tabs with spaces in scriban templates
* update README
* update README, add description for AI support
* bump version to v5.0.0
* exclude tests/golden/core/bin/ from git ignore
* bump version of Luban/Luban.Mcp/Luban.Agent to v5.1.0

### Performance

* SheetLoadUtil::LoadSheetTableDefInfo 没有找到有效表定义时的错误信息中加上sheetName

### Refactor

* SteamParser重命名为StreamParser
* rename namespace SimpleJSON to Luban.SimpleJson to avoid conflict with SimpleJSON in TextMateSharp of com.unity.collab-proxy.
* remove code target gdscript-bin
* introduce PipelineScope for per-run ambient context

### build

* 升级 Scriban 至 6.2.1
* 升级运行时依赖包版本
* 禁用镜像 provenance 和 sbom 附件

### change

* 消除Luban.Dart项目的编译警告
* 解决DMap的Datas字段与DType的Datas同名引发的编译警告。改名为DataMap。
* 优化流式格式中string类型的解析方式，完全忽略空白字符，同时跟lite格式一样会剔除字符串首尾的''和""
* schema 文件不包含后缀时抛出清晰的错误异常
* typescript-bin所依赖的ByteBuf的函数名的命名风格由首字母大写变成小写，相应调用代码生成
* 移除 JsonDeserializeVisitor中对type.FinalElementType != null的检查。完全没有必要！
* 自动导入表支持文件名后面添加`-<注释>`格式的表注释，该注释会作为table的comment。
* 生成的C#代码中table类的返回数据map和列表的接口的返回值改为对应IReadOnlyXXX类型(#234)

### new

* 解析excel数据时支持0x前缀的16进制整数

### opt

* 优化生成的rust代码中表查询接口的 key 借用方式 (#248)
* 清除rust-bin生成的代码的大部分编译警告。
* 清理rust-json生成的代码中的部分编译警告。

### remove

* 删除无用的 `AutoTableImporter.cs` 文件

### update

* 升级Scriban到v7.1.0

### version

* bump version to 4.6.0
## [3.13.1] - 2026-07-07

### Bug Fixes

* 修复 table 模板 list/one 模式生成重复字段与构造函数

### Features

* 新增 cs-litjson / cs-litjson-split 代码生成目标
## [3.13.0] - 2026-07-07

### Features

* 支持枚举 autoExtend 自动收集未定义枚举值
## [3.12.0] - 2026-06-20

### Bug Fixes

* 修复多层容器类型嵌套情况下校验器失效的问题 (#139)
* 读取字节使用 ReadExactly 避免短读

### Documentation

* 补充 split 生成目标说明
* 补充 Docker 镜像使用说明

### Features

* 新增 split 系列代码生成目标
* 添加 split 代码生成目标

### build

* 升级目标框架至 net10.0
* 升级 Docker 构建到 .NET 10
* 提取统一版本配置

### ci

* 添加 Docker 镜像发布流水线
* 升级发布流水线的 actions 版本
* 添加自动版本发布 workflow
## [2.0.0] - 2023-08-19
<!-- generated by git-cliff -->
