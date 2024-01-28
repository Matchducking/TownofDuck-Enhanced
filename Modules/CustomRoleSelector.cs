﻿using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using TOHE.Roles.Double;
using TOHE.Roles.Impostor;
using TOHE.Roles.Neutral;
using static TOHE.Translator;

namespace TOHE.Modules;

internal class CustomRoleSelector
{
    public static Dictionary<PlayerControl, CustomRoles> RoleResult;
    public static IReadOnlyList<CustomRoles> AllRoles => RoleResult.Values.ToList();

    public static void SelectCustomRoles()
    {
        RoleResult = [];
        var rd = IRandom.Instance;
        int playerCount = Main.AllAlivePlayerControls.Length;
        int optImpNum = Main.RealOptionsData.GetInt(Int32OptionNames.NumImpostors);
        int optNonNeutralKillingNum = 0;
        int optNeutralKillingNum = 0;

        if (Options.NonNeutralKillingRolesMaxPlayer.GetInt() > 0 && Options.NonNeutralKillingRolesMaxPlayer.GetInt() >= Options.NonNeutralKillingRolesMinPlayer.GetInt())
        {
            optNonNeutralKillingNum = rd.Next(Options.NonNeutralKillingRolesMinPlayer.GetInt(), Options.NonNeutralKillingRolesMaxPlayer.GetInt() + 1);
        }
        if (Options.NeutralKillingRolesMaxPlayer.GetInt() > 0 && Options.NeutralKillingRolesMaxPlayer.GetInt() >= Options.NeutralKillingRolesMinPlayer.GetInt())
        {
            optNeutralKillingNum = rd.Next(Options.NeutralKillingRolesMinPlayer.GetInt(), Options.NeutralKillingRolesMaxPlayer.GetInt() + 1);
        }

        int readyRoleNum = 0;
        int readyNonNeutralKillingNum = 0;
        int readyNeutralKillingNum = 0;

        List<CustomRoles> rolesToAssign = [];
        List<CustomRoles> roleList = [];
        List<CustomRoles> roleOnList = [];

        List<CustomRoles> ImpOnList = [];
        List<CustomRoles> MiniOnList = [];
        List<CustomRoles> ImpRateList = [];
        List<CustomRoles> MiniRateList = [];

        List<CustomRoles> NonNeutralKillingOnList = [];
        List<CustomRoles> NonNeutralKillingRateList = [];

        List<CustomRoles> NeutralKillingOnList = [];
        List<CustomRoles> NeutralKillingRateList = [];

        List<CustomRoles> roleRateList = [];

        if (Options.CurrentGameMode == CustomGameMode.FFA)
        {
            RoleResult = [];
            foreach (PlayerControl pc in Main.AllAlivePlayerControls)
            {
                RoleResult.Add(pc, CustomRoles.Killer);
            }

            return;
        }

        foreach (var cr in CustomRolesHelper.AllRoles)
        {
            var role = (CustomRoles)Enum.Parse(typeof(CustomRoles), cr.ToString());
            if (role.IsVanilla() || role.IsAdditionRole()) continue;
            if (role is CustomRoles.GM or CustomRoles.NotAssigned) continue;

            if (GameStates.FungleIsActive) // The Fungle
            {
                if (role is CustomRoles.DarkHide) continue;
            }
            //else if (Options.IsActiveDleks) // Dleks
            //{
            //    // This roles need additional conditions - Witch & Spellсaster & Hex Master

            //    if (role is CustomRoles.Swooper
            //        or CustomRoles.Miner
            //        or CustomRoles.Lurker
            //        or CustomRoles.EngineerTOHE
            //        or CustomRoles.Paranoia
            //        or CustomRoles.Veteran
            //        or CustomRoles.Alchemist
            //        or CustomRoles.Bastion
            //        or CustomRoles.Grenadier
            //        or CustomRoles.DovesOfNeace // Pacifist
            //        or CustomRoles.Mole
            //        or CustomRoles.Addict
            //        or CustomRoles.TimeMaster
            //        or CustomRoles.Lighter
            //        or CustomRoles.Chameleon
            //        or CustomRoles.Mario // Vector
            //        or CustomRoles.Wraith
            //        or CustomRoles.Arsonist) continue;
            //    if (role == CustomRoles.Witch && (Witch.SwitchTrigger)Witch.ModeSwitchAction.GetValue() == Witch.SwitchTrigger.Vent || // Spellcaster
            //        role == CustomRoles.HexMaster && (HexMaster.SwitchTrigger)HexMaster.ModeSwitchAction.GetValue() == HexMaster.SwitchTrigger.Vent) continue;
            //}

            for (int i = 0; i < role.GetCount(); i++)
                roleList.Add(role);
        }

        // 职业设置为：优先
        foreach (var role in roleList.ToArray()) if (role.GetMode() == 2)
        {
            if (CustomRolesHelper.IsGhostRole(role)) continue; // Prevent ghost-spawn
            if (role.IsImpostor()) ImpOnList.Add(role);
            else if (role.IsMini()) MiniOnList.Add(role);
            else if (role.IsNonNK()) NonNeutralKillingOnList.Add(role);
            else if (role.IsNK()) NeutralKillingOnList.Add(role);
            else roleOnList.Add(role);
        }
        // 职业设置为：启用
        foreach (var role in roleList.ToArray()) if (role.GetMode() == 1)
        {
            if (CustomRolesHelper.IsGhostRole(role)) continue; // Prevent ghost-spawn
            if (role.IsImpostor()) ImpRateList.Add(role);
            else if (role.IsMini()) MiniRateList.Add(role);
            else if (role.IsNonNK()) NonNeutralKillingRateList.Add(role);
            else if (role.IsNK()) NeutralKillingRateList.Add(role);
            else roleRateList.Add(role);
        }
        while (MiniOnList.Count == 1)
        {
            var select = MiniOnList[rd.Next(0, MiniOnList.Count)];
            MiniOnList.Remove(select);
            Mini.SetMiniTeam(Mini.EvilMiniSpawnChances.GetFloat());
            if (!Mini.IsEvilMini)
            {
                roleOnList.Add(CustomRoles.NiceMini);
            }
            if (Mini.IsEvilMini)
            {
                ImpOnList.Add(CustomRoles.EvilMini);
            }
        }
        while (MiniRateList.Count == 1)
        {
            var select = MiniRateList[rd.Next(0, MiniRateList.Count)];
            MiniRateList.Remove(select);
            Mini.SetMiniTeam(Mini.EvilMiniSpawnChances.GetFloat());
            if (!Mini.IsEvilMini)
            {
                roleRateList.Add(CustomRoles.NiceMini);
            }
            if (Mini.IsEvilMini)
            {
                ImpRateList.Add(CustomRoles.EvilMini);
            }
        }

        // Select Impostors "Always"
        while (ImpOnList.Count > 0)
        {
            var select = ImpOnList[rd.Next(0, ImpOnList.Count)];
            ImpOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            Logger.Info(select.ToString() + " Add to Impostor waiting list (priority)", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyRoleNum >= optImpNum) break;
        }
        // Select Impostors "Random"
        if (readyRoleNum < playerCount && readyRoleNum < optImpNum)
        {
            while (ImpRateList.Count > 0)
            {
                var select = ImpRateList[rd.Next(0, ImpRateList.Count)];
                ImpRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                Logger.Info(select.ToString() + " Add to Impostor waiting list", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyRoleNum >= optImpNum) break;
            }
        }

        // Select NonNeutralKilling "Always"
        while (NonNeutralKillingOnList.Count > 0 && optNonNeutralKillingNum > 0)
        {
            var select = NonNeutralKillingOnList[rd.Next(0, NonNeutralKillingOnList.Count)];
            NonNeutralKillingOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            readyNonNeutralKillingNum += select.GetCount();
            Logger.Info(select.ToString() + " Add to Non NeutralKilling waiting list (priority)", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyNonNeutralKillingNum >= optNonNeutralKillingNum) break;
        }

        // Select NonNeutralKilling "Random"
        if (readyRoleNum < playerCount && readyNonNeutralKillingNum < optNonNeutralKillingNum)
        {
            while (NonNeutralKillingRateList.Count > 0 && optNonNeutralKillingNum > 0)
            {
                var select = NonNeutralKillingRateList[rd.Next(0, NonNeutralKillingRateList.Count)];
                NonNeutralKillingRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                readyNonNeutralKillingNum += select.GetCount();
                Logger.Info(select.ToString() + "Add to Non Neutral Killing waiting list", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyNonNeutralKillingNum >= optNonNeutralKillingNum) break;
            }
        }

        // Select NeutralKilling "Always"
        while (NeutralKillingOnList.Count > 0 && optNeutralKillingNum > 0)
        {
            var select = NeutralKillingOnList[rd.Next(0, NeutralKillingOnList.Count)];
            NeutralKillingOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            readyNeutralKillingNum += select.GetCount();
            Logger.Info(select.ToString() + " Add to NeutralKilling waiting list (priority)", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyNeutralKillingNum >= optNeutralKillingNum) break;
        }

        // Select NeutralKilling "Random"
        if (readyRoleNum < playerCount && readyNeutralKillingNum < optNeutralKillingNum)
        {
            while (NeutralKillingRateList.Count > 0 && optNeutralKillingNum > 0)
            {
                var select = NeutralKillingRateList[rd.Next(0, NeutralKillingRateList.Count)];
                NeutralKillingRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                readyNeutralKillingNum += select.GetCount();
                Logger.Info(select.ToString() + " Add to NeutralKilling waiting list", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyNeutralKillingNum >= optNeutralKillingNum) break;
            }
        }

        // Select Crewmates "Always"
        while (roleOnList.Count > 0)
        {
            var select = roleOnList[rd.Next(0, roleOnList.Count)];
            roleOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            Logger.Info(select.ToString() + " Add to Crewmate waiting list (preferred)", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
        }
        // Select Crewmates "Random"
        if (readyRoleNum < playerCount)
        {
            while (roleRateList.Count > 0)
            {
                var select = roleRateList[rd.Next(0, roleRateList.Count)];
                roleRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                Logger.Info(select.ToString() + " Add to Crewmate waiting list", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
            }
        }

    // 职业抽取结束
    EndOfAssign:

        if (rd.Next(0, 100) < Options.SunnyboyChance.GetInt() && rolesToAssign.Remove(CustomRoles.Jester)) rolesToAssign.Add(CustomRoles.Sunnyboy);
        if (rd.Next(0, 100) < Sans.BardChance.GetInt() && rolesToAssign.Remove(CustomRoles.Sans)) rolesToAssign.Add(CustomRoles.Bard);
        if (rd.Next(0, 100) < Vampire.VampiressChance.GetInt() && rolesToAssign.Remove(CustomRoles.Vampire)) rolesToAssign.Add(CustomRoles.Vampiress);
        if (rd.Next(0, 100) < Options.NukerChance.GetInt() && rolesToAssign.Remove(CustomRoles.Bomber)) rolesToAssign.Add(CustomRoles.Nuker);

        if (NSerialKiller.HasSerialKillerBuddy.GetBool() && rolesToAssign.Contains(CustomRoles.NSerialKiller))
        {
            if (rd.Next(0, 100) < NSerialKiller.ChanceToSpawn.GetInt()) rolesToAssign.Add(CustomRoles.NSerialKiller);
            //if (rd.Next(0, 100) < NSerialKiller.ChanceToSpawnAnother.GetInt()) rolesToAssign.Add(CustomRoles.NSerialKiller);
        }

        if (Options.NeutralKillingRolesMaxPlayer.GetInt() > 1 && !Options.TemporaryAntiBlackoutFix.GetBool())
        {
            _ = new LateTask(() =>
            {
                Logger.SendInGame(GetString("NeutralKillingBlackoutWarning"));
            }, 4f, "Neutral Killing Blackout Warning");
        }

        if (Romantic.IsEnable)
        {
            if (rolesToAssign.Contains(CustomRoles.Romantic))
            {
                if (rolesToAssign.Contains(CustomRoles.Lovers))
                    rolesToAssign.Remove(CustomRoles.Lovers);
                if (rolesToAssign.Contains(CustomRoles.Ntr))
                    rolesToAssign.Remove(CustomRoles.Ntr);
            }
        }

        /*  if (!rolesToAssign.Contains(CustomRoles.Lovers) && rolesToAssign.Contains(CustomRoles.FFF) || !rolesToAssign.Contains(CustomRoles.Ntr) && rolesToAssign.Contains(CustomRoles.FFF))
              rolesToAssign.Remove(CustomRoles.FFF); 
              rolesToAssign.Add(CustomRoles.Jester); */

        /*   if (!Options.DisableSaboteur.GetBool()) // no longer hidden
           {
               if (rd.Next(0, 100) < 25 && rolesToAssign.Remove(CustomRoles.Inhibitor)) rolesToAssign.Add(CustomRoles.Saboteur);
           } */

        // EAC封禁名单玩家开房将被分配为小丑
        if (BanManager.CheckEACList(PlayerControl.LocalPlayer.FriendCode, PlayerControl.LocalPlayer.GetClient().GetHashedPuid()))
        {
            if (!rolesToAssign.Contains(CustomRoles.Jester))
                rolesToAssign.Add(CustomRoles.Jester);
            Main.DevRole.Remove(PlayerControl.LocalPlayer.PlayerId);
            Main.DevRole.Add(PlayerControl.LocalPlayer.PlayerId, CustomRoles.Jester);
        }

        // Dev Roles List Edit
        foreach (var dr in Main.DevRole)
        {
            if (dr.Key == PlayerControl.LocalPlayer.PlayerId && Main.EnableGM.Value) continue;
            if (rolesToAssign.Contains(dr.Value))
            {
                rolesToAssign.Remove(dr.Value);
                rolesToAssign.Insert(0, dr.Value);
                Logger.Info("Role list improved priority：" + dr.Value, "Dev Role");
                continue;
            }
            for (int i = 0; i < rolesToAssign.Count; i++)
            {
                var role = rolesToAssign[i];
                if (dr.Value.GetMode() != role.GetMode()) continue;
                if (
                    (dr.Value.IsMini() && role.IsMini()) ||
                    (dr.Value.IsImpostor() && role.IsImpostor()) ||
                    (dr.Value.IsNonNK() && role.IsNonNK()) ||
                    (dr.Value.IsNK() && role.IsNK()) ||
                    (dr.Value.IsCrewmate() & role.IsCrewmate())
                    )
                {
                    rolesToAssign.RemoveAt(i);
                    rolesToAssign.Insert(0, dr.Value);
                    Logger.Info("Coverage role list：" + i + " " + role.ToString() + " => " + dr.Value, "Dev Role");
                    break;
                }
            }
        }

        var AllPlayer = Main.AllAlivePlayerControls.ToList();

        while (AllPlayer.Count > 0 && rolesToAssign.Count > 0)
        {
            PlayerControl delPc = null;
            foreach (var pc in AllPlayer.ToArray())
                foreach (var dr in Main.DevRole.Where(x => pc.PlayerId == x.Key).ToArray())
                {
                    if (dr.Key == PlayerControl.LocalPlayer.PlayerId && Main.EnableGM.Value) continue;
                    var id = rolesToAssign.IndexOf(dr.Value);
                    if (id == -1) continue;
                    RoleResult.Add(pc, rolesToAssign[id]);
                    Logger.Info($"Role Priority Assignment：{AllPlayer[0].GetRealName()} => {rolesToAssign[id]}", "CustomRoleSelector");
                    delPc = pc;
                    rolesToAssign.RemoveAt(id);
                    goto EndOfWhile;
                }

            var roleId = rd.Next(0, rolesToAssign.Count);
            RoleResult.Add(AllPlayer[0], rolesToAssign[roleId]);
            Logger.Info($"Role grouping：{AllPlayer[0].GetRealName()} => {rolesToAssign[roleId]}", "CustomRoleSelector");
            AllPlayer.RemoveAt(0);
            rolesToAssign.RemoveAt(roleId);

        EndOfWhile:;

            if (delPc != null)
            {
                AllPlayer.Remove(delPc);
                Main.DevRole.Remove(delPc.PlayerId);
            }
        }

        if (AllPlayer.Count > 0)
            Logger.Error("Role assignment error: There are players who have not been assigned role", "CustomRoleSelector");
        
        if (rolesToAssign.Count > 0)
            Logger.Error("Role assignment error: There is an unassigned role", "CustomRoleSelector");

    }

    public static int addScientistNum = 0;
    public static int addEngineerNum = 0;
    public static int addShapeshifterNum = 0;
    public static void CalculateVanillaRoleCount()
    {
        addEngineerNum = 0;
        addScientistNum = 0;
        addShapeshifterNum = 0;
        foreach (var role in AllRoles.ToArray())
        {
            switch (CustomRolesHelper.GetVNRole(role))
            {
                case CustomRoles.Scientist: addScientistNum++; break;
                case CustomRoles.Engineer: addEngineerNum++; break;
                case CustomRoles.Shapeshifter: addShapeshifterNum++; break;
            }
        }
    }

    public static List<CustomRoles> AddonRolesList = new();
    public static void SelectAddonRoles()
    {
        if (Options.CurrentGameMode == CustomGameMode.FFA) return;
        
        AddonRolesList = [];
        foreach (var cr in CustomRolesHelper.AllRoles)
        {
            CustomRoles role = (CustomRoles)Enum.Parse(typeof(CustomRoles), cr.ToString());
            if (!role.IsAdditionRole()) continue;
            if (role is CustomRoles.Madmate && Options.MadmateSpawnMode.GetInt() != 0) continue;
            if (role is CustomRoles.Lovers or CustomRoles.LastImpostor or CustomRoles.Workhorse) continue;

            if (GameStates.FungleIsActive) // The Fungle
            {
                if (role is CustomRoles.Mare) continue;
            }
            //else if (Options.IsActiveDleks) // Dleks
            //{
            //    if (role is CustomRoles.Nimble
            //        or CustomRoles.Burst
            //        or CustomRoles.Circumvent) continue;
            //}

            AddonRolesList.Add(role);
        }
    }
    public static void GhostAssignPatch(PlayerControl player)
    {
        var getplrRole = player.GetCustomRole();
        if (!CustomRolesHelper.IsCrewmate(getplrRole) || getplrRole == CustomRoles.Retributionist) return; //might make a RestrictGhostRole method later if needed

        List<CustomRoles> HauntedList = [];
        List<CustomRoles> RateHauntedList = [];
        CustomRoles ChosenRole = CustomRoles.NotAssigned;
        bool IsSetRole = false;

        foreach (var ghostRole in Options.CustomGhostRoleCounts.Keys) if (ghostRole.GetMode() == 2)
            {
                if (HauntedList.Contains(ghostRole) && !GhostAssign(ghostRole))
                    HauntedList.Remove(ghostRole);

                if (HauntedList.Contains(ghostRole) || !GhostAssign(ghostRole))
                    continue;

                HauntedList.Add(ghostRole);
            }
        foreach (var ghostRole in Options.CustomGhostRoleCounts.Keys) if (ghostRole.GetMode() == 1)
            {
                if (RateHauntedList.Contains(ghostRole) && !GhostAssign(ghostRole))
                    RateHauntedList.Remove(ghostRole);

                if (RateHauntedList.Contains(ghostRole) || !GhostAssign(ghostRole))
                    continue;

                RateHauntedList.Add(ghostRole);
            }

        if (HauntedList.Count > 0)
        {
            System.Random rnd = new System.Random();
            int randindx = rnd.Next(HauntedList.Count);
            ChosenRole = HauntedList[randindx];
            IsSetRole = true;
        }
        else if (RateHauntedList.Count > 0 && !IsSetRole)
        {
            System.Random rnd = new System.Random();
            int randindx = rnd.Next(HauntedList.Count);
            ChosenRole = RateHauntedList[randindx];
        }

        switch (ChosenRole)
        {
            case CustomRoles.Warden:
                player.RpcSetRole(RoleTypes.GuardianAngel);
                player.RpcSetRoleDesync(RoleTypes.GuardianAngel, player.GetClientId());
                player.RpcSetCustomRole(CustomRoles.Warden);
                break;

            default:
                break;
        }
    }
    public static bool GhostAssign(CustomRoles role)
    {
        int getCount = Options.CustomGhostRoleCounts[role].GetInt();

        if (getCount > 0)
        {
            getCount--;
            return true;
        }
        return false;
    }
}