using Bloonchipper.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.MainMenuWorld.Scripts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.LevelUp;
using Il2CppAssets.Scripts.Unity.UI_New.Main.HeroSelect;
using Il2CppAssets.Scripts.Unity.UI_New.Main.WorldItems;
using Il2CppAssets.Scripts.Utils;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Il2CppTask = Il2CppSystem.Threading.Tasks.Task;

[assembly: MelonInfo(typeof(Bloonchipper.Mod), Bloonchipper.Mod.Name, Bloonchipper.Mod.Version, Bloonchipper.Mod.Author)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonColor(255, 49, 158, 252)]
[assembly: MelonAuthorColor(255, 255, 104, 0)]

// TODO: change model so that back wheel is sideways
namespace Bloonchipper {

    /// <summary>
    /// The main entrypoint for the mod and where all the methods are patched
    /// </summary>
    [HarmonyPatch]
    public sealed class Mod : MelonMod {
        public const string Name = "Bloonchipper";
        public const string Version = "0.1.0";
        public const string Author = "Baydock";
        public const string Desc = "Adds the much requested Bloonchipper to BloonsTD6. Equipped with 3 5-tiered paths, a paragon upgrade, and full 3d models, Bloonchipper is here to chip some bloons.";

        public static MelonLogger.Instance Logger { get; private set; }

        public static GameModel GameModel { get; private set; }

        public override void OnInitializeMelon() {
            Logger = LoggerInstance;
        }

