using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace SSLightningRod;

public class Command_ChangeMode : Command
{
    private readonly SoundDef turnOnSound = SoundDefOf.Checkbox_TurnedOn;
    public Func<int> Mode;

    public Action toggleAction;

    public override SoundDef CurActivateSound => turnOnSound;

    private static string abbrevs(int a)
    {
        switch (a)
        {
            case 1:
                return "PS";
            case 2:
                return "NM";
            case 3:
                return "FC";
            default:
                return "PS";
        }
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        toggleAction();
    }

    public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth, GizmoRenderParms parms)
    {
        var result = base.GizmoOnGUI(loc, maxWidth, parms);
        var rect = new Rect(loc.x, loc.y, GetWidth(maxWidth), 75f);
        var position = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
        var modStr = abbrevs(Mode());
        Text.Font = GameFont.Tiny;
        Widgets.Label(position, modStr);
        return result;
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        return other is Command_ChangeMode commandToggle && commandToggle.Mode() == Mode();
    }
}