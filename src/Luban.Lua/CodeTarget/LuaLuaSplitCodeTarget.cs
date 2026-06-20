using Luban.CodeTarget;

namespace Luban.Lua.CodeTarget;

[CodeTarget("lua-lua-split")]
public class LuaLuaSplitCodeTarget : LuaSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "lua-lua";

    protected override string DefTemplateDir => "lua-lua-split";
}
