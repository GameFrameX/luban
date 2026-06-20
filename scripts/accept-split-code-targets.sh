#!/usr/bin/env bash
set -euo pipefail

LUBAN_ROOT="${LUBAN_ROOT:-/Users/blank/Documents/GithubWorks/luban}"
GFX_CONFIG_DIR="${GFX_CONFIG_DIR:-/Users/blank/Documents/GithubWorks/GameFrameX/Config}"
DOTNET_DLL="${DOTNET_DLL:-${LUBAN_ROOT}/src/Luban/bin/Debug/net8.0/Luban.dll}"
BASE="${BASE:-/tmp/luban-split-code-targets-acceptance}"
GO_MODULE="${GO_MODULE:-github.com/gameframex/config}"

fail() {
  echo "FAIL: $1" >&2
  exit 1
}

check_file() {
  [[ -f "${BASE}/$1" ]] || fail "missing $1"
}

check_absent() {
  [[ ! -e "${BASE}/$1" ]] || fail "unexpected $1"
}

run_luban() {
  local target=$1
  local data_target=$2
  local code_target=$3
  local go_flag=${4:-}

  echo "RUN target=${target} data=${data_target} code=${code_target}"
  local args=(
    dotnet "${DOTNET_DLL}"
    --target "${target}"
    --dataTarget "${data_target}"
    --codeTarget "${code_target}"
    --xargs "outputDataDir=${BASE}/${target}-${code_target}-data"
    --xargs "outputCodeDir=${BASE}/${target}-${code_target}-code"
    --xargs "tableImporter.name=gameframex"
    -x "l10n.provider=gameframex"
    -x "l10n.textFile.keyFieldName=key"
    -x "l10n.textFile.path=./Excels/Local/"
    --conf ./luban.conf
  )
  if [[ "${go_flag}" == "go" ]]; then
    args+=(--xargs "lubanGoModule=${GO_MODULE}")
  fi
  (cd "${GFX_CONFIG_DIR}" && "${args[@]}") >"${BASE}/${target}-${code_target}.log" 2>&1
}

run_luban_may_fail() {
  local target=$1
  local data_target=$2
  local code_target=$3

  echo "RUN may-fail target=${target} data=${data_target} code=${code_target}"
  set +e
  (cd "${GFX_CONFIG_DIR}" && dotnet "${DOTNET_DLL}" \
    --target "${target}" \
    --dataTarget "${data_target}" \
    --codeTarget "${code_target}" \
    --xargs "outputDataDir=${BASE}/${target}-${code_target}-data" \
    --xargs "outputCodeDir=${BASE}/${target}-${code_target}-code" \
    --xargs "tableImporter.name=gameframex" \
    -x "l10n.provider=gameframex" \
    -x "l10n.textFile.keyFieldName=key" \
    -x "l10n.textFile.path=./Excels/Local/" \
    --conf ./luban.conf) >"${BASE}/${target}-${code_target}.log" 2>&1
  local rc=$?
  set -e
  echo "${rc}" >"${BASE}/${target}-${code_target}.exit"
}

