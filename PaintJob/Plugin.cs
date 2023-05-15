using System;
using System.IO;
using System.Runtime.CompilerServices;
using ClientPlugin.GUI;
using ClientPlugin.Shared.Config;
using ClientPlugin.Shared.Logging;
using ClientPlugin.Shared.Patches;
using ClientPlugin.Shared.Plugin;
using DryIoc;
using HarmonyLib;
using PaintJob.App;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage.FileSystem;
using VRage.Plugins;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, ICommonPlugin
    {
        public const string Name = "PicassoGrids";
        private static readonly IPluginLogger Logger = new PluginLogger(Name);
        private static readonly string ConfigFileName = $"{Name}.cfg";

        private static bool initialized;
        private static bool failed;
        private Container _container;
        private PersistentConfig<PluginConfig> config;
        private IPaintApp _app => _container.Resolve<IPaintApp>();
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

        private void Initialize()
        {
            _container = IoC.Init(Logger);
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