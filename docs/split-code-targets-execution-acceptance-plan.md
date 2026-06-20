# Split Code Targets Execution And Acceptance Plan

## Background

The current C# split implementation establishes the target rule that must be reused for other languages:

- Keep every existing code target unchanged.
- Add a new split target by appending `-split` to the original target name.
- Do not reintroduce `*-ai-def`, `schema.cs`, or all-in-one AI-only outputs.
- Generate per-type files where the definition file is AI-readable and the implementation file keeps runtime behavior.
- Preserve each target's original namespace, package, module, imports, runtime base types, and serialization semantics.

For C#, the baseline split targets are:

- `cs-simple-json-split`
- `cs-bin-split`
- `cs-dotnet-json-split`
- `cs-dotnet-bin-split`

## Scope

### In Scope

Add split targets for languages where the language model and existing Luban code generation model can support a safe definition/implementation separation.

Primary candidates:

- Go
  - `go-json-split`
  - `go-bin-split`
- Rust
  - `rust-json-split`
  - `rust-bin-split`
- C++
  - `cpp-rawptr-bin-split`
  - `cpp-sharedptr-bin-split`
- Java
  - `java-json-split`
  - `java-bin-split`

Secondary candidates, after the first group is stable:

- TypeScript
  - `typescript-json-split`
  - `typescript-bin-split`
  - `typescript-protobuf-split`
- Python
  - `python-json-split`
- Lua
  - `lua-bin-split`
  - `lua-lua-split`
- PHP
  - `php-json-split`
- GDScript
  - `gdscript-json-split`

### Out Of Scope

- Changing existing target names.
- Changing existing non-split output behavior.
- Adding or restoring `*-ai-def` targets.
- Generating global schema files as the primary AI-readable output.
- Forcing languages without partial-class semantics into unsafe API changes.

## Target Inventory

Current code targets in this repo:

| Language | Existing Targets | Split Plan |
|---|---|---|
| C# | `cs-simple-json`, `cs-bin`, `cs-dotnet-json`, `cs-dotnet-bin`, `cs-newtonsoft-json`, `cs-editor-json` | C# baseline already covers the four GameFrameX runtime targets. Consider `cs-newtonsoft-json-split` only if still used. Do not split editor target unless requested. |
| Go | `go-json`, `go-bin` | Safe first batch. Go supports same-package multi-file output. |
| Rust | `rust-json`, `rust-bin` | Safe with care. Requires module layout validation. |
| C++ | `cpp-rawptr-bin`, `cpp-sharedptr-bin` | Safe with care. Existing header/source split must be respected. |
| Java | `java-json`, `java-bin` | Needs design decision. Java cannot split one class across files. Prefer `XxxDef.java` + unchanged runtime class, or interface/base-class pattern if accepted. |
| TypeScript | `typescript-json`, `typescript-bin`, `typescript-protobuf` | Later batch. Current targets are schema-style all-in-one. |
| Python | `python-json` | Later batch. Current target is schema-style all-in-one. |
| Lua | `lua-bin`, `lua-lua` | Later batch. Current target is schema-style all-in-one. |
| PHP | `php-json` | Later batch. Current target is schema-style all-in-one. |
| GDScript | `gdscript-json` | Later batch. Current target is schema-style all-in-one. |
| Protobuf/FlatBuffers | `protobuf2`, `protobuf3`, `cs-protobuf3`, `flatbuffers` | Do not split unless there is a separate requirement. These are schema definition targets already. |

## Universal Split Rules

Every new split target must follow these rules:

1. The target name is exactly the original target name plus `-split`.
2. The original target must continue generating byte-for-byte compatible output where practical.
3. The split target must preserve original data target compatibility.
4. Definition files contain:
   - type/class/struct declaration
   - fields/properties
   - comments
   - constants needed by runtime methods
   - package/namespace/module declarations
   - minimal imports required for type declarations
