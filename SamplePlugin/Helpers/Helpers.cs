using ClickLib.Clicks;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Memory;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Helpers
{
    internal unsafe static class Helpers
    {
        // Stolen from AutoRetainer, Change later :)
        internal static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>()!.GetRow(2000401)!.Singular.ExtractText(), "リテイナーベル"];

        internal static readonly string[] Entrance =
    [
        "ハウスへ入る",
        "进入房屋",
        "進入房屋",
        "Eingang",
        "Entrée",
        "Entrance"
    ];

        internal static readonly string[] ConfirmHouseEntrance =
    [
        "「ハウス」へ入りますか？",
        "要进入这间房屋吗？",
        "要進入這間房屋嗎？",
        "Das Gebäude betreten?",
        "Entrer dans la maison ?",
        "Enter the estate hall?"
    ];

        internal static readonly string[] GoToYourApartment =
    [
        "Go to your apartment",
        "自分の部屋に移動する",
        "移动到自己的房间",
        "移動到自己的房間",
        "Die eigene Wohnung betreten",
        "Aller dans votre appartement"
    ];

        internal static bool IsRetainerBell(this GameObject o)
        {
            return o != null &&
                (o.ObjectKind == ObjectKind.EventObj || o.ObjectKind == ObjectKind.Housing)
                && o.Name.ToString().EqualsIgnoreCaseAny(BellName);
        }

        internal static GameObject GetReachableRetainerBell()
        {
            if (Player.Object is null) return null;

            foreach (var x in Svc.Objects)
            {
                if ((x.ObjectKind == ObjectKind.Housing || x.ObjectKind == ObjectKind.EventObj) && x.Name.ToString().EqualsIgnoreCaseAny(BellName))
                {
                    var distance = GetValidInteractionDistance(x);
                    if (Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position) < distance && x.IsTargetable)
                    {
                        return x;
                    }
                }
            }
            return null;
        }

        internal static GameObject GetClosestRetainerBell()
        {
            var item = from x in Svc.Objects
                       where x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(BellName)
                       orderby Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position)
                       select x;

            return item.FirstOrDefault();
        }

        internal static GameObject GetClosestEntrance()
        {
            var item = from x in Svc.Objects
                       where x.IsTargetable && x.Name.ToString().EqualsIgnoreCaseAny(Entrance)
                       orderby Vector3.Distance(x.Position, Svc.ClientState.LocalPlayer.Position)
                       select x;

            return item.FirstOrDefault();
        }

        internal static float GetValidInteractionDistance(GameObject bell)
        {
            if (bell.ObjectKind == ObjectKind.Housing)
            {
                return 6.5f;
            }
            else if (Inns.List.Contains(Svc.ClientState.TerritoryType))
            {
                return 4.75f;
            }
            else
            {
                return 4.6f;
            }
        }

        internal static AtkUnitBase* GetSpecificYesno(params string[] s)
        {
            for (var i = 1; i < 100; i++)
            {
                try
                {
                    var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                    if (addon == null) return null;
                    if (IsAddonReady(addon))
                    {
                        var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                        var text = MemoryHelper.ReadSeString(&textNode->NodeText).ExtractText().Replace(" ", "");
                        if (text.EqualsAny(s.Select(x => x.Replace(" ", ""))))
                        {
                            DuoLog.Verbose($"SelectYesno {s.Print()} addon {i}");
                            return addon;
                        }
                    }
                }
                catch (Exception e)
                {
                    e.Log();
                    return null;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddonReady(AtkUnitBase* Addon)
        {
            return Addon->IsVisible && Addon->UldManager.LoadedState == AtkLoadState.Loaded;
        }

        internal static bool TrySelectSpecificEntry(IEnumerable<string> text, Func<bool> Throttler = null)
        {
            return TrySelectSpecificEntry((x) => x.EqualsAny(text), Throttler);
        }

        internal static bool TrySelectSpecificEntry(Func<string, bool> inputTextTest, Func<bool> Throttler = null)
        {
            if (GenericHelpers.TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
            {
                var entry = GetEntries(addon).FirstOrDefault(inputTextTest);
                if (entry != null)
                {
                    var index = GetEntries(addon).IndexOf(entry);
                    if (index >= 0 && IsSelectItemEnabled(addon, index) && (Throttler?.Invoke() ?? EzThrottler.Throttle("GenericThrottle", 200)))
                    {
                        ClickSelectString.Using((nint)addon).SelectItem((ushort)index);
                        DuoLog.Information($"TrySelectSpecificEntry: selecting {entry}/{index}");
                        return true;
                    }
                }
            }
            else
            {
                EzThrottler.Throttle("GenericThrottle", 200, true);
            }
            return false;
        }

        internal static bool? waitUntilTimestamp(DateTimeOffset time)
        {
            if (DateTimeOffset.Now > time)
            {
                DuoLog.Information($"Timestamp reached: {DateTimeOffset.Now.Ticks} > {time.Ticks}");
                return true;
            }
            return false;
        }

        internal static List<string> GetEntries(AddonSelectString* addon)
        {
            var list = new List<string>();
            for (var i = 0; i < addon->PopupMenu.PopupMenu.EntryCount; i++)
            {
                list.Add(MemoryHelper.ReadSeStringNullTerminated((nint)addon->PopupMenu.PopupMenu.EntryNames[i]).ExtractText());
            }
            return list;
        }

        internal static bool IsSelectItemEnabled(AddonSelectString* addon, int index)
        {
            var step1 = (AtkTextNode*)addon->AtkUnitBase
                        .UldManager.NodeList[2]
                        ->GetComponent()->UldManager.NodeList[index + 1]
                        ->GetComponent()->UldManager.NodeList[3];
            return GenericHelpers.IsSelectItemEnabled(step1);
        }







    }
}
