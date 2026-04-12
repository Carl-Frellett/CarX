using CarX.API.Enums;
using CarX.API.Extensions;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarX.API.Features
{
    public class Player
    {
        private ReferenceHub referenceHub;

        public Player(ReferenceHub referenceHub) => ReferenceHub = referenceHub;

        public Player(GameObject gameObject) => ReferenceHub = ReferenceHub.GetHub(gameObject);

        public static Dictionary<GameObject, Player> Dictionary { get; } = new Dictionary<GameObject, Player>();

        public static IEnumerable<Player> List => Dictionary.Values;

        public static Dictionary<string, Player> UserIdsCache { get; } = new Dictionary<string, Player>();
        public static Dictionary<int, Player> IdsCache { get; } = new Dictionary<int, Player>();

        public GameObject GameObject { get; private set; }

        public AmmoBox Ammo { get; set; }

        public Inventory Inventory { get; private set; }

        public Transform CameraTransform { get; private set; }
        public GrenadeManager GrenadeManager { get; private set; }

        public ReferenceHub ReferenceHub
        {
            get => referenceHub;
            private set
            {
                referenceHub = value ?? throw new NullReferenceException("Player's ReferenceHub cannot be null!");

                GameObject = value.gameObject;
                Ammo = value.ammoBox;
                Inventory = value.inventory;
                GrenadeManager = value.GetComponent<GrenadeManager>();
            }
        }
        public int Id
        {
            get => ReferenceHub.queryProcessor.NetworkPlayerId;
            set => ReferenceHub.queryProcessor.NetworkPlayerId = value;
        }
        public string UserId
        {
            get => ReferenceHub.characterClassManager.NetworkSteamId;
        }
        public string Nickname => ReferenceHub.nicknameSync.NetworkmyNick;
        public bool DoNotTrack => ReferenceHub.serverRoles.DoNotTrack;
        public bool RemoteAdminAccess => ReferenceHub.serverRoles.RemoteAdmin;
        public bool IsOverwatchEnabled
        {
            get => ReferenceHub.serverRoles.OverwatchEnabled;
            set => ReferenceHub.serverRoles.SetOverwatchStatus(value);
        }

        public Vector3 Position
        {
            get => ReferenceHub.plyMovementSync.position;
            set => ReferenceHub.plyMovementSync.position = value;
        }

        public RoleType Role
        {
            get => ReferenceHub.characterClassManager.curClass;
            set => SetRole(value);
        }
        public Team Team => Role.GetTeam();

        public Color RoleColor => Role.GetColor();

        public string IPAddress
        {
            get => ReferenceHub.queryProcessor.ipAddress;
            set => ReferenceHub.queryProcessor.ipAddress = value;
        }

        //public bool NoClipEnabled
        //{
        //  todo
        //}

        public Side Side => Team.GetSide();
        public NetworkConnection Connection => ReferenceHub.netIdentity.connectionToClient;
        public bool IsHost => ReferenceHub.characterClassManager.isLocalPlayer;
        public bool IsAlive => !IsDead;
        public bool IsDead => Team == Team.RIP;

        public bool IsBypassModeEnabled
        {
            get => ReferenceHub.serverRoles.BypassMode;
            set => ReferenceHub.serverRoles.BypassMode = value;
        }
        public bool IsMuted
        {
            get => ReferenceHub.characterClassManager.NetworkMuted;
            set => ReferenceHub.characterClassManager.NetworkMuted = value;
        }
        public bool IsIntercomMuted
        {
            get => ReferenceHub.characterClassManager.NetworkIntercomMuted;
            set => ReferenceHub.characterClassManager.NetworkIntercomMuted = value;
        }
        public bool IsGodModeEnabled
        {
            get => ReferenceHub.characterClassManager.GodMode;
            set => ReferenceHub.characterClassManager.GodMode = value;
        }
        public int MaxHealth
        {
            get => ReferenceHub.playerStats.maxHP;
            set => ReferenceHub.playerStats.maxHP = value;
        }
        public float Health
        {
            get => ReferenceHub.playerStats.health;
            set
            {
                ReferenceHub.playerStats.health = value;
                if (value > MaxHealth)
                    MaxHealth = (int)value;
            }
        }
        public int MaxAdrenalineHealth
        {
            get => ReferenceHub.playerStats.maxArtificialHealth;
            set => ReferenceHub.playerStats.maxArtificialHealth = value;
        }
        public float AdrenalineHealth
        {
            get => ReferenceHub.playerStats.artificialHealth;
            set
            {
                ReferenceHub.playerStats.artificialHealth = value;
                if (value > MaxAdrenalineHealth)
                    MaxAdrenalineHealth = (int)value;
            }
        }
        public Inventory.SyncItemInfo CurrentItem
        {
            get => Inventory.GetItemInHand();
            set => Inventory.SetCurItem(Inventory.GetItemInHand().id,value.id);
        }
        public int CurrentItemIndex => Inventory.GetItemIndex();
        public Scp079PlayerScript.Ability079[] Abilities
        {
            get => ReferenceHub.scp079PlayerScript?.abilities;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.abilities = value;
            }
        }
        public Scp079PlayerScript.Level079[] Levels
        {
            get => ReferenceHub.scp079PlayerScript?.levels;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.levels = value;
            }
        }
        public string Speaker
        {
            get => ReferenceHub.scp079PlayerScript?.Speaker;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.Speaker = value;
            }
        }
        public SyncList<string> LockedDoors
        {
            get => ReferenceHub.scp079PlayerScript?.lockedDoors;
            set
            {
                if (ReferenceHub.scp079PlayerScript != null)
                    ReferenceHub.scp079PlayerScript.lockedDoors = value;
            }
        }
        public float Experience
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Exp : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null)
                    return;

                ReferenceHub.scp079PlayerScript.Exp = value;
                ReferenceHub.scp079PlayerScript.OnExpChange();
            }
        }
        public int Level
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Lvl : int.MinValue;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null || ReferenceHub.scp079PlayerScript.Lvl == value)
                    return;

                ReferenceHub.scp079PlayerScript.Lvl = value;

                ReferenceHub.scp079PlayerScript.TargetLevelChanged(Connection, value);
            }
        }
        public float MaxEnergy
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.NetworkmaxMana : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null)
                    return;

                ReferenceHub.scp079PlayerScript.NetworkmaxMana = value;
                ReferenceHub.scp079PlayerScript.levels[Level].maxMana = value;
            }
        }
        public float Energy
        {
            get => ReferenceHub.scp079PlayerScript != null ? ReferenceHub.scp079PlayerScript.Mana : float.NaN;
            set
            {
                if (ReferenceHub.scp079PlayerScript == null)
                    return;

                ReferenceHub.scp079PlayerScript.Mana = value;
            }
        }
        public bool IsStaffBypassEnabled => ReferenceHub.serverRoles.BypassStaff;
        public string GroupName
        {
            get => ServerStatic.PermissionsHandler._members.TryGetValue(UserId, out string groupName) ? groupName : null;
            set => ServerStatic.PermissionsHandler._members[UserId] = value;
        }
        public string RankColor
        {
            get => ReferenceHub.serverRoles.NetworkMyColor;
            set => ReferenceHub.serverRoles.SetColor(ReferenceHub.serverRoles.NetworkMyColor,value);
        }
        public string RankName
        {
            get => ReferenceHub.serverRoles.NetworkMyText;
            set => ReferenceHub.serverRoles.SetText(ReferenceHub.serverRoles.NetworkMyText,value);
        }
        public void SetRank(string name, UserGroup group)
        {
            if (ServerStatic.GetPermissionsHandler()._groups.ContainsKey(name))
            {
                ServerStatic.GetPermissionsHandler()._groups[name].BadgeColor = group.BadgeColor;
                ServerStatic.GetPermissionsHandler()._groups[name].BadgeText = name;
                ServerStatic.GetPermissionsHandler()._groups[name].HiddenByDefault = !group.Cover;
                ServerStatic.GetPermissionsHandler()._groups[name].Cover = group.Cover;

                ReferenceHub.serverRoles.SetGroup(ServerStatic.GetPermissionsHandler()._groups[name], false, false, group.Cover);
            }
            else
            {
                ServerStatic.GetPermissionsHandler()._groups.Add(name, group);

                ReferenceHub.serverRoles.SetGroup(group, false, false, group.Cover);
            }

            if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(UserId))
                ServerStatic.GetPermissionsHandler()._members[UserId] = name;
            else
                ServerStatic.GetPermissionsHandler()._members.Add(UserId, name);
        }
        public void SetRole(RoleType newRole, bool lite = false, bool isEscaped = false)
        {
            ReferenceHub.characterClassManager.SetPlayersClass(newRole, GameObject, lite, isEscaped);
        }

        public void DropItem(Inventory.SyncItemInfo item)
        {
            Inventory.SetPickup(item.id, item.durability, Position, Inventory.camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
            Inventory.items.Remove(item);
        }

        public void RemoveItem(Inventory.SyncItemInfo item) => Inventory.items.Remove(item);

        public void RemoveItem() => Inventory.items.Remove(ReferenceHub.inventory.GetItemInHand());
        public void SendConsoleMessage(string message, string color) => SendConsoleMessage(this, message, color);

        public void SendConsoleMessage(Player target, string message, string color) => ReferenceHub.characterClassManager.TargetConsolePrint(target.Connection, message, color);

        public void Disconnect(string reason = null) => global::ServerConsole.Disconnect(GameObject, string.IsNullOrEmpty(reason) ? string.Empty : reason);

        public void Hurt(float damage, DamageTypes.DamageType damageType = default, string attackerName = "WORLD", int attackerId = 0)
        {
            ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, attackerName, damageType ?? DamageTypes.None, attackerId), GameObject);
        }

        public void Hurt(float damage, Player attacker, DamageTypes.DamageType damageType = default) => Hurt(damage, damageType, attacker?.Nickname, attacker?.Id ?? 0);

        public void Kill(DamageTypes.DamageType damageType = default) => Hurt(-1f, damageType);

        public void Ban(int duration, string reason, string issuer = "Server") => Server.BanPlayer.BanUser(GameObject, duration, reason, issuer, false);

        public void Kick(string reason, string issuer = "Server") => Ban(0, reason, issuer);

        public bool BadgeHidden
        {
            get => string.IsNullOrEmpty(ReferenceHub.serverRoles.HiddenBadge);
            set
            {
                if (value)
                    ReferenceHub.characterClassManager.CmdRequestHideTag();
                else
                    ReferenceHub.characterClassManager.CmdRequestShowTag(false);
            }
        }
        public IEnumerator<float> BlinkTag()
        {
            yield return Timing.WaitForOneFrame;

            BadgeHidden = !BadgeHidden;

            yield return Timing.WaitForOneFrame;

            BadgeHidden = !BadgeHidden;
        }
        public void Broadcast(ushort duration, string message, bool ismonospaced)
        {
            Server.Broadcast.TargetAddElement(Connection, message, duration, ismonospaced);
        }

        public void ClearBroadcasts() => Server.Broadcast.TargetClearElements(Connection);

        public void AddItem(int itemType) => Inventory.AddNewItem(itemType);

        public void AddItem(Inventory.SyncItemInfo item) => Inventory.AddNewItem(item.id, item.durability, item.modSight, item.modBarrel, item.modOther);

        public void ClearInventory() => Inventory.Clear();

        public void DropItems() => Inventory.ServerDropAll();

        public static Player Get(ReferenceHub referenceHub) => referenceHub == null ? null : Get(referenceHub.gameObject);
        public static Player Get(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            Dictionary.TryGetValue(gameObject, out Player player);

            return player;
        }
        public static Player Get(int id)
        {
            if (IdsCache.TryGetValue(id, out Player player) && player?.ReferenceHub != null)
                return player;

            foreach (Player playerFound in Dictionary.Values)
            {
                if (playerFound.Id != id)
                    continue;

                IdsCache[id] = playerFound;

                return playerFound;
            }

            return null;
        }
        public static Player Get(string args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args))
                    return null;

                if (UserIdsCache.TryGetValue(args, out Player playerFound) && playerFound?.ReferenceHub != null)
                    return playerFound;

                if (int.TryParse(args, out int id))
                    return Get(id);

                if (args.EndsWith("@steam") || args.EndsWith("@discord") || args.EndsWith("@northwood") || args.EndsWith("@patreon"))
                {
                    foreach (Player player in Dictionary.Values)
                    {
                        if (player.UserId == args)
                        {
                            playerFound = player;
                            break;
                        }
                    }
                }
                else
                {
                    int maxNameLength = 31, lastnameDifference = 31;
                    string firstString = args.ToLower();

                    foreach (Player player in Dictionary.Values)
                    {
                        if (!player.Nickname.Contains(args, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (firstString.Length < maxNameLength)
                        {
                            int x = maxNameLength - firstString.Length;
                            int y = maxNameLength - player.Nickname.Length;
                            string secondString = player.Nickname;

                            for (int i = 0; i < x; i++)
                                firstString += "z";

                            for (int i = 0; i < y; i++)
                                secondString += "z";

                            int nameDifference = firstString.GetDistance(secondString);
                            if (nameDifference < lastnameDifference)
                            {
                                lastnameDifference = nameDifference;
                                playerFound = player;
                            }
                        }
                    }
                }

                if (playerFound != null)
                    UserIdsCache[args] = playerFound;

                return playerFound;
            }
            catch (Exception exception)
            {
                Log.Error($"Player.Get error: {exception}");
                return null;
            }
        }
    }
}