5. Implementation files contain:
   - constructors
   - deserialization/loading
   - table indexing
   - reference resolving
   - localization behavior
   - `ToString`/debug helpers
   - runtime registration and manager loading behavior
6. Enums remain single-file unless the language requires otherwise.
7. No split output may create a global all-tables schema as the AI-facing artifact.
8. If a language cannot split one type across files, document and use the least invasive language-native substitute.

## Implementation Plan

### Phase 0: Baseline Lock

Goal: freeze the C# behavior as the reference before touching other languages.

Tasks:

1. Confirm C# split target registration:
   - `cs-simple-json-split`
   - `cs-bin-split`
   - `cs-dotnet-json-split`
   - `cs-dotnet-bin-split`
2. Confirm `src/Luban.CSharp/Luban.CSharp.csproj` uses:

   ```xml
   <None Update="Templates\**\*.sbn">
     <CopyToOutputDirectory>Always</CopyToOutputDirectory>
   </None>
   ```

3. Run C# baseline generation using GameFrameX config.
4. Record generated output file shape:
   - `ItemConfig.Def.cs`
   - `ItemConfig.Impl.cs`
   - no `ItemConfig.cs` for split targets
5. Confirm no residual `cs-ai-def`, `schema.cs`, or `cs-def` source references.

Acceptance:

- C# build passes.
- Four C# split targets generate successfully.
- Existing four C# non-split targets still generate successfully.
- Generated namespaces match `targets[].topModule`.

### Phase 1: Go Split Targets

Targets:

- `go-json-split`
- `go-bin-split`

Suggested files:

- `src/Luban.Golang/CodeTarget/GoSplitCodeTargetBase.cs`
- `src/Luban.Golang/CodeTarget/GoJsonSplitCodeTarget.cs`
- `src/Luban.Golang/CodeTarget/GoBinSplitCodeTarget.cs`
- `src/Luban.Golang/Templates/go-json-split/*.sbn`
- `src/Luban.Golang/Templates/go-bin-split/*.sbn`

Output shape:

- `item_config.def.go`
- `item_config.impl.go`
- table manager definition/implementation split files

Implementation notes:

- Keep generated files in the same Go package.
- Put struct definitions in `.def.go`.
- Put methods, loading, reference resolving, and serialization helpers in `.impl.go`.
- Keep original `go-json` and `go-bin` untouched.

Acceptance:

- `go-json` and `go-bin` still generate.
- `go-json-split` and `go-bin-split` generate.
- `go test` or `go test ./...` passes for a generated fixture if a fixture module is available.
- No duplicate struct definitions between def and impl files.

### Phase 2: Rust Split Targets

Targets:

- `rust-json-split`
- `rust-bin-split`

Suggested files:

- `src/Luban.Rust/CodeTarget/RustSplitCodeTargetBase.cs`
- `src/Luban.Rust/CodeTarget/RustJsonSplitCodeTarget.cs`
- `src/Luban.Rust/CodeTarget/RustBinSplitCodeTarget.cs`
- `src/Luban.Rust/Templates/rust-json-split/*.sbn`
- `src/Luban.Rust/Templates/rust-bin-split/*.sbn`

Implementation notes:

- Preserve existing `lib.sbn`, `mod.sbn`, and `toml.sbn` semantics.
- Do not break Rust module visibility.
- Prefer one module per generated type when that matches existing output.
- Keep struct/enum shape in definition modules.
- Put impl blocks and load/deserialize behavior in implementation modules.

Acceptance:

- `rust-json` and `rust-bin` still generate.
- `rust-json-split` and `rust-bin-split` generate.
- `cargo check` passes for generated fixtures.
- Public module paths remain compatible with original target expectations.

### Phase 3: C++ Split Targets

Targets:

- `cpp-rawptr-bin-split`
- `cpp-sharedptr-bin-split`

Suggested files:

