# Luban

![icon](docs/images/logo.png)

[![license](http://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT) ![star](https://img.shields.io/github/stars/focus-creative-games/luban?style=flat-square) ![release](https://img.shields.io/github/v/release/GameFrameX/luban?style=flat-square)

[中文](./README.md) | **English**

luban is a powerful, easy-to-use, elegant, and stable game configuration solution. It is designed to meet the needs of simple to complex game configuration workflows from small to very large game projects.

luban can handle a variety of file types, supports popular languages, can generate multiple export formats, supports rich data inspection functions, has good cross-platform capabilities, and generates extremely fast.
Luban has a clear and elegant generation pipeline design, supports good modularization and plug-in, and is convenient for developers to carry out secondary development. Developers can easily adapt luban to their own configuration format, and customize powerful configuration tools that meet project requirements.

Luban standardizes the game configuration development workflow, which can greatly improve the efficiency of planning and programming.

Since **5.0.0**, Luban is **AI Native**: official Agent Skills, machine-readable schema and diagnostics, `Luban.Agent` CLI, and an MCP Server—so agents can reliably add tables, inspect schema, validate, and generate.

## Core features

- Rich source data format. Support excel family (csv, tsv, xls, xlsx, xlsm), json, xml, yaml, lua, etc.
- Rich export formats. Support generating binary, json, bson, xml, lua, yaml and other format data
- Enhanced excel format. Simple configurations such as simple lists, substructures, structured lists, and arbitrarily complex deep nested structures can be concisely configured
- Complete type system. Not only can it express common specification line lists, but it can flexibly and elegantly express complex GamePlay data such as behavior trees, skills, plots, and dungeons because **supports OOP type inheritance**
- Support multiple languages. Supports generating language codes such as c#, java, go, cpp, lua, python, javascript, typescript, php, rust, godot, etc.
- Support popular message schemes. protobuf(schema + binary + json), flatbuffers(schema + json), msgpack(binary)
- Powerful data verification capability. ref reference check, path resource path, range range check, etc.
- Perfect localization support
- **AI Native (5.0.0+)**. Official Agent Skills, `-c schema-json`, `--errorFormat json`, `Luban.Agent` (validate / list / describe), and `Luban.Mcp` (IDE tool calling)
- Supports all major game engines and platforms. Support Unity, Unreal, Cocos2x, Godot, WeChat games, etc.
- Good cross-platform capability. It can run well on Win, Linux, and Mac platforms.
- Support all mainstream hot update solutions. hybridclr, ilruntime, {x,t,s}lua, puerts, etc.
- The generated code makes no reflection API calls, ensuring compatibility with common code obfuscation and hardening tools such as [Obfuz](https://github.com/focus-creative-games/obfuz), Obfuscator, Confuser, and .NET Refactor.
- Clear and elegant generation pipeline, it is easy to carry out secondary development on the basis of luban, and customize a configuration tool suitable for your own project style.

## GameFrameX fork notes

This repository is the GameFrameX fork of [focus-creative-games/luban](https://github.com/focus-creative-games/luban). It has merged upstream **v5.1.0**, taking in the v4 / v5 core refactoring and AI capabilities while keeping the GameFrameX features and fixes listed below. Please report fork-specific issues at [GameFrameX/luban Issues](https://github.com/GameFrameX/luban/issues).

### Differences from upstream

| Feature | Description |
|:--------|:------------|
| Split code targets | Original target name plus `-split`; definitions and runtime code are emitted into separate files, see [details](#split-code-generation-targets) |
| `cs-litjson` / `cs-litjson-split` | C# code targets based on GameFrameX LitJson, see [details](#cs-litjson--cs-litjson-split-code-generation-targets) |
| C# runtime shape | `cs-bin`, `cs-dotnet-bin`, `cs-dotnet-json`, `cs-simple-json`, `cs-litjson` and their `-split` variants generate the GameFrameX runtime shape: table classes inherit `BaseDataTable<T>`, and data loads asynchronously via `LoadAsync` after `Tables.Init(ConfigComponent)`; `cs-code`, `cs-editor-json`, and `cs-newtonsoft-json` do not use this shape |
| File-name based table import | `tableImporter.name=gameframex` derives table name, group, and namespace from file names and directories, see [details](#file-name-based-automatic-table-import) |
| Excluded import directories | `--excludePath`, see [details](#exclude-directories-from-import---excludepath) |
| Localization folders | `l10n.provider=gameframex`; `l10n.textFile.path` accepts multiple folders, see [details](#localization-folder-configuration) |
| Inline text localization | `l10n.inline.*`, see [details](#inline-localization-for-text-fields) |
| Enum auto collection | `auto_extend`, see [details](#enum-auto-collection-autoextend) |
| Repeated `--xargs` | Options with the same name may repeat; list-semantic options take every value in order |
| Target framework | `net10.0` everywhere (upstream uses `net8.0`) |
| `Luban.GUI` | Avalonia desktop export tool (`src/Luban.GUI`) |
| Docker image | See [Docker](#docker) |

### Status of custom fixes after merging v5.1.0

| Fix | Original commit | Status after merge |
|:----|:----------------|:-------------------|
| l10n: `TranslateText` emits `virtual` / `override` by inheritance | `24774645` | Kept |
| l10n: inline localization xlsx merges incrementally, skips rewriting when unchanged, syncs source-language values | `a8fd260c`, `b2a3e7b7`, `421349aa` | Kept |
| cs-dotnet-json: table class owns `_loadFunc` instead of chaining the base JSON constructor | `e1f39884` | Kept |
| cs-dotnet-json: non-nullable ref fields resolve via `TryGet` | `27b318b8` | Kept |
| cs-dotnet-bin: `override` on list / one table `LoadAsync`; map tables clear `LongDataMaps` before reload | `ff138c33` | Kept |
| cs-bin-split: drop the redundant `using SimpleJSON` from the `bean.def` template | `95b08b0b` | Kept |
| Table template list / one mode generating duplicate fields and constructors | `92aea61d` | Kept |
| Read bytes with `ReadExactly` to avoid short reads | `1e2333f3` | Kept |
| `0x` hexadecimal literals in integer fields | `dd520088` | Superseded by upstream (`42e5a380`, `LoadDataUtil.TryParseExcelIntFromNumberOrConstAlias`) |
| `0x` hexadecimal literals in `map<int, _>` keys | `a93ab40f` | Kept (upstream does not cover the map key parsing path) |
| Compile error when a bean full name equals a field name, `[*items,,,*items]` multi-row markers, out-of-range merged last header column | `368c5344`, `d5cff3d4`, `d6c7a87c`, `534dd6c0` | Equivalent upstream fixes exist; upstream implementation wins |

## AI support

Upstream v5 AI Native capabilities came into this fork with v5.1.0:

| Capability | Entry | Purpose |
|:-----------|:------|:--------|
| Agent Skills | [`ai/skills/`](./ai/skills) | Six skills: add table, fill Excel, schema design, validators, generation debugging, runtime loading |
| schema-json | `-c schema-json` | Emits a machine-readable `schema.json` |
| JSON diagnostics | `--errorFormat json` | Writes structured JSON diagnostics to stderr on failure |
| Luban.Agent | [`src/Luban.Agent`](./src/Luban.Agent/README.md) | `capabilities` / `list-tables` / `describe` / `schema` / `validate`; prints a single JSON result on stdout |
| Luban.Mcp | [`src/Luban.Mcp`](./src/Luban.Mcp/README.md) | stdio MCP server for IDEs / agents |

### Build

This fork ships the AI tools as three separate output directories (the Docker image contains the main `Luban` only):

```bash
dotnet build src/Luban/Luban.csproj -c Release -o <out>/Luban
dotnet build src/Luban.Agent/Luban.Agent.csproj -c Release -o <out>/Luban.Agent
dotnet build src/Luban.Mcp/Luban.Mcp.csproj -c Release -o <out>/Luban.Mcp
```

### Start the MCP server

Register it in your MCP client configuration (`mcpServers` format; replace `<out>` with your output directory):

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

- `LUBAN_AGENT_DLL` and `LUBAN_DLL` are required: without them Luban.Mcp only falls back to `bin/Release|Debug/net8.0` inside the source tree, and this fork targets `net10.0`, so the fallback never matches.
- `LUBAN_DOC` is optional and points to the `docs` directory of the luban-doc repository for `search_docs`.
- Exposed tools: `list_tables` / `get_schema` / `describe` / `validate` / `generate` / `search_docs`; see [`src/Luban.Mcp/README.md`](./src/Luban.Mcp/README.md) for arguments.

### Use the skills

Copy the skill directories under `ai/skills/` into your agent's skill directory (e.g. `.cursor/skills/`, `~/.claude/skills/`). See [`ai/README.md`](./ai/README.md).

### Command line

```bash
# Export the machine-readable schema (schema.json goes to outputCodeDir)
dotnet <out>/Luban/Luban.dll --conf luban.conf -t all -c schema-json -d json -x outputCodeDir=<codeDir> -x outputDataDir=<dataDir>

# Print diagnostics as JSON on failure (written to stderr)
dotnet <out>/Luban/Luban.dll --conf luban.conf -t all -d json --errorFormat json -x outputDataDir=<dataDir>

# Validate only, no file output
dotnet <out>/Luban.Agent/Luban.Agent.dll validate --conf luban.conf -t all
```

Docs: [AI overview](https://www.datable.cn/en/docs/ai/overview) · [Skills](https://www.datable.cn/en/docs/ai/skills) · [Agent CLI](https://www.datable.cn/en/docs/ai/agent-cli) · [MCP](https://www.datable.cn/en/docs/ai/mcp) · [schema-json](https://www.datable.cn/en/docs/ai/schema-json)

## Changes and additions

### Split code generation targets

A set of split code generation targets is added without touching the original ones — each new target is the original target name plus a `-split` suffix.

Split targets break the generated code into finer-grained files: definition information goes into Def files designed for reading, review, and AI context, while parsing, loading, and other runtime code stays in Impl or runtime files. When you only care about a single config type, read its Def file instead of the full schema file.

#### When to use

- **AI-assisted development**: feed Def files to the model as context — they contain only field and type definitions without deserialization code, so they are small and have a high signal-to-noise ratio.
- **Code review and daily lookup**: open the Def file of a table to check its fields instead of locating it inside a thousands-of-lines full schema file.
- **Fewer merge conflicts**: schema changes concentrate in Def files and no longer interleave with runtime code changes, keeping diffs focused.
- **Non-invasive coexistence**: original non-split targets are unchanged; both kinds can be selected per export target.

#### How to use

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

### cs-litjson / cs-litjson-split code generation targets

Two additional C# code generation targets, `cs-litjson` and `cs-litjson-split`, are added while `cs-simple-json` / `cs-simple-json-split` (which keep emitting SimpleJSON-based code) remain unchanged. The new targets emit deserialization code based on the GameFrameX LitJson package (namespace `GameFrameX.LitJSON.Runtime`, main classes `JsonData` / `JsonMapper`). Fully backward compatible — pick the target matching the JSON library already present in your runtime.

| Target | Original counterpart | Runtime dependency | Output |
|:-------|:---------------------|:-------------------|:-------|
| `cs-litjson` | `cs-simple-json` | `GameFrameX.LitJSON.Runtime` | single file, definition and runtime code merged |
| `cs-litjson-split` | `cs-simple-json-split` | `GameFrameX.LitJSON.Runtime` | split output, definitions in Def files, runtime code in Impl files |

#### When to use

- Your runtime (especially Unity hot-update layers) already standardizes on GameFrameX LitJson, and you want config loading to share that dependency instead of pulling in SimpleJSON as well.
- C# projects that want split granularity (definition / runtime separation) with LitJson deserialization.

#### How to use

```text
--codeTarget cs-litjson        # single merged file
--codeTarget cs-litjson-split  # Def / Impl separated output
```

Prerequisite: the runtime project references the GameFrameX LitJson package (namespace `GameFrameX.LitJSON.Runtime`, main classes `JsonData` / `JsonMapper`). The generated code depends only on that package, not on SimpleJSON; the targets do not affect the `cs-simple-json` family, so you can pick per export target.

### Localization folder configuration

In real projects, localization tables often run into conflicts during collaboration. The fix is to split the table files themselves per module — it is no longer just a matter of splitting sheets. The localization folder support makes every recognized file inside the configured folders load as a localization table.

#### Example configuration

- `l10n.provider=gameframex` — required. Any other value makes localization file detection fail.
- `l10n.textFile.path=./Excels/Tables/Localization/` — required. The value must be a folder path, otherwise localization file detection fails. The option can be repeated to specify multiple folders.

Export parameter reference:

```
--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key  --xargs l10n.textFile.path=./Excels/Tables/Localization/
```

#### Loading multiple folders (repeated options)

`l10n.textFile.path` can be repeated to load multiple folders, so localization tables can be split per module to avoid collaboration conflicts:

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

### Inline localization for text fields

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

`l10n.inline.outputPath` is the output directory of the generated `.xlsx` files. Both relative and absolute paths are supported; relative paths are resolved against the current working directory of the Luban process, for example:

```text
--xargs l10n.inline.outputPath=/path/to/Config/Excels/Local/Generated
```

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

#### Merge semantics on re-generation

The `.xlsx` files generated by `l10n.inline.*` are never deleted or overwritten wholesale; they are merged incrementally:

- **Keys already present in the xlsx**: the source-language column named by `l10n.inline.languageFieldName` follows the business table and is updated when the value changes; other language columns (translator content) stay unchanged.
- **Keys newly added to business tables**: appended to the end of the file with the inline default value.
- **Keys removed from business tables**: kept in the file (not deleted).
- **Rows added manually by users**: preserved.
- **Language columns**: all existing columns are preserved; if the column named by `l10n.inline.languageFieldName` is missing in an existing file, it is extended in the metadata.
- **Row order**: existing rows keep their exact order; new rows are appended at the end.
- **File rewrite**: the xlsx is rewritten only when there are new keys, a new language column, or changed source-language values; otherwise the file is left untouched. A rewrite keeps cell text and `##` comment rows but drops styling (colors, column widths, comments, etc.).

In short: **append-only, no deletion, no reordering; the source-language column follows the business table, other language columns are never overwritten**. Treat the generated directory as the translation entry point after the first run: subsequent runs only append newly appearing keys and sync source-language values, while translated content in other languages is always kept.

Generated files use the standard Luban table format, keep the original metadata header layout, fill data rows starting from column B (column A stays empty), use `key | zh_CN` as the header language columns, and use the `.xlsx` extension. External hand-maintained localization tables take precedence over inline values; an inline key must carry the same value across tables; valid empty values are preserved.

### File-name based automatic table import

Tables are registered automatically through file name conventions: the table schema is defined inside the xlsx itself, and the file name carries the sort prefix, table name, export group, and comment. Together with the directory layout this also derives the namespace — no per-table XML declarations needed.

#### When to use

- **Schema-free table registration**: adding a config table only takes a new xlsx file named by convention; there is no XML definition to maintain in parallel.
- **Directory as namespace**: organize tables into per-module folders and get the generated code namespaces for free.
- **Multi-file sub-tables**: split one table's rows across several files; they are merged back into a single table by file name.
- **Group-filtered export**: the group segment in the file name (e.g. `s` for client, `c` for server) lets one table repository serve different targets with different subsets.

#### Required export parameter

```text
--xargs tableImporter.name=gameframex
```

#### Naming format

`[any letter]-[table name]-[group]-[comment].xlsx`

- The file name starts with `any letter-`.
- The table name segment is English without spaces; underscores are allowed.
- The group segment must be one of the defined groups (`s` or `c`) and is optional.
- The comment segment can be any length. Only the content between the first and second `-` is used, with `Tb` prefixed, as the final table name.

#### Examples

##### Table names

- `L-Localization.xlsx` => `Tb`Localization
- `C-Achievement-成就表.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA-BBB.xlsx` => `Tb`Achievement
- `C-Achievement-成就表-AAA-BBB-CCC.xlsx` => `Tb`Achievement

##### Group names

- `C-Achievement-s-成就表.xlsx` => `Tb`Achievement, exported only when the current export target is `s`
- `C-Achievement-c-成就表-AAA.xlsx` => `Tb`Achievement, exported only when the current export target is `c`
- `C-Achievement-s-成就表-AAA-BBB.xlsx` => `Tb`Achievement, exported only when the current export target is `s`
- `C-Achievement-c-成就表-AAA-BBB-CCC.xlsx` => `Tb`Achievement, exported only when the current export target is `c`

#### Advanced options and behavior details

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

### Exclude directories from import (`--excludePath`)

Works with file-name based automatic table import (`tableImporter.name=gameframex`): table files under the given directories are skipped while scanning the data directory. The option can be repeated:

```text
--excludePath Local --excludePath Test
```

- Matching is a case-insensitive prefix match against the path relative to the data directory; `Local` also matches `LocalExtra/`.
- `/` in the value is converted to `\`. On Windows multi-level directories work (e.g. `Local\UI\`); on macOS / Linux the scanned relative paths use `/`, so only a top-level directory name without separators (e.g. `Local`) matches.

### Enum auto collection (autoExtend)

A new `autoExtend` capability is added: when an enum is marked as auto-extended, the build scans all table data referencing that enum, collects values not declared in the definition, and adds them as formal enum items. There is no need to enumerate all values in the schema up front — values grow naturally with the data, and the generated enum code contains both predefined and auto-collected items.

#### When to use

- Enum values keep growing with business data and you do not want to maintain the enum definition every time.
- The "name" of a value matters more than its number (recommended together with json/string exports).

#### Configuration

XML schema (dedicated attribute):

```xml
<enum name="Color" auto_extend="true">
  <var name="RED" value="0" />
  <var name="GREEN" value="1" />
</enum>
```

Excel schema (reuses the existing `tags` column; no new column needed): write `auto_extend` or `auto_extend=1` into the `tags` column of the enum's row in `__enums__` to enable it.

#### Behavior

- Collection runs after type compilation and before code generation and formal data loading; the generated enum code includes the auto-collected items.
- Undefined name values (e.g. `PURPLE`) get new int values assigned in lexicographic order (incrementing from `max(predefined items) + 1`).
- Undefined numeric values (e.g. `42`) keep the literal value and are named `AUTO_VALUE_42`.
- Results are fully deterministic for the same input data (independent of loading order).
- Auto-collected items are tagged `auto=1`, and the details of new items are printed to the log.

#### Notes

- The int values of auto-collected items depend on the current data set — **adding or removing data may change the assigned values**. Binary exports (int-encoded) need extra care; json/string exports (name-encoded) are unaffected.
- Enums without `auto_extend` behave exactly as before; undefined values still raise errors under the original logic.

## Docker

Luban provides Docker images. Image tags match Git tags and use SemVer without the `v` prefix, e.g. `3.13.1`; see [Releases](https://github.com/GameFrameX/luban/releases) for available versions. The image contains the main `Luban` program (`Luban.dll`) only, without `Luban.Agent` or `Luban.Mcp`.

Images:

| Registry | Image |
|:---------|:------|
| Docker Hub | `docker.io/gameframex/gameframex-luban:<version>` |
| GitHub Container Registry | `ghcr.io/gameframex/gameframex-luban:<version>` |
| Aliyun Container Registry | `<ALIYUN_REGISTRY_URL>/<ALIYUN_NAMESPACE>/gameframex-luban:<version>` |

Example:

```bash
docker run --rm docker.io/gameframex/gameframex-luban:<version> --help
```

Mount the working directory to run Luban in the current project directory:

```bash
docker run --rm -v "$PWD:/work" -w /work docker.io/gameframex/gameframex-luban:<version> --help
```

Only exact version tags are published; there are no floating tags such as `latest`, `5`, or `5.1`.

## Documentation

- [Official Documentation](https://www.datable.cn/en/docs/intro)
- [Quick Start](https://www.datable.cn/en/docs/guide/install)
- [AI support (Skills / Agent / MCP / schema-json)](https://www.datable.cn/en/docs/ai/overview)
- [What's new in 5.x (vs 4.x)](https://www.datable.cn/en/docs/other/whats-new)
- **Example Project** ([github](https://github.com/focus-creative-games/luban_examples)) ([gitee](https://gitee.com/focus-creative-games/luban_examples))

AI assets in this repo: [`ai/`](./ai/README.md).

## Developers: running tests

In-repo xUnit integration tests (no dependency on external `luban_examples`):

```bash
dotnet test src/Luban.Tests/Luban.Tests.csproj
```

- Fixtures and expected outputs: `tests/fixtures/`, `tests/golden/`
- Refresh goldens after export changes: `pwsh tests/scripts/update-goldens.ps1` or `bash tests/scripts/update-goldens.sh`
- Or set `LUBAN_UPDATE_GOLDEN=1` while running tests to rewrite goldens in place

## Support and contact

- QQ group: 692890842 (Luban development exchange group)
- discord: <https://discord.gg/dGY4zzGMJ4>
- Email: <luban@code-philosophy.com>

## license

Luban is licensed under the [MIT](https://github.com/focus-creative-games/luban/blob/main/LICENSE) license
