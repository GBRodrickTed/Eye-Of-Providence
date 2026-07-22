using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using PluginConfig.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeOfProvidence;
using UnityEngine;
using UnityEngine.UIElements;
using FloatField = PluginConfig.API.Fields.FloatField;
using UnityEngine.ProBuilder;
using static UnityEngine.XR.XRDisplaySubsystem;
using UnityEngine.SocialPlatforms;
using PluginConfiguratorComponents;

namespace EyeOfProvidence
{
    public enum ProjectionMode
    {
        Equirectangular = 0,
        Fisheye = 1,
        Stereographic = 2,
        Hammer = 3,
        Panini = 4,
        Winkel = 5
    }
    public enum GlobeMode
    {
        Cube = 0,
        TriPrism = 1,
        Tetrahedron = 2
    }
    public static class ConfigManager
    {
        public static PluginConfigurator config;
        public static BoolField UltraFOV;
        public static FloatField PlayerFOVUncapped;
        public static FloatSliderField PlayerFOV;
        public static BoolField PlayerFOVShouldUncap;
        //public static BoolField Fisheye;
        public static EnumField<ProjectionMode> Projection;
        public static EnumField<GlobeMode> Globe;
        public static BoolField Stretch;
        public static FloatSliderField FisheyeFit;
        public static FloatSliderField PaniniFactor;
        public static FloatSliderField WinkelFit;

        public static FloatField Quality;
        // Real ones remember UltraFOV
        public static KeyCodeField UltraFOVBind;
        public static BoolField EnableZooming;
        //public static BoolField Debug;
        //public static KeyCodeField DebugBind;
        public static BoolField Grid;
        public static KeyCodeField GridBind;
        public static FloatSliderField GridOpac;
        public static BoolField Map;
        public static KeyCodeField MapBind;
        public static FloatSliderField MapOpac;

        //Settings for Globe.
        //public static BoolField ShowGlobeSettings;

        public static FloatSliderField GlobeRotateX;
        public static FloatSliderField GlobeRotateY;
        public static FloatSliderField GlobeRotateZ;

        public static BoolField EnableGlobeSkybox;
        public static IntField GlobeSkyboxFramerate;

        public static List<BoolField> GlobeEnablePlanes = new List<BoolField>();
        public static int GlobeEnabledPlaneMask = int.MaxValue;
        public static BoolField GlobeChangeAllPlaneQualitys;
        public static FloatField GlobeQuality;
        public static List<FloatField> GlobePlaneQualitys = new List<FloatField>();

        public static List<KeyCodeField> projectionBinds = new List<KeyCodeField>();
        public static List<FloatSliderField> projectionFOV = new List<FloatSliderField>();
        public static List<BoolField> projectionStretch = new List<BoolField>();

        public static List<ConfigField> configs = new List<ConfigField>();

        public static ConfigPanel ProjectionPanel;
        public static ConfigPanel GlobePanel;
        public static ConfigPanel MiscPanel;

