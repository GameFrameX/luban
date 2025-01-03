using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Luban.GUI.Models;

public sealed class Setting
{
    public string ConfigFile { get; set; }
    public string ClientDataTarget { get; set; }
    public string ClientCodeTarget { get; set; }
    public string ClientLocalizationPath { get; set; }
    public string ServerDataTarget { get; set; }
    public string ServerCodeTarget { get; set; }
}

public class SettingData
{
    public Setting Options { get; private set; }

    public SettingData()
    {
        Options = new Setting();
        Options.ConfigFile = "./../Luban.conf";
        Options.ClientDataTarget = "../../Unity/Assets//Bundles/Config";
        Options.ClientCodeTarget = "../../Unity/Assets//Hotfix/Config/Generate";
        Options.ClientLocalizationPath = "./../Excels/Localization/";

        Options.ServerDataTarget = "../../Server-develop/GameFrameX.Config/Json";
        Options.ServerCodeTarget = "../../Server-develop/GameFrameX.Config/Config";
    }

    public static SettingData Instance { get; } = new SettingData();

    public const string SettingPath = "Setting.json";

    public static void LoadSetting()
    {
        if (File.Exists(SettingPath))
        {
            var json = File.ReadAllText(SettingPath);
            Instance.Options = JsonConvert.DeserializeObject<Setting>(json);
        }
    }

    public static void SaveSetting()
    {
        File.WriteAllText(SettingPath, JsonConvert.SerializeObject(Instance.Options, Formatting.Indented));
    }

    public static string[] GetClientArgs(bool isBinary)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("--target client ");
        stringBuilder.Append("--xargs outputDataDir=").Append(Instance.Options.ClientDataTarget).Append(' ');
        stringBuilder.Append("--xargs outputCodeDir=").Append(Instance.Options.ClientCodeTarget).Append(' ');
        if (isBinary)
        {
            stringBuilder.Append("--codeTarget cs-bin ");
            stringBuilder.Append("--dataTarget bin ");
        }
        else
        {
            stringBuilder.Append("--codeTarget cs-simple-json ");
            stringBuilder.Append("--dataTarget json ");
        }

        stringBuilder.Append("--xargs tableImporter.name=gameframex ");
        stringBuilder.Append("--xargs l10n.provider=gameframex --xargs l10n.textFile.keyFieldName=key  --xargs l10n.textFile.path=").Append(Instance.Options.ClientLocalizationPath).Append(' ');
        stringBuilder.Append("--conf ").Append(Instance.Options.ConfigFile).Append(' ');
        string result = stringBuilder.ToString();
        Console.WriteLine(result);
        var args = result.Split(' ');
        args = args.Where(m => string.IsNullOrWhiteSpace(m) == false).ToArray();
        return args;
    }

    public static string[] GetServerArgs(bool isBinary)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("--target server ");
        stringBuilder.Append("--xargs outputDataDir=").Append(Instance.Options.ServerDataTarget).Append(' ');
        stringBuilder.Append("--xargs outputCodeDir=").Append(Instance.Options.ServerCodeTarget).Append(' ');
        if (isBinary)
        {
            stringBuilder.Append("--codeTarget cs-bin ");
            stringBuilder.Append("--dataTarget bin ");
        }
        else
        {
            stringBuilder.Append("--codeTarget cs-dotnet-json ");
            stringBuilder.Append("--dataTarget json ");
        }

        stringBuilder.Append("--xargs tableImporter.name=gameframex ");
        stringBuilder.Append("--conf ").Append(Instance.Options.ConfigFile).Append(' ');
        string result = stringBuilder.ToString();
        Console.WriteLine(result);
        var args = result.Split(' ');
        args = args.Where(m => string.IsNullOrWhiteSpace(m) == false).ToArray();
        return args;
    }
}
