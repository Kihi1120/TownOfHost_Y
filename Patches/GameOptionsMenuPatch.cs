using System;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace TownOfHostY;

public static class ModGameOptionsMenu
{
    public static int TabIndex = 0;
    public static Dictionary<OptionBehaviour, int> OptionList = new();
    public static Dictionary<int, OptionBehaviour> BehaviourList = new();
    public static Dictionary<int, CategoryHeaderMasked> CategoryHeaderList = new();
}
[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;

        if (__instance.Children == null || __instance.Children.Count == 0)
        {
            __instance.MapPicker.gameObject.SetActive(false);
            //__instance.MapPicker.Initialize(20);
            //BaseGameSetting mapNameSetting = GameManager.Instance.GameSettingsList.MapNameSetting;
            //__instance.MapPicker.SetUpFromData(mapNameSetting, 20);
            __instance.Children = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
            //__instance.Children.Add(__instance.MapPicker);
            __instance.CreateSettings();
            __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
            for (int i = 0; i < __instance.Children.Count; i++)
            {
                OptionBehaviour optionBehaviour = __instance.Children[i];
                optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
                //if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                //{
                //    optionBehaviour.SetAsPlayer();
                //}
            }
            __instance.InitializeControllerNavigation();
        }

        return false;
    }
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings)), HarmonyPrefix]
    private static bool CreateSettingsPrefix(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;
        var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);

        //float num = 0.713f;
        float num = 2.0f;
        for (int index = 0; index < OptionItem.AllOptions.Count; index++)
        {
            var option = OptionItem.AllOptions[index];
            if (option.Tab != modTab) continue;

            var enabled = !option.IsHiddenOn(Options.CurrentGameMode)
                         && (option.Parent == null || (!option.Parent.IsHiddenOn(Options.CurrentGameMode) && option.Parent.GetBool()));

            if (option.IsHeader || option is TextOptionItem)
            {
                CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                categoryHeaderMasked.SetHeader(StringNames.RolesCategory, 20);
                categoryHeaderMasked.Title.text = option.GetName();
                categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                categoryHeaderMasked.gameObject.SetActive(enabled);
                ModGameOptionsMenu.CategoryHeaderList.TryAdd(index, categoryHeaderMasked);

                if (enabled) num -= 0.63f;
            }
            if (option is TextOptionItem) continue;

            var baseGameSetting = GetSetting(option);
            if (baseGameSetting == null) continue;

            OptionBehaviour optionBehaviour;
            switch (baseGameSetting.Type)
            {
                case OptionTypes.Checkbox:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(__instance.checkboxOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }
                case OptionTypes.String:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(__instance.stringOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }
                case OptionTypes.Float:
                case OptionTypes.Int:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<NumberOption>(__instance.numberOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }
                //case OptionTypes.Player:
                //    {
                //        OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<PlayerOption>(__instance.playerOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                //        break;
                //    }
                default:
                    continue;
            }
            optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
            optionBehaviour.SetClickMask(__instance.ButtonClickMask);
            optionBehaviour.SetUpFromData(baseGameSetting, 20);
            ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
            ModGameOptionsMenu.BehaviourList.TryAdd(index, optionBehaviour);
            optionBehaviour.gameObject.SetActive(enabled);
            __instance.Children.Add(optionBehaviour);

            if (enabled) num -= 0.45f;
        }

        __instance.ControllerSelectable.Clear();
        foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
            __instance.ControllerSelectable.Add(x);
        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);

        return false;
    }
    [HarmonyPatch(nameof(GameOptionsMenu.ValueChanged)), HarmonyPrefix]
    private static bool ValueChangedPrefix(GameOptionsMenu __instance, OptionBehaviour option)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;

        if (ModGameOptionsMenu.OptionList.TryGetValue(option, out var index))
        {
            var item = OptionItem.AllOptions[index];
            if (item != null && item.Children.Count > 0) ReCreateSettings(__instance);
        }
        return false;
    }
    private static void ReCreateSettings(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return;
        var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);

        //float num = 0.713f;
        float num = 2.0f;
        for (int index = 0; index < OptionItem.AllOptions.Count; index++)
        {
            var option = OptionItem.AllOptions[index];
            if (option.Tab != modTab) continue;

            var enabled = !option.IsHiddenOn(Options.CurrentGameMode)
                         && (option.Parent == null || (!option.Parent.IsHiddenOn(Options.CurrentGameMode) && option.Parent.GetBool()));

            if (ModGameOptionsMenu.CategoryHeaderList.TryGetValue(index, out var categoryHeaderMasked))
            {
                categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                categoryHeaderMasked.gameObject.SetActive(enabled);
                if (enabled) num -= 0.63f;
            }
            if (ModGameOptionsMenu.BehaviourList.TryGetValue(index, out var optionBehaviour))
            {
                optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                optionBehaviour.gameObject.SetActive(enabled);
                if (enabled) num -= 0.45f;
            }
        }

        __instance.ControllerSelectable.Clear();
        foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
            __instance.ControllerSelectable.Add(x);
        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }
    private static BaseGameSetting GetSetting(OptionItem item)
    {
        BaseGameSetting baseGameSetting = null;

        if (item is BooleanOptionItem)
        {
            baseGameSetting = new CheckboxGameSetting
            {
                Type = OptionTypes.Checkbox,
            };
        }
        else if (item is IntegerOptionItem)
        {
            IntegerOptionItem intItem = item as IntegerOptionItem;
            baseGameSetting = new IntGameSetting
            {
                Type = OptionTypes.Int,
                Value = intItem.GetInt(),
                Increment = intItem.Rule.Step,
                ValidRange = new IntRange(intItem.Rule.MinValue, intItem.Rule.MaxValue),
                ZeroIsInfinity = false,
                SuffixType = NumberSuffixes.Multiplier,
                FormatString = string.Empty,
            };
        }
        else if (item is FloatOptionItem)
        {
            FloatOptionItem floatItem = item as FloatOptionItem;
            baseGameSetting = new FloatGameSetting
            {
                Type = OptionTypes.Float,
                Value = floatItem.GetFloat(),
                Increment = floatItem.Rule.Step,
                ValidRange = new FloatRange(floatItem.Rule.MinValue, floatItem.Rule.MaxValue),
                ZeroIsInfinity = false,
                SuffixType = NumberSuffixes.Multiplier,
                FormatString = string.Empty,
            };
        }
        else if (item is StringOptionItem)
        {
            StringOptionItem stringItem = item as StringOptionItem;
            baseGameSetting = new StringGameSetting
            {
                Type = OptionTypes.String,
                Values = new StringNames[stringItem.Selections.Length], //ダミー
                Index = stringItem.GetInt(),
            };
        }

        if (baseGameSetting != null)
        {
            baseGameSetting.Title = StringNames.Accept; //ダミー
        }

        return baseGameSetting;
    }
}


