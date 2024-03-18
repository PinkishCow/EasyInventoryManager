using Dalamud.Game.ClientState.Objects.Types;
using EasyInventoryManager.Helpers;
using EasyInventoryManager.Retainer;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyInventoryManager.Tasks
{
    internal unsafe class SpawnRetainer
    {
        internal static void Enqueue(int retainerIndex)
        {

            Instance.TaskManager.EnqueueImmediate(() => RetainerListHelpers.SelectRetainerBySortedIndex(retainerIndex), "SelectRetainer");

            Instance.TaskManager.EnqueueImmediate(() => Helpers.Helpers.waitForSeconds(2), 1000 * 60, "WaitForTime");
            Instance.TaskManager.EnqueueImmediate(() => RetainerListHelpers.ClickTalkBox(), "ClickTalkBox");

            Instance.TaskManager.EnqueueImmediate(() => Helpers.Helpers.waitForSeconds(2), 1000 * 60, "WaitForTime");
            Instance.TaskManager.EnqueueImmediate(() => RetainerListHelpers.CheckRetainerAvailable(out _), "CheckRetainerAvailable");
            Instance.TaskManager.EnqueueImmediate(RetainerListHelpers.CloseRetainer, "CloseRetainer");

            Instance.TaskManager.EnqueueImmediate(() => Helpers.Helpers.waitForSeconds(2), 1000 * 60, "WaitForTime");
            Instance.TaskManager.EnqueueImmediate(() => RetainerListHelpers.ClickTalkBox(), "ClickTalkBox");

            Instance.TaskManager.EnqueueImmediate(() => Helpers.Helpers.waitForSeconds(3), 1000 * 60, "WaitForTime");
        }
    }
}
