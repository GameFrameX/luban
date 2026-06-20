# Split Code Targets Execution Report

## Current Status

Execution started from:

- Plan: `docs/split-code-targets-execution-acceptance-plan.md`
- Branch: `main`
- Baseline commit: `b504d1a feat(csharp): 新增 split 系列代码生成目标`

This report records evidence gathered while executing the plan. It is not the final completion report.

## Current Acceptance Audit

Status: implementation and direct generation are mostly complete, but final acceptance is not complete.

Evidence proved in this workspace:

- Main Luban build passes with 0 warnings and 0 errors.
- Split template directories are copied to `src/Luban/bin/Debug/net8.0/Templates`.
- No source code target named `*-ai-def` or `cs-ai-def` is present.
- All split-enabled language projects use `Templates\**\*.sbn` copy rules.
- Direct GameFrameX generation from `/Users/blank/Documents/GithubWorks/GameFrameX/Config` passes for all implemented old/split targets except Python.
- Python split generation is blocked by the same preserved-keyword failure as the existing `python-json` target.
- GameFrameX server C# config project compiles with generated split C# files.
- GameFrameX Unity hotfix project compiles with generated split C# files.
- Rust JSON old/split generated fixtures pass `cargo check`.

Registered split code targets found in source:

- `cs-simple-json-split`
- `cs-bin-split`
- `cs-dotnet-json-split`
- `cs-dotnet-bin-split`
- `go-json-split`
- `go-bin-split`
- `rust-json-split`
- `rust-bin-split`
- `cpp-rawptr-bin-split`
- `cpp-sharedptr-bin-split`
- `java-json-split`
- `java-bin-split`
- `typescript-json-split`
- `typescript-bin-split`
- `typescript-protobuf-split`
- `python-json-split`
- `lua-bin-split`
- `lua-lua-split`
- `php-json-split`
- `gdscript-json-split`

Evidence not yet available:

- `gfx-kernel-*` acceptance has not run because the command family is not available on `PATH`.
- Language-native checks are incomplete for Go, Java, TypeScript, PHP, Lua, GDScript, C++, and Rust bin fixtures.
- Python native validation is blocked before code output by the existing enum item keyword issue.

Current local tool availability:

| Tool | Status |
|---|---|
| `cargo` | available at `/Users/blank/.cargo/bin/cargo` |
| `cmake` | available at `/opt/homebrew/bin/cmake` |
| `c++` | available at `/usr/bin/c++` |
| `javac` / `java` | available at `/usr/bin`, but Java runtime lookup fails |
| `go` | unavailable |
| `tsc` | unavailable |
| `php` | unavailable |
| `lua` | unavailable |
| `godot` | unavailable |
| `gfx-kernel-init` / `gfx-kernel-plan` / `gfx-kernel-doc` / `gfx-kernel-maintain` | unavailable |

Python blocker decision:

- `python-json` and `python-json-split` both fail on `GiftFilterType::None`.
- The generator can theoretically escape enum member names, for example `None` -> `None_`, but that changes Python generated-code API surface.
- Because the plan says to stop when a language requires a public API change, this is recorded as a design decision instead of being changed silently.

GameFrameX search result:

- A repository search excluding `.git` and `node_modules` found no local `gfx-kernel-*` command implementation.
- Only direct Luban config scripts such as `/Users/blank/Documents/GithubWorks/GameFrameX/Config/gen-client-bin.sh` and `/Users/blank/Documents/GithubWorks/GameFrameX/Config/gen-client-json.sh` are present.
- A second focused search for `gfx-kernel`, `/gfx-kernel`, and `kernel-` also found no executable acceptance runner.

## Phase 0: C# Baseline

Status: passed.

Build command:

```bash
dotnet build /Users/blank/Documents/GithubWorks/luban/src/Luban/Luban.csproj --no-restore
```

Result:

- Build succeeded.
- 0 warnings.
- 0 errors.

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `client` | `json` | `cs-simple-json` | passed |
| `client` | `bin` | `cs-bin` | passed |
| `server` | `json` | `cs-dotnet-json` | passed |
| `server` | `bin` | `cs-dotnet-bin` | passed |
| `client` | `json` | `cs-simple-json-split` | passed |
| `client` | `bin` | `cs-bin-split` | passed |
| `server` | `json` | `cs-dotnet-json-split` | passed |
| `server` | `bin` | `cs-dotnet-bin-split` | passed |

Evidence directory:

```text
/tmp/luban-phase0-csharp
```

Confirmed split output shape:

- `Tables/ItemConfig.Def.cs`
- `Tables/ItemConfig.Impl.cs`
- `TablesComponent.Def.cs`
- `TablesComponent.Impl.cs`