- `src/Luban.Cpp/CodeTarget/CppSplitCodeTargetBase.cs`
- `src/Luban.Cpp/CodeTarget/CppBinRawptrSplitCodeTarget.cs`
- `src/Luban.Cpp/CodeTarget/CppBinSharedptrSplitCodeTarget.cs`
- `src/Luban.Cpp/Templates/cpp-rawptr-bin-split/*.sbn`
- `src/Luban.Cpp/Templates/cpp-sharedptr-bin-split/*.sbn`

Implementation notes:

- Respect existing `.h` and `.cpp` split.
- Avoid creating a C++ equivalent of `schema.cpp` as the AI-readable artifact.
- Candidate output:
  - `ItemConfig.Def.h`
  - `ItemConfig.Impl.h`
  - `ItemConfig.Impl.cpp`
- If the existing runtime requires one aggregate schema header/source, keep those as runtime manager files but do not use them as the AI definition artifact.

Acceptance:

- `cpp-rawptr-bin` and `cpp-sharedptr-bin` still generate.
- Split variants generate.
- A generated fixture compiles with the same compiler flags as current C++ generated code.
- Headers do not define duplicate symbols.

### Phase 4: Java Split Design And Implementation

Targets:

- `java-json-split`
- `java-bin-split`

Important constraint:

Java cannot split a single class across multiple files like C# partial classes or Go package files. This phase requires an explicit design choice before implementation.

Preferred option:

- Generate `ItemConfigDef.java` as the AI-readable structure.
- Keep `ItemConfig.java` or `ItemConfigImpl.java` as the runtime class.
- Do not change existing runtime APIs unless explicitly approved.

Alternative option:

- Generate an interface or base class for definitions.
- Runtime class extends/implements it.
- This has higher API risk.

Acceptance:

- Existing `java-json` and `java-bin` remain unchanged.
- Split target output compiles with `javac` or the project build tool.
- Runtime public API changes are documented.

### Phase 5: Schema-Style Languages

Languages:

- TypeScript
- Python
- Lua
- PHP
- GDScript

Implementation notes:

- These targets currently lean toward all-in-one schema generation.
- Before implementation, add or reuse a per-type output base similar to C# split handling.
- Each language needs a language-native output convention:
  - TypeScript: module files or `*.def.ts`/`*.impl.ts`
  - Python: modules or partial-style companion files with imports
  - Lua: table/module split files
  - PHP: class/interface separation
  - GDScript: likely separate scripts/resources

Acceptance:

- Original targets still generate.
- Split targets produce per-table/per-bean definition artifacts.
- Runtime generated code remains importable/compilable/interpretable.

## gfx-kernel-* Acceptance Flow

GameFrameX uses the `gfx-kernel-*` workflow family rather than a single bare `gfx-kernel` command. The GameFrameX documents currently reference:

- `gfx-kernel-init`
- `gfx-kernel-plan`
- `gfx-kernel-doc sync`
- `gfx-kernel-maintain`

Before execution, identify the concrete command that owns generated-code acceptance in the local environment. If the command family is not available on `PATH`, use direct Luban generation from `/Users/blank/Documents/GithubWorks/GameFrameX/Config` as the local evidence source and record the missing runner as an external acceptance blocker.

Once the actual command is confirmed, use the following standard flow for every implemented language batch.

### gfx-kernel-* Preflight

1. Build Luban:

   ```bash
   dotnet build /Users/blank/Documents/GithubWorks/luban/src/Luban/Luban.csproj --no-restore
   ```

2. Confirm split templates are copied:

   ```bash
   find /Users/blank/Documents/GithubWorks/luban/src/Luban/bin/Debug/net8.0/Templates -name '*.sbn'
   ```

3. Confirm target registration:

   ```bash
   dotnet /Users/blank/Documents/GithubWorks/luban/src/Luban/bin/Debug/net8.0/Luban.dll --help
   ```

### gfx-kernel-* Generation Matrix

For each language batch, run both old and split targets.

Example matrix:

