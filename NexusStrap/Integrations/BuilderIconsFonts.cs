using System.Reflection;

namespace NexusStrap.Integrations
{
    public static class BuilderIconsFonts
    {
        private static readonly string[] FontNames =
        {
            "BuilderIcons-Regular.ttf",
            "BuilderIcons-Filled.ttf"
        };

        public static string FontDirectory { get; } = Path.Combine(Paths.Cache, "Font Preview");

        public static void EnsureExtracted()
        {
            Directory.CreateDirectory(FontDirectory);

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var name in FontNames)
            {
                string path = Path.Combine(FontDirectory, name);

                if (File.Exists(path))
                    continue;

                using var stream = assembly.GetManifestResourceStream($"NexusStrap.Resources.Fonts.{name}")!;
                using var output = File.Create(path);
                stream.CopyTo(output);
            }
        }
    }
}