Confirmed namespaces:

- `client`: `Hotfix.Config`
- `server`: `GameFrameX.Config`

Confirmed implementation files do not duplicate data fields, constants, or base class declarations already owned by definition files.

C# native compile checks:

| Fixture | Command | Result |
|---|---|---|
| Server config | `dotnet build /Users/blank/Documents/GithubWorks/GameFrameX/Server/GameFrameX.Config/GameFrameX.Config.csproj --no-restore` | passed, 0 warnings, 0 errors |
| Unity hotfix | `dotnet build /Users/blank/Documents/GithubWorks/GameFrameX/Unity/Unity.HotFix.csproj --no-restore` | passed, 111 warnings, 0 errors |

Unity hotfix note:

- The warnings are existing Unity/project dependency warnings and obsolete API warnings.
- The generated split config files under `Assets/Hotfix/Config/Generate` are included by the project and did not produce compile errors.

## Phase 1: Go Split Targets

Status: generation passed; native compile check not run because Go is not installed in this environment.

Implemented targets:

- `go-json-split`
- `go-bin-split`

Added code target files:

- `src/Luban.Golang/CodeTarget/GoSplitCodeTargetBase.cs`
- `src/Luban.Golang/CodeTarget/GoJsonSplitCodeTarget.cs`
- `src/Luban.Golang/CodeTarget/GoBinSplitCodeTarget.cs`

Added template directories:

- `src/Luban.Golang/Templates/go-json-split`
- `src/Luban.Golang/Templates/go-bin-split`

Updated project template copy rule:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `client` | `json` | `go-json` | passed |
| `client` | `bin` | `go-bin` | passed |
| `client` | `json` | `go-json-split` | passed |
| `client` | `bin` | `go-bin-split` | passed |
| `all` | `json` | `go-json-split` | passed |
| `all` | `bin` | `go-bin-split` | passed |

Evidence directories:

```text
/tmp/luban-phase1-go
/tmp/luban-phase1-go-all
```

Confirmed split output shape:

- `Tables.ItemConfig.def.go`
- `Tables.ItemConfig.impl.go`
- `TablesComponent.def.go`
- `TablesComponent.impl.go`

Confirmed definition ownership:

- `type TablesItemConfig struct` is generated in `Tables.ItemConfig.def.go`.
- `const TypeId_TablesItemConfig` is generated in `Tables.ItemConfig.def.go`.
- `NewTablesItemConfig` and `GetTypeId` are generated in `Tables.ItemConfig.impl.go`.
- `JsonLoader` / `ByteBufLoader` and `TablesComponent` are generated in `TablesComponent.def.go`.
- `NewTables` is generated in `TablesComponent.impl.go`.

Native check:

```bash
go version
```

Result:

```text
zsh:1: command not found: go
```

This leaves the Go native compile acceptance item incomplete until a Go toolchain is available.

Note:

- GameFrameX `client` target uses `topModule = Hotfix.Config`, which is not a valid Go package identifier.
- The additional `target all` run uses `topModule = cfg`, which is suitable for Go package-level validation once Go is available.

## Phase 2: Rust Split Targets

Status: generation passed; `rust-json` and `rust-json-split` native checks passed; `rust-bin` native checks are blocked by missing `luban_lib` runtime dependency.

Implemented targets:

- `rust-json-split`
- `rust-bin-split`

Added code target files:

- `src/Luban.Rust/CodeTarget/RustSplitCodeTargetBase.cs`
- `src/Luban.Rust/CodeTarget/RustJsonSplitCodeTarget.cs`
- `src/Luban.Rust/CodeTarget/RustBinSplitCodeTarget.cs`

Added definition type visitors:

- `src/Luban.Rust/TypeVisitors/RustDefDeclaringTypeNameVisitor.cs`
- `src/Luban.Rust/TypeVisitors/RustDefDeclaringBoxTypeNameVisitor.cs`

Added template directories:

- `src/Luban.Rust/Templates/rust-json-split`
- `src/Luban.Rust/Templates/rust-bin-split`

Updated project template copy rule:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Baseline fix:

- The old Rust generator produced a module/type name collision for GameFrameX `target all`: `pub struct Tables` and `pub mod Tables`.
- Rust module names are now generated in lowercase (`tables`, `local`), while generated Rust type names remain unchanged.
- This preserves the `Tables` manager type and fixes references such as `crate::tables::ItemConfig`.

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `all` | `json` | `rust-json` | passed |
| `all` | `bin` | `rust-bin` | passed |
| `all` | `json` | `rust-json-split` | passed |
| `all` | `bin` | `rust-bin-split` | passed |

