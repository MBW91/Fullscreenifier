using PInvoke;

namespace Fullscreenifier
{
    public static class RECTExtensions
    {
        public static int GetWidth(this RECT rect)
        {
            return rect.right - rect.left;
        }

        public static int GetHeight(this RECT rect)
        {
            return rect.bottom - rect.top;
        }
    }
}