assert_outputs() {
  if rg -n "ai-def|cs-ai-def" "${BASE}" >"${BASE}/ai-def-hits.txt"; then
    cat "${BASE}/ai-def-hits.txt"
    fail "ai-def remnants found"
  fi
  rg -n "GiftFilterType::None|preserved keyword" \
    "${BASE}/all-python-json.log" \
    "${BASE}/all-python-json-split.log" >/dev/null || fail "Python expected keyword blocker not found"

  check_file "client-cs-simple-json-code/Tables/ItemConfig.cs"
  check_file "client-cs-bin-code/Tables/ItemConfig.cs"
  check_file "server-cs-dotnet-json-code/Tables/ItemConfig.cs"
  check_file "server-cs-dotnet-bin-code/Tables/ItemConfig.cs"
  for dir in client-cs-simple-json-split-code client-cs-bin-split-code server-cs-dotnet-json-split-code server-cs-dotnet-bin-split-code; do
    check_file "${dir}/Tables/ItemConfig.Def.cs"
    check_file "${dir}/Tables/ItemConfig.Impl.cs"
    check_absent "${dir}/Tables/ItemConfig.cs"
  done
  rg -n "namespace Hotfix\\.Config" "${BASE}/client-cs-simple-json-split-code/Tables/ItemConfig.Def.cs" >/dev/null || fail "Unity C# split namespace mismatch"
  rg -n "namespace GameFrameX\\.Config" "${BASE}/server-cs-dotnet-bin-split-code/Tables/ItemConfig.Def.cs" >/dev/null || fail "Server C# split namespace mismatch"
  if rg -n "public .*\\{|const .*=" "${BASE}/client-cs-bin-split-code/Tables/ItemConfig.Impl.cs" >"${BASE}/cs-impl-fields.txt"; then
    cat "${BASE}/cs-impl-fields.txt"
    fail "C# impl appears to contain field/const declarations"
  fi

  check_file "all-go-json-code/Tables.ItemConfig.go"
  check_file "all-go-bin-code/Tables.ItemConfig.go"
  check_file "all-go-json-split-code/Tables.ItemConfig.def.go"
  check_file "all-go-json-split-code/Tables.ItemConfig.impl.go"
  check_file "all-go-bin-split-code/Tables.ItemConfig.def.go"
  check_file "all-go-bin-split-code/Tables.ItemConfig.impl.go"
  rg -n "type TablesItemConfig struct" "${BASE}/all-go-json-split-code/Tables.ItemConfig.def.go" >/dev/null || fail "Go def missing struct"
  if rg -n "type TablesItemConfig struct" "${BASE}/all-go-json-split-code/Tables.ItemConfig.impl.go" >"${BASE}/go-dup.txt"; then
    cat "${BASE}/go-dup.txt"
    fail "Go impl duplicates struct"
  fi

  check_file "all-rust-json-split-code/cfg/src/Tables.ItemConfig.Def.rs"
  check_file "all-rust-json-split-code/cfg/src/Tables.TbItemConfig.Def.rs"
  check_file "all-rust-json-split-code/cfg/src/Tables.Def.rs"
  check_file "all-rust-json-split-code/cfg/src/lib.rs"
  check_file "all-rust-json-split-code/cfg/src/tables.rs"
  check_file "all-rust-bin-split-code/cfg/src/Tables.ItemConfig.Def.rs"
  check_file "all-rust-bin-split-code/cfg/src/tables.rs"
  rg -n "crate::tables::ItemConfigDef" "${BASE}/all-rust-json-split-code/cfg/src/Tables.TbItemConfig.Def.rs" >/dev/null || fail "Rust table def does not reference ItemConfigDef"
  if rg -n "deserialize|ByteBuf|load_from|fn new" "${BASE}/all-rust-json-split-code/cfg/src/Tables.ItemConfig.Def.rs" >"${BASE}/rust-runtime.txt"; then
    cat "${BASE}/rust-runtime.txt"
    fail "Rust def contains runtime markers"
  fi

  check_file "all-cpp-rawptr-bin-split-code/schema.h"
  check_file "all-cpp-rawptr-bin-split-code/Tables.ItemConfig.Def.h"
  check_file "all-cpp-rawptr-bin-split-code/Tables.ItemConfig.Impl.cpp"
  check_file "all-cpp-rawptr-bin-split-code/Tables.TbItemConfig.Impl.cpp"
  check_file "all-cpp-rawptr-bin-split-code/Tables.Impl.cpp"
  check_file "all-cpp-sharedptr-bin-split-code/Tables.ItemConfig.Def.h"
  check_file "all-cpp-sharedptr-bin-split-code/Tables.ItemConfig.Impl.cpp"
  check_file "all-cpp-sharedptr-bin-split-code/Tables.TbItemConfig.Impl.cpp"
  check_file "all-cpp-sharedptr-bin-split-code/Tables.Impl.cpp"
  check_absent "all-cpp-rawptr-bin-split-code/schema_0.cpp"
  check_absent "all-cpp-sharedptr-bin-split-code/schema_0.cpp"
  if rg -n "bool load\\(::luban::ByteBuf& _buf\\)\\s*\\{|bool load\\(::luban::Loader<::luban::ByteBuf> loader\\)\\s*\\{" \
    "${BASE}/all-cpp-rawptr-bin-split-code/schema.h" \
    "${BASE}/all-cpp-sharedptr-bin-split-code/schema.h" >"${BASE}/cpp-inline-load.txt"; then
    cat "${BASE}/cpp-inline-load.txt"
    fail "C++ split schema.h contains inline load bodies"
  fi

  check_file "all-java-json-split-code/Tables/ItemConfig.java"
  check_file "all-java-json-split-code/Tables/ItemConfigDef.java"
  check_file "all-java-bin-split-code/Tables/ItemConfig.java"
  check_file "all-java-bin-split-code/Tables/ItemConfigDef.java"
  if rg -n "deserialize|ByteBuf|JsonObject|getTypeId|extends AbstractBean" "${BASE}/all-java-json-split-code/Tables/ItemConfigDef.java" >"${BASE}/java-runtime.txt"; then
    cat "${BASE}/java-runtime.txt"
    fail "Java def contains runtime markers"
  fi

  check_file "all-typescript-json-split-code/schema.ts"
  check_file "all-typescript-json-split-code/Tables.ItemConfig.def.ts"
  check_file "all-typescript-json-split-code/Tables.TbItemConfig.def.ts"
  check_file "all-typescript-json-split-code/Tables.def.ts"
  rg -n "Tables\\.ItemConfigDef" "${BASE}/all-typescript-json-split-code/Tables.TbItemConfig.def.ts" >/dev/null || fail "TS table def does not reference ItemConfigDef"
  if rg -n "constructor|deserialize|resolve\\(|ByteBuf|loader|new |class |pb\\.cfg" "${BASE}"/all-typescript-*-split-code/*.def.ts >"${BASE}/ts-runtime.txt"; then
    cat "${BASE}/ts-runtime.txt"
    fail "TypeScript def contains runtime markers"
  fi

  check_file "all-php-json-split-code/Tables.ItemConfig.def.php"
  check_file "all-gdscript-json-split-code/Tables.ItemConfig.def.gd"
  check_file "all-lua-bin-split-code/Tables.ItemConfig.def.lua"
  check_file "all-lua-lua-split-code/Tables.ItemConfig.def.lua"
  if rg -n "__construct|deserialize|_init|func |loader|ByteBuf|readInt|readString|SimpleClass|constructFrom|fromJson" \
    "${BASE}"/all-php-json-split-code/*.def.php \
    "${BASE}"/all-gdscript-json-split-code/*.def.gd \
    "${BASE}"/all-lua-*-split-code/*.def.lua >"${BASE}/script-runtime.txt"; then
    cat "${BASE}/script-runtime.txt"
    fail "script def contains runtime markers"
  fi
}

rm -rf "${BASE}"
mkdir -p "${BASE}"

dotnet build "${LUBAN_ROOT}/src/Luban/Luban.csproj" --no-restore

run_luban client json cs-simple-json
run_luban client bin cs-bin
run_luban server json cs-dotnet-json
run_luban server bin cs-dotnet-bin
run_luban client json cs-simple-json-split
run_luban client bin cs-bin-split
run_luban server json cs-dotnet-json-split
run_luban server bin cs-dotnet-bin-split

run_luban all json go-json go
run_luban all bin go-bin go
run_luban all json go-json-split go
run_luban all bin go-bin-split go

run_luban all json rust-json
run_luban all bin rust-bin
run_luban all json rust-json-split
run_luban all bin rust-bin-split

run_luban all bin cpp-rawptr-bin
run_luban all bin cpp-sharedptr-bin
run_luban all bin cpp-rawptr-bin-split
run_luban all bin cpp-sharedptr-bin-split

run_luban all json java-json
run_luban all bin java-bin
run_luban all json java-json-split
run_luban all bin java-bin-split

run_luban all json typescript-json
run_luban all bin typescript-bin
run_luban all protobuf3-bin typescript-protobuf
run_luban all json typescript-json-split
run_luban all bin typescript-bin-split
run_luban all protobuf3-bin typescript-protobuf-split

run_luban_may_fail all json python-json
run_luban_may_fail all json python-json-split

run_luban all json php-json
run_luban all json php-json-split
run_luban all json gdscript-json
run_luban all json gdscript-json-split
run_luban all bin lua-bin
run_luban all bin lua-bin-split
run_luban all lua lua-lua
run_luban all lua lua-lua-split

assert_outputs

echo "BASE=${BASE}"
printf "files="
find "${BASE}" -type f | wc -l | tr -d ' '
printf "\n"
echo "split code target acceptance assertions passed"
