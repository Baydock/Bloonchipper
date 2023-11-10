using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;
using NKVector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using NKVector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace Bloonchipper {
    internal sealed class SuckedBloon {
        private static readonly Dictionary<int, SuckedBloon> SuckedBloons = new(); // int is the bloon id

        private Bloon SimBloon { get; set; }

        private BloonchipperBarrel SuckedBy { get; }

        private int InitialElapsed { get; set; } = -1;

        private int PreviousElapsed { get; set; } = -1;

        private SuckState State { get; set; } = SuckState.Sucking;

        // This looping shit got me scratching my head 
        private float LoopSpeed { get; set; } = 1; // TODO: make the looping work like in the reference games

        private NKVector3 LoopDirection { get; set; } = new NKVector3(1, 1).Normalized();

        private SuckedBloon(Bloon bloon, BloonchipperBarrel suckedBy, Bloon parent) {
            SimBloon = bloon;
            UpdateAction = new System.Action<int>(Update);
            DestroyAction = new System.Action<RootObject>(_ => Destroy());

            SimBloon.RemoveMutatorsByKeys(new Il2CppStringArray(2) { [0] = "Wind", [1] = "Knockback" });
            SimBloon.offTrackCount = 1;
            SimBloon.add_process(UpdateAction);
            SimBloon.add_OnDestroyEvent(DestroyAction);

            if (parent is not null) {
                SuckedBloon suckedParent = SuckedBloons[parent.Id.Id];
                SuckedBy = suckedParent.SuckedBy;
                InitialElapsed = suckedParent.InitialElapsed;
                PreviousElapsed = suckedParent.PreviousElapsed;
                State = suckedParent.State;
                LoopSpeed = suckedParent.LoopSpeed;
                LoopDirection = suckedParent.LoopDirection;
                SimBloon.Position.Set(parent.Position);
            } else
                SuckedBy = suckedBy;

            if (State != SuckState.Returning)
                SuckedBy.AddSuckedBloon(this, true);
        }

        private readonly Simulation.ProcessDelegate UpdateAction;
        private void Update(int elapsed) {
            if (PreviousElapsed == -1)
                PreviousElapsed = elapsed - 1;

            if (InitialElapsed == -1)
                InitialElapsed = elapsed;

            int framesElapsed = elapsed - PreviousElapsed;

            switch (State) {
                case SuckState.Sucking: {
                    if (MoveBloonTo(SuckedBy.GetEntrancePos(), SuckedBy.SuckSpeed, framesElapsed, out NKVector3 newPos, out float rotation)) {
                        SimBloon.Position.Set(newPos);
                        if (SimBloon.bloonModel.isMoab)
                            SimBloon.SetRotation(rotation);
                    } else {
                        State = SuckState.Shredding;
                        InitialElapsed = elapsed;
                        SimBloon.Position.Set(-3000, 0, 0);
                    }
                    break;
                }
                case SuckState.Shredding: {
                    if (SuckedBy.HasDamageTicked(elapsed - InitialElapsed)) {
                        if (SimBloon.health == 1)
                            Release(true);
                        else
                            InitialElapsed = elapsed;
                        NKVector3 bloonPos = SimBloon.Position;
                        NKVector3 towerPos = SuckedBy.Tower.Position;
                        towerPos.z = bloonPos.z;
                        SimBloon.Position.Set(towerPos); // To get pop effect on top of tower
                        SimBloon.Damage(SuckedBy.Damage, null, true, false, true, SuckedBy.Tower); // TODO: fix bloon children bug with >1 damage
                        SimBloon.Position.Set(bloonPos);
                    }
                    break;
                }
                case SuckState.Returning: {
                    NKVector3 trackPos = SimBloon.path.DistanceToPoint(SimBloon.distanceTraveled);
                    trackPos.z = GetBloonHeight(SimBloon);
                    if (MoveBloonTo(trackPos, SuckedBy.ReturnSpeed, framesElapsed, out NKVector3 newPos, out float rotation, true)) {
                        SimBloon.Position.Set(newPos);
                        if (SimBloon.bloonModel.isMoab)
                            SimBloon.SetRotation(rotation + 180);
                    } else
                        Returned();
                    break;
                }
            }

            PreviousElapsed = elapsed;
        }

        private readonly RootObject.DestroyedEventHandler DestroyAction;
        private void Destroy() {
            SuckedBy.RemoveSuckedBloon(this);
            Returned();
        }

        // Returns true if the bloon should continue moving
        private bool MoveBloonTo(NKVector3 to, float speed, int framesElapsed, out NKVector3 newPos, out float rotation, bool doALoop = false) {
            NKVector3 from = SimBloon.Position;
            NKVector2 from2 = from.ToVector2();
            NKVector2 to2 = to.ToVector2();
            NKVector3 direction = to - from;
            direction.z = 0;
            direction.Normalize();
            newPos = from + direction * framesElapsed * speed;
            if (doALoop)
                newPos += LoopDirection * framesElapsed * LoopSpeed;
            newPos.z = to.z;

            float traveled = from2.Distance(newPos.ToVector2());
            float distance = from2.Distance(to2);

            rotation = from2.AngleBetween(to2);

            if (doALoop) {
                LoopSpeed -= .5f;
                if (LoopSpeed < 0)
                    LoopSpeed = 0;
            }

            return traveled < distance;
        }

        // TODO: Don't remove it from sucked bloons to add delay before retarget as possible solution
        private void Returned(bool dontRemove = false) {
            SimBloon.remove_process(UpdateAction);
            SimBloon.remove_OnDestroyEvent(DestroyAction);
            SimBloon.offTrackCount = 0;

            if (!dontRemove)
                SuckedBloons.Remove(SimBloon.Id.Id);
        }

        private static float GetBloonHeight(Bloon bloon) => bloon.bloonModel.isBoss ? Constants.bossHeight : bloon.bloonModel.isMoab ? Constants.moabHeight : Constants.bloonHeight;

        /// <summary>
        /// Attached and takes over the <see cref="Bloon"/>'s movement to be sucked
        /// </summary>
        /// <param name="bloon">The <see cref="Bloon"/> to be attached to</param>
        /// <param name="suckedBy">The <see cref="BloonchipperBarrel"/> that it is being sucked by, null if using parent</param>
        /// <param name="parent">The <see cref="Bloon"/> that spawned this one</param>
        public static void AttachTo(Bloon bloon, BloonchipperBarrel suckedBy, Bloon parent = null) => SuckedBloons.Add(bloon.Id.Id, new SuckedBloon(bloon, suckedBy, parent));

        /// <summary>
        /// Whether the given <see cref="Bloon"/> is already being sucked
        /// </summary>
        /// <param name="bloon">The <see cref="Bloon"/> in question</param>
        /// <returns>True if being sucked, false otherwise</returns>
        public static bool IsBeingSucked(Bloon bloon) => SuckedBloons.ContainsKey(bloon.Id.Id);

        /// <summary>
        /// Whether the bloon can be reached by attacks
        /// </summary>
        /// <param name="bloon">The <see cref="Bloon"/> in question</param>
        /// <returns>True if it cannot be reached, false otherwise</returns>
        public static bool IsUnreachable(Bloon bloon) => IsBeingSucked(bloon) && (SuckedBloons[bloon.Id.Id].State == SuckState.Sucking || SuckedBloons[bloon.Id.Id].State == SuckState.Shredding);

        /// <summary>
        /// Removes the SuckedBloon from the <see cref="BloonchipperBarrel"/> and sends it back to the track
        /// </summary>
        /// <param name="ejected">Whether to send it back to the track or not</param>
        /// <param name="dontRemove">Whether to remove it from the <see cref="BloonchipperBarrel"/></param>
        public void Release(bool ejected = false, bool dontRemove = false) {
            if (ejected) {
                SimBloon.Position.Set(SuckedBy.GetExitPos());
            } else {
                NKVector3 endPos = SuckedBy.Tower.Position;
                endPos.z = GetBloonHeight(SimBloon);
                SimBloon.Position.Set(endPos);
            }

            if (!dontRemove)
                SuckedBy.RemoveSuckedBloon(this);

            State = SuckState.Returning;
        }

        /// <summary>
        /// Stops all bloons from being sucked
        /// </summary>
        public static void ClearAll() {
            foreach (SuckedBloon suckedBloon in SuckedBloons.Values)
                suckedBloon.Returned(dontRemove: true);
            SuckedBloons.Clear();
        }

        private enum SuckState {
            Sucking,
            Shredding,
            Returning
        }
    }
}
