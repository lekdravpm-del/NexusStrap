using NexusStrap.Models;

namespace NexusStrap
{
    public static class FFlagTemplateManager
    {
        private static readonly HashSet<string> RendererPreferenceFlags = new(StringComparer.Ordinal)
        {
            "FFlagDebugGraphicsPreferD3D11",
            "FFlagDebugGraphicsPreferVulkan",
            "FFlagDebugGraphicsPreferOpenGL",
        };

        private static readonly List<FFlagTemplate> _templates = new()
        {
            new FFlagTemplate
            {
                Name = "Maximum FPS Performance",
                Description = "Removes unnecessary visual effects to maximize frame rate. Best for competitive gaming.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFFlagDebugPauseVoxelizer", "True" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "DFIntTextureQualityOverride", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Tom Pearl FFlags",
                Description = "Enables the highest visual quality settings for screenshots and recording.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "FIntDebugForceMSAASamples", "8" },
                    { "DFIntDebugFRMQualityLevelOverride", "21" },
                    { "DFIntTextureQualityOverride", "16" },
                    { "FIntFRMMinGrassDistance", "100" },
                    { "FIntFRMMaxGrassDistance", "500" },
                }
            },

            new FFlagTemplate
            {
                Name = "Low End PC",
                Description = "For older or low-spec computers. Sacrifices visuals for smooth gameplay.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFFlagDebugPauseVoxelizer", "True" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "DFIntTextureQualityOverride", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                    { "FFlagDebugSkyGray", "True" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "FFlagDebugGraphicsPreferVulkan", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "No Grass",
                Description = "Removes grass rendering entirely. Significant performance boost with minimal visual impact.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Vulkan Renderer",
                Description = "Uses Vulkan rendering API. Better performance on modern GPUs (NVIDIA RTX, AMD RX 6000+).",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugGraphicsPreferVulkan", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "OpenGL Renderer",
                Description = "Uses OpenGL rendering. May help on older AMD/Intel GPUs.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugGraphicsPreferOpenGL", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Gray Sky",
                Description = "Replaces sky with solid gray. Reduces GPU load from sky rendering.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugSkyGray", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Disable Particles",
                Description = "Reduces particle effect quality to improve performance in effects-heavy games.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "DFFlagDebugPauseVoxelizer", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Network Optimizer",
                Description = "Adjusts network settings for more stable connections.",
                Category = FFlagTemplateCategories.Network,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntDebugFRMQualityLevelOverride", "3" },
                }
            },

            new FFlagTemplate
            {
                Name = "Anti-Aliasing Only",
                Description = "Enables anti-aliasing without other quality changes. Smooths jagged edges.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "FIntDebugForceMSAASamples", "4" },
                }
            },

            new FFlagTemplate
            {
                Name = "Studio Optimization",
                Description = "Optimizes Roblox Studio performance for faster development.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFFlagDebugPauseVoxelizer", "True" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "FFlagDebugSkyGray", "True" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Mobile-like Quality",
                Description = "Reduces quality to mobile-like levels. Useful for very low-end PCs.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTextureQualityOverride", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Clean Render",
                Description = "Removes post-processing effects like bloom and color correction for a cleaner look.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "FFlagDebugSkyGray", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Reduce Mesh Complexity",
                Description = "Switches to lower polygon models. Great performance boost with subtle visual change.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Disable Shadows",
                Description = "Disables shadow rendering. Major performance improvement.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                }
            },

