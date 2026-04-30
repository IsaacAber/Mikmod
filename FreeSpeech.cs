using Code.Core.Network;
using Code.Core.Player;
using HarmonyLib;
using LitJson;
using MelonLoader;
using SmartFoxClientAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikmod
{
    // The internet is a space where people have the right to express their opinions freely.
    // This includes discussing personal topics, viewpoints, and adult-related issues.

    // Instead of applying broad age restrictions across all software and platforms,
    // a more effective approach is parental control and supervision.

    // Parents are responsible for managing their children's internet access.
    // Tools for filtering, monitoring, and restricting content already exist.

    // Overly restrictive systems can negatively impact all users,
    // while parental management allows for more flexible control.

    public static class FreeSpeech
    {
        public const int MAX_MESSAGE_LENGTH = 128;
        public static string inputMessage = "";
        public static Dictionary<Chat_Handler, int> messageIds = new Dictionary<Chat_Handler, int>();
        public static void TalkSomeShit(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (message.Length > MAX_MESSAGE_LENGTH)
                message = message.Substring(0, MAX_MESSAGE_LENGTH);

            string encoded = EncodeModMessage(message);

            Hashtable commandData = new Hashtable { { "e", 1 }, { "p", encoded } };
            Request_Process request_Process = new Request_Process("ms_s", 0, null, null, 6f, false, false);
            request_Process.SetAdditionalData(commandData);
            Comm.Get.SendExtension(request_Process);
        }
        public static string EncodeModMessage(string message)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            string digits = string.Join("", Array.ConvertAll(bytes, b => b.ToString("D3")));
            int half = (digits.Length / 2 / 3) * 3;
            string x = "0." + digits.Substring(0, half);
            string y = "0." + digits.Substring(half);
            return x + "," + y;
        }

        public static string DecodeModMessage(string position)
        {
            string[] parts = position.Split(',');
            string digits = parts[0].Split('.')[1] + parts[1].Split('.')[1];
            List<byte> bytes = new List<byte>();
            for (int i = 0; i + 3 <= digits.Length; i += 3)
                bytes.Add(byte.Parse(digits.Substring(i, 3)));
            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }
        public static void CleanupDeadHandlers()
        {
            var toRemove = new List<Chat_Handler>();

            foreach (var kvp in messageIds)
            {
                if (kvp.Key == null || kvp.Key.ChatBubble == null)
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
                messageIds.Remove(key);
        }
        public static Chat_Handler GetUserChatHandler(int userId)
        {
            foreach (Chat_Bubble bubble in GameObject.FindObjectsOfType<Chat_Bubble>())
            {
                Transform grandparent = bubble.gameObject.transform.parent?.parent;
                if (grandparent != null && grandparent.name.Contains($"user_{userId}_"))
                    return bubble.GetComponent<Chat_Handler>();
            }
            return null;
        }

    }
    [HarmonyPatch(typeof(SmartFoxClient), "DispatchEvent")]
    public static class Patch_DispatchEvent
    {
        private static IEnumerator SetMessageSafe(Chat_Handler handler, string message)
        {
            // Ensure we are on Unity main thread
            yield return null;

            if (handler == null || handler.ChatBubble == null)
                yield break;

            // Assign message ID (prevents old timers from clearing new messages)
            if (!FreeSpeech.messageIds.ContainsKey(handler))
                FreeSpeech.messageIds[handler] = 0;

            FreeSpeech.messageIds[handler]++;
            int currentId = FreeSpeech.messageIds[handler];

            // Set message
            handler.SetMessage(message);

            // No timeout → exit
            if (Settings.MessageLifetime.Value <= 0)
                yield break;

            // Wait before hiding
            yield return new WaitForSeconds(Settings.MessageLifetime.Value);

            if (handler == null || handler.ChatBubble == null)
                yield break;

            // Only hide if this is still the latest message
            if (FreeSpeech.messageIds.ContainsKey(handler) &&
                FreeSpeech.messageIds[handler] == currentId)
            {
                try
                {
                    handler.HideBubble();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[FreeSpeech] Timeout clear failed: {ex}");
                }
            }
        }
        public static void Postfix(SFSEvent evt)
        {
            if (evt.GetType() != "OnExtensionResponse") return;

            string type = evt.GetParameter("type") as string;
            if (type != "json") return;

            JsonData data = evt.GetParameter("dataObj") as JsonData;
            if (data == null) return;

            string cmd = data["_cmd"]?.ToString();
            if (cmd != "ms_s") return;

            string position = data["p"]?.ToString();
            if (position == null) return;

            int entityId = int.Parse(data["e"].ToString());
            if (entityId != 1) return;

            int senderId = int.Parse(data["s"].ToString());
            string message = FreeSpeech.DecodeModMessage(position);

            if (message.Length > FreeSpeech.MAX_MESSAGE_LENGTH)
                message = message.Substring(0, FreeSpeech.MAX_MESSAGE_LENGTH); MelonLogger.Msg($"[FreeSpeech] packet detected from {senderId}: {message}");

            var handler = FreeSpeech.GetUserChatHandler(senderId);

            if (handler != null)
            {
                MelonCoroutines.Start(SetMessageSafe(handler, message));
            }
            else
            {
                MelonLogger.Msg($"[FreeSpeech] No handler found for user {senderId}");
            }
        }
    }
}