using NexusStrap.Models;

namespace NexusStrap
{
    public static class FFlagTemplateManager
    {
        private static readonly HashSet<string> RendererPreferenceFlags = new(StringComparer.Ordinal)
        {
            // renderer flags are like a polycule: only one can be active at a time or drama happens
            "FFlagDebugGraphicsPreferD3D11",
            "FFlagDebugGraphicsPreferVulkan",
            "FFlagDebugGraphicsPreferOpenGL",
        };

        private static readonly List<FFlagTemplate> _templates = new()
        {
            new FFlagTemplate
            {
                Name = "144 FPS Zero Ping",
                Description = "Extreme performance and network optimization for 144+ FPS with minimal latency. Strips grass, shadows, telemetry, and maxes out thread scheduling.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FLogNetwork", "7" },
                    { "FFlagHandleAltEnterFullscreenManually", "False" },
                    { "DFIntOcclusionShelfScalarNumerator", "2" },
                    { "FFlagFRMRefactor", "False" },
                    { "FFlagUISUseLastFrameTimeInUpdateInputSignal", "True" },
                    { "FFlagSimEnableDCD16", "True" },
                    { "DFFlagFrameTimeJitterMedians2", "False" },
                    { "DFFlagReplicatorSeparateVarThresholds", "True" },
                    { "FFlagFasterPreciseTime4", "True" },
                    { "FIntActivatedCountTimerMSKeyboard", "0" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "DFIntNetworkClusterPacketCacheNumParallelTasks", "12" },
                    { "FFlagLargeReplicatorRead2", "True" },
                    { "DFIntMegaReplicatorNumParallelTasks", "12" },
                    { "DFIntGraphicsOptimizationModeMaxFrameTimeTargetMs", "25" },
                    { "DFIntGraphicsOptimizationModeMinFrameTimeTargetMs", "16" },
                    { "FFlagDebugDisableTelemetryV2Stat", "True" },
                    { "FFlagDebugDisableTelemetryV2Counter", "True" },
                    { "DFIntTaskSchedulerJobInitThreads", "12" },
                    { "FIntRuntimeMaxNumOfMutexes", "1000000" },
                    { "FIntSSAOMipLevels", "0" },
                    { "DFIntMemoryUtilityCurveNumSegments", "100" },
                    { "DFFlagMouseMoveOncePerFrame", "False" },
                    { "DFIntRakNetResendRttMultiple", "1" },
                    { "FIntActivatedCountTimerMSMouse", "0" },
                    { "DFFlagMergeFakeInputEvents3", "True" },
                    { "FIntSSAO", "0" },
                    { "DFIntBatchThumbnailResultsSizeCap", "200" },
                    { "FFlagDebugDisableTelemetryPoint", "True" },
                    { "FIntRuntimeMaxNumOfDPCs", "64" },
                    { "DFIntReplicationDataCacheNumParallelTasks", "12" },
                    { "DFIntDebugPerformanceControlFrameTime", "2" },
                    { "DFIntClientPacketMaxDelayMs", "1" },
                    { "FIntRuntimeMaxNumOfSchedulers", "1000000" },
                    { "FFlagDebugDisableTelemetryEphemeralCounter", "True" },
                    { "DFIntAnimationLodFacsDistanceMax", "0" },
                    { "DFIntClientNetworkInfluxHundredthsPercentage", "0" },
                    { "FFlagDebugDisableTelemetryEphemeralStat", "True" },
                    { "FFlagDisablePostFx", "True" },
                    { "DFIntOcclusionFresnelEllipsoids", "6" },
                    { "DFIntMaxDataPacketPerSend", "100000" },
                    { "DFIntOcclusionFresnelConsensusNumerator", "2" },
                    { "DFIntRakNetNakResendDelayMs", "1" },
                    { "FIntGrassMovementReducedMotionFactor", "0" },
                    { "FIntRuntimeMaxNumOfConditions", "1000000" },
                    { "DFFlagClampIncomingReplicationLag", "True" },
                    { "DFIntJoinDataCompressionLevel", "0" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "FIntRenderGrassDetailStrands", "0" },
                    { "FIntRenderShadowIntensity", "0" },
                    { "DFIntMaxProcessPacketsStepsAccumulated", "0" },
                    { "DFIntRakNetLoopMs", "1" },
                    { "FIntRuntimeMaxNumOfLatches", "1000000" },
                    { "FFlagMessageBusCallOptimization", "True" },
                    { "DFIntRakNetSelectTimeoutMs", "1" },
                    { "FIntTaskSchedulerAutoThreadLimit", "12" },
                    { "FIntFRMMinGrassDistance", "0" },
                    { "DFFlagDebugSkipMeshVoxelizer", "True" },
                    { "FIntCameraMaxZoomDistance", "2147483647" },
                    { "DFIntTextureQualityOverride", "0" },
                    { "FIntRenderShadowmapBias", "0" },
                    { "DFIntMaxProcessPacketsJobScaling", "10000" },
                    { "DFIntMaxProcessPacketsStepsPerCyclic", "5000" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFIntTaskSchedulerJobInGameThreads", "12" },
                    { "FIntRuntimeMaxNumOfThreads", "1000000" },
                    { "DFIntNetworkSchemaCompressionRatio", "0" },
                    { "DFIntClientPacketHealthyAllocationPercent", "20" },
                    { "FFlagSortKeyOptimization", "True" },
                    { "DFIntRakNetApplicationFeedbackScaleUpThresholdPercent", "0" },
                    { "DFFlagRobloxTelemetryAddDeviceRAM", "False" },
                    { "DFIntContentProviderPreloadHangTelemetryHundredthsPercentage", "0" },
                    { "FIntLuaGcParallelMinMultiTasks", "12" },
                    { "DFFlagReplicatorDisKickSize", "True" },
                    { "FFlagNextGenReplicatorEnabledRead2", "True" },
                    { "FFlagDebugGraphicsPreferD3D11", "True" },
                    { "DFFlagDebugPerfMode", "True" },
                    { "FFlagLargeReplicatorWrite2", "True" },
                    { "DFIntGraphicsOptimizationModeFRMFrameRateTarget", "1000" },
                    { "DFIntRakNetApplicationFeedbackScaleUpFactorHundredthPercent", "0" },
                    { "FFlagDebugDisableTelemetryEventIngest", "True" },
                    { "DFIntPhysicsReceiveNumParallelTasks", "12" },
                    { "FFlagDebugCheckRenderThreading", "True" },
                    { "DFIntTargetTimeDelayFacctorTenths", "15" },
                    { "DFIntWaitOnUpdateNetworkLoopEndedMS", "100" },
                    { "FFlagDebugDisableTelemetryV2Event", "True" },
                    { "DFIntHttpBatchApi_maxWaitMs", "40" },
                    { "DFIntMaxReceiveToDeserializeLatencyMilliseconds", "10" },
                    { "FIntInterpolationAwareTargetTimeLerpHundredth", "40" },
                    { "DFIntHttpBatchApi_minWaitMs", "5" },
                    { "DFIntHttpBatchApi_cacheDelayMs", "15" },
                    { "DFIntMemoryUtilityCurveTotalMemoryReserve", "0" },
                    { "FFlagLuaMenuPerfImprovements", "True" },
                    { "FFlagEnablePartyVoiceOnlyForUnfilteredThreads", "False" },
                    { "FFlagAdServiceEnabled", "False" },
                    { "DFIntS2PhysicsSenderRate", "128" },
                    { "FIntSmoothMouseSpringFrequencyTenths", "100" },
                    { "FIntV1MenuLanguageSelectionFeaturePerMillageRollout", "0" },
                    { "DFIntMaxFrameBufferSize", "4" },
                    { "FFlagTaskSchedulerLimitTargetFpsTo2402", "False" },
                }
            },

