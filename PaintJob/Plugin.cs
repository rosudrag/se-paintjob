using System;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;
using PaintJob.App;
using PaintJob.GUI;
using PaintJob.Shared.Config;
using PaintJob.Shared.Logging;
using PaintJob.Shared.Patches;
using PaintJob.Shared.Plugin;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage.FileSystem;
using VRage.Plugins;

namespace PaintJob
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, ICommonPlugin
    {
        public const string Name = "PaintJob";
        private static readonly IPluginLogger Logger = new PluginLogger(Name);
        private static readonly string ConfigFileName = $"{Name}.cfg";

        private static bool initialized;
        private static bool failed;
        private PersistentConfig<PluginConfig> config;
        private static IPaintApp _app => SimpleIoC.Resolve<IPaintApp>();
        public static Plugin Instance { get; private set; }

        public long Tick { get; private set; }

        public IPluginLogger Log => Logger;

        public IPluginConfig Config => config?.Data;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            Log.Info("Loading");

            var configPath = Path.Combine(MyFileSystem.UserDataPath, ConfigFileName);
            config = PersistentConfig<PluginConfig>.Load(Log, configPath);

            Common.SetPlugin(this);

            if (!PatchHelpers.HarmonyPatchAll(Log, new Harmony(Name)))
            {
                failed = true;
                return;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {
            try
            {
                _app.Save();
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Dispose failed");
            }

            Instance = null;
        }

        public void Update()
        {
            if (MySession.Static == null)
            {
                return;
            }

            EnsureInitialized();
            try
            {
                if (!failed)
                {
                    CustomUpdate();
                    Tick++;
                }
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Update failed");
                failed = true;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized || failed)
                return;

            Log.Info("Initializing");
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Failed to initialize plugin");
                failed = true;
                return;
            }

            Log.Debug("Successfully initialized");
            initialized = true;
        }

        private static void Initialize()
        {
            IoC.Init(Logger);
            _app.Initialize();
        }

        private void CustomUpdate()
        {
            _app.CustomUpdate();
        }


        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginConfigDialog());
        }
    }
}