Evidence directory:

```text
/tmp/luban-phase2-rust-split
```

Confirmed split output shape:

- `cfg/src/Tables.ItemConfig.Def.rs`
- `cfg/src/Tables.TbItemConfig.Def.rs`
- `cfg/src/Tables.Def.rs`
- runtime files remain present:
  - `cfg/src/lib.rs`
  - `cfg/src/tables.rs`
  - `cfg/src/local.rs`

Confirmed definition ownership:

- `Tables.ItemConfig.Def.rs` contains fields, comments, and `impl ItemConfigDef { pub const __ID__: ... }`.
- `Tables.ItemConfig.Def.rs` does not contain `new`, `deserialize`, JSON loader, or ByteBuf loader logic.
- `Tables.TbItemConfig.Def.rs` references `crate::tables::ItemConfigDef`, not runtime `crate::tables::ItemConfig`.
- Nullable field shape is preserved, for example `ExpireTime: Option<u64>`.
- Runtime loading/deserialization remains in `lib.rs`, `tables.rs`, and `local.rs`.

Native checks:

| Fixture | Command | Result |
|---|---|---|
| `rust-json` | `cd /tmp/luban-phase2-rust-split/rust-json-code/cfg && cargo check` | passed |
| `rust-json-split` | `cd /tmp/luban-phase2-rust-split/rust-json-split-code/cfg && cargo check` | passed |
| `rust-bin` | `cd /tmp/luban-phase2-rust-split/rust-bin-code/cfg && cargo check` | blocked |
| `rust-bin-split` | `cd /tmp/luban-phase2-rust-split/rust-bin-split-code/cfg && cargo check` | blocked |

`rust-bin` / `rust-bin-split` blocker:

```text
failed to read `/private/tmp/luban-phase2-rust-split/luban_lib/Cargo.toml`
No such file or directory (os error 2)
```

Additional check:

- `find /Users/blank/Documents/GithubWorks/GameFrameX /Users/blank/Documents/GithubWorks/luban -name 'luban_lib' -o -path '*/luban_lib/Cargo.toml'` found no `luban_lib` crate.
- Both current Luban templates and the bundled GameFrameX Luban templates reference `luban_lib` for `rust-bin`.

Conclusion:

- The previous Rust JSON baseline failure is fixed.
- `rust-json-split` has a native compile check.
- `rust-bin-split` generation is implemented, but native compile acceptance needs the `luban_lib` runtime crate to be available in the expected fixture path.

Next Rust action:

1. Add or expose the `luban_lib` crate used by `rust-bin` fixtures.
2. Re-run `cargo check` for `rust-bin` and `rust-bin-split`.

## Phase 3: C++ Split Targets

Status: conservative generation passed; native compile check not run because no C++ fixture build command is currently identified.

Implemented targets:

- `cpp-rawptr-bin-split`
- `cpp-sharedptr-bin-split`

Added code target files:

- `src/Luban.Cpp/CodeTarget/CppSplitCodeTargetBase.cs`
- `src/Luban.Cpp/CodeTarget/CppBinRawptrSplitCodeTarget.cs`
- `src/Luban.Cpp/CodeTarget/CppBinSharedptrSplitCodeTarget.cs`

Added template directories:

- `src/Luban.Cpp/Templates/cpp-rawptr-bin-split`
- `src/Luban.Cpp/Templates/cpp-sharedptr-bin-split`

Updated project template copy rule:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `all` | `bin` | `cpp-rawptr-bin` | passed |
| `all` | `bin` | `cpp-sharedptr-bin` | passed |
| `all` | `bin` | `cpp-rawptr-bin-split` | passed |
| `all` | `bin` | `cpp-sharedptr-bin-split` | passed |

Evidence directory:

```text
/tmp/luban-phase3-cpp
```

Confirmed old output shape:

- `schema.h`
- `schema_0.cpp`

Confirmed split output shape:

- `schema.h`
- `Tables.ItemConfig.Def.h`
- `Tables.ItemConfig.Impl.cpp`
- `Tables.TbItemConfig.Def.h`
- `Tables.TbItemConfig.Impl.cpp`
- `Tables.Def.h`
- `Tables.Impl.cpp`
- equivalent per-type `*.Def.h` files for other exported beans/tables
- equivalent per-bean and per-table `*.Impl.cpp` files for exported beans/tables

Implementation notes:

- This C++ implementation preserves the existing aggregate runtime header (`schema.h`) and adds per-type definition headers for AI-readable lookup.
- Bean deserialization method bodies are split into per-bean `*.Impl.cpp` files.
- Table load/getter method bodies are split into per-table `*.Impl.cpp` files.
- Manager loading is split into `Tables.Impl.cpp`.
- Split C++ no longer emits the old aggregate `schema_0.cpp`, avoiding duplicate symbols when per-bean implementation files are compiled.
- `schema.h` keeps declarations, fields, and manager/table member declarations needed for compatibility.
- The per-type `*.Def.h` files contain declarations and fields, but also retain minimal runtime method declarations such as `deserialize` and `getTypeId`.

Reason for the split shape:

- The existing C++ generator is schema-aggregate based, not per-type based.
- The current split target keeps the original public aggregate include file name (`schema.h`) while moving runtime method bodies into implementation files.
- This avoids breaking existing `schemaFileNameWithoutExt` behavior while giving AI a per-type definition artifact such as `Tables.ItemConfig.Def.h`.

Native check:

- Not run. No C++ fixture build command or `gfx-kernel-*` acceptance command for C++ was identified in this environment.
- A minimal compile of one generated `*.Impl.cpp` alone is not a meaningful check because generated C++ includes runtime headers such as `CfgBean.h` and uses `::luban::ByteBuf`; those runtime headers are not present in the generated fixture directory.

## Phase 4: Java Split Targets

Status: generation passed; native compile check not run because this environment exposes `/usr/bin/javac` but has no Java Runtime installed.

Implemented targets:

- `java-json-split`
- `java-bin-split`

Added code target files:

- `src/Luban.Java/CodeTarget/JavaSplitCodeTargetBase.cs`
- `src/Luban.Java/CodeTarget/JavaJsonSplitCodeTarget.cs`
- `src/Luban.Java/CodeTarget/JavaBinSplitCodeTarget.cs`

Added definition type visitors:

- `src/Luban.Java/TypeVisitors/JavaDefDeclaringTypeNameVisitor.cs`
- `src/Luban.Java/TypeVisitors/JavaDefDeclaringBoxTypeNameVisitor.cs`

Added template directories:

- `src/Luban.Java/Templates/java-json-split`
- `src/Luban.Java/Templates/java-bin-split`

Updated project template copy rule:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Design decision:

- Java cannot split one public class across multiple files.
- The split targets preserve the original runtime Java files and add AI-readable companion definition files.
- Runtime public APIs are unchanged.

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `all` | `json` | `java-json` | passed |
| `all` | `bin` | `java-bin` | passed |
| `all` | `json` | `java-json-split` | passed |
| `all` | `bin` | `java-bin-split` | passed |

Evidence directory:

```text
/tmp/luban-phase4-java
```

Confirmed split output shape:

- `Tables/ItemConfig.java`
- `Tables/ItemConfigDef.java`
- `Tables/TbItemConfig.java`
- `Tables/TbItemConfigDef.java`
- `Tables.java`
- `TablesDef.java`

Confirmed definition ownership:

- `Tables/ItemConfigDef.java` contains fields, comments, package declaration, and `__ID__`.
- `Tables/ItemConfigDef.java` does not contain `deserialize`, `ByteBuf`, `JsonObject`, `getTypeId`, or `extends AbstractBean`.
- `Tables/TbItemConfigDef.java` references `cfg.Tables.ItemConfigDef`, not runtime `cfg.Tables.ItemConfig`.
- Runtime loading/deserialization remains in `Tables/ItemConfig.java`, `Tables/TbItemConfig.java`, and `Tables.java`.

Native check:

```bash
javac -version
```

Result:

```text
The operation couldn’t be completed. Unable to locate a Java Runtime.
Please visit http://www.java.com for information on installing Java.
```

This leaves the Java native compile acceptance item incomplete until a Java runtime/toolchain and generated-code dependencies are available.

## gfx-kernel-*

Status: not run.

Observed GameFrameX workflow names:

- `gfx-kernel-init`
- `gfx-kernel-plan`
- `gfx-kernel-doc sync`
- `gfx-kernel-maintain`

Reason:

- GameFrameX documentation references the `gfx-kernel-*` command family, not a bare `gfx-kernel` command.
- None of the `gfx-kernel-*` commands are currently available on `PATH` in this shell.
- The plan requires identifying the actual GameFrameX-side acceptance entry point before this gate can be executed.

Local substitute evidence:

- Direct Luban generation was run against `/Users/blank/Documents/GithubWorks/GameFrameX/Config`.
- C#, Go, Rust baseline, C++, Java, TypeScript, PHP, Lua, and GDScript generation results are recorded in this report.
- Split template copy was checked with:

```bash
find /Users/blank/Documents/GithubWorks/luban/src/Luban/bin/Debug/net8.0/Templates -path '*split*' -name '*.sbn'
```

Result:

- All implemented split template directories were present in the built Luban output.

