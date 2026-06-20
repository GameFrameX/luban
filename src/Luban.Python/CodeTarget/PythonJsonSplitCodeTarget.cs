using Luban.CodeTarget;

namespace Luban.Python.CodeTarget;

[CodeTarget("python-json-split")]
public class PythonJsonSplitCodeTarget : PythonSplitCodeTargetBase
{
    protected override string RuntimeTemplateDir => "python-json";

    protected override string DefTemplateDir => "python-json-split";
}
