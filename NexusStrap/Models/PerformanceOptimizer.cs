namespace NexusStrap.Models
{
    /// <summary>
    /// Applies hardware-tier based recommendations to the FastFlags
    /// so the game runs at the best settings for the user's PC.
    /// Every applied setting maps to a preset on the FastFlags page,
    /// so the user can review or undo anything after the fact.
    /// </summary>
    public static class PerformanceOptimizer
    {
        public static void Apply(PerformanceTier tier)
        {
            const string LOG_IDENT = "PerformanceOptimizer::Apply";

            App.Logger.WriteLine(LOG_IDENT, $"Applying performance tier '{tier}'");

            App.FastFlags.suspendUndoSnapshot = true;
            App.FastFlags.SaveUndoSnapshot();

            switch (tier)
            {
                case PerformanceTier.Ultra:
                    App.FastFlags.SetPreset("Rendering.MSAA1", "4");
                    App.FastFlags.SetPreset("Rendering.FrmQuality", "21");
                    break;

                case PerformanceTier.High:
                    App.FastFlags.SetPreset("Rendering.MSAA1", "4");
                    App.FastFlags.SetPreset("Rendering.FrmQuality", "18");
                    break;

                case PerformanceTier.Mid:
                    App.FastFlags.SetPreset("Rendering.MSAA1", "2");
                    App.FastFlags.SetPreset("Rendering.FrmQuality", "14");
                    break;

                case PerformanceTier.Low:
                    // MSAA off, no grass, low poly meshes, pause voxelizer, low quality override
                    App.FastFlags.SetPreset("Rendering.MSAA1", "1");
                    App.FastFlags.SetPreset("Rendering.RemoveGrass1", "0");
                    App.FastFlags.SetPreset("Rendering.RemoveGrass2", "0");
                    App.FastFlags.SetPreset("Rendering.RemoveGrass3", "0");
                    App.FastFlags.SetPreset("Rendering.PauseVoxerlizer", "True");
                    App.FastFlags.SetPreset("Rendering.FrmQuality", "9");

                    int[] baseValues = { 2000, 1500, 1000, 500 };
                    int level = 8;

                    for (int i = 0; i < 4; i++)
                        App.FastFlags.SetPreset($"Rendering.LowPolyMeshes{i + 1}", ((baseValues[i] * level) / 9).ToString());

                    break;
            }

            App.FastFlags.suspendUndoSnapshot = false;
            App.FastFlags.Save();
        }
    }
}