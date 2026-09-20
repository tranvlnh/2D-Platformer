public static class Utils
{
    public static void Swap<T>(ref T a, ref T b) where T : class
    {
        (a, b) = (b, a);
    }
}