        public static void Setup()
        {
            config = PluginConfigurator.Create(PluginInfo.Name, PluginInfo.GUID);
            

            configs.Add(UltraFOV = new BoolField(config.rootPanel, "Enable Mod", "bool.ultrafov", true));

            configs.Add(UltraFOVBind = new KeyCodeField(config.rootPanel, "Activation Keybind", "keycode.ultrafov", UnityEngine.KeyCode.None));

            ProjectionPanel = new ConfigPanel(config.rootPanel, "Projection Settings", "panel.projection");
            GlobePanel = new ConfigPanel(config.rootPanel, "Globe Settings", "panel.globe");
            MiscPanel = new ConfigPanel(config.rootPanel, "Misc Settings", "panel.misc");

            
            /*configs.Add(Debug = new BoolField(config.rootPanel, "Debug View", "bool.debug", false));
            configs.Add(DebugBind = new KeyCodeField(config.rootPanel, "Debug Keybind", "keycode.debug", UnityEngine.KeyCode.None));*/
            
            //configs.Add(PlayerFOV = new FloatSliderField(config.rootPanel, "Player Fov", "slider.playerfov", new Tuple<float, float>(0, 360), 360, 0, true, true));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                float defaultFOV = 360;
                switch((ProjectionMode)i)
                {
                    case ProjectionMode.Panini:
                        defaultFOV = 200;
                        break;
                    case ProjectionMode.Stereographic:
                        defaultFOV = 270;
                        break;
                }
                FloatSliderField slider = new FloatSliderField(ProjectionPanel, "Player Fov", "slider.playerfov." + ((ProjectionMode)i).ToString().ToLower(), new Tuple<float, float>(0, 360), defaultFOV, 0, true, true);
                configs.Add(slider);
                projectionFOV.Add(slider);
            }
            configs.Add(Projection = new EnumField<ProjectionMode>(ProjectionPanel, "Projection", "enum.projection", ProjectionMode.Panini));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                KeyCodeField bind = new KeyCodeField(ProjectionPanel, ((ProjectionMode)i).ToString() + " Keybind", "keycode.projection." + ((ProjectionMode)i).ToString().ToLower(), UnityEngine.KeyCode.None);
                configs.Add(bind);
                projectionBinds.Add(bind);
            }

            configs.Add(FisheyeFit = new FloatSliderField(ProjectionPanel, "Fisheye Fit", "slider.fisheyefit", new Tuple<float, float>(0, 2), 0, 2));
            configs.Add(PaniniFactor = new FloatSliderField(ProjectionPanel, "Panini Intensity", "slider.paninifactor", new Tuple<float, float>(0, 1), 0.45f, 2));
            configs.Add(WinkelFit = new FloatSliderField(ProjectionPanel, "Winkel Fit", "slider.winkelfit", new Tuple<float, float>(0, 1), 0f, 2));