Target registration evidence:

- `dotnet /Users/blank/Documents/GithubWorks/luban/src/Luban/bin/Debug/net8.0/Luban.dll --help` does not list code targets.
- Registration was verified by successful direct generation for each generated split target and by `[CodeTarget("...-split")]` source declarations.
- A source scan found no `[CodeTarget]` declarations containing `ai-def` or non-suffix `def` target names.

Template copy rule evidence:

- Every split-enabled project currently contains:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

## Phase 5: TypeScript Split Targets

Status: generation passed; native TypeScript check not run because `tsc` is not installed in this environment.

Implemented targets:

- `typescript-json-split`
- `typescript-bin-split`
- `typescript-protobuf-split`

Added code target files:

- `src/Luban.Typescript/CodeTarget/TypescriptSplitCodeTargetBase.cs`
- `src/Luban.Typescript/CodeTarget/TypescriptJsonSplitCodeTarget.cs`
- `src/Luban.Typescript/CodeTarget/TypescriptBinSplitCodeTarget.cs`
- `src/Luban.Typescript/CodeTarget/TypescriptProtobufSplitCodeTarget.cs`

Added definition type visitors:

- `src/Luban.Typescript/TypeVisitors/DefDeclaringTypeNameVisitor.cs`
- `src/Luban.Typescript/TypeVisitors/DefUnderlyingDeclaringTypeNameVisitor.cs`

Added template directories:

- `src/Luban.Typescript/Templates/typescript-json-split`
- `src/Luban.Typescript/Templates/typescript-bin-split`
- `src/Luban.Typescript/Templates/typescript-protobuf-split`

Updated project template copy rule:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Design decision:

- TypeScript targets currently generate an aggregate runtime `schema.ts`.
- The split targets preserve that runtime file and add AI-readable per-type `*.def.ts` companion files.
- This avoids changing existing import/runtime behavior while letting AI read a narrow file such as `Tables.ItemConfig.def.ts`.

Build command:

```bash
dotnet build /Users/blank/Documents/GithubWorks/luban/src/Luban/Luban.csproj --no-restore
```

Result:

- Build succeeded.
- 0 warnings.
- 0 errors.

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `all` | `json` | `typescript-json` | passed |
| `all` | `bin` | `typescript-bin` | passed |
| `all` | `protobuf3-bin` | `typescript-protobuf` | passed |
| `all` | `json` | `typescript-json-split` | passed |
| `all` | `bin` | `typescript-bin-split` | passed |
| `all` | `protobuf3-bin` | `typescript-protobuf-split` | passed |

Evidence directory:

```text
/tmp/luban-phase5-typescript
```

Confirmed split output shape:

- `schema.ts`
- `Tables.ItemConfig.def.ts`
- `Tables.TbItemConfig.def.ts`
- `Tables.def.ts`
- equivalent per-type `*.def.ts` files for other exported beans/tables

Confirmed definition ownership:

- `Tables.ItemConfig.def.ts` contains fields, comments, namespace declaration, and `ItemConfigTypeId`.
- `Tables.TbItemConfig.def.ts` references `Tables.ItemConfigDef`, not runtime `Tables.ItemConfig`.
- Def files do not contain `constructor`, `deserialize`, `resolve`, `ByteBuf`, loader, runtime `class`, `new`, or `pb.cfg` usage.
- Runtime loading/deserialization remains in `schema.ts`.

Native check:

```bash
tsc --version
```

Result:

```text
zsh:1: command not found: tsc
```

This leaves the TypeScript native compile acceptance item incomplete until a TypeScript compiler and generated-code dependencies are available.

## Phase 6: Python / Lua / PHP / GDScript Split Targets

Status: Lua, PHP, and GDScript generation passed; Python generation is blocked by an existing non-split target failure in the GameFrameX schema.

Implemented targets:

- `python-json-split`
- `lua-bin-split`
- `lua-lua-split`
- `php-json-split`
- `gdscript-json-split`

Added split target base/code target files:

- `src/Luban.Python/CodeTarget/PythonSplitCodeTargetBase.cs`
- `src/Luban.Python/CodeTarget/PythonJsonSplitCodeTarget.cs`
- `src/Luban.Lua/CodeTarget/LuaSplitCodeTargetBase.cs`
- `src/Luban.Lua/CodeTarget/LuaBinSplitCodeTarget.cs`
- `src/Luban.Lua/CodeTarget/LuaLuaSplitCodeTarget.cs`
- `src/Luban.PHP/CodeTarget/PHPSplitCodeTargetBase.cs`
- `src/Luban.PHP/CodeTarget/PHPJsonSplitCodeTarget.cs`
- `src/Luban.Gdscript/CodeTarget/GdscriptSplitCodeTargetBase.cs`
- `src/Luban.Gdscript/CodeTarget/GdscriptJsonSplitCodeTarget.cs`

