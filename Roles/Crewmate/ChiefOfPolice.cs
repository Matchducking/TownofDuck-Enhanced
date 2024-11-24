using TOHE.Roles.Core;
using UnityEngine;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Crewmate;

internal class ChiefOfPolice : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 12600;
    public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.ChiefOfPolice);
    public override bool IsDesyncRole => true;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.CrewmatePower;
    //==================================================================\\

    private static OptionItem SkillCooldown;
    private static OptionItem CanRecruitImpostorAndNeutarl;
    private static OptionItem PreventRecruitNonKiller;
    private static OptionItem SuidiceWhenTargetNotKiller;

    public override void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.ChiefOfPolice);
        SkillCooldown = FloatOptionItem.Create(Id + 10, "ChiefOfPoliceSkillCooldown", new(2.5f, 900f, 2.5f), 20f, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.ChiefOfPolice])
            .SetValueFormat(OptionFormat.Seconds);
        CanRecruitImpostorAndNeutarl = BooleanOptionItem.Create(Id + 11, "PolicCanImpostorAndNeutarl", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.ChiefOfPolice]);
        PreventRecruitNonKiller = BooleanOptionItem.Create(Id + 12, "PolicRecruitNonKiller", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.ChiefOfPolice]);
        SuidiceWhenTargetNotKiller = BooleanOptionItem.Create(Id + 13, "PolicSuidiceWhenTargetNotKiller", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.ChiefOfPolice]);
    }

    public override void Add(byte playerId)
    {
        AbilityLimit = 1;
    }

    public override bool CanUseKillButton(PlayerControl pc) => AbilityLimit > 0;

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = AbilityLimit > 0 ? SkillCooldown.GetFloat() : 999f;

    public override bool KnowRoleTarget(PlayerControl seer, PlayerControl target)
    {
        if (seer.IsAnySubRole(x => x.IsConverted()) || target.IsAnySubRole(x => x.IsConverted()))
            return false;
        if (seer.Is(CustomRoles.ChiefOfPolice) && target.Is(CustomRoles.Sheriff))
            return true;
        if (seer.Is(CustomRoles.Sheriff) && target.Is(CustomRoles.ChiefOfPolice))
            return true;
        return false;
    }

    public override bool ForcedCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (AbilityLimit < 1) return false;
        bool suidice = false;

        if (target.GetCustomRole().IsCrewmate() && !target.IsAnySubRole(x => x.IsConverted()))
        {
            if (PreventRecruitNonKiller.GetBool() && !target.CanUseKillButton())
            {
                suidice = true;
            }
            else
            {
                AbilityLimit--;
                killer.RpcGuardAndKill(target);
                killer.ResetKillCooldown();
                killer.SetKillCooldown();

                target.GetRoleClass()?.OnRemove(target.PlayerId);
                target.RpcChangeRoleBasis(CustomRoles.Sheriff);
                target.RpcSetCustomRole(CustomRoles.Sheriff);
                target.GetRoleClass()?.OnAdd(target.PlayerId);

                target.RpcGuardAndKill(killer);
                target.ResetKillCooldown();
                target.SetKillCooldown();

                killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.ChiefOfPolice), GetString("SheriffSuccessfullyRecruited")));
                target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.ChiefOfPolice), GetString("BeSheriffByPolice")));

                Utils.NotifyRoles(killer);
                Utils.NotifyRoles(target);
            }
        }
        else
        {
            if (!CanRecruitImpostorAndNeutarl.GetBool())
            {
                suidice = true;
            }
            else
            {
                if (PreventRecruitNonKiller.GetBool() && !target.CanUseKillButton())
                {
                    suidice = true;
                }
                else
                {
                    AbilityLimit--;
                    killer.RpcGuardAndKill(target);
                    killer.ResetKillCooldown();
                    killer.SetKillCooldown();

                    target.GetRoleClass()?.OnRemove(target.PlayerId);
                    target.RpcChangeRoleBasis(CustomRoles.Sheriff);
                    target.RpcSetCustomRole(CustomRoles.Sheriff);
                    target.GetRoleClass()?.OnAdd(target.PlayerId);

                    target.RpcGuardAndKill(killer);
                    target.ResetKillCooldown();
                    target.SetKillCooldown();

                    killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.ChiefOfPolice), GetString("SheriffSuccessfullyRecruited")));
                    target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.ChiefOfPolice), GetString("BeSheriffByPolice")));

                    Utils.NotifyRoles(killer);
                    Utils.NotifyRoles(target);
                }
            }
        }

        if (suidice && SuidiceWhenTargetNotKiller.GetBool())
        {
            AbilityLimit--;
            killer.RpcMurderPlayer(killer);
            killer.SetDeathReason(PlayerState.DeathReason.Misfire);
            killer.SetRealKiller(killer);
        }
        else
        {
            killer.ResetKillCooldown();
            killer.SetKillCooldown(forceAnime: true);
            killer.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.ChiefOfPolice), GetString("PoliceFailedRecruit")));
            Utils.NotifyRoles(killer);
        }

        SendSkillRPC();
        return false;
    }
    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText(GetString("ChiefOfPoliceKillButtonText"));
    }

    public override string GetProgressText(byte playerId, bool commns)
    => !commns ? Utils.ColorString(AbilityLimit > 0 ? Utils.GetRoleColor(CustomRoles.ChiefOfPolice).ShadeColor(0.25f) : Color.gray, $"({AbilityLimit})") : "";
}