            new FFlagTemplate
            {
                Name = "Balanced Performance",
                Description = "A balanced preset that improves FPS without sacrificing too much quality.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "10" },
                    { "FIntFRMMaxGrassDistance", "50" },
                    { "DFIntDebugFRMQualityLevelOverride", "5" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "1000" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "750" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "500" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "250" },
                }
            },

            new FFlagTemplate
            {
                Name = "High Quality Anti-Aliasing",
                Description = "Enables maximum MSAA for the smoothest edges possible.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "FIntDebugForceMSAASamples", "8" },
                }
            },

            new FFlagTemplate
            {
                Name = "Voxelizer Pause",
                Description = "Pauses the voxelizer system to save CPU. Useful when world building is not needed.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFFlagDebugPauseVoxelizer", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Minimal Visuals",
                Description = "Strips everything non-essential. Maximum FPS at the cost of visual fidelity.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFFlagDebugPauseVoxelizer", "True" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "DFIntTextureQualityOverride", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "FFlagDebugSkyGray", "True" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Disable DPI Scaling",
                Description = "Disables DPI scaling. Fixes blurry UI on high-DPI displays.",
                Category = FFlagTemplateCategories.Stability,
                Flags = new Dictionary<string, string>
                {
                    { "DFFlagDisableDPIScale", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Custom Resolution Quality",
                Description = "Sets texture quality to mid-range for a balance of clarity and performance.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTextureQualityOverride", "8" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Unlock FPS 240",
                Description = "Raises the frame rate cap from 60 to 240 for buttery smooth gameplay. Best paired with a high refresh rate monitor.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTaskSchedulerTargetFps", "240" },
                    { "FFlagGameBasicSettingsFramerateCap", "False" },
                }
            },

            new FFlagTemplate
            {
                Name = "Unlock FPS 144",
                Description = "Raises the frame rate cap to 144. A gentler option than 240 for 144Hz displays.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTaskSchedulerTargetFps", "144" },
                    { "FFlagGameBasicSettingsFramerateCap", "False" },
                }
            },

            new FFlagTemplate
            {
                Name = "Brighter & Clearer",
                Description = "Disables post-processing (bloom, vignette, color grading) for a brighter, clearer view. Makes dark games much easier to see.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDisablePostFx", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Shadow Tuning",
                Description = "Softens or removes heavy shadows so enemies and terrain stand out. Set to 0 to fully disable shadow intensity.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FIntRenderShadowIntensity", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Future Is Bright Lighting",
                Description = "Forces the new 'Future is Bright' shadow-map lighting engine for improved, more colorful lighting.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugForceFutureIsBrightPhase2", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "On-Screen FPS Counter",
                Description = "Shows a small FPS counter in the corner of the game, handy for checking your performance after tuning flags.",
                Category = FFlagTemplateCategories.Debug,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugDisplayFPS", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Crisp Gameplay",
                Description = "Disables post-processing and heavy shadows at once for the cleanest possible look during gameplay.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDisablePostFx", "True" },
                    { "FIntRenderShadowIntensity", "0" },
                    { "DFIntDebugFRMQualityLevelOverride", "3" },
                }
            },

            new FFlagTemplate
            {
                Name = "High Quality Textures",
                Description = "Forces maximum texture quality so details, skins and surfaces stay sharp up close.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTextureQualityOverride", "16" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                }
            },
        };

        public static IReadOnlyList<FFlagTemplate> GetAll() => _templates.AsReadOnly();

        public static IReadOnlyList<FFlagTemplate> GetByCategory(string category)
        {
            return _templates.Where(t => t.Category == category).ToList().AsReadOnly();
        }

        public static IReadOnlyList<FFlagTemplate> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();

            string lower = query.ToLowerInvariant();

            return _templates
                .Where(t => t.Name.ToLowerInvariant().Contains(lower)
                         || t.Description.ToLowerInvariant().Contains(lower)
                         || t.Category.ToLowerInvariant().Contains(lower))
                .ToList()
                .AsReadOnly();
        }

        public static void ApplyTemplate(FFlagTemplate template)
        {
            const string LOG_IDENT = "FFlagTemplateManager::ApplyTemplate";

            App.FastFlags.suspendUndoSnapshot = true;
            App.FastFlags.SaveUndoSnapshot();

            // When the template pins a renderer, clear the others so two
            // renderer preferences can never be active at the same time.
            if (template.Flags.Keys.Any(RendererPreferenceFlags.Contains))
            {
                foreach (string rendererFlag in RendererPreferenceFlags)
                {
                    if (!template.Flags.ContainsKey(rendererFlag))
                        App.FastFlags.SetValue(rendererFlag, null);
                }
            }

            foreach (var flag in template.Flags)
                App.FastFlags.SetValue(flag.Key, flag.Value);

            App.FastFlags.suspendUndoSnapshot = false;

            App.Logger.WriteLine(LOG_IDENT, $"Applied template '{template.Name}' ({template.Flags.Count} flags)");
        }

        public static FFlagTemplate? ImportFromJson(string json)
        {
            try
            {
                var template = JsonSerializer.Deserialize<FFlagTemplate>(json);

                if (template == null || string.IsNullOrWhiteSpace(template.Name))
                    return null;

                if (template.Flags == null)
                    template.Flags = new Dictionary<string, string>();

                return template;
            }
            catch
            {
                return null;
            }
        }

        public static string ExportToJson(FFlagTemplate template)
        {
            return JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