Added template directories:

- `src/Luban.Python/Templates/python-json-split`
- `src/Luban.Lua/Templates/lua-bin-split`
- `src/Luban.Lua/Templates/lua-lua-split`
- `src/Luban.PHP/Templates/php-json-split`
- `src/Luban.Gdscript/Templates/gdscript-json-split`

Updated project template copy rule for these projects:

```xml
<None Update="Templates\**\*.sbn">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Design decision:

- These targets are all schema-style aggregate runtime generators.
- The split variants preserve the original runtime `schema.py`, `schema.lua`, `schema.php`, or `schema.gd` and add per-type `*.def.*` companion files.
- Def files are intentionally narrow AI-readable artifacts, not alternate runtime loaders.

Generation matrix run from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`:

| Target | Data Target | Code Target | Result |
|---|---|---|---|
| `all` | `json` | `python-json` | blocked |
| `all` | `json` | `python-json-split` | blocked |
| `all` | `json` | `php-json` | passed |
| `all` | `json` | `php-json-split` | passed |
| `all` | `json` | `gdscript-json` | passed |
| `all` | `json` | `gdscript-json-split` | passed |
| `all` | `bin` | `lua-bin` | passed |
| `all` | `bin` | `lua-bin-split` | passed |
| `all` | `lua` | `lua-lua` | passed |
| `all` | `lua` | `lua-lua-split` | passed |

Evidence directories:

```text
/tmp/luban-phase6-dynamic-langs
/tmp/luban-phase6-dynamic-langs-rerun2
```

Confirmed split output shape:

- PHP: `schema.php`, `Tables.ItemConfig.def.php`, `Tables.TbItemConfig.def.php`, `Tables.def.php`
- GDScript: `schema.gd`, `Tables.ItemConfig.def.gd`, `Tables.TbItemConfig.def.gd`, `Tables.def.gd`
- Lua bin/lua: `schema.lua`, `Tables.ItemConfig.def.lua`, `Tables.TbItemConfig.def.lua`, `Tables.def.lua`

Confirmed definition ownership:

- PHP/GDScript/Lua `ItemConfig` Def files contain fields, comments, and type-id constants where applicable.
- PHP/GDScript/Lua table Def files reference `ItemConfigDef` style types, not the runtime loaded classes as the main artifact.
- PHP/GDScript/Lua Def files do not contain constructors, deserialization, loader callbacks, byte readers, `fromJson`, `constructFrom`, or runtime table loading logic.
- Runtime loading/deserialization remains in the generated aggregate `schema.*` files.

Python blocker:

```text
the name of enum item 'GiftFilterType::None' is preserved keyword
```

This happens for both `python-json` and `python-json-split`, so it is inherited from the existing Python target and GameFrameX schema combination, not introduced by the split target.

Native checks:

| Tool | Result |
|---|---|
| `python3 --version` | `Python 3.14.2` |
| `php -v` | `zsh:1: command not found: php` |
| `lua -v` | `zsh:1: command not found: lua` |
| `godot --version` | `zsh:1: command not found: godot` |

No generated-code native execution was run for this phase because required runtime dependencies and fixture commands are not available. Python native validation is also blocked until the enum preserved-keyword issue is resolved or a compatible schema fixture is used.

Next action:

- Provide or expose the concrete `gfx-kernel-*` acceptance command.
- Wire the generated-output assertions from the plan into that runner.

## Current Cross-Target Acceptance Run

Status: direct generation and file-shape assertions passed, with toolchain-specific native checks still gated by the missing tools listed above.

Evidence directory:

```text
/tmp/luban-acceptance-current-20260620-01
```

Generation scope:

- Existing and split C# Unity/server targets:
  - `cs-simple-json`
  - `cs-bin`
  - `cs-dotnet-json`
  - `cs-dotnet-bin`
  - `cs-simple-json-split`
  - `cs-bin-split`
  - `cs-dotnet-json-split`
  - `cs-dotnet-bin-split`
- Existing and split Go targets:
  - `go-json`
  - `go-bin`
  - `go-json-split`
  - `go-bin-split`
