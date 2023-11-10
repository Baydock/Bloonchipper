using Bloonchipper.DataTypes;
using Bloonchipper.Utils;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using System.Collections.Generic;
using NKVector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace Bloonchipper {
    // TODO: analyze 030, also make it use all 3 barrels
    /// <summary>
    /// A barrel of a bloonchipper. Handles the sucking of bloons.
    /// </summary>
    internal sealed class BloonchipperBarrel {
        private static readonly Dictionary<int, BloonchipperBarrel> Barrels = new();

        /// <summary>
        /// The bloonchipper tower that this is attached to
        /// </summary>
        public Tower Tower { get; }

        /// <summary>
        /// The damage dealt by this barrel
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// The speed that a bloon approaches the bloonchipper while sucked
        /// </summary>
        public float SuckSpeed { get; private set; }

        /// <summary>
        /// The speed that a bloon returns to the track
        /// </summary>
        public float ReturnSpeed { get; private set; }

        private List<SuckedBloon> BloonsSucked { get; } = new();

        private int MaxBloons { get; set; }

        private int DamageTick { get; set; }

        private NKVector3 Entrance { get; set; }

        private NKVector3 Exit { get; set; }

        private bool IsFull => BloonsSucked.Count >= MaxBloons;

        private Tiers Tiers { get; set; }

        private BloonchipperBarrel(Tower tower) {
            Tower = tower;

            DestroyAction = new System.Action<RootObject>(_ => Destroy());
            Tower.add_OnDestroyEvent(DestroyAction);

            UpgradeAction = new System.Action(ApplyStats);
            Tower.add_onUpgraded(UpgradeAction);

            ApplyStats();
        }

        private readonly Tower.OnUpgradedDelegate UpgradeAction;
        private void ApplyStats() {
            Tiers = (Tower.towerModel.tiers[0], Tower.towerModel.tiers[1], Tower.towerModel.tiers[2]);

            MaxBloons = Tiers.Mid > 1 ? 6 : 4;
            DamageTick = Tiers.Bot > 2 ? 20 : 60;
            Damage = Tiers.Top > 2 ? 2 : 1;
            SuckSpeed = Tiers.Top > 0 ? 3 : 2;
            ReturnSpeed = 2;
            Entrance = new NKVector3(0, 15, 21);
            Exit = new NKVector3(0, -10, 20);
        }

        private readonly RootObject.DestroyedEventHandler DestroyAction;
        private void Destroy(bool dontRemove = false) {
            Tower.remove_OnDestroyEvent(DestroyAction);
            Tower.remove_onUpgraded(UpgradeAction);
            foreach (SuckedBloon bloonSucked in BloonsSucked)
                bloonSucked.Release(dontRemove: true);
            BloonsSucked.Clear();

            if (!dontRemove)
                Barrels.Remove(Tower.Id.Id);
        }

        /// <summary>
        /// Creates a new barrel for the bloonchipper
        /// </summary>
        /// <param name="tower">The bloonchipper tower</param>
        public static void AttachTo(Tower tower) {
            if (Barrels.ContainsKey(tower.Id.Id)) {
                Barrels[tower.Id.Id].Destroy();
                Barrels[tower.Id.Id] = new BloonchipperBarrel(tower);
            } else
                Barrels.Add(tower.Id.Id, new BloonchipperBarrel(tower));
        }

        /// <summary>
        /// Finds the barrel that corresponds to the given bloonchipper
        /// </summary>
        /// <param name="tower">The bloonchipper tower</param>
        /// <param name="bloonchipper">The barrel found</param>
        /// <returns>True if a barrel was found, false otherwise</returns>
        public static bool TryGetBloonchipper(Tower tower, out BloonchipperBarrel bloonchipper) {
            if (Barrels.ContainsKey(tower.Id.Id)) {
                bloonchipper = Barrels[tower.Id.Id];
                return true;
            }
            bloonchipper = null;
            return false;
        }

        /// <summary>
        /// Determines if a damage tick has passed
        /// </summary>
        /// <param name="frames">How many frames since last tick</param>
        /// <returns>True if should damage, false otherwise</returns>
        public bool HasDamageTicked(int frames) => frames >= DamageTick;

        /// <summary>
        /// Where the entrance of the barrel currently is
        /// </summary>
        /// <returns>Its position</returns>
        public NKVector3 GetEntrancePos() => GetPositionWithRotation(Entrance);

        /// <summary>
        /// Where the exit of the barrel currently is
        /// </summary>
        /// <returns>Its position</returns>
        public NKVector3 GetExitPos() => GetPositionWithRotation(Exit);

        private NKVector3 GetPositionWithRotation(NKVector3 offset) {
            offset.Rotate(Tower.Rotation);
            offset.y -= 0.5773f * offset.z; // Approximation for nk's nonsensical compensation for the camera angle
            return Tower.Position.ToVector3() + offset;
        }

        // TODO: add shaking animation
        /// <summary>
        /// Adds a <see cref="SuckedBloon"/> to the barrel
        /// </summary>
        /// <param name="suckedBloon">The <see cref="SuckedBloon"/> to add</param>
        /// <param name="overrideMax">Whether to add the <see cref="SuckedBloon"/> if the barrel is full already</param>
        /// <returns>True if it was added successfully, false otherwise</returns>
        public bool AddSuckedBloon(SuckedBloon suckedBloon, bool overrideMax = false) {
            if (BloonsSucked.Count < MaxBloons || overrideMax) {
                BloonsSucked.Add(suckedBloon);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the <see cref="SuckedBloon"/> from the barrel
        /// </summary>
        /// <param name="suckedBloon">The <see cref="SuckedBloon"/> to remove</param>
        public void RemoveSuckedBloon(SuckedBloon suckedBloon) => BloonsSucked.Remove(suckedBloon);

        /// <summary>
        /// Whether or not the <see cref="SuckedBloon"/> is able to be sucked
        /// </summary>
        /// <param name="bloon">The <see cref="SuckedBloon"/> in question</param>
        /// <returns>True if it can be sucked, false otherwise</returns>
        public bool CanSuckBloon(Bloon bloon) {
            if (IsFull || bloon is null)
                return false;

            BloonModel bloonModel = bloon.bloonModel;

            if (bloonModel.isBoss || bloonModel.baseId.Equals("Bad") || bloonModel.baseId.Equals("Golden") || SuckedBloon.IsBeingSucked(bloon))
                return false;

            if (Tiers.Top < 2 && bloonModel.bloonProperties.HasAnyFlag(BloonProperties.Frozen, BloonProperties.Lead))
                return false;

            if (Tiers.Top < 4 && bloonModel.isMoab)
                return false;

            return true;
        }

        /// <summary>
        /// Removes all <see cref="SuckedBloon"/>s from the barrel
        /// </summary>
        public static void ClearAll() {
            foreach (BloonchipperBarrel barrel in Barrels.Values)
                barrel.Destroy(dontRemove: true);
            Barrels.Clear();
        }
    }
}
