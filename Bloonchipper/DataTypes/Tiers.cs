using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Bloonchipper.DataTypes {
    /// <summary>
    /// The tier of each of the three paths of a BTD6 tower
    /// </summary>
    /// <param name="Top">The tier of the top path</param>
    /// <param name="Mid">The tier of the middle path</param>
    /// <param name="Bot">The tier of the bottom path</param>
    internal readonly record struct Tiers(byte Top, byte Mid, byte Bot) {
        public static implicit operator Tiers((byte top, byte mid, byte bot) tiers) => new(tiers.top, tiers.mid, tiers.bot);

        public static implicit operator Tiers((int top, int mid, int bot) tiers) => new((byte)tiers.top, (byte)tiers.mid, (byte)tiers.bot);

        public static implicit operator Tiers((Path path, byte tier) upgrade) => new(upgrade.path == Path.Top ? upgrade.tier : (byte)0,
                                                                                     upgrade.path == Path.Mid ? upgrade.tier : (byte)0,
                                                                                     upgrade.path == Path.Bot ? upgrade.tier : (byte)0);

        /// <summary>
        /// If all tiers are 0
        /// </summary>
        public bool IsBase { get; } = Top + Mid + Bot == 0;

        /// <summary>
        /// The highest tier
        /// </summary>
        public int Tier { get; } = System.Math.Max(System.Math.Max(Top, Mid), Bot);

        /// <summary>
        /// The tiers as an Il2CppArray
        /// </summary>
        public Il2CppStructArray<int> ToIl2CppArray() => new(3) { [0] = Top, [1] = Mid, [2] = Bot };

        public override int GetHashCode() => Top * 100 + Mid * 10 + Bot;

        /// <summary>
        /// The tiers as a string
        /// </summary>
        public override string ToString() => $"{Top}{Mid}{Bot}";
    }
}
