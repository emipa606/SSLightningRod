using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace SSLightningRod;

public class Command_ChangeMode : Command
{
    public Func<int> Mode;

    public Action toggleAction;

    public SoundDef turnOnSound = SoundDefOf.Checkbox_TurnedOn;

    public override SoundDef CurActivateSound => turnOnSound;

    public string Abbrevs(int a)
    {
        string returnstr;
        switch (a)
        {
            case 1:
                returnstr = "PS";
                break;
            case 2:
                returnstr = "NM";
                break;
            case 3:
                returnstr = "FC";
                break;
            default:
                returnstr = "PS";
                break;
        }

        return returnstr;
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
        var modestr = Abbrevs(Mode());
        Text.Font = GameFont.Tiny;
        Widgets.Label(position, modestr);
        return result;
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        return other is Command_ChangeMode command_Toggle && command_Toggle.Mode() == Mode();
    }
}