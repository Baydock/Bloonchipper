using Bloonchipper.DataTypes;
using Bloonchipper.Utils;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.Main.HeroSelect;
using Il2CppAssets.Scripts.Unity.UI_New.Main.WorldItems;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NKVector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;
using Resources = Bloonchipper.Properties.Resources;

namespace Bloonchipper {
    internal static class Bloonchipper {
        public const string Name = "Bloonchipper";
        public const string GiftName = $"{Name}Gift";
        public static System.Collections.Generic.Dictionary<Tiers, string> Names { get; } = new() {
            [(0, 0, 0)] = Name,
            [(0, 1, 0)] = "Long Range Suck",
            [(0, 2, 0)] = "Bigger Blades",
            [(0, 3, 0)] = "Triple Barrel",
            [(1, 0, 0)] = "Prioritize Suction",
            [(2, 0, 0)] = "Heavy Duty Suction",
            [(3, 0, 0)] = "Dual Layer Blades",
            [(4, 0, 0)] = "Super Wide Funnel"
        };

        public const string After = "EngineerMonkey";

        private const TowerSet Set = TowerSet.Support;

        private const string ResourcePrefix = $"{Name}.";
        private const string PortraitPrefix = "Portraits.";
        private const string UpgradeIconPrefix = "UpgradeIcons.";
        private const string InstaIconPrefix = "Instas.";

        private const int Cost = 750;
        private const int Radius = 11;

        private const int Range000 = 40;
        private const int Range010 = 50;

        public const int LevelUnlock = 30;

        private static readonly AssetBundle AssetBundle = Resources.LoadAssetBundle("bloonchipper");

        #region Upgrades and Towers