        /// <summary>
        /// Adds all tower and upgrade data to the GameModel so that it appears in game
        /// </summary>
        [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
        [HarmonyPostfix]
        public static void Load(ref GameModel __result) {
            GameModel = __result;

            AddTower(Bloonchipper.Details, Bloonchipper.After,
                new TowerModel[] {
                    Bloonchipper.Tower000,
                    Bloonchipper.Tower010,
                    Bloonchipper.Tower020,
                    Bloonchipper.Tower030,
                    Bloonchipper.Tower100,
                    Bloonchipper.Tower110,
                    Bloonchipper.Tower120,
                    Bloonchipper.Tower130,
                    Bloonchipper.Tower200,
                    Bloonchipper.Tower210,
                    Bloonchipper.Tower220,
                    Bloonchipper.Tower230,
                    Bloonchipper.Tower300,
                    Bloonchipper.Tower310,
                    Bloonchipper.Tower320,
                    Bloonchipper.Tower400,
                    Bloonchipper.Tower410,
                    Bloonchipper.Tower420
                },
                new UpgradeModel[] {
                    Bloonchipper.Upgrade010,
                    Bloonchipper.Upgrade020,
                    Bloonchipper.Upgrade030,
                    Bloonchipper.Upgrade100,
                    Bloonchipper.Upgrade200,
                    Bloonchipper.Upgrade300,
                    Bloonchipper.Upgrade400
                });
            Bloonchipper.AddLocalization();
        }

        private static void AddTower(ShopTowerDetailsModel details, string after, TowerModel[] towers, UpgradeModel[] upgrades) {
            int index = 0;
            bool foundIndex = false;
            for (int i = 0; i < GameModel.towerSet.Length; i++) {
                if (foundIndex) {
                    GameModel.towerSet[i].towerIndex++;
                } else if (GameModel.towerSet[i].towerId.Equals(after)) {
                    foundIndex = true;
                    index = i + 1;
                }
            }
            if (!foundIndex) index = GameModel.towerSet.Length;

            details.towerIndex = index;
            GameModel.towerSet = GameModel.towerSet.Insert(index, details);
            GameModel.childDependants.Add(details);

            TowerType.towers = TowerType.towers.Insert(index, details.towerId);

            GameModel.towers = GameModel.towers.Append(towers);
            GameModel.childDependants.Add(towers);

            GameModel.upgrades = GameModel.upgrades.Append(upgrades);
            GameModel.childDependants.Add(upgrades);

            GameModel.UpdateUpgradeNames();
        }

        /// <summary>
        /// Accesses the player's save when loaded, for unlocking towers and upgrades
        /// </summary>
        [HarmonyPatch(typeof(PlayerService), nameof(PlayerService.Load))]
        [HarmonyPostfix]
        public static void UnlockModdedTowers(PlayerService __instance, Il2CppTask __result) {
            __result.ContinueWith(new System.Action<Il2CppTask>(t => {
                Btd6Player player = __instance.Player;
                ProfileModel profile = player.Data;

                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(0, 1, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(0, 2, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(0, 3, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(1, 0, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(2, 0, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(3, 0, 0)]);
                profile.acquiredUpgrades.AddIfNotPresent(Bloonchipper.Names[(4, 0, 0)]);

                // For testing
                //profile.unlockedTowers.Remove(Bloonchipper.Name);
                foreach (TowerModel tower in Game.instance.model.GetTowersWithBaseId(Bloonchipper.Name))
                    player.AddInstaTower(Bloonchipper.Name, tower.tiers, 1);
            }));
        }

        #region Bloonchipper assets

        /// <summary>
        /// For loading 3d models into the game
        /// </summary>
        [HarmonyPatch(typeof(Factory.__c__DisplayClass21_0), nameof(Factory.__c__DisplayClass21_0._CreateAsync_b__0))]
        [HarmonyPrefix]
        public static bool LoadProtos(ref Factory.__c__DisplayClass21_0 __instance, ref UnityDisplayNode prototype) {
            string objectId = __instance.objectId.guidRef;
            if (!string.IsNullOrEmpty(objectId)) {
                UnityDisplayNode display = Bloonchipper.LoadDisplay(objectId);
                if (display is not null) {
                    display.transform.parent = __instance.__4__this.DisplayRoot;
                    prototype = display;
                    __instance.__4__this.active.Add(display);
                    __instance.onComplete?.Invoke(display);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// For loading sprites into the game
        /// </summary>
        [HarmonyPatch(typeof(SpriteAtlas), nameof(SpriteAtlas.GetSprite))]
        [HarmonyPrefix]
        public static bool LoadSprites(ref Sprite __result, string name) {
            if (!string.IsNullOrEmpty(name)) {
                Sprite sprite = Bloonchipper.LoadSprite(name);
                if (sprite is not null) {
                    __result = sprite;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// For highlighting the tower 3d model
        /// </summary>
        [HarmonyPatch(typeof(UnityDisplayNode), nameof(UnityDisplayNode.Hilight), MethodType.Setter)]
        [HarmonyPostfix]
        public static void SetHilight(ref UnityDisplayNode __instance, bool value) {
            if (__instance.genericRenderers.Length < 1)
                __instance.RecalculateGenericRenderers();
            if (__instance.genericRenderers.Length < 1) // Easy win
                return;

            Material material = __instance.genericRenderers[0].material;
            if (material.HasProperty($"_{Bloonchipper.Name}")) {
                material.SetFloat("_Selected", value ? 1 : 0);
                material.SetInt("_ZTest", (int)(value ? CompareFunction.Always : CompareFunction.Less));
            }
        }

        #endregion

        #region Bloonchipper InGame

        /// <summary>
        /// Attaches barrels to the bloonchippers
        /// </summary>
        [HarmonyPatch(typeof(Tower), nameof(Tower.Initialise))]
        [HarmonyPostfix]
        public static void InitialiseBloonchipper(ref Tower __instance) {
            if (__instance.towerModel.baseId.Equals(Bloonchipper.Name))
                BloonchipperBarrel.AttachTo(__instance);
        }

        /// <summary>
        /// Determines when the bloonchipper can suck a bloon
        /// </summary>
        [HarmonyPatch(typeof(Attack), nameof(Attack.HasValidTarget))]
        [HarmonyPostfix]
        public static void EnforceCorrectSuck(ref Attack __instance, ref bool __result) {
            if (BloonchipperBarrel.TryGetBloonchipper(__instance.tower, out BloonchipperBarrel bloonchipper)) {
                __result = bloonchipper.CanSuckBloon(__instance.target.bloon);
            } else if (__instance.target.bloon is not null && SuckedBloon.IsUnreachable(__instance.target.bloon))
                __result = false;
        }

        /// <summary>
        /// On fire, takes each bloon target and sucks them
        /// </summary>
        [HarmonyPatch(typeof(Weapon), nameof(Weapon.Emit))]
        [HarmonyPostfix]
        public static void SuckBloon(Tower owner, SizedList<Projectile> created) {
            if (BloonchipperBarrel.TryGetBloonchipper(owner, out BloonchipperBarrel bloonchipper)) {
                foreach (Projectile proj in created.list) {
                    Bloon bloon = proj.target.bloon;
                    if (!SuckedBloon.IsBeingSucked(bloon))
                        SuckedBloon.AttachTo(bloon, bloonchipper);
                }
            }
        }

        /// <summary>
        /// Makes projectiles not target unreachable bloons
        /// </summary>
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.FilterBloon))]
        [HarmonyPostfix]
        public static void DontTargetBloonInside(Bloon bloon, ref bool __result) {
            if (SuckedBloon.IsUnreachable(bloon))
                __result = false;
        }

        /// <summary>
        /// Makes projectiles not collide with unreachable bloons
        /// </summary>
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.IsCollisionValid))]
        [HarmonyPostfix]
        public static void DontCollideBloonInside(Bloon bloon, ref bool __result) {
            if (SuckedBloon.IsUnreachable(bloon))
                __result = false;
        }

        /// <summary>
        /// Don't allow blow/knockback to affect sucked bloons
        /// </summary>
        [HarmonyPatch(typeof(Bloon), nameof(Bloon.AddMutator))]
        [HarmonyPrefix]
        public static bool DontChangePositionWhenSucked(ref Bloon __instance, BehaviorMutator mutator) {
            if (SuckedBloon.IsBeingSucked(__instance) && (mutator.id.Contains("Wind") || mutator.id.Contains("Knockback")))
                return false;
            return true;
        }

        /// <summary>
        /// Makes sure children of sucked bloons are sucked correctly as well
        /// </summary>
        [HarmonyPatch(typeof(SpawnChildren), nameof(SpawnChildren.CreatedChildren))]
        [HarmonyPostfix]
        public static void DontTeleportSpawnedChildren(ref SpawnChildren __instance, SizedList<Bloon> childernCreatedIn /* This is actually mispelled by nk lol */) {
            if (SuckedBloon.IsBeingSucked(__instance.bloon)) {
                foreach (Bloon child in childernCreatedIn.list) {
                    if (child.Id != __instance.bloon.Id)
                        SuckedBloon.AttachTo(child, null, __instance.bloon);
                }
            }
        }

        /// <summary>
        /// Removes all barrels and sucked bloons when the map resets
        /// </summary>
        [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.ClearMap))]
        [HarmonyPostfix]
        public static void ClearMap() {
            BloonchipperBarrel.ClearAll();
            SuckedBloon.ClearAll();
        }

        #endregion

        #region Bloonchipper as a gift

        /// <summary>
        /// Generates new giftbox for the bloonchipper
        /// </summary>
        [HarmonyPatch(typeof(MainMenuWorldChoreographer), nameof(MainMenuWorldChoreographer.Start))]
        [HarmonyPostfix]
        public static void AddBloonchipperGift(ref MainMenuWorldChoreographer __instance) {
            Transform mainMenuWorld = __instance.gameObject.transform;
            GameObject dartlingGift = mainMenuWorld.Find("Interactive")?.Find("GiftBox")?.gameObject;
            if (dartlingGift is not null) {
                GameObject bloonchipperGiftObject = Object.Instantiate(dartlingGift, dartlingGift.transform.parent);

                OpenGiftbox bloonchipperGift = bloonchipperGiftObject.GetComponent<OpenGiftbox>();

                bloonchipperGiftObject.name = Bloonchipper.GiftName;
                Bloonchipper.ModifyGift(bloonchipperGift);

                bloonchipperGift.OnEnable();
            }
        }

        /// <summary>
        /// Makes the new bloonchipper giftbox interact as a bloonchipper gift
        /// </summary>
        [HarmonyPatch(typeof(OpenGiftbox), nameof(OpenGiftbox.OnInteract))]
        [HarmonyPrefix]
        public static bool InteractWithBloonchipperGift(OpenGiftbox __instance) {
            if (__instance.gameObject.name.Equals(Bloonchipper.GiftName) && __instance.container.active) {
                if (Game.Player.Data.rank.ValueInt < Bloonchipper.LevelUnlock) {
                    __instance.dialogText.text = LocalizationManager.Instance.Format("Unlock at level", new Il2CppReferenceArray<Il2CppSystem.Object>(1) { [0] = Bloonchipper.LevelUnlock.ToString() });
                    __instance.dialogObject.SetActive(true);
                    Game.instance.audioFactory.PlaySoundFromUnity(__instance.clickSound, null, "FX", 1, 1);
                } else {
                    InteractionChecker checker = __instance.GetComponentInParent<InteractionChecker>();

                    checker.enabled = false;
                    Bloonchipper.OpenGift(onConfirm: () => {
                        MenuManager.instance.OpenMenu("DartlingGunnerUnlockUI", Bloonchipper.Name);
                        Game.instance.audioFactory.PlaySoundFromUnity(__instance.openChestSound, null, "FX", 1, 1);
                        Game.Player.Data.unlockedTowers.Add(Bloonchipper.Name);
                    }, onComplete: () => {
                        checker.enabled = true; // To get around being clickable while the menu is open
                    });
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Updates the bloonchipper giftbox visibility and animations when enabled
        /// </summary>
        [HarmonyPatch(typeof(OpenGiftbox), nameof(OpenGiftbox.OnEnable))]
        [HarmonyPrefix]
        public static bool OnEnableBloonchipperGift(ref OpenGiftbox __instance) {
            if (__instance.gameObject.name.Equals(Bloonchipper.GiftName)) {
                if (Game.Player.Data.unlockedTowers.Contains(Bloonchipper.Name)) {
                    __instance.container.SetActive(false);
                } else {
                    if (Game.Player.Data.rank.ValueInt < Bloonchipper.LevelUnlock)
                        __instance.animator.SetTrigger("LockBox");
                    else
                        __instance.animator.SetTrigger("ReadyToOpenBox");

                    __instance.container.SetActive(true);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Disables giftbox updates so that I have complete control over it
        /// </summary>
        [HarmonyPatch(typeof(OpenGiftbox), nameof(OpenGiftbox.Update))]
        [HarmonyPrefix]
        public static bool DisablePrimaryUpdate(ref OpenGiftbox __instance) => !__instance.gameObject.name.Equals(Bloonchipper.GiftName);

        /// <summary>
        /// Disables giftbox updates so that I have complete control over it
        /// </summary>
        [HarmonyPatch(typeof(OpenGiftbox), nameof(OpenGiftbox.UpdateGiftboxState))]
        [HarmonyPrefix]
        public static bool DisableSecondaryUpdate(ref OpenGiftbox __instance) => !__instance.gameObject.name.Equals(Bloonchipper.GiftName);

        /// <summary>
        /// Changes the tower unlock splash screen to a bloonchipper one
        /// </summary>
        [HarmonyPatch(typeof(GiftboxUnlockSplash), nameof(GiftboxUnlockSplash.Open))]
        [HarmonyPostfix]
        public static void BloonchipperUnlockSplashOpen(ref GiftboxUnlockSplash __instance, Il2CppSystem.Object data) {
            string message = data?.ToString();
            if (message is not null && message.Equals(Bloonchipper.Name))
                Bloonchipper.ModifyGiftSplash(__instance);
        }

        /// <summary>
        /// Removes bloonchipper from the list of towers able to be unlocked on level up
        /// </summary>
        [HarmonyPatch(typeof(TowerUnlockScreen), nameof(TowerUnlockScreen.GetTowerList))]
        [HarmonyPostfix]
        public static void OnlyUnlockBloonchipperFromGift(ref Il2CppSystem.Collections.Generic.List<TowerModel> __result) => __result.RemoveAll(new System.Func<TowerModel, bool>(t => t.baseId.Equals(Bloonchipper.Name)));

        #endregion
    }
}
