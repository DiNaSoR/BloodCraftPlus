using Bloodcraft;
using Bloodcraft.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using static Bloodcraft.Services.PlayerService;

namespace Bloodcraft.Patches;

[HarmonyPatch]
internal static class ChatMessageSystemPatch
{
    static readonly Regex _regexMAC = new(";mac([^;]+)$");
    const string EclipsePrefix = "[ECLIPSE]";
    static readonly Regex _regexEclipse = new(@"^\[ECLIPSE\]\[(\d+)\]:(\d+\.\d+\.\d+);(\d+)$");
    const int StatusToClientEventId = 8; // ClientChatSystemPatch.NetworkEventSubType.StatusToClient

    [HarmonyBefore("CrimsonChatFilter")]
    [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ChatMessageSystem __instance)
    {
        if (!Core.IsReady) return;

        NativeArray<Entity> entities = __instance.EntityQueries[0].ToEntityArray(Allocator.Temp);
        NativeArray<ChatMessageEvent> chatMessageEvents = __instance.EntityQueries[0].ToComponentDataArray<ChatMessageEvent>(Allocator.Temp);

        try
        {
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                ChatMessageEvent chatMessageEvent = chatMessageEvents[i];

                string raw = chatMessageEvent.MessageText.Value;
                if (!raw.StartsWith(EclipsePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (CheckMAC(raw, out string originalMessage))
                {
                    if (Core.Eclipsed)
                    {
                        EclipseService.HandleClientMessage(originalMessage);
                    }
                    else
                    {
                        TryReplySyncDisabled(originalMessage);
                    }
                    entity.Destroy(true);
                }
                else
                {
                    DebugToolsBridge.TryLogWarning($"[Eclipse] Dropped client message (MAC missing/invalid). Raw='{raw}'");
                }
            }
        }
        finally
        {
            entities.Dispose();
            chatMessageEvents.Dispose();
        }
    }
    public static bool CheckMAC(string receivedMessage, out string originalMessage)
    {
        Match match = _regexMAC.Match(receivedMessage);
        originalMessage = "";

        if (match.Success)
        {
            string receivedMAC = match.Groups[1].Value;
            string intermediateMessage = _regexMAC.Replace(receivedMessage, "");

            if (VerifyMAC(intermediateMessage, receivedMAC, Core.NEW_SHARED_KEY))
            {
                originalMessage = intermediateMessage;

                return true;
            }
        }

        return false;
    }
    static bool VerifyMAC(string message, string receivedMAC, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes = hmac.ComputeHash(messageBytes);
        string recalculatedMAC = Convert.ToBase64String(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(recalculatedMAC),
            Encoding.UTF8.GetBytes(receivedMAC));
    }
    public static string GenerateMAC(string message)
    {
        using var hmac = new HMACSHA256(Core.NEW_SHARED_KEY);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes = hmac.ComputeHash(messageBytes);

        return Convert.ToBase64String(hashBytes);
    }

    static void TryReplySyncDisabled(string originalMessage)
    {
        // We only reply to valid Eclipse registration messages so the client can display the status in UI.
        Match match = _regexEclipse.Match(originalMessage);
        if (!match.Success)
        {
            return;
        }

        if (!int.TryParse(match.Groups[1].Value, out int eventType) || eventType != 0)
        {
            return;
        }

        if (!ulong.TryParse(match.Groups[3].Value, out ulong steamId))
        {
            return;
        }

        if (!steamId.TryGetPlayerInfo(out PlayerInfo playerInfo))
        {
            return;
        }

        // Keep this payload compact; it must fit in FixedString512Bytes after `;mac...`.
        string payload = $"[{StatusToClientEventId}]:disabled,server-config";
        string messageWithMac = $"{payload};mac{GenerateMAC(payload)}";

        LocalizationService.HandleServerReply(Core.EntityManager, playerInfo.User, messageWithMac);
    }
}
