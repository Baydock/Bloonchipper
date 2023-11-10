using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppInterop.Runtime;

namespace Bloonchipper.Utils {
    internal static class ModelUtils {
        /// <summary>
        /// A methods that clones the given <see cref="Model"/> and casts it back to its original type
        /// </summary>
        /// <typeparam name="T">The original type of the <see cref="Model"/></typeparam>
        /// <param name="model">The <see cref="Model"/></param>
        /// <returns>A cloned <see cref="Model"/> of the same type</returns>
        public static T CloneCast<T>(this T model) where T : Model => model.Clone().Cast<T>();

        /// <summary>
        /// Finds the first behavior of the <see cref="TowerModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="tower">The <see cref="TowerModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this TowerModel tower) where T : Model {
            foreach(Model behavior in tower.behaviors) {
                if (Il2CppType.Of<T>().IsAssignableFrom(behavior.GetIl2CppType()))
                    return behavior.Cast<T>();
            }
            return null;
        }
    }
}