- Split Rust, C++, Java, TypeScript, PHP, GDScript, and Lua targets:
  - `rust-json`
  - `rust-bin`
  - `rust-json-split`
  - `rust-bin-split`
  - `cpp-rawptr-bin`
  - `cpp-sharedptr-bin`
  - `cpp-rawptr-bin-split`
  - `cpp-sharedptr-bin-split`
  - `java-json`
  - `java-bin`
  - `java-json-split`
  - `java-bin-split`
  - `typescript-json`
  - `typescript-bin`
  - `typescript-protobuf`
  - `typescript-json-split`
  - `typescript-bin-split`
  - `typescript-protobuf-split`
  - `php-json`
  - `php-json-split`
  - `gdscript-json`
  - `gdscript-json-split`
  - `lua-bin`
  - `lua-lua`
  - `lua-bin-split`
  - `lua-lua-split`
- Python old/split targets were also run and both failed with the same expected existing preserved-keyword blocker:
  - `python-json`
  - `python-json-split`

Go generation note:

- Go targets require `--xargs lubanGoModule=...`.
- A first Go rerun without that option failed with `option 'lubanGoModule' not exists`.
- The Go old/split comparison was rerun successfully with:

```bash
--xargs lubanGoModule=github.com/gameframex/config
```

Current generation result:

```text
files=882
all-python-json.exit=1
all-python-json-split.exit=1
current file-shape assertions passed
```

Assertions covered:

- No generated current output contains `ai-def` or `cs-ai-def`.
- Existing C# targets still generate `Tables/ItemConfig.cs`.
- C# split targets generate `Tables/ItemConfig.Def.cs` and `Tables/ItemConfig.Impl.cs`.
- C# split targets do not generate `Tables/ItemConfig.cs`.
- Unity C# split namespace remains `Hotfix.Config`.
- Server C# split namespace remains `GameFrameX.Config`.
- C# implementation files do not duplicate data fields or constants from definition files.
- Go old targets generate `Tables.ItemConfig.go`.
- Go split targets generate `Tables.ItemConfig.def.go` and `Tables.ItemConfig.impl.go`.
- Go Def files own struct declarations, and Go Impl files do not duplicate those struct declarations.
- Rust split targets generate per-type Def companions and preserve runtime `lib.rs` / `tables.rs` layout.
- Rust table Def files reference `crate::tables::ItemConfigDef`, not runtime `crate::tables::ItemConfig`.
- Rust Def files do not contain deserialization, `ByteBuf`, loader, or constructor runtime markers.
- C++ split targets generate per-type `Tables.ItemConfig.Def.h` and `Tables.ItemConfig.Impl.cpp`, preserve aggregate runtime header `schema.h`, and do not generate split-mode `schema_0.cpp`.
- Java split targets generate runtime `ItemConfig.java` and companion `ItemConfigDef.java`.
- Java Def files do not contain `deserialize`, `ByteBuf`, `JsonObject`, `getTypeId`, or `extends AbstractBean`.
- TypeScript split targets generate `schema.ts` plus per-type `*.def.ts` files.
- TypeScript table Def files reference `Tables.ItemConfigDef`, not runtime `Tables.ItemConfig`.
- TypeScript Def files do not contain runtime markers such as constructors, deserialization, `ByteBuf`, loader logic, runtime classes, `new`, or protobuf runtime references.
- PHP, GDScript, and Lua split targets generate per-type `*.def.*` files.
- PHP, GDScript, and Lua Def files do not contain constructors, deserialization, loader callbacks, byte readers, `fromJson`, `constructFrom`, or runtime table loading logic.

Current native check rerun:

| Check | Result |
|---|---|
| `dotnet build /Users/blank/Documents/GithubWorks/luban/src/Luban/Luban.csproj --no-restore` | passed, 0 warnings, 0 errors |
| `dotnet build /Users/blank/Documents/GithubWorks/GameFrameX/Server/GameFrameX.Config/GameFrameX.Config.csproj --no-restore` | passed, 0 warnings, 0 errors |
| `dotnet build /Users/blank/Documents/GithubWorks/GameFrameX/Unity/Unity.HotFix.csproj --no-restore` | passed, 74 warnings, 0 errors |
| `cargo check` for `/tmp/luban-acceptance-current-20260620-01/all-rust-json-code/cfg` | passed |
| `cargo check` for `/tmp/luban-acceptance-current-20260620-01/all-rust-json-split-code/cfg` | passed |
| `cargo check` for `/tmp/luban-acceptance-current-20260620-01/all-rust-bin-code/cfg` | blocked by missing `../../luban_lib/Cargo.toml` |
| `cargo check` for `/tmp/luban-acceptance-current-20260620-01/all-rust-bin-split-code/cfg` | blocked by missing `../../luban_lib/Cargo.toml` |

Current toolchain/gfx-kernel availability:

```text
javac/java: command exists, but Java Runtime lookup fails
go: command not found
tsc: command not found
php: command not found
lua: command not found
godot: command not found
gfx-kernel-init: command not found
gfx-kernel-plan: command not found
gfx-kernel-doc: command not found
gfx-kernel-maintain: command not found
```