        public static ShopTowerDetailsModel Details => new(Name, -1, 4, 3, 0, -1, 0, null);
        public static TowerModel Tower000 => GetTower((0, 0, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 1, 0)], $"{Name}-010"),
            [1] = new UpgradePathModel(Names[(1, 0, 0)], $"{Name}-100")
        });

        public static UpgradeModel Upgrade010 => GetUpgrade(Path.Mid, 1, 300);
        public static TowerModel Tower010 => GetTower((0, 1, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 2, 0)], $"{Name}-020"),
            [1] = new UpgradePathModel(Names[(1, 0, 0)], $"{Name}-110")
        });

        public static UpgradeModel Upgrade020 => GetUpgrade(Path.Mid, 2, 600);
        public static TowerModel Tower020 => GetTower((0, 2, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 3, 0)], $"{Name}-030"),
            [1] = new UpgradePathModel(Names[(1, 0, 0)], $"{Name}-120")
        });

        public static UpgradeModel Upgrade030 => GetUpgrade(Path.Mid, 3, 4500);
        public static TowerModel Tower030 => GetTower((0, 3, 0), new Il2CppReferenceArray<UpgradePathModel>(1) {
            [0] = new UpgradePathModel(Names[(1, 0, 0)], $"{Name}-130")
        });

        public static UpgradeModel Upgrade100 => GetUpgrade(Path.Top, 1, 150);
        public static TowerModel Tower100 => GetTower((1, 0, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 1, 0)], $"{Name}-110"),
            [1] = new UpgradePathModel(Names[(2, 0, 0)], $"{Name}-200")
        });

        public static TowerModel Tower110 => GetTower((1, 1, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 2, 0)], $"{Name}-120"),
            [1] = new UpgradePathModel(Names[(2, 0, 0)], $"{Name}-210")
        });

        public static TowerModel Tower120 => GetTower((1, 2, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 3, 0)], $"{Name}-130"),
            [1] = new UpgradePathModel(Names[(2, 0, 0)], $"{Name}-220")
        });

        public static TowerModel Tower130 => GetTower((1, 3, 0), new Il2CppReferenceArray<UpgradePathModel>(1) {
            [0] = new UpgradePathModel(Names[(2, 0, 0)], $"{Name}-230")
        });

        public static UpgradeModel Upgrade200 => GetUpgrade(Path.Top, 2, 300);
        public static TowerModel Tower200 => GetTower((2, 0, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 1, 0)], $"{Name}-210"),
            [1] = new UpgradePathModel(Names[(3, 0, 0)], $"{Name}-300")
        });

        public static TowerModel Tower210 => GetTower((2, 1, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 2, 0)], $"{Name}-220"),
            [1] = new UpgradePathModel(Names[(3, 0, 0)], $"{Name}-310")
        });

        public static TowerModel Tower220 => GetTower((2, 2, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 3, 0)], $"{Name}-230"),
            [1] = new UpgradePathModel(Names[(3, 0, 0)], $"{Name}-320")
        });

        public static TowerModel Tower230 => GetTower((2, 3, 0), new Il2CppReferenceArray<UpgradePathModel>(0));

        public static UpgradeModel Upgrade300 => GetUpgrade(Path.Top, 3, 850);
        public static TowerModel Tower300 => GetTower((3, 0, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 1, 0)], $"{Name}-310"),
            [1] = new UpgradePathModel(Names[(4, 0, 0)], $"{Name}-400")
        });

        public static TowerModel Tower310 => GetTower((3, 1, 0), new Il2CppReferenceArray<UpgradePathModel>(2) {
            [0] = new UpgradePathModel(Names[(0, 2, 0)], $"{Name}-320"),
            [1] = new UpgradePathModel(Names[(4, 0, 0)], $"{Name}-410")
        });

        public static TowerModel Tower320 => GetTower((3, 2, 0), new Il2CppReferenceArray<UpgradePathModel>(1) {
            [0] = new UpgradePathModel(Names[(4, 0, 0)], $"{Name}-420")
        });

        public static UpgradeModel Upgrade400 => GetUpgrade(Path.Top, 4, 4500);
        public static TowerModel Tower400 => GetTower((4, 0, 0), new Il2CppReferenceArray<UpgradePathModel>(1) {
            [0] = new UpgradePathModel(Names[(0, 1, 0)], $"{Name}-410")
        });

        public static TowerModel Tower410 => GetTower((4, 1, 0), new Il2CppReferenceArray<UpgradePathModel>(1) {
            [0] = new UpgradePathModel(Names[(0, 2, 0)], $"{Name}-420")
        });

        public static TowerModel Tower420 => GetTower((4, 2, 0), new Il2CppReferenceArray<UpgradePathModel>(0));

        #endregion

        private static TowerModel GetTower(Tiers t, Il2CppReferenceArray<UpgradePathModel> upgrades) {
            string tiersString = t.ToString();
            string name = t.IsBase ? Name : $"{Name}-{tiersString}";
            string display = $"{ResourcePrefix}{tiersString}";

            string portrait;
            if (t.Bot > t.Top && t.Bot > t.Mid)
                portrait = $"00{t.Bot}";
            else if (t.Top > t.Mid && t.Top >= t.Bot)
                portrait = $"{t.Top}00";
            else
                portrait = $"0{t.Mid}0";
            portrait = $"Ui[{ResourcePrefix}{PortraitPrefix}{portrait}]";

            List<string> appliedUpgrades = new();
            for (byte i = 0; i < t.Top; i++)
                appliedUpgrades.Add(Names[(i, 0, 0)]);
            for (byte i = 0; i < t.Mid; i++)
                appliedUpgrades.Add(Names[(0, i, 0)]);
            for (byte i = 0; i < t.Bot; i++)
                appliedUpgrades.Add(Names[(0, 0, i)]);

            int range = t.Mid > 0 ? Range010 : Range000;

            TowerModel bloonchipper = Mod.GameModel.GetTowerFromId("BombShooter").CloneCast();
            bloonchipper.name = name;
            bloonchipper.baseId = Name;
            bloonchipper.towerSet = Set;
            bloonchipper.display = new() { guidRef = display };
            bloonchipper.cost = Cost;
            bloonchipper.radius = Radius;
            bloonchipper.range = range;
            bloonchipper.tier = t.Tier;
            bloonchipper.tiers = t.ToIl2CppArray();
            bloonchipper.appliedUpgrades = appliedUpgrades.ToArray().Cast<Il2CppStringArray>();
            bloonchipper.upgrades = upgrades;
            bloonchipper.icon = new() { guidRef = $"Ui[{ResourcePrefix}{PortraitPrefix}000]" };
            bloonchipper.portrait = new() { guidRef = portrait };
            bloonchipper.instaIcon = new() { guidRef = $"Ui[{ResourcePrefix}{InstaIconPrefix}000]" }; // TODO: make custom insta icon
            bloonchipper.mods = new Il2CppReferenceArray<ApplyModModel>(0); // TODO: add monkey knowledge
            bloonchipper.footprint = new CircleFootprintModel("", Radius, false, false, false);
            bloonchipper.behaviors = new Model[] {
                bloonchipper.FirstBehavior<CreateSoundOnTowerPlaceModel>(),
                bloonchipper.FirstBehavior<CreateSoundOnSellModel>(),
                bloonchipper.FirstBehavior<CreateSoundOnUpgradeModel>(),
                bloonchipper.FirstBehavior<CreateEffectOnPlaceModel>(),
                bloonchipper.FirstBehavior<CreateEffectOnSellModel>(),
                bloonchipper.FirstBehavior<CreateEffectOnUpgradeModel>(),
                bloonchipper.FirstBehavior<AttackModel>(),
                bloonchipper.FirstBehavior<DisplayModel>()
            };

            AttackModel attackModel = bloonchipper.FirstBehavior<AttackModel>();
            attackModel.name = "";
            attackModel.range = range;
            attackModel.offsetX = 0;
            attackModel.offsetY = 0;
            attackModel.offsetZ = 0;
            attackModel.behaviors = new Model[] {
                new RotateToTargetModel("", true, false, false, 0, true, false),
                new AttackFilterModel("", new Il2CppReferenceArray<FilterModel>(1) { [0] = new FilterInvisibleModel("", true, false) }),
                new TargetFirstModel("", true, false),
                new TargetLastModel("", true, false),
                new TargetCloseModel("", true, false),
                new TargetStrongModel("", true, false)
            };

            WeaponModel weaponModel = attackModel.weapons[0];
            weaponModel.name = "";
            weaponModel.animation = -1;
            weaponModel.rate = .3f;
            weaponModel.ejectX = 0;
            weaponModel.ejectY = 0;
            weaponModel.ejectZ = 0;
            weaponModel.emission = new InstantDamageEmissionModel("", null);
            weaponModel.behaviors = null;

            ProjectileModel projectileModel = weaponModel.projectile;
            projectileModel.id = "";
            projectileModel.display = new() { guidRef = null };
            projectileModel.pierce = 1;
            projectileModel.maxPierce = 0;
            projectileModel.filters = new Il2CppReferenceArray<FilterModel>(1) { [0] = new FilterAllExceptTargetModel("") };
            projectileModel.behaviors = new Model[] {
                new ProjectileFilterModel("", projectileModel.filters),
                new InstantModel("", false),
                new AgeModel("", .1f, 0, false, null),
                new DisplayModel("", projectileModel.display, 0, NKVector3.zero, 1, false, 0)
            };

            DisplayModel displayModel = bloonchipper.FirstBehavior<DisplayModel>();
            displayModel.display = bloonchipper.display;

            return bloonchipper;
        }

        private static UpgradeModel GetUpgrade(Path path, byte tier, int cost) {
            Tiers tiers = (path, tier);

            return new UpgradeModel(Names[tiers], cost, 0, new() { guidRef = $"Ui[{ResourcePrefix}{UpgradeIconPrefix}{tiers}]" }, (int)path, tier, 0, "", "");
        }

        /// <summary>
        /// Loads the model for the given name
        /// </summary>
        /// <param name="name">The name of the model</param>
        /// <returns>The <see cref="UnityDisplayNode"/> that was attached to the model, null if not found</returns>
        public static UnityDisplayNode LoadDisplay(string name) {
            if (IsResource(name, out string resourceName)) {
                Object asset = AssetBundle.LoadAsset(resourceName);
                if (asset is not null) {
                    GameObject proto = asset.Cast<GameObject>();
                    GameObject obj = Object.Instantiate(proto);
                    obj.name = Name + obj.name;
                    return obj.AddComponent<UnityDisplayNode>();
                }
            }
            return null;
        }

        /// <summary>
        /// Loads the <see cref="Sprite"/> for the given name
        /// </summary>
        /// <param name="name">The name of the <see cref="Sprite"/></param>
        /// <returns>The <see cref="Sprite"/> that was loaded, null if not found</returns>
        public static Sprite LoadSprite(string name) => IsResource(name, out string resourceName) ? Resources.LoadSprite(resourceName) : null;

        private static bool IsResource(string name, out string resourceName) {
            if (name.StartsWith(ResourcePrefix)) {
                resourceName = name[ResourcePrefix.Length..];
                return true;
            }
            resourceName = null;
            return false;
        }

        /// <summary>
        /// Changes the given <see cref="OpenGiftbox"/> into a bloonchipper one
        /// </summary>
        /// <param name="gift">The <see cref="OpenGiftbox"/> to be modified</param>
        public static void ModifyGift(OpenGiftbox gift) {
            Vector3 pos = gift.transform.localPosition;
            pos.x = 50;
            gift.transform.localPosition = pos;

            Object.Destroy(gift.container.transform.Find("GiftBox").gameObject);

            Object asset = AssetBundle.LoadAsset("BloonchipperGift");
            if (asset is not null) {
                GameObject giftModel = asset.Cast<GameObject>();
                Object.Instantiate(giftModel, gift.container.transform);
            }
        }

        /// <summary>
        /// Shows a popup for the bloonchipper gift
        /// </summary>
        /// <param name="onConfirm">An action that is called when the popup is confirmed by the user</param>
        /// <param name="onComplete">An action that is called when the popup completes</param>
        public static void OpenGift(System.Action onConfirm, System.Action onComplete) => PopupScreen.instance.ShowPopup(placement: PopupScreen.Placement.menuCenter,
                                                                                               title: "Tower Unlock",
                                                                                               body: "Do you want to clean up the Bloonchipper?",
                                                                                               okCallback: onConfirm.And(onComplete),
                                                                                               okString: "Yes",
                                                                                               cancelCallback: onComplete,
                                                                                               cancelString: "Later",
                                                                                               transition: Popup.TransitionAnim.Scale,
                                                                                               background: PopupScreen.BackGround.Grey);

        /// <summary>
        /// Changes the given <see cref="GiftboxUnlockSplash"/> into a bloonchipper one
        /// </summary>
        /// <param name="unlockSplash">The <see cref="GiftboxUnlockSplash"/> to be modified</param>
        public static void ModifyGiftSplash(GiftboxUnlockSplash unlockSplash) {
            Transform bloonchipperReveal = unlockSplash.transform.Find("NewTowerReveal");

            bloonchipperReveal.Find("TowerIcon").GetComponent<Image>().sprite = Resources.LoadSprite("UnlockScreen.UnlockIcon");
            bloonchipperReveal.Find("Lightbox").GetComponent<Image>().sprite = Resources.LoadSprite("UnlockScreen.UnlockBackground");

            Object.Destroy(bloonchipperReveal.Find("BangFx").gameObject);
            Object.Destroy(bloonchipperReveal.Find("BangFx (1)").gameObject);
            Object.Destroy(bloonchipperReveal.Find("BangFx (2)").gameObject);
            Object.Destroy(bloonchipperReveal.Find("BangFx (3)").gameObject);

            Object.Destroy(bloonchipperReveal.Find("Darts").gameObject);
            Object.Destroy(bloonchipperReveal.Find("Darts (1)").gameObject);
            Object.Destroy(bloonchipperReveal.Find("Darts (2)").gameObject);
        }

        public static void AddLocalization() {
            Dictionary<string, string> table = LocalizationManager.Instance.defaultTable;
            table.AddIfNotPresent(Name, "Bloonchipper");
            table.AddIfNotPresent(Name + " Description", "Rapidly sucks up and shreds bloons, spitting what's left out the back.");
            table.AddIfNotPresent(Name + " Short Description", "Chipping Machine");
            table.AddIfNotPresent(Names[(0, 1, 0)] + " Description", "Increases suction range.");
            table.AddIfNotPresent(Names[(0, 2, 0)] + " Description", "A more space efficient design allows for larger blades that can handle more bloons.");
            table.AddIfNotPresent(Names[(0, 3, 0)] + " Description", "Triple barrels provide the most efficient bloon shredding possible.");
            table.AddIfNotPresent(Names[(1, 0, 0)] + " Description", "Increases the power applied to the suction of bloons, making the sucked bloons faster.");
            table.AddIfNotPresent(Names[(2, 0, 0)] + " Description", "Higher wattage motor allows for Frozen and Lead bloon popping.");
            table.AddIfNotPresent(Names[(3, 0, 0)] + " Description", "Dual layer blade array doubly shreds bloons, taking off 2 layers instead of 1.");
            table.AddIfNotPresent(Names[(4, 0, 0)] + " Description", "Wide funnel can suck in all but the strongest MOAB class bloons.");
        }
    }
}