[HarmonyPatch(typeof(ToggleOption))]
public static class ToggleOptionPatch
{
    [HarmonyPatch(nameof(ToggleOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(ToggleOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "ToggleOption.Initialize.TryGetValue");
            __instance.TitleText.text = item.GetName();
            __instance.CheckMark.enabled = item.GetBool();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(ToggleOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(ToggleOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "ToggleOption.UpdateValue.TryGetValue");
            item.SetValue(__instance.GetBool() ? 1 : 0);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(NumberOption))]
public static class NumberOptionPatch
{
    [HarmonyPatch(nameof(NumberOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.Initialize.TryGetValue");
            __instance.TitleText.text = item.GetName();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.UpdateValue.TryGetValue");

            if (item is IntegerOptionItem integerOptionItem)
            {
                integerOptionItem.SetValue(__instance.GetInt());
            }
            else if (item is FloatOptionItem floatOptionItem)
            {
                floatOptionItem.SetValue(floatOptionItem.Rule.GetNearestIndex(__instance.GetFloat()));
            }

            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.FixedUpdate)), HarmonyPrefix]
    private static bool FixedUpdatePrefix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.FixedUpdate.TryGetValue");

            if (__instance.oldValue != __instance.Value)
            {
                __instance.oldValue = __instance.Value;
                __instance.ValueText.text = GetValueString(__instance, __instance.Value, item);
            }
            return false;
        }
        return true;
    }
    public static string GetValueString(NumberOption __instance, float value, OptionItem item)
    {
        if (__instance.ZeroIsInfinity && Mathf.Abs(value) < 0.0001f) return "<b>∞</b>";
        if (item == null) return value.ToString(__instance.FormatString);
        return string.Format(Translator.GetString("Format." + item.ValueFormat), value);
    }
    [HarmonyPatch(nameof(NumberOption.Increase)), HarmonyPrefix]
    public static bool IncreasePrefix(NumberOption __instance)
    {
        if (__instance.Value == __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.min;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.Decrease)), HarmonyPrefix]
    public static bool DecreasePrefix(NumberOption __instance)
    {
        if (__instance.Value == __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.max;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(StringOption))]
public static class StringOptionPatch
{
    [HarmonyPatch(nameof(StringOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "StringOption.Initialize.TryAdd");
            __instance.TitleText.text = item.GetName();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            Logger.Info($"{item.Name}, {index}", "StringOption.UpdateValue.TryAdd");

            item.SetValue(__instance.GetInt());
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.FixedUpdate)), HarmonyPrefix]
    private static bool FixedUpdatePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];

            if (item is StringOptionItem stringOptionItem)
            {
                if (__instance.oldValue != __instance.Value)
                {
                    __instance.oldValue = __instance.Value;
                    __instance.ValueText.text = Translator.GetString(stringOptionItem.Selections[stringOptionItem.Rule.GetValueByIndex(__instance.Value)]);
                }
            }
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.Increase)), HarmonyPrefix]
    public static bool IncreasePrefix(StringOption __instance)
    {
        if (__instance.Value == __instance.Values.Length - 1)
        {
            __instance.Value = 0;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.Decrease)), HarmonyPrefix]
    public static bool DecreasePrefix(StringOption __instance)
    {
        if (__instance.Value == 0)
        {
            __instance.Value = __instance.Values.Length - 1;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
//public class StringOptionIncreasePatch
//{
//    public static bool Prefix(StringOption __instance)
//    {
//        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
//        if (option == null) return true;

//        option.SetValue(option.CurrentValue + (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
//        return false;
//    }
//}

//[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
//public class StringOptionDecreasePatch
//{
//    public static bool Prefix(StringOption __instance)
//    {
//        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
//        if (option == null) return true;

//        option.SetValue(option.CurrentValue - (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
//        return false;
//    }
//}