| Language | Old Target | Split Target | Data Target |
|---|---|---|---|
| C# Unity JSON | `cs-simple-json` | `cs-simple-json-split` | `json` |
| C# Unity BIN | `cs-bin` | `cs-bin-split` | `bin` |
| C# Server JSON | `cs-dotnet-json` | `cs-dotnet-json-split` | `json` |
| C# Server BIN | `cs-dotnet-bin` | `cs-dotnet-bin-split` | `bin` |
| Go JSON | `go-json` | `go-json-split` | `json` |
| Go BIN | `go-bin` | `go-bin-split` | `bin` |
| Rust JSON | `rust-json` | `rust-json-split` | `json` |
| Rust BIN | `rust-bin` | `rust-bin-split` | `bin` |
| C++ raw ptr BIN | `cpp-rawptr-bin` | `cpp-rawptr-bin-split` | `bin` |
| C++ shared ptr BIN | `cpp-sharedptr-bin` | `cpp-sharedptr-bin-split` | `bin` |
| Java JSON | `java-json` | `java-json-split` | `json` |
| Java BIN | `java-bin` | `java-bin-split` | `bin` |

The concrete `gfx-kernel-*` acceptance command should receive or derive:

- Luban executable path
- GameFrameX config directory
- target name
- data target
- code target
- temporary output code directory
- temporary output data directory
- expected generated-file assertions

### Required gfx-kernel-* Assertions

For each split target, the `gfx-kernel-*` workflow must assert:

1. Command exits with code `0`.
2. Output contains per-type definition files.
3. Output contains implementation files.
4. Old single-file output is not produced for split mode, unless the language-specific plan explicitly allows it.
5. Definition file contains structure and comments.
6. Implementation file contains runtime loading/deserialization behavior.
7. No duplicate type/field/member definitions between def and impl files.
8. Namespace/package/module matches the original target.
9. Old target still generates successfully after split target is added.
10. Generated code compiles or passes the language-native check.

### Required Language-Native Checks

| Language | Check |
|---|---|
| C# | Unity compile where available; server `.NET` compile where available |
| Go | `go test ./...` or `go test` fixture |
| Rust | `cargo check` |
| C++ | project compiler or fixture CMake build |
| Java | `javac` or project Gradle/Maven build |
| TypeScript | `tsc --noEmit` |
| Python | `python -m py_compile` |
| Lua | project Lua syntax/runtime check |
| PHP | `php -l` |
| GDScript | Godot headless import/check if available |

## Manual Review Checklist

Before each commit:

- Search for accidental AI-only target names:

  ```bash
  rg -n "ai-def|schema\\.cs|schema\\.(go|java|rs|ts|py|lua|php)" src docs
  ```

- Search for duplicate generated runtime members in split impl files.
- Confirm every new template directory is covered by project copy rules.
- Confirm old code targets still use their original template directories.
- Confirm new split targets use only `-split` suffix.
- Confirm no GameFrameX config scripts were modified unless explicitly requested.

## Commit Strategy

Use one commit per language batch after acceptance:

1. `feat(go): add split code targets`
2. `feat(rust): add split code targets`
3. `feat(cpp): add split code targets`
4. `feat(java): add split code targets`

If Java requires an API decision, commit the design document first:

```text
docs(java): plan split code target design
```

## Stop Conditions

Stop and ask for review if any of the following occurs:

- A language requires a public API change.
- A split target cannot preserve package/module namespace compatibility.
- Runtime generated code compiles only when the definition file is excluded.
- The only viable design regresses old non-split target behavior.
- `gfx-kernel-*` acceptance runner is unavailable or its expected command contract is unclear.

## Final Acceptance Criteria

The overall task is complete only when:

- All approved split targets are implemented.
- All old targets still pass generation.
- All new split targets pass generation.
- `gfx-kernel-*` acceptance passes for every implemented language.
- Generated code passes language-native compile/check steps.
- No `*-ai-def` targets or schema-only AI outputs are introduced.
- Documentation lists supported split targets and language-specific limitations.
