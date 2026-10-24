# Luban.Mcp

stdio MCP Server for Luban: list/get schema, validate, generate, search docs.

## Build

Preferred (with sibling `luban_examples`):

```bash
# Windows
luban_examples/Tools/build-luban.bat

# macOS / Linux
luban_examples/Tools/build-luban.sh
```

Outputs under `luban_examples/Tools/`:

- `Luban/` — main generator
- `Luban.Agent/` — agent CLI
- `Luban.Mcp/` — this MCP server

Or from the luban repo:

```bash
dotnet build src/Luban/Luban.csproj -c Release -o <out>/Luban
dotnet build src/Luban.Agent/Luban.Agent.csproj -c Release -o <out>/Luban.Agent
dotnet build src/Luban.Mcp/Luban.Mcp.csproj -c Release -o <out>/Luban.Mcp
```

## Env

- `LUBAN_AGENT_DLL` — path to `Luban.Agent.dll`
- `LUBAN_DLL` — path to `Luban.dll`
- `LUBAN_DOC` — path to `luban-doc/docs`

## Tools

MCP 实际暴露的工具名为 snake_case（模型序列化默认行为）：

- `list_tables` / `get_schema` / `describe` / `validate` / `generate` / `search_docs`

`generate` 透传参数给 `Luban.dll` 并默认追加 `--errorFormat json`。注意 `-d` 是 data target 名（如 `json`），输出目录用 `-x outputCodeDir=... -x outputDataDir=...`，例如：

```bash
--conf <luban.conf> -t all -c schema-json -d json -x outputCodeDir=<codeDir> -x outputDataDir=<dataDir>
```

See documentation: `docs/ai/mcp` in luban-doc.
