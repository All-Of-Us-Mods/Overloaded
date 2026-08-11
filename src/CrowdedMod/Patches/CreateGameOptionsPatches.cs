using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CrowdedMod.Patches;

internal static class CreateGameOptionsPatches
{
    private static GameOptionButton _minButton = null!;
    private static GameOptionButton _maxButton = null!;
    private static GameOptionButton _doubleMinusButton = null!;
    private static GameOptionButton _doublePlusButton = null!;
    
    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
    public static class CreateGameOptions_Start
    {
        public static void Postfix(CreateGameOptions __instance)
        {
            // set new valid range
            var newRange = new IntRange(1, CrowdedModPlugin.MaxPlayers);
            var newFloatRange = new FloatRange(1, CrowdedModPlugin.MaxPlayers);
            
            __instance.capacitySetting.ValidRange = newRange;
            __instance.capacityOption.ValidRange  = newFloatRange;
            
            // move stuff to make space for -5 and +5 buttons
            foreach (Il2CppSystem.Object obj in __instance.capacityOption.transform)
            {
                var t = obj.Cast<Transform>();
                if (!t || t.gameObject.name is "LabelBackground" or "Title Text") continue;
                t.localPosition += new Vector3(1, 0, 0);
            }

            // create -5 and +5 buttons
            _doubleMinusButton = Object.Instantiate(__instance.capacityOption.MinusBtn, __instance.capacityOption.MinusBtn.transform.parent);
            var dmText = _doubleMinusButton.GetComponentInChildren<TextMeshPro>();
            dmText.text = "-5";
            dmText.fontStyle = FontStyles.Normal;
            _doubleMinusButton.transform.localPosition -= new Vector3(0.5f, 0, 0);
            _doubleMinusButton.OnClick = new Button.ButtonClickedEvent();
            _doubleMinusButton.OnClick.AddListener((UnityAction)(() =>
            {
                __instance.capacityOption.Increment = 5;
                __instance.capacityOption.Decrease();
                __instance.capacitySetting.Increment = 1;
            }));
            
            _doublePlusButton = Object.Instantiate(__instance.capacityOption.PlusBtn, __instance.capacityOption.PlusBtn.transform.parent);
            var dpText = _doublePlusButton.GetComponentInChildren<TextMeshPro>();
            dpText.text = "+5";
            dpText.fontStyle = FontStyles.Normal;
            _doublePlusButton.transform.localPosition += new Vector3(0.5f, 0, 0);
            _doublePlusButton.OnClick = new Button.ButtonClickedEvent();
            _doublePlusButton.OnClick.AddListener((UnityAction)(() =>
            {
                __instance.capacityOption.Increment = 5;
                __instance.capacityOption.Increase();
                __instance.capacitySetting.Increment = 1;
            }));

            // create min and max buttons
            _minButton = Object.Instantiate(__instance.capacityOption.MinusBtn, __instance.capacityOption.MinusBtn.transform.parent);
            var minText = _minButton.GetComponentInChildren<TextMeshPro>();
            minText.text = "1";
            minText.fontStyle = FontStyles.Normal;
            _minButton.SetInteractable(true);
            _minButton.transform.localPosition -= new Vector3(1, 0, 0);
            _minButton.OnClick = new Button.ButtonClickedEvent();
            _minButton.OnClick.AddListener((UnityAction)(() =>
            {
                __instance.capacityOption.Increment = int.MaxValue;
                __instance.capacityOption.Decrease();
                __instance.capacityOption.Increment = 1;
            }));
            
            // create max button
            _maxButton = Object.Instantiate(__instance.capacityOption.PlusBtn, __instance.capacityOption.PlusBtn.transform.parent);
            var maxText = _maxButton.GetComponentInChildren<TextMeshPro>();
            maxText.text = CrowdedModPlugin.MaxPlayers.ToString();
            maxText.fontSize = maxText.fontSizeMax = 3;
            maxText.fontStyle = FontStyles.Normal;
            _maxButton.SetInteractable(true);
            _maxButton.transform.localPosition += new Vector3(1, 0, 0);
            _maxButton.OnClick = new Button.ButtonClickedEvent();
            _maxButton.OnClick.AddListener((UnityAction)(() =>
            {
                __instance.capacityOption.Increment = int.MaxValue;
                __instance.capacityOption.Increase();
                __instance.capacityOption.Increment = 1;
            }));
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.ValueChanged))]
    public static class CreateGameOptions_ValueChanged
    {
        public static void Postfix(CreateGameOptions __instance, OptionBehaviour option)
        {
            var numOpt = option.Cast<NumberOption>();
            if (!numOpt) return;

            _minButton.SetInteractable(true);
            _maxButton.SetInteractable(true);
            _doubleMinusButton.SetInteractable(true);
            _doublePlusButton.SetInteractable(true);

            if (Mathf.Approximately(numOpt.Value, numOpt.ValidRange.max))
            {
                _maxButton.SetInteractable(false);
                _doublePlusButton.SetInteractable(false);
            }
            else if (Mathf.Approximately(numOpt.Value, numOpt.ValidRange.min))
            {
                _minButton.SetInteractable(false);
                _doubleMinusButton.SetInteractable(false);
            }
        }
    }
}