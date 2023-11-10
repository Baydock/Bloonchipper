namespace Bloonchipper.Utils {
    internal static class DelegateUtils {
        /// <summary>
        /// Some syntactic sugar for <see cref="System.Delegate.Combine"/> that retains the original type
        /// </summary>
        /// <typeparam name="T">The type of delegate wanted to be combined</typeparam>
        /// <param name="a">The first delegate</param>
        /// <param name="b">The second delegate</param>
        /// <returns>The combined delegate</returns>
        public static T And<T>(this T a, T b) where T : System.Delegate => (T)System.Delegate.Combine(a, b);
    }
}