            new FFlagTemplate
            {
                Name = "Gamer Tested",
                Description = "Balanced performance preset. Removes grass and heavy shadows while keeping decent visuals. Good for mid-range PCs.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "FIntFRMMinGrassDistance", "0" },
                    { "FIntFRMMaxGrassDistance", "0" },
                    { "DFIntDebugFRMQualityLevelOverride", "1" },
                    { "DFIntTextureQualityOverride", "1" },
                    { "DFFlagTextureQualityOverrideEnabled", "True" },
                    { "FFlagDebugSkyGray", "True" },
                    { "FIntRenderShadowIntensity", "0" },
                    { "FIntDebugForceMSAASamples", "1" },
                    { "FFlagDebugGraphicsPreferVulkan", "True" },
                    { "DFIntCSGLevelOfDetailSwitchingDistance", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL12", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL23", "0" },
                    { "DFIntCSGLevelOfDetailSwitchingDistanceL34", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Bloxstrap Optimized",
                Description = "Pre-tuned flags used by the Bloxstrap community. Focuses on network and rendering efficiency without destroying visuals.",
                Category = FFlagTemplateCategories.Network,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntClientPacketMaxDelayMs", "1" },
                    { "DFIntRakNetResendRttMultiple", "1" },
                    { "DFIntRakNetNakResendDelayMs", "1" },
                    { "DFIntRakNetLoopMs", "1" },
                    { "DFIntRakNetSelectTimeoutMs", "1" },
                    { "DFIntNetworkQualityResponderMaxWaitTime", "1" },
                    { "DFIntMaxAcceptableUpdateDelay", "1" },
                    { "DFIntServerFramesBetweenJoins", "1" },
                    { "DFIntS2PhysicsSenderRate", "128" },
                    { "DFIntNetworkSchemaCompressionRatio", "0" },
                    { "DFIntJoinDataCompressionLevel", "0" },
                    { "DFIntMaxDataPacketPerSend", "100000" },
                    { "DFIntCodecMaxOutgoingFrames", "1000" },
                    { "DFIntCodecMaxIncomingPackets", "100" },
                    { "DFIntBufferCompressionLevel", "0" },
                    { "DFFlagClampIncomingReplicationLag", "True" },
                    { "DFIntReplicationDataCacheNumParallelTasks", "12" },
                    { "DFIntNetworkClusterPacketCacheNumParallelTasks", "12" },
                }
            },