Acceptance interpretation:

- The current output shape matches the requested direction: old targets remain intact, new targets use the `-split` suffix, and AI-readable artifacts are narrow per-type Def files rather than global AI schema files.
- This does not close the `gfx-kernel-*` gate because no `gfx-kernel-*` command is available in this environment.
- This does not close missing language-native gates for toolchains that are not installed or whose generated-code runtime dependencies are unavailable.

## Local Acceptance Script

Status: added and verified.

Script:

```text
scripts/accept-split-code-targets.sh
```

Purpose:

- Provides a repeatable local substitute for the unavailable `gfx-kernel-*` generated-code acceptance runner.
- Builds Luban.
- Runs the current direct GameFrameX generation matrix from `/Users/blank/Documents/GithubWorks/GameFrameX/Config`.
- Treats `python-json` and `python-json-split` as expected-fail checks and asserts both fail for the same existing `GiftFilterType::None` preserved-keyword blocker.
- Runs the generated-output shape and ownership assertions recorded above.

Environment overrides:

```bash
LUBAN_ROOT=/Users/blank/Documents/GithubWorks/luban
GFX_CONFIG_DIR=/Users/blank/Documents/GithubWorks/GameFrameX/Config
DOTNET_DLL=/Users/blank/Documents/GithubWorks/luban/src/Luban/bin/Debug/net8.0/Luban.dll
BASE=/tmp/luban-split-code-targets-acceptance
GO_MODULE=github.com/gameframex/config
```

Verification command:

```bash
BASE=/tmp/luban-acceptance-script-20260620-03 scripts/accept-split-code-targets.sh
```

Result:

```text
BASE=/tmp/luban-acceptance-script-20260620-03
files=916
split code target acceptance assertions passed
```

Pre-commit verification command:

```bash
BASE=/tmp/luban-acceptance-precommit-20260620 scripts/accept-split-code-targets.sh
```

Pre-commit result:

```text
BASE=/tmp/luban-acceptance-precommit-20260620
files=916
split code target acceptance assertions passed
```

This script does not replace final `gfx-kernel-*` acceptance. It makes the local direct-Luban evidence reproducible while that external runner is unavailable.

## C++ Impl Split Status

Status: generated-output split shape is implemented; native C++ compile validation is still blocked by missing generated-code runtime fixture/dependencies.

Current C++ split behavior:

- Old targets remain unchanged.
- Split targets still generate an aggregate runtime header:
  - `schema.h`
- Split targets do not generate the old aggregate `schema_0.cpp`.
- Split targets generate per-type AI-readable definition headers:
  - `Tables.ItemConfig.Def.h`
  - `Tables.TbItemConfig.Def.h`
  - `Tables.Def.h`
  - equivalent `*.Def.h` files for other exported types.
- Split targets generate per-bean runtime implementation files:
  - `Tables.ItemConfig.Impl.cpp`
  - equivalent `*.Impl.cpp` files for other exported beans.
- Split targets generate per-table runtime implementation files:
  - `Tables.TbItemConfig.Impl.cpp`
  - equivalent `*.Impl.cpp` files for other exported tables.
- Split targets generate manager runtime implementation:
  - `Tables.Impl.cpp`

Verified current split invariants:

- `schema.h` does not contain inline `load(::luban::ByteBuf&)` method bodies.
- `schema.h` does not contain inline `load(::luban::Loader<::luban::ByteBuf>)` method bodies.
- `schema_0.cpp` is absent for split targets.

Acceptance still needed:

- A real generated C++ fixture build is identified and run with the same include/runtime dependencies as existing C++ generated code.

## Remaining Work

- Provide or identify a C++ generated-code fixture build with Luban runtime headers such as `CfgBean.h`, then compile `cpp-rawptr-bin-split` and `cpp-sharedptr-bin-split`.
- Provide the Rust `luban_lib` runtime crate, then re-run `cargo check` for `rust-bin` and `rust-bin-split`.
- Run `gfx-kernel-*` once the actual runner command is known.
- Run language-native checks for Go after installing or exposing a Go toolchain.
- Run language-native checks for Java after installing or exposing a Java runtime/toolchain and generated-code dependencies.
- Run language-native checks for TypeScript after installing or exposing `tsc` and generated-code dependencies.
- Decide whether to fix Python reserved keyword escaping for enum item names or validate `python-json-split` with a schema fixture that does not contain `GiftFilterType::None`.
- Run language-native checks for PHP, Lua, and GDScript after installing or exposing their runtimes and generated-code dependencies.
