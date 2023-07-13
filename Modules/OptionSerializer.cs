using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using AmongUs.GameOptions;

namespace TownOfHost.Modules;

public static class OptionSerializer
{
    private static LogHandler logger = Logger.Handler(nameof(OptionSerializer));
    private const string Header = "%TOHOptions%", Footer = "%End%";
    private static readonly DirectoryInfo exportDir = new("./TOH_DATA/OptionOutputs");
    public static void SaveToClipboard()
    {
        GUIUtility.systemCopyBuffer = ToString();
        Logger.SendInGame(Utils.ColorString(Color.green, Translator.GetString("Message.CopiedOptions")));
    }
    public static void SaveToFile()
    {
        if (!exportDir.Exists)
        {
            exportDir.Create();
        }
        var output = $"{exportDir.FullName}/Preset{OptionItem.CurrentPreset}_{DateTime.Now.Ticks}.txt";
        File.WriteAllText(output, ToString());
        Process.Start(exportDir.FullName);
        Logger.SendInGame(Utils.ColorString(Color.green, Translator.GetString("Message.ExportedOptions")));
    }
    public static void LoadFromClipboard()
    {
        FromString(GUIUtility.systemCopyBuffer);
    }
    /// <summary>
    /// 現在のMod設定とバニラ設定を<see cref="FromString"/>で読み込める文字列に変換します<br/>
    /// enumは元の整数型に変換します<br/>
    /// 10以上になる可能性のある整数型は，文字数削減のため62進数に変換します<br/>
    /// Mod設定は，プリセットでなく，Valueが0でないもの(データ量削減のため)を書き込みます<br/>
    /// <see cref="Header"/>から始まって<see cref="Footer"/>で終わります<br/>
    /// '&amp;'がMod設定とバニラ設定を区切ります<br/>
    /// Mod設定は，'!'が各オプションを区切り，','が前のオプションIDとの差とオプションの値を区切ります<br/>
    /// 前のオプションIDとの差が1の場合，空文字列で表現します<br/>
    /// [オプション1のID - 0],[オプションの1の値]![オプション2のID - オプション1のID],[オプション2の値]!...<br/>
    /// バニラ設定は，'!'が各オプションを区切り，役職オプション以外のオプションは以下のフォーマットです<br/>
    /// [<see cref="OptionType"/>],[オプション名=<see cref="BoolOptionNames"/>など],[オプション値]<br/>
    /// バニラの役職オプションは以下のフォーマットです<br/>
    /// [<see cref="OptionType.RoleRate"/>],[<see cref="RoleTypes"/>],[最大数],[確率]
    /// </summary>
    /// <returns>変換された文字列</returns>
    public new static string ToString()
    {
        var builder = new StringBuilder(Header);
        var options = OptionItem.AllOptions.Where(option => option is not PresetOptionItem).OrderBy(option => option.Id);
        var lastId = 0;
        foreach (var option in options)
        {
            var value = option.GetValue();
            if (value == 0)
            {
                continue;
            }
            var idDelta = option.Id - lastId;
            builder.Append(idDelta == 1 ? "" : Base62.ToBase62(idDelta)).Append(",").Append(Base62.ToBase62(option.GetValue())).Append("!");
            lastId = option.Id;
        }

        builder.Append("&");
        var vanillaOptions = new OptionBackupData(GameOptionsManager.Instance.CurrentGameOptions);
        foreach (var option in vanillaOptions.AllValues)
        {
            builder.Write(option);
        }
        builder.Append(Footer);
        return builder.ToString();
    }
    /// <summary>
    /// <see cref="ToString"/>で変換された形式の文字列を読み込んで現在のプリセットを上書きします
    /// </summary>
    /// <param name="source">オプション情報の文字列</param>
    public static void FromString(string source)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            Logger.SendInGame(Translator.GetString("Message.OnlyHostCanLoadOptions"));
            return;
        }

        if (string.IsNullOrEmpty(source))
        {
            logger.Info("文字列が空");
            goto Failed;
        }

        var headerAt = source.IndexOf(Header);
        if (headerAt < 0)
        {
            logger.Info("ヘッダがありません");
            goto Failed;
        }
        var footerAt = source.IndexOf(Footer, headerAt);
        if (footerAt < 0)
        {
            logger.Info("フッタがありません");
            goto Failed;
        }
        // ヘッダ以前とフッタ以降を削除
        source = source[(headerAt + Header.Length)..footerAt];

        try
        {
            var entries = source.Split('&');

            var modOptions = entries[0].Split('!', StringSplitOptions.RemoveEmptyEntries);
            var parsedModOptions = new Dictionary<int, int>(modOptions.Length);
            var lastId = 0;
            foreach (var modOption in modOptions)
            {
                var split = modOption.Split(',');
                var idDelta = split[0] == "" ? 1 : Base62.ToInt(split[0]);
                lastId += idDelta;
                parsedModOptions[lastId] = Base62.ToInt(split[1]);
            }
            foreach (var option in OptionItem.AllOptions)
            {
                if (option is PresetOptionItem)
                {
                    continue;
                }
                option.SetValue(parsedModOptions.TryGetValue(option.Id, out var value) ? value : 0, false);
            }

            var vanillaOptions = entries[1].Split('!', StringSplitOptions.RemoveEmptyEntries);
            foreach (var vanillaOption in vanillaOptions)
            {
                var split = vanillaOption.Split(',');
                split.Read();
            }

            GameManager.Instance.LogicOptions.SetDirty();
            OptionItem.SyncAllOptions();

            Logger.SendInGame(Utils.ColorString(Color.green, Translator.GetString("Message.LoadedOptions")));
        }
        catch (Exception ex)
        {
            logger.Exception(ex);
            goto Failed;
        }
        return;

    Failed:
        Logger.SendInGame(Translator.GetString("Message.FailedToLoadOptions"));
    }
    private static StringBuilder Write(this StringBuilder builder, OptionBackupValue option)
    {
        switch (option)
        {
            case ByteOptionBackupValue byteOption: builder.WriteByte(byteOption); break;
            case BoolOptionBackupValue boolOption: builder.WriteBool(boolOption); break;
            // floatは10進
            case FloatOptionBackupValue floatOption: builder.WriteFloat(floatOption); break;
            case IntOptionBackupValue intOption: builder.WriteInt(intOption); break;
            case UIntOptionBackupValue uIntOption: builder.WriteUInt(uIntOption); break;
            case RoleRateBackupValue roleRate: builder.WriteRoleRate(roleRate); break;
            default: logger.Warn("不明なオプションの書き込み"); break;
        }
        builder.Append("!");
        return builder;
    }
    private static StringBuilder WriteByte(this StringBuilder builder, ByteOptionBackupValue byteOption) =>
        builder.Append((int)OptionType.Byte).Append(",").Append(Base62.ToBase62((int)byteOption.OptionName)).Append(",").Append(Base62.ToBase62(byteOption.Value));
    private static StringBuilder WriteBool(this StringBuilder builder, BoolOptionBackupValue boolOption) =>
        builder.Append((int)OptionType.Bool).Append(",").Append(Base62.ToBase62((int)boolOption.OptionName)).Append(",").Append(Convert.ToInt32(boolOption.Value));
    private static StringBuilder WriteFloat(this StringBuilder builder, FloatOptionBackupValue floatOption) =>
        builder.Append((int)OptionType.Float).Append(",").Append(Base62.ToBase62((int)floatOption.OptionName)).Append(",").Append(floatOption.Value);
    private static StringBuilder WriteInt(this StringBuilder builder, IntOptionBackupValue intOption) =>
        builder.Append((int)OptionType.Int).Append(",").Append(Base62.ToBase62((int)intOption.OptionName)).Append(",").Append(Base62.ToBase62(intOption.Value));
    private static StringBuilder WriteUInt(this StringBuilder builder, UIntOptionBackupValue uIntOption) =>
        builder.Append((int)OptionType.UInt).Append(",").Append(Base62.ToBase62((int)uIntOption.OptionName)).Append(",").Append(Base62.ToBase62(uIntOption.Value));
    private static StringBuilder WriteRoleRate(this StringBuilder builder, RoleRateBackupValue roleRate) =>
        builder.Append((int)OptionType.RoleRate).Append(",").Append((ushort)roleRate.roleType).Append(",").Append(Base62.ToBase62(roleRate.maxCount)).Append(",").Append(Base62.ToBase62(roleRate.chance));
    private static void Read(this string[] args)
    {
        var optionType = (OptionType)Convert.ToInt32(args[0]);
        switch (optionType)
        {
            case OptionType.Byte: ReadByte(args); break;
            case OptionType.Bool: ReadBool(args); break;
            case OptionType.Float: ReadFloat(args); break;
            case OptionType.Int: ReadInt(args); break;
            case OptionType.UInt: ReadUInt(args); break;
            case OptionType.RoleRate: ReadRoleRate(args); break;
            default: logger.Warn($"不明なオプションタイプの読み込み: {optionType}"); break;
        }
    }
    private static void ReadByte(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.SetByte((ByteOptionNames)Base62.ToInt(args[1]), Base62.ToByte(args[2]));
    private static void ReadBool(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.SetBool((BoolOptionNames)Base62.ToInt(args[1]), Convert.ToInt32(args[2]) > 0);
    private static void ReadFloat(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.SetFloat((FloatOptionNames)Base62.ToInt(args[1]), Convert.ToSingle(args[2]));
    private static void ReadInt(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.SetInt((Int32OptionNames)Base62.ToInt(args[1]), Base62.ToInt(args[2]));
    private static void ReadUInt(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.SetUInt((UInt32OptionNames)Base62.ToInt(args[1]), Base62.ToUInt(args[2]));
    private static void ReadRoleRate(string[] args) =>
        GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate((RoleTypes)Convert.ToUInt16(args[1]), Base62.ToInt(args[2]), Base62.ToInt(args[3]));

    private enum OptionType { Byte, Bool, Float, Int, UInt, RoleRate, }
}
