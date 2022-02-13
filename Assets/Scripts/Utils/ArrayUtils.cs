namespace Utils {
    public static class ArrayUtils {
        public static T[] SubArray<T>(this T[] array, int offset, int count) {
            var result = new T[count];
            for (int i = offset; i < offset + count; ++i) {
                result[i - offset] = array[i];
            }
            return result;
        }
    }
}