            //configs.Add(Stretch = new BoolField(config.rootPanel, "Ignore Aspect Ratio", "bool.stretch", false));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                BoolField ignore = new BoolField(ProjectionPanel, "Ignore Aspect Ratio", "bool.stretch." + ((ProjectionMode)i).ToString().ToLower(), false);
                configs.Add(ignore);
                projectionStretch.Add(ignore);
            }

            configs.Add(Quality = new FloatField(GlobePanel, "Quality", "float.quality", 9, 0, 10));

            //configs.Add(ShowGlobeSettings = new BoolField(config.rootPanel, "Show Globe Settings", "bool.showglobesettings", true));
            configs.Add(Globe = new EnumField<GlobeMode>(GlobePanel, "Globe", "enum.globe", GlobeMode.Cube));

            configs.Add(EnableGlobeSkybox = new BoolField(GlobePanel, "Enable Globe Skybox", "bool.globeskybox", true));
            configs.Add(GlobeSkyboxFramerate = new IntField(GlobePanel, "Globe Skybox Refresh Rate", "int.globeskyboxfps", 60));

            configs.Add(GlobeRotateX = new FloatSliderField(GlobePanel, "Globe X Rotation", "slider.globerotx", new Tuple<float, float>(-1, 1), 0, 2));
            configs.Add(GlobeRotateY = new FloatSliderField(GlobePanel, "Globe Y Rotation", "slider.globeroty", new Tuple<float, float>(-1, 1), 0, 2));
            configs.Add(GlobeRotateZ = new FloatSliderField(GlobePanel, "Globe Z Rotation", "slider.globerotz", new Tuple<float, float>(-1, 1), 0, 2));

            for (int i = 0; i < 6; i++)
            {
                BoolField enablePanel = new BoolField(GlobePanel, "Globe Enable Panel " + (i+1), "bool.globeenablepanel." + (i+1), true);
                configs.Add(enablePanel);
                GlobeEnablePlanes.Add(enablePanel);
            }

            configs.Add(Grid = new BoolField(MiscPanel, "Grid View", "bool.grid", false));
            configs.Add(GridBind = new KeyCodeField(MiscPanel, "Grid Keybind", "keycode.grid", UnityEngine.KeyCode.None));
            configs.Add(GridOpac = new FloatSliderField(MiscPanel, "Grid Opacity", "slider.gridopac", new Tuple<float, float>(0, 1), 0.2f, 2));

            configs.Add(Map = new BoolField(MiscPanel, "Map View", "bool.map", false));
            configs.Add(MapBind = new KeyCodeField(MiscPanel, "Map Keybind", "keycode.map", UnityEngine.KeyCode.None));
            configs.Add(MapOpac = new FloatSliderField(MiscPanel, "Map Opacity", "slider.mapopac", new Tuple<float, float>(0, 1), 0.75f, 2));

            configs.Add(EnableZooming = new BoolField(MiscPanel, "Enable Zooming", "bool.zooming", true));

            //Debug.LogError(PerspectiveMode.Equirectangular.ToString());

            // Dynamically adds functionality to update on field changes
            for (int i = 0; i < configs.Count(); i++)
            {
                Type type = configs[i].GetType();
                if (configs[i] is FloatSliderField)
                {
                    FloatSliderField thing = configs[i] as FloatSliderField;
                    thing.postValueChangeEvent += (e, f) =>
                    {
                        UpdateValeus();
                    };
                } else
                {
                    switch (configs[i]) {
                        case BoolField boolField:
                            boolField.postValueChangeEvent += (e) =>
                            {
                                UpdateValeus();
                            };
                            break;
                        case IntField intField:
                            intField.postValueChangeEvent += (e) =>
                            {
                                UpdateValeus();
                            };
                            break;
                        /*case KeyCodeField keyCodeField:
                            *//*keyCodeField.onValueChange += (e) =>
                            {
                                if (keyCodeField.value != e.value)
                                {
                                    UpdateValeus();
                                }
                            };*//*
                            break;*/
                        case EnumField<ProjectionMode> enumField:
                            enumField.postValueChangeEvent += (e) =>
                            {
                                UpdateValeus();
                            };
                            break;
                        case EnumField<GlobeMode> enumField2:
                            enumField2.postValueChangeEvent += (e) =>
                            {
                                UpdateValeus();
                            };
                            break;
                        case FloatField floatField:
                            floatField.postValueChangeEvent += (e) =>
                            {
                                UpdateValeus();
                            };
                            break;
                    }
                }
            }

            UpdateValeus();

            string workingDirectory = Utils.ModDir();
            string iconFilePath = Path.Combine(Path.Combine(workingDirectory, "Data"), "icon.png");
            ConfigManager.config.SetIconWithURL("file://" + iconFilePath);
        }
        public static void Update()
        {
            if (Input.GetKeyUp(UltraFOVBind.value))
            {
                UltraFOV.value = !UltraFOV.value;
                UpdateValeus();
            }
            if (UltraFOV.value || true)
            {
                if (Input.GetKeyUp(MapBind.value))
                {
                    Map.value = !Map.value;
                    UpdateValeus();
                }
                if (Input.GetKeyUp(GridBind.value))
                {
                    Grid.value = !Grid.value;
                    UpdateValeus();
                }
                for (int i = 0; i < projectionBinds.Count(); i++)
                {
                    if (Input.GetKeyUp(projectionBinds[i].value))
                    {
                        Projection.value = ((ProjectionMode)i);
                        UpdateValeus();
                    }
                }
            }
        }
        public static void UpdateValeus()
        {
            Plugin.UltraFOV = UltraFOV.value;
            //PostProssesingBaby.Debug = Debug.value;
            Plugin.EnableGlobeSkybox = EnableGlobeSkybox.value;
            Plugin.GlobeSkyboxFramerate = GlobeSkyboxFramerate.value;
            PostProssesingBaby.Grid = Grid.value;
            PostProssesingBaby.GridOpac = GridOpac.value;
            PostProssesingBaby.Map = Map.value;
            PostProssesingBaby.MapOpac = MapOpac.value;
            PostProssesingBaby.Projection = Projection.value;
            Plugin.Globe = Globe.value;

            //PostProssesingBaby.PlayerFOV = projectionFOV[(int)Projection.value].value;//PlayerFOV.value;
            if (Plugin.DefaultFOV != projectionFOV[(int)Projection.value].value)
            {
                Plugin.DefaultFOV = projectionFOV[(int)Projection.value].value;
                Plugin.FOV = Plugin.DefaultFOV;
                PostProssesingBaby.PlayerFOV = Plugin.FOV;
            }

            Plugin.EnableZooming = EnableZooming.value;

            PostProssesingBaby.Quality = Quality.value;
            PostProssesingBaby.FisheyeFit = FisheyeFit.value;
            PostProssesingBaby.Stretch = projectionStretch[(int)Projection.value].value;//Stretch.value;
            PostProssesingBaby.PaniniFactor = PaniniFactor.value;
            PostProssesingBaby.WinkelFit = WinkelFit.value;

            PostProssesingBaby.GlobeRotX = GlobeRotateX.value;
            Plugin.GlobeRotX = GlobeRotateX.value;
            PostProssesingBaby.GlobeRotY = GlobeRotateY.value;
            Plugin.GlobeRotY = GlobeRotateY.value;
            PostProssesingBaby.GlobeRotZ = GlobeRotateZ.value;
            Plugin.GlobeRotZ = GlobeRotateZ.value;


            GlobeEnabledPlaneMask = 0;
            for (int i = 0; i < GlobeEnablePlanes.Count; i++)
            {
                int value = GlobeEnablePlanes[i].value ? 1 : 0;
                GlobeEnabledPlaneMask += (value << i);
            }

            Plugin.GlobeMask = GlobeEnabledPlaneMask;

            for (int i = 0; i < configs.Count(); i++)
            {
                if (configs[i].guid != "bool.ultrafov")
                {
                    configs[i].hidden = true;
                }
            }




            // Projections
            UltraFOV.hidden = false;
            if (UltraFOV.value)
            {
                UltraFOVBind.hidden = false;
            }
            Projection.hidden = false;
            projectionBinds[(int)Projection.value].hidden = false;
            projectionFOV[(int)Projection.value].hidden = false;
            projectionStretch[(int)Projection.value].hidden = false;
            EnableZooming.hidden = false;

            switch (Projection.value)
            {
                case ProjectionMode.Equirectangular:
                    break;
                case ProjectionMode.Fisheye:
                    FisheyeFit.hidden = false;
                    break;
                case ProjectionMode.Stereographic:
                    break;
                case ProjectionMode.Hammer:
                    break;
                case ProjectionMode.Panini:
                    PaniniFactor.hidden = false;
                    break;
                case ProjectionMode.Winkel:
                    WinkelFit.hidden = false;
                    break;
            }

            // Globes
            Globe.hidden = false;
            Quality.hidden = false;
            EnableGlobeSkybox.hidden = false;
            if (EnableGlobeSkybox.value)
            {
                GlobeSkyboxFramerate.hidden = false;
            }
            GlobeRotateX.hidden = false;
            GlobeRotateY.hidden = false;
            GlobeRotateZ.hidden = false;
            for (int i = 0; i < GlobeEnablePlanes.Count; i++)
            {
                GlobeEnablePlanes[i].hidden = false;
            }

            // Misc
            Grid.hidden = false;
            Map.hidden = false;
            if (Grid.value)
            {
                GridOpac.hidden = false;
                GridBind.hidden = false;
            }
            if (Map.value)
            {
                MapOpac.hidden = false;
                MapBind.hidden = false;
            }
            
        }
    }
}