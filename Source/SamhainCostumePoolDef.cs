using System;
using System.Collections.Generic;
using Verse;

namespace SamhainXenotypeHH;

public class SamhainCostumePoolDef : Def
{
    private static List<SamhainCostumeSet>? additionalFullHeadCostumes;

    public List<SamhainCostumeSet> adultCostumes = new();
    public List<SamhainCostumeSet> childCostumes = new();

    public static List<SamhainCostumeSet> GetAdditionalFullHeadCostumes()
    {
        if (additionalFullHeadCostumes != null)
            return additionalFullHeadCostumes;

        var alreadyInCuratedPool = new HashSet<string>();
        SamhainCostumePoolDef? pool = DefDatabase<SamhainCostumePoolDef>.GetNamedSilentFail("OMW_SamhainCostumePool");
        if (pool != null)
        {
            AddPoolDefNames(alreadyInCuratedPool, pool.adultCostumes);
            AddPoolDefNames(alreadyInCuratedPool, pool.childCostumes);
        }

        additionalFullHeadCostumes = new List<SamhainCostumeSet>();
        foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            if (!thingDef.IsApparel || alreadyInCuratedPool.Contains(thingDef.defName) || !CoversFullHead(thingDef))
                continue;

            additionalFullHeadCostumes.Add(new SamhainCostumeSet
            {
                headwear = thingDef,
                defaultEnabled = false
            });
        }

        additionalFullHeadCostumes.Sort((a, b) =>
        {
            int byMod = string.Compare(a.headwear?.modContentPack?.Name, b.headwear?.modContentPack?.Name, StringComparison.OrdinalIgnoreCase);
            return byMod != 0
                ? byMod
                : string.Compare(a.headwear?.LabelCap.ToString(), b.headwear?.LabelCap.ToString(), StringComparison.OrdinalIgnoreCase);
        });
        return additionalFullHeadCostumes;
    }

    private static void AddPoolDefNames(HashSet<string> defNames, List<SamhainCostumeSet> costumes)
    {
        foreach (SamhainCostumeSet? costume in costumes)
        {
            if (costume?.headwear != null)
                defNames.Add(costume.headwear.defName);
            if (costume?.bodyPiece != null)
                defNames.Add(costume.bodyPiece.defName);
        }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        adultCostumes.RemoveAll(costume => !CoversFullHead(costume));
        childCostumes.RemoveAll(costume => !CoversFullHead(costume));
    }

    private static bool CoversFullHead(SamhainCostumeSet? costume)
    {
        return CoversFullHead(costume?.headwear) || CoversFullHead(costume?.bodyPiece);
    }

    private static bool CoversFullHead(ThingDef? apparel)
    {
        if (apparel?.apparel?.bodyPartGroups == null)
            return false;

        foreach (BodyPartGroupDef group in apparel.apparel.bodyPartGroups)
        {
            if (group.defName == "FullHead")
                return true;
        }
        return false;
    }
}

public class SamhainCostumeSet
{
    public ThingDef? headwear;
    public ThingDef? bodyPiece;
    public bool defaultEnabled = true;
}
