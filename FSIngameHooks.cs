using Code.Core.Input_Utils;
using HarmonyLib;
using MelonLoader;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikmod
{
    public static class FSIngameHooks
    {
        private const string ClonedToggleButtonName = "FSIngameToggle";
        private const string BuiltinButtonContainerPath =
            "Toolbar(Clone)/New_Canvas/Communication_Container/Social_Container/Social_Panel/Public_Chat_Button/Builtin_Button_Container";

        public static bool IsHookEnabled = false;

        private static float timer = 0f;

        public static void OnUpdate()
        {
            timer -= Time.unscaledDeltaTime;

            if (timer > 0f)
                return;

            timer = 0.5f;

            GameObject t = GameObject.Find(BuiltinButtonContainerPath);

            if (t == null)
                return;

            InjectInto(t);
        }


        private static void InjectInto(GameObject go)
        {
            var parent = go.transform.parent;

            if (parent == null)
                return;

            if (parent.Find(ClonedToggleButtonName) != null)
                return;

            var clone = UnityEngine.Object.Instantiate(go);

            clone.name = ClonedToggleButtonName;

            clone.transform.SetParent(parent, false);

            var layout = clone.GetComponent<LayoutElement>();

            if (layout == null)
                layout = clone.AddComponent<LayoutElement>();

            layout.ignoreLayout = true;
            layout.preferredWidth = -1f;
            layout.preferredHeight = -1f;
            layout.flexibleWidth = -1f;
            layout.flexibleHeight = -1f;

            CopyChildRects(go.transform, clone.transform);

            var originalRect = go.GetComponent<RectTransform>();
            var cloneRect = clone.GetComponent<RectTransform>();

            if (originalRect != null && cloneRect != null)
            {
                CopyRect(originalRect, cloneRect);
            }
            else
            {
                clone.transform.SetPositionAndRotation(
                    go.transform.position,
                    go.transform.rotation);

                clone.transform.localScale =
                    go.transform.localScale;
            }

            clone.transform.SetSiblingIndex(
                go.transform.GetSiblingIndex() + 1);

            clone.SetActive(true);

            var images =
                clone.GetComponentsInChildren<Image>(true);

            foreach (var img in images)
            {
                if (img == null)
                    continue;

                img.color = Color.red;
                img.preserveAspect = true;
            }

            var toggle =
                clone.GetComponent<FSButtonToggle>();

            if (toggle == null)
                toggle = clone.AddComponent<FSButtonToggle>();

            toggle.Init(images);

            var buttons =
                clone.GetComponentsInChildren<Button>(true);

            foreach (var btn in buttons)
            {
                if (btn == null)
                    continue;

                btn.transition =
                    Selectable.Transition.None;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(toggle.Toggle);
            }

            var canvasGroup =
                clone.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup =
                    clone.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private static void CopyRect(
            RectTransform from,
            RectTransform to)
        {
            if (from == null || to == null)
                return;

            to.anchorMin = from.anchorMin;
            to.anchorMax = from.anchorMax;
            to.pivot = from.pivot;
            to.sizeDelta = from.sizeDelta;
            to.anchoredPosition = from.anchoredPosition;
            to.localPosition = from.localPosition;
            to.localRotation = from.localRotation;
            to.localScale = from.localScale;
            to.offsetMin = from.offsetMin;
            to.offsetMax = from.offsetMax;
        }

        private static void CopyChildRects(
            Transform original,
            Transform clone)
        {
            if (original == null || clone == null)
                return;

            var originalRect =
                original.GetComponent<RectTransform>();

            var cloneRect =
                clone.GetComponent<RectTransform>();

            if (originalRect != null && cloneRect != null)
                CopyRect(originalRect, cloneRect);

            int count = original.childCount;

            for (int i = 0; i < count; i++)
            {
                if (i >= clone.childCount)
                    break;

                CopyChildRects(
                    original.GetChild(i),
                    clone.GetChild(i));
            }
        }
    }

    [HarmonyPatch(typeof(SmartFoxClientAPI.SmartFoxClient))]
    [HarmonyPatch("SendPublicMessage")]
    [HarmonyPatch(new[] { typeof(string), typeof(int) })]
    internal class SendPublicMessagePatch
    {
        private static bool Prefix(string message)
        {
            if (!FSIngameHooks.IsHookEnabled)
                return true;

            try
            {
                if (!string.IsNullOrEmpty(message))
                    FreeSpeech.TalkSomeShit(message);

                return false;
            }
            catch (Exception ex)
            {
                MelonLogger.Msg(
                    "[FSIngameHooks] Send failed, toggling back to safe mode and not sending the message so it's user fault: " +
                    ex);
                FSIngameHooks.IsHookEnabled = false;
                return false;
            }
        }
    }

    // Patching to make the input box filter turned off when hook is on
    [HarmonyPatch]
    internal static class InputFieldManagerPatch
    {
        [HarmonyPatch(typeof(InputField_Manager), nameof(InputField_Manager.SanitizeText))]
        [HarmonyPrefix]
        private static bool SanitizeTextPrefix(
            ref string sentence,
            ref string __result)
        {
            if (!FSIngameHooks.IsHookEnabled)
                return true;

            sentence = sentence
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\u202A", "")
                .Replace("\u202B", "")
                .Replace("\u202C", "")
                .Replace("\u202D", "")
                .Replace("\u202E", "")
                .Replace("\u200E", "");

            bool hasHebrew = false;

            for (int i = 0; i < sentence.Length; i++)
            {
                char c = sentence[i];

                if (c >= '\u0590' && c <= '\u05FF')
                {
                    hasHebrew = true;
                    break;
                }
            }

            if (hasHebrew)
                sentence = "\u200F" + sentence;

            __result = sentence;
            return false;
        }

        [HarmonyPatch(typeof(InputField_Manager), "Start")]
        [HarmonyPostfix]
        private static void StartPostfix(InputField_Manager __instance)
        {
            Apply(__instance);
        }

        [HarmonyPatch(typeof(InputField_Manager), nameof(InputField_Manager.OnValueChanged))]
        [HarmonyPostfix]
        private static void OnValueChangedPostfix(InputField_Manager __instance)
        {
            Apply(__instance);
        }

        private static void Apply(InputField_Manager manager)
        {
            if (!FSIngameHooks.IsHookEnabled || manager == null)
                return;

            var input = manager.InputField;

            if (input == null)
                return;

            const int max = int.MaxValue;

            input.characterLimit = max;
            input.characterValidation = TMP_InputField.CharacterValidation.None;
            input.lineType = TMP_InputField.LineType.MultiLineNewline;

            var t = Traverse.Create(manager);

            t.Field("_charMax").SetValue(max);
            t.Field("_includesNumbers").SetValue(true);
            t.Field("_includeSpacebar").SetValue(true);
            t.Field("_includeHebrew").SetValue(true);
            t.Field("_includeSpecial").SetValue(true);
            t.Field("_includeSpecialExpanded").SetValue(true);
            t.Field("_includeEmail").SetValue(true);
            t.Field("_includeEmoji").SetValue(true);
            t.Field("_includeEmojiBuiltin").SetValue(true);
        }
    }
}