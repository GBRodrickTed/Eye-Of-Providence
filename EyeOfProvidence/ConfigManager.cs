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

namespace EyeOfProvidence
{
    public enum ProjectionMode
    {
        Equirectangular = 0,
        Fisheye = 1,
        Stereographic = 2,
        Hammer = 3,
        Panini = 4
    }
    public enum GlobeMode
    {
        Cube = 0
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
        public static BoolField Stretch;
        public static FloatSliderField FisheyeFit;
        public static FloatSliderField PaniniFactor;

        public static FloatField Quality;
        // Real ones remember UltraFOV
        public static KeyCodeField UltraFOVBind;
        //public static BoolField Debug;
        //public static KeyCodeField DebugBind;
        public static BoolField Grid;
        public static KeyCodeField GridBind;
        public static FloatSliderField GridOpac;
        public static BoolField Map;
        public static KeyCodeField MapBind;
        public static FloatSliderField MapOpac;

        //Settings for Globe.
        public static BoolField ShowGlobeSettings;

        public static FloatSliderField GlobeRotateX;
        public static FloatSliderField GlobeRotateY;
        public static FloatSliderField GlobeRotateZ;

        public static List<BoolField> GlobeEnablePlanes = new List<BoolField>();
        public static int GlobeEnabledPlaneMask = int.MaxValue;
        public static BoolField GlobeChangeAllPlaneQualitys;
        public static FloatField GlobeQuality;
        public static List<FloatField> GlobePlaneQualitys = new List<FloatField>();

        public static List<KeyCodeField> projectionBinds = new List<KeyCodeField>();
        public static List<FloatSliderField> projectionFOV = new List<FloatSliderField>();
        public static List<BoolField> projectionStretch = new List<BoolField>();

        public static List<ConfigField> configs = new List<ConfigField>();

        public static void Setup()
        {
            config = PluginConfigurator.Create(PluginInfo.Name, PluginInfo.GUID);

            configs.Add(UltraFOV = new BoolField(config.rootPanel, "Enable Mod", "bool.ultrafov", true));
            configs.Add(UltraFOVBind = new KeyCodeField(config.rootPanel, "Activation Keybind", "keycode.ultrafov", UnityEngine.KeyCode.None));
            /*configs.Add(Debug = new BoolField(config.rootPanel, "Debug View", "bool.debug", false));
            configs.Add(DebugBind = new KeyCodeField(config.rootPanel, "Debug Keybind", "keycode.debug", UnityEngine.KeyCode.None));*/
            
            //configs.Add(PlayerFOV = new FloatSliderField(config.rootPanel, "Player Fov", "slider.playerfov", new Tuple<float, float>(0, 360), 360, 0, true, true));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                float defaultFOV = 360;
                switch((ProjectionMode)i)
                {
                    case ProjectionMode.Panini:
                        defaultFOV = 180;
                        break;
                    case ProjectionMode.Stereographic:
                        defaultFOV = 270;
                        break;
                }
                FloatSliderField slider = new FloatSliderField(config.rootPanel, "Player Fov", "slider.playerfov." + ((ProjectionMode)i).ToString().ToLower(), new Tuple<float, float>(0, 360), defaultFOV, 0, true, true);
                configs.Add(slider);
                projectionFOV.Add(slider);
            }
            configs.Add(Projection = new EnumField<ProjectionMode>(config.rootPanel, "Projection", "enum.projection", ProjectionMode.Panini));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                KeyCodeField bind = new KeyCodeField(config.rootPanel, ((ProjectionMode)i).ToString() + " Keybind", "keycode.projection." + ((ProjectionMode)i).ToString().ToLower(), UnityEngine.KeyCode.None);
                configs.Add(bind);
                projectionBinds.Add(bind);
            }

            configs.Add(FisheyeFit = new FloatSliderField(config.rootPanel, "Fisheye Fit", "slider.fisheyefit", new Tuple<float, float>(0, 2), 0, 2));
            configs.Add(PaniniFactor = new FloatSliderField(config.rootPanel, "Panini Intensity", "slider.paninifactor", new Tuple<float, float>(0, 1), 0.5f, 2));

            configs.Add(Quality = new FloatField(config.rootPanel, "Quality", "float.quality", 9, 0, 10));

            //configs.Add(Stretch = new BoolField(config.rootPanel, "Ignore Aspect Ratio", "bool.stretch", false));
            for (int i = 0; i < Enum.GetNames(typeof(ProjectionMode)).Length; i++)
            {
                BoolField ignore = new BoolField(config.rootPanel, "Ignore Aspect Ratio", "bool.stretch." + ((ProjectionMode)i).ToString().ToLower(), false);
                configs.Add(ignore);
                projectionStretch.Add(ignore);
            }

