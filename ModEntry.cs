using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;

namespace PersonalIndoorFarmChinese
{
    public class ModEntry : Mod
    {
        private Harmony? _harmony;

        private static readonly Dictionary<string, string> Translations = new()
        {
            // GMCM 选项名和描述
            ["Door Owner (Farmhouse)"] = "门主人（农舍内）",
            ["Which players room does a door lead to when inside a farmhouse"] = "当在农舍内时，门通向哪位玩家的房间",
            ["Door Owner (Outside)"] = "门主人（农舍外）",
            ["Which players room does a door lead to when outside a farmhouse"] = "当在农舍外时，门通向哪位玩家的房间",
            ["Selection Menu Style"] = "选择菜单样式",
            ["Choose your preferred room selection menu style"] = "选择您偏好的房间选择菜单样式",
            ["Locked Door Color"] = "上锁的门颜色",
            ["Accessibility option. Changes the color of the locked door icon."] = "无障碍选项。更改上锁门图标的颜色。",
            ["Locked When Offline Door Color"] = "离线时上锁的门颜色",
            ["Accessibility option. Changes the color of the locked when offline door icon."] = "无障碍选项。更改离线时上锁门图标的颜色。",
            ["Unlocked Door Color"] = "未上锁的门颜色",
            ["Accessibility option. Changes the color of the unlocked door icon."] = "无障碍选项。更改未上锁门图标的颜色。",
        };

        public override void Entry(IModHelper helper)
        {
            _harmony = new Harmony(ModManifest.UniqueID);

            Assembly? targetMod = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "PersonalIndoorFarm");

            if (targetMod == null)
            {
                Monitor.Log("未找到 PersonalIndoorFarm 程序集，跳过汉化。", LogLevel.Warn);
                return;
            }

            MethodInfo transpiler = typeof(ModEntry).GetMethod(
                nameof(Transpiler),
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

            var harmonyTranspiler = new HarmonyMethod(transpiler);
            int patchedCount = 0;

            foreach (Type type in targetMod.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.DeclaredOnly))
                {
                    try
                    {
                        if (method.GetMethodBody() == null) continue;
                        _harmony.Patch(method, transpiler: harmonyTranspiler);
                        patchedCount++;
                    }
                    catch { }
                }
            }

            Monitor.Log($"PersonalIndoorFarm 汉化补丁已加载，共修补 {patchedCount} 个方法。", LogLevel.Info);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr &&
                    instruction.operand is string original &&
                    Translations.TryGetValue(original, out string? translated))
                {
                    instruction.operand = translated;
                }
                yield return instruction;
            }
        }
    }
}