            new FFlagTemplate
            {
                Name = "Low End Savior",
                Description = "For older or low-spec computers. Sacrifices all visuals for the smoothest possible gameplay on weak hardware.",
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
                    { "FFlagDisablePostFx", "True" },
                    { "FIntRenderShadowIntensity", "0" },
                    { "FIntSSAO", "0" },
                    { "FIntSSAOMipLevels", "0" },
                    { "FIntRenderGrassDetailStrands", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Maximum FPS",
                Description = "Removes all non-essential visual effects to maximize frame rate. Best for competitive gaming on any hardware.",
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
                    { "FFlagDisablePostFx", "True" },
                    { "FIntRenderShadowIntensity", "0" },
                    { "FIntSSAO", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Tom Pearl FFlags",
                Description = "Enables the highest visual quality settings for screenshots and recording. Uses maximum MSAA and texture quality.",
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
                Name = "Brighter and Clearer",
                Description = "Disables post-processing (bloom, vignette, color grading) for a brighter, clearer view. Makes dark games much easier to see.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDisablePostFx", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Crisp Gameplay",
                Description = "Disables post-processing, heavy shadows, and lowers render quality for the cleanest possible look during gameplay.",
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
                Name = "Future is Bright",
                Description = "Forces the new Future is Bright shadow-map lighting engine for improved, more colorful lighting.",
                Category = FFlagTemplateCategories.GraphicsQuality,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugForceFutureIsBrightPhase2", "True" },
                    { "FFlagDebugForceFutureIsBrightPhase3", "True" },
                }
            },

            new FFlagTemplate
            {
                Name = "Telemetry Blocker",
                Description = "Disables all Roblox telemetry and data collection. Improves privacy and reduces background network usage.",
                Category = FFlagTemplateCategories.Network,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugDisableTelemetryV2Stat", "True" },
                    { "FFlagDebugDisableTelemetryV2Counter", "True" },
                    { "FFlagDebugDisableTelemetryEphemeralCounter", "True" },
                    { "FFlagDebugDisableTelemetryEphemeralStat", "True" },
                    { "FFlagDebugDisableTelemetryPoint", "True" },
                    { "FFlagDebugDisableTelemetryEventIngest", "True" },
                    { "FFlagDebugDisableTelemetryV2Event", "True" },
                    { "DFIntContentProviderPreloadHangTelemetryHundredthsPercentage", "0" },
                    { "DFFlagRobloxTelemetryAddDeviceRAM", "False" },
                    { "FFlagAddDMLogging", "False" },
                }
            },

            new FFlagTemplate
            {
                Name = "Shadow Tuning",
                Description = "Removes shadow intensity and lowers shadow atlas usage. Enemies and terrain stand out more clearly.",
                Category = FFlagTemplateCategories.VisualEffects,
                Flags = new Dictionary<string, string>
                {
                    { "FIntRenderShadowIntensity", "0" },
                    { "FIntRenderShadowmapBias", "0" },
                    { "FIntRenderMaxShadowAtlasUsageBeforeDownscale", "1" },
                }
            },

            new FFlagTemplate
            {
                Name = "On-Screen FPS Counter",
                Description = "Shows a small FPS counter in the corner of the game. Handy for checking your performance after tuning flags.",
                Category = FFlagTemplateCategories.Debug,
                Flags = new Dictionary<string, string>
                {
                    { "FFlagDebugDisplayFPS", "True" },
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
                    { "FIntGrassMovementReducedMotionFactor", "0" },
                    { "FIntRenderGrassDetailStrands", "0" },
                }
            },

            new FFlagTemplate
            {
                Name = "Threading Powerhouse",
                Description = "Maxes out thread scheduling, mutexes, and latches for systems with many CPU cores. Reduces scheduling bottlenecks.",
                Category = FFlagTemplateCategories.Performance,
                Flags = new Dictionary<string, string>
                {
                    { "DFIntTaskSchedulerJobInitThreads", "12" },
                    { "DFIntTaskSchedulerJobInGameThreads", "12" },
                    { "FIntRuntimeMaxNumOfThreads", "1000000" },
                    { "FIntRuntimeMaxNumOfMutexes", "1000000" },
                    { "FIntRuntimeMaxNumOfLatches", "1000000" },
                    { "FIntRuntimeMaxNumOfSemaphores", "1000000" },
                    { "FIntRuntimeMaxNumOfConditions", "1000000" },
                    { "FIntRuntimeMaxNumOfSchedulers", "1000000" },
                    { "FIntRuntimeMaxNumOfDPCs", "64" },
                    { "DFIntMegaReplicatorNumParallelTasks", "12" },
                    { "DFIntReplicationDataCacheNumParallelTasks", "12" },
                    { "DFIntPhysicsReceiveNumParallelTasks", "12" },
                    { "FIntTaskSchedulerAutoThreadLimit", "12" },
                    { "FIntLuaGcParallelMinMultiTasks", "12" },
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