            configs.Add(ShowGlobeSettings = new BoolField(config.rootPanel, "Show Globe Settings", "bool.showglobesettings", false));

            configs.Add(GlobeRotateX = new FloatSliderField(config.rootPanel, "Globe X Rotation", "slider.globerotx", new Tuple<float, float>(-1, 1), 0, 2));
            configs.Add(GlobeRotateY = new FloatSliderField(config.rootPanel, "Globe Y Rotation", "slider.globeroty", new Tuple<float, float>(-1, 1), 0, 2));
            configs.Add(GlobeRotateZ = new FloatSliderField(config.rootPanel, "Globe Z Rotation", "slider.globerotz", new Tuple<float, float>(-1, 1), 0, 2));

            for (int i = 0; i < 6; i++)
            {
                BoolField enablePanel = new BoolField(config.rootPanel, "Globe Enable Panel " + (i+1), "bool.globeenablepanel." + (i+1), true);
                configs.Add(enablePanel);
                GlobeEnablePlanes.Add(enablePanel);
            }

            configs.Add(Grid = new BoolField(config.rootPanel, "Grid View", "bool.grid", false));
            configs.Add(GridBind = new KeyCodeField(config.rootPanel, "Grid Keybind", "keycode.grid", UnityEngine.KeyCode.None));
            configs.Add(GridOpac = new FloatSliderField(config.rootPanel, "Grid Opacity", "slider.gridopac", new Tuple<float, float>(0, 1), 0.2f, 2));

            configs.Add(Map = new BoolField(config.rootPanel, "Map View", "bool.map", false));
            configs.Add(MapBind = new KeyCodeField(config.rootPanel, "Map Keybind", "keycode.map", UnityEngine.KeyCode.None));
            configs.Add(MapOpac = new FloatSliderField(config.rootPanel, "Map Opacity", "slider.mapopac", new Tuple<float, float>(0, 1), 0.75f, 2));

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
            if (OptionsManager.Instance && !OptionsManager.Instance.paused)
            {
                if (Input.GetKeyUp(UltraFOVBind.value))
                {
                    UltraFOV.value = !UltraFOV.value;
                    UpdateValeus();
                }
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
            PostProssesingBaby.Grid = Grid.value;
            PostProssesingBaby.GridOpac = GridOpac.value;
            PostProssesingBaby.Map = Map.value;
            PostProssesingBaby.MapOpac = MapOpac.value;
            PostProssesingBaby.Projection = Projection.value;
            PostProssesingBaby.PlayerFOV = projectionFOV[(int)Projection.value].value;//PlayerFOV.value;
            PostProssesingBaby.Quality = Quality.value;
            PostProssesingBaby.FisheyeFit = FisheyeFit.value;
            PostProssesingBaby.Stretch = projectionStretch[(int)Projection.value].value;//Stretch.value;
            PostProssesingBaby.PaniniFactor = PaniniFactor.value;

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
            
            if (UltraFOV.value)
            {
                //DebugBind.hidden = false;
                UltraFOVBind.hidden = false;
                //Debug.hidden = false;
                Grid.hidden = false;
                Map.hidden = false;

                //PlayerFOV.hidden = false;
                Projection.hidden = false;
                Quality.hidden = false;

                ShowGlobeSettings.hidden = false;
                //Stretch.hidden = false;

                if (ShowGlobeSettings.value)
                {
                    GlobeRotateX.hidden = false;
                    GlobeRotateY.hidden = false;
                    GlobeRotateZ.hidden = false;
                    for (int i = 0; i < GlobeEnablePlanes.Count; i++)
                    {
                        GlobeEnablePlanes[i].hidden = false;
                    }
                }

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

                switch (Projection.value)
                {
                    case ProjectionMode.Equirectangular:
                        break;
                    case ProjectionMode.Fisheye:
                        FisheyeFit.hidden = false;
                        break;
                    case ProjectionMode.Stereographic:
                        //PlayerFOV.hidden = true;
                        break;
                    case ProjectionMode.Hammer:
                        break;
                    case ProjectionMode.Panini:
                        //PlayerFOV.hidden = true;
                        PaniniFactor.hidden = false;
                        break;
                }
                projectionBinds[(int)Projection.value].hidden = false;
                projectionFOV[(int)Projection.value].hidden = false;
                projectionStretch[(int)Projection.value].hidden = false;
            }
            
        }
    }
}