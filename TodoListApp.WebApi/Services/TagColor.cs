namespace TodoListApp.WebApi.Services;

public static class TagColor
{
    public static string FromName(string name)
    {
        var n = (name ?? string.Empty).Trim().ToLowerInvariant();
        if (n.Length == 0)
        {
            n = "tag";
        }

        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in n)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            var hue = hash % 360u;
            return HslToHex((int)hue, 45, 85);
        }
    }

    private static string HslToHex(int h, int s, int l)
    {
        double sd = s / 100.0, ld = l / 100.0;
        double c = (1 - Math.Abs((2 * ld) - 1)) * sd;
        double x = c * (1 - Math.Abs(((h / 60.0) % 2) - 1));
        double m = ld - (c / 2.0);
        (double r, double g, double b) = h switch
        {
            < 60 => (c, x, 0d),
            < 120 => (x, c, 0d),
            < 180 => (0d, c, x),
            < 240 => (0d, x, c),
            < 300 => (x, 0d, c),
            _ => (c, 0d, x)
        };
        byte r1 = (byte)Math.Round((r + m) * 255);
        byte g1 = (byte)Math.Round((g + m) * 255);
        byte b1 = (byte)Math.Round((b + m) * 255);
        return $"#{r1:X2}{g1:X2}{b1:X2}";
    }
}
