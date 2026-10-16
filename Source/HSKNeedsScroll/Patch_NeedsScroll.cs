using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSKNeedsScroll;

[StaticConstructorOnStartup]
public static class HSKNeedsScrollInit
{
    static HSKNeedsScrollInit()
    {
        var harmony = new Harmony("linya.hskneedsscroll");
        harmony.PatchAll();
        Log.Message("[HSKNeedsScroll] Patches applied.");
    }
}

[HarmonyPatch(typeof(NeedsCardUtility), nameof(NeedsCardUtility.DoNeeds))]
public static class Patch_NeedsScroll
{
    private static Vector2 needsScrollPosition;
    private const float MinNeedHeight = 50f;

    public static bool Prefix(Rect rect, Pawn pawn)
    {
        var displayNeeds = new List<Need>();
        foreach (var need in pawn.needs.AllNeeds)
        {
            if (need.ShowOnNeedList)
                displayNeeds.Add(need);
        }
        PawnNeedsUIUtility.SortInDisplayOrder(displayNeeds);

        if (displayNeeds.Count == 0)
            return false;

        float needHeight = Mathf.Min(70f, rect.height / displayNeeds.Count);

        // If needs fit fine, let vanilla handle it
        if (needHeight >= MinNeedHeight)
            return true;

        // Calculate total height with minimum size
        float totalHeight = 0f;
        for (int i = 0; i < displayNeeds.Count; i++)
        {
            float h = MinNeedHeight;
            if (displayNeeds[i].def.major)
                h = Mathf.Max(h, 50f);
            totalHeight += h;
            if (i > 0 && displayNeeds[i - 1].def.major && !displayNeeds[i].def.major)
                totalHeight += 10f;
        }

        Rect viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight);
        Widgets.BeginScrollView(rect, ref needsScrollPosition, viewRect);

        float y = 0f;
        for (int i = 0; i < displayNeeds.Count; i++)
        {
            var need = displayNeeds[i];
            float h = MinNeedHeight;
            if (need.def.major)
                h = Mathf.Max(h, 50f);

            Rect needRect = new Rect(0f, y, viewRect.width, h);
            if (!need.def.major)
            {
                if (i > 0 && displayNeeds[i - 1].def.major)
                    needRect.y += 10f;
                needRect.width *= 0.73f;
                needRect.height = Mathf.Max(needRect.height * 0.666f, 30f);
            }

            need.DrawOnGUI(needRect);
            y = needRect.yMax;
        }

        Widgets.EndScrollView();
        return false;
    }
}
