using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager
{
    internal static class Helpers
    {
        // Stolen from AutoRetainer, Change later :)
        internal static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>()!.GetRow(2000401)!.Singular.ExtractText(), "リテイナーベル"];

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

        internal static GameObject GetClosestEntrance()
        {

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
    }
}
