using BridgeWE;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace _BaseModule
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{typeof(Mod).Assembly.GetName().Name}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private static readonly BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.GetProperty;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            GameManager.instance.onGameLoadingComplete += DoWhenLoaded;
        }

        private void DoWhenLoaded(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            log.Info($"Loading patches");
            DoPatches();
            RegisterModFiles();
            GameManager.instance.onGameLoadingComplete -= DoWhenLoaded;
        }

        private void RegisterModFiles()
        {
            GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset);
            var modDir = Path.GetDirectoryName(asset.path);

            var imagesDirectory = Path.Combine(modDir, "atlases");
            if (Directory.Exists(imagesDirectory))
            {
                var atlases = Directory.GetDirectories(imagesDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (var atlasFolder in atlases)
                {
                    WEImageManagementBridge.RegisterImageAtlas(typeof(Mod).Assembly, Path.GetFileName(atlasFolder), Directory.GetFiles(atlasFolder, "*.png"));
                }
            }

            var layoutsDirectory = Path.Combine(modDir, "layouts");
            WETemplatesManagementBridge.RegisterCustomTemplates(typeof(Mod).Assembly, layoutsDirectory);
            WETemplatesManagementBridge.RegisterLoadableTemplatesFolder(typeof(Mod).Assembly, layoutsDirectory);


            var fontsDirectory = Path.Combine(modDir, "fonts");
            WEFontManagementBridge.RegisterModFonts(typeof(Mod).Assembly, fontsDirectory);
        }

        private void DoPatches()
        {
            if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly)
            {
                var exportedTypes = weAssembly.ExportedTypes;
                foreach (var (type, sourceClassName) in new List<(Type, string)>() {
                    (typeof(WEFontManagementBridge), "FontManagementBridge"),
                    (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                    (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                })
                {
                    var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var srcMethod = targetType.GetMethod(method.Name, allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                        if (srcMethod != null) Harmony.ReversePatch(srcMethod, method);
                        else log.Warn($"Method not found while patching WE: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                    }
                }
            }
            else
            {
                throw new Exception("Write Everywhere dll file required for using this mod! Check if it's enabled.");
            }
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
