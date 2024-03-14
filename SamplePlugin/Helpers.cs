using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
