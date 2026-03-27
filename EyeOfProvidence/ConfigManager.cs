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

namespace EyeOfProvidence
{
    public enum PerspectiveMode
    {
        Equirectangular = 0,
        Fisheye = 1,
        Stereographic = 2,
        Hammer = 3,
        Panini = 4
    }
    public static class ConfigManager
    {
        public static PluginConfigurator config;
        public static BoolField UltraFOV;
        public static FloatField PlayerFOVUncapped;
        public static FloatSliderField PlayerFOV;
        public static BoolField PlayerFOVShouldUncap;
        //public static BoolField Fisheye;
        public static EnumField<PerspectiveMode> Perspective;
        public static BoolField Stretch;
        public static FloatSliderField FisheyeFit;
        public static FloatSliderField StereoFactor;
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

        public static List<ConfigField> configs = new List<ConfigField>();

        public static void Setup()
        {
            config = PluginConfigurator.Create(PluginInfo.Name, PluginInfo.GUID);

            //new ConfigHeader(config.rootPanel, "<color=red>Does not confict with modded aim assist</color>", 15);
            configs.Add(UltraFOV = new BoolField(config.rootPanel, "Enable Mod", "bool.ultrafov", true));
            configs.Add(UltraFOVBind = new KeyCodeField(config.rootPanel, "Activation Keybind", "keycode.ultrafov", UnityEngine.KeyCode.None));
            /*configs.Add(Debug = new BoolField(config.rootPanel, "Debug View", "bool.debug", false));
            configs.Add(DebugBind = new KeyCodeField(config.rootPanel, "Debug Keybind", "keycode.debug", UnityEngine.KeyCode.None));*/
            configs.Add(Grid = new BoolField(config.rootPanel, "Grid View", "bool.grid", false));
            configs.Add(GridBind = new KeyCodeField(config.rootPanel, "Grid Keybind", "keycode.grid", UnityEngine.KeyCode.None));
            configs.Add(GridOpac = new FloatSliderField(config.rootPanel, "Grid Opacity", "slider.gridopac", new Tuple<float, float>(0, 1), 0.2f, 2));
            
            configs.Add(Map = new BoolField(config.rootPanel, "Map View", "bool.map", false));
            configs.Add(MapBind = new KeyCodeField(config.rootPanel, "Map Keybind", "keycode.map", UnityEngine.KeyCode.None));
            configs.Add(MapOpac = new FloatSliderField(config.rootPanel, "Map Opacity", "slider.mapopac", new Tuple<float, float>(0, 1), 0.75f, 2));
            
            configs.Add(PlayerFOV = new FloatSliderField(config.rootPanel, "Player Fov", "slider.playerfov", new Tuple<float, float>(0, 360), 360, 0, true, true));
            configs.Add(Perspective = new EnumField<PerspectiveMode>(config.rootPanel, "Perspective", "enum.perspective", PerspectiveMode.Panini));        
            configs.Add(Stretch = new BoolField(config.rootPanel, "Stretch to View", "bool.stretch", true));

            configs.Add(FisheyeFit = new FloatSliderField(config.rootPanel, "Fisheye Fit", "slider.fisheyefit", new Tuple<float, float>(0, 2), 0, 2));
            configs.Add(StereoFactor = new FloatSliderField(config.rootPanel, "Stereographic Scale", "slider.stereofactor", new Tuple<float, float>(0, 3), 0, 2));
            configs.Add(PaniniFactor = new FloatSliderField(config.rootPanel, "Panini Intensity", "slider.paninifactor", new Tuple<float, float>(0, 1), 1, 2));

            configs.Add(Quality = new FloatField(config.rootPanel, "Quality", "float.quality", 9, 0, 10));

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
                        case KeyCodeField keyCodeField:
                            /*keyCodeField.onValueChange += (e) =>
                            {
                                if (keyCodeField.value != e.value)
                                {
                                    UpdateValeus();
                                }
                            };*/
                            break;
                        case EnumField<PerspectiveMode> enumField:
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
            if (Input.GetKeyDown(UltraFOVBind.value))
            {
                UltraFOV.value = !UltraFOV.value;
                UpdateValeus();
            }
            if (Input.GetKeyDown(MapBind.value))
            {
                Map.value = !Map.value;
                UpdateValeus();
            }
            if (Input.GetKeyDown(GridBind.value))
            {
                Grid.value = !Grid.value;
                UpdateValeus();
            }
            /*if (Input.GetKeyDown(DebugBind.value))
            {
                Debug.value = !Debug.value;
                UpdateValeus();
            }*/
        }
        public static void UpdateValeus()
        {
            Plugin.UltraFOV = UltraFOV.value;
            //PostProssesingBaby.Debug = Debug.value;
            PostProssesingBaby.Grid = Grid.value;
            PostProssesingBaby.GridOpac = GridOpac.value;
            PostProssesingBaby.Map = Map.value;
            PostProssesingBaby.MapOpac = MapOpac.value;
            PostProssesingBaby.PlayerFOV = PlayerFOV.value;
            PostProssesingBaby.Perspective = Perspective.value;
            PostProssesingBaby.Quality = Quality.value;
            PostProssesingBaby.FisheyeFit = FisheyeFit.value;
            PostProssesingBaby.Stretch = Stretch.value;
            PostProssesingBaby.StereoFactor = StereoFactor.value;
            PostProssesingBaby.PaniniFactor = PaniniFactor.value;

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

                PlayerFOV.hidden = false;
                Perspective.hidden = false;
                Quality.hidden = false;
                Stretch.hidden = false;

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

                switch (Perspective.value)
                {
                    case PerspectiveMode.Equirectangular:
                        break;
                    case PerspectiveMode.Fisheye:
                        FisheyeFit.hidden = false;
                        break;
                    case PerspectiveMode.Stereographic:
                        PlayerFOV.hidden = true;
                        StereoFactor.hidden = false;
                        break;
                    case PerspectiveMode.Hammer:
                        break;
                    case PerspectiveMode.Panini:
                        PlayerFOV.hidden = true;
                        PaniniFactor.hidden = false;
                        break;
                }
            }
            
        }
    }
}