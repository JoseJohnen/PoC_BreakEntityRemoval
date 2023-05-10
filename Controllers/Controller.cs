using Stride.Core.Collections;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Stride.Core.IO;
using Stride.Rendering;
using System.Threading.Tasks;
using Map_Editor_HoD.Enums;
using Stride.Physics;
using Map_Editor_HoD.Assistants;
using Material = Stride.Rendering.Material;
using System.IO;
using Map_Editor_HoD.TilesModels;
using Map_Editor_HoD.WorldModels;
using Stride.UI;
using Stride.UI.Panels;
using Stride.Rendering.Sprites;
using Map_Editor_HoD.Code.Models;
using Stride.Input;
using BulletSharp.Math;

namespace Map_Editor_HoD.Controllers
{
    public class Controller : SyncScript
    {
        public WorldController worldController;
        public static Controller controller;
        public static SceneSystem systemController;
        //private static string instrucciones;

        public IDatabaseFileProviderService dataFileProviderService;

        private Timer CadaCiertosSegundos;
        private Timer CadaMedioSegundo;

        private UInt16 segundosTranscurridos = 0; //Da para un máximo de 65535, que son 18.204167 horas.
        //private UInt16 seguroSegundosTranscurridos = 0;

        private float medioSegundoTranscurridos = 0; //Da para un máximo de 65535, que son 18.204167 horas.
        //private float seguroMedioSegundoTranscurridos = 0;

        UIComponent uIComponent;
        EditText txtX;
        EditText txtY;
        EditText txtName;
        Button btnCreate;
        Button btnDelete;
        Button btnLoad;
        Button btnSave;

        public List<Prefab> l_Prefabs = new List<Prefab>();
        public List<Model> l_Models = new List<Model>();
        public List<SpriteSheet> l_Sprites = new List<SpriteSheet>();
        public List<SpriteSheet> l_Tileset = new List<SpriteSheet>();
        public List<Texture> l_Textures = new List<Texture>();
        public List<Material> l_Materials = new List<Material>();
        public List<UIPage> l_UI = new List<UIPage>();

        public bool movementDisable = false;
        public bool autoMovement = false;
        public bool isHistoryRelatedUsage = false;
        public bool isPressedZToRun = false;

        //bool ActivateOrDeactivateLastText = false;
        //string LastText = string.Empty;

        public bool isLoginInProcess = false;
        public bool isLoginSuccessfull = false;
        public DateTime dtIsLoginInProcess = DateTime.Now;

        public static TaskStatus dataAnswer = TaskStatus.Created;
        public static TaskStatus dataContinous = TaskStatus.Created;

        //public Entity CursorPos = null;
        public string NameOfSelectedType = string.Empty;

        private readonly FastList<CameraComponent> cameraDb = new FastList<CameraComponent>();

        public DateTime lastPositionBeforeMove = DateTime.Now;

        bool textoFueraDeCutscene = false;
        private DateTime dateTimeTextoFueraDeCutScene = DateTime.Now;

        public SpriteSheet MainSceneImages { get; set; }
        public bool TextoFueraDeCutscene
        {
            get => textoFueraDeCutscene; set
            {
                textoFueraDeCutscene = value;
                if (textoFueraDeCutscene == true)
                {
                    dateTimeTextoFueraDeCutScene = DateTime.Now;
                }
            }
        }

        private string Token;

        Tile tile = null;
        public override void Start()
        {
            //Preparing to work itself
            Services.AddService(this);
            base.Start();
            controller = Entity.Get<Controller>();
            //Starting to prepare everything else
            systemController = SceneSystem;

            InitTimer();

            //Prepare Service References
            dataFileProviderService = Services.GetService<IDatabaseFileProviderService>();

            //playerController = Services.GetService<PlayerController>();
            worldController = Services.GetService<WorldController>();
            //connectionManager = Services.GetService<ConnectionManager>();

            //Take active elements to be used
            l_Prefabs = GetItemsFromVirtualGameFolder<Prefab>("Prefabs");
            l_Models = GetItemsFromVirtualGameFolder<Model>("Models");
            l_Sprites = GetItemsFromVirtualGameFolder<SpriteSheet>("Sprites");
            //l_Tileset = GetItemsFromVirtualGameFolder<SpriteSheet>("Tilesets");
            l_Textures = GetItemsFromVirtualGameFolder<Texture>("Textures");
            l_Materials = GetItemsFromVirtualGameFolder<Material>("Materials");
            l_UI = GetItemsFromVirtualGameFolder<UIPage>("UI");


            WorldController.TestWorld = new Map_Editor_HoD.WorldModels.BaseWorld();
            WorldController.TestWorld.WestEast = 3;
            WorldController.TestWorld.FrontBack = 3;
            WorldController.TestWorld.RegisterWorld("Some Test World");
            WorldController.TestWorld.FillWorld("Grass");
            WorldController.TestWorld.InstanceWorldEditorReqMechanics();

            KeyValuePair<string,Tile> kvp = WorldController.TestWorld.dic_worldTiles.First();
            kvp.Value.ChangeType("Sand", kvp.Key);
            
            //TODO: TEST: You just change the instanced sound in the right context, and maybie it start to sound.

            /*DicMusic.Add("BackgroundMusic",BackgroundMusic);
            DicMusic.Add("GhostMusic", GhostMusic);

            DicEffect.Add("GhostLullaby", GhostLullaby);
            DicEffect.Add("GhostScream", GhostScream);
            DicEffect.Add("BabyCry", BabyCry);
            DicEffect.Add("SonidoChoque", SonidoChoque);
            DicEffect.Add("Thunder", Thunder);

            ChangeMusic("BackgroundMusic");*/

            //effect = GhostLullaby.CreateInstance();
            /*audioEmitterComponent = Entity.Get<AudioEmitterComponent>();
            mySound1Controller = audioEmitterComponent["Background Music"];
            mySound2Controller = audioEmitterComponent["Ghost Music"];*/

            PreparingCameras();
            PrepareUI();

            //TODO: Delete when login is Added
        }

        public Controller()
        {

        }

        public override void Update()
        {
            UpdatingCamera();
            WorldController.WorldController_Tick();

            ClickResult clickResult = new ClickResult();
            if (UtilityAssistant.ScreenPositionToWorldPositionRaycast(Input.MousePosition, Controller.controller.GetActiveCamera(), Controller.controller.GetSimulation(), out clickResult))
            {
                if (Input.HasMouse)
                {
                    if (Input.IsMouseButtonDown(MouseButton.Left))
                    {
                        Console.WriteLine("X: " + Input.MousePosition.X + " Y: " + Input.MousePosition.Y);
                        Console.WriteLine("Entity: " + clickResult.ClickedEntity); //clickResult.HitResult;

                        Tile tle = null;
                        WorldController.TestWorld.dic_worldTiles.TryGetValue(clickResult.ClickedEntity.Name, out tle);
                        if (tle != null)
                        {
                            tle.ChangeType(NameOfSelectedType, clickResult.ClickedEntity.Name);
                        }
                    }
                }
            }

            /*foreach (Entity item in this.Entity.Scene.Entities.Reverse())
            {
                if(string.IsNullOrWhiteSpace(item.Name))
                {
                    this.Entity.Scene.Entities.Remove(item);
                }
            }*/
        }

        #region Utilitarios
        #region Preparativos
        public List<T> GetItemsFromVirtualGameFolder<T>(string path, string filterByName = "") where T : class
        {
            try
            {
                string strPrepared = "*";
                if (!String.IsNullOrWhiteSpace(filterByName))
                {
                    strPrepared = "*" + filterByName + "*";
                }

                string[] l_strings = Controller.controller.dataFileProviderService.FileProvider.ListFiles(path, strPrepared, VirtualSearchOption.AllDirectories);
                List<T> l_temp = new List<T>();

                string strTemp = string.Empty;
                int i = 0;
                List<string> l_ignoreStrings = new List<string>();
                bool state = false;
                foreach (string item in l_strings)
                {
                    if (item.Contains("/gen/") || item.Contains("__ATLAS_TEXTURE__0") || item.Contains("_Data"))
                    {
                        if (item.Contains("/gen/"))
                        {
                            strTemp = item.Substring(item.IndexOf("/gen/"));
                            strTemp = item.Replace(strTemp, "");
                        }
                        else if (item.Contains("__ATLAS_TEXTURE__0"))
                        {
                            strTemp = item.Substring(item.IndexOf("__ATLAS_TEXTURE__0"));
                            strTemp = item.Replace(strTemp, "");
                        }
                        else if (item.Contains("_Data"))
                        {
                            strTemp = item.Substring(item.IndexOf("_Data"));
                            strTemp = item.Replace(strTemp, "");
                        }

                        foreach (string itm in l_ignoreStrings)
                        {
                            if (strTemp.ToUpper().Equals(itm.ToUpper()))
                            {
                                state = true;
                                break;
                            }
                        }

                        if (state == true)
                        {
                            state = false;
                            continue;
                        }

                        //Si no ha sido registrado antes y pasa el break
                        T tmp = Content.Load<T>(strTemp);
                        if (tmp.GetType().Name == "SpriteSheet")
                        {
                            strTemp = strTemp.Replace("Sprites/", "");
                            foreach (string itm in l_ignoreStrings)
                            {
                                if (strTemp.ToUpper().Equals(itm.ToUpper()))
                                {
                                    state = true;
                                    break;
                                }
                            }

                            if (state == true)
                            {
                                state = false;
                                continue;
                            }

                            Sprite spr = new Sprite();
                            spr.Name = strTemp;
                            (tmp as SpriteSheet).Sprites.Add(spr);
                            l_ignoreStrings.Add(strTemp);
                        }

                        l_temp.Add(tmp);
                        i++;
                        l_ignoreStrings.Add(item);
                        continue;
                    }

                    T temp = Content.Load<T>(item);
                    l_temp.Add(temp);
                    i++;
                    l_ignoreStrings.Add(item);
                }
                var itemFirst = l_temp[0];
                if (itemFirst.GetType().Name == "SpriteSheet")
                {
                    l_temp = l_temp.GroupBy(x => (x as SpriteSheet).Sprites).Select(y => y.First()).ToList();
                }
                return l_temp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: string GetItemsFromVirtualGameFolder: " + ex.Message);
                return default(List<T>);
            }
        }
        #endregion

        #region Sonido y Musica
        //public void ChangeMusic(string nameSoundToPlay)
        //{
        //    return;
        //    if (music != null)
        //    {
        //        music.Stop();
        //    }
        //    music = null;
        //    music = DicMusic.GetValueOrDefault(nameSoundToPlay).CreateInstance();
        //    music.IsLooping = true;
        //    music.Play();
        //}

        //public void ChangeEffect(string nameSoundToPlay)
        //{
        //    return;
        //    if (effect != null)
        //    {
        //        effect.Stop();
        //    }
        //    effect = null;
        //    effect = DicEffect.GetValueOrDefault(nameSoundToPlay).CreateInstance();
        //    effect.Play();
        //}
        #endregion

        #region Contadores de Tiempo
        private void ContadorDeSegundos(object sender, EventArgs e)
        {
            if (segundosTranscurridos == 65000)
            {
                segundosTranscurridos = 0;
                //TODO: Recuerda que el contador debe regresar al mínimo conveniente, esto quiere decir
                //que debe volver al valor siguiente al valor del segundo de la última acción que ya no se debe realizar
                //que este fijada por este reloj.
            }

            segundosTranscurridos++;
        }

        private void ContadorDeMediosSegundos(object sender, EventArgs e)
        {
            if (medioSegundoTranscurridos == 65000)
            {
                medioSegundoTranscurridos = 0;
                //TODO: Recuerda que el contador debe regresar al mínimo conveniente, esto quiere decir
                //que debe volver al valor siguiente al valor del segundo de la última acción que ya no se debe realizar
                //que este fijada por este reloj.
            }

            medioSegundoTranscurridos += 0.5f;
        }

        public void InitTimer()
        {
            CadaCiertosSegundos = new Timer();
            CadaCiertosSegundos.Elapsed += new ElapsedEventHandler(ContadorDeSegundos);
            CadaCiertosSegundos.Interval = 1000; // in miliseconds

            CadaMedioSegundo = new Timer();
            CadaMedioSegundo.Elapsed += new ElapsedEventHandler(ContadorDeMediosSegundos);
            CadaMedioSegundo.Interval = 500; // in miliseconds
        }
        #endregion

        #region UI
        public void PrepareUI()
        {
            try
            {
                uIComponent = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(a => a.Name == "IntroCamera")?.Get<UIComponent>();
                UIPage re = null;
                if (uIComponent != null)
                {
                    re = uIComponent.Page;
                    txtX = (EditText)re.RootElement.FindName("txtX");
                    txtY = (EditText)re.RootElement.FindName("txtY");
                    txtName = (EditText)re.RootElement.FindName("txtName");

                    btnCreate = (Button)re.RootElement.FindName("btnCreate");
                    btnDelete = (Button)re.RootElement.FindName("btnDelete");
                    btnLoad = (Button)re.RootElement.FindName("btnLoad");
                    btnSave = (Button)re.RootElement.FindName("btnSave");

                    btnCreate.Click += BtnCreate_Click;
                    btnDelete.Click += BtnDelete_Click;
                    btnLoad.Click += BtnLoad_Click;
                    btnSave.Click += BtnSave_Click;

                    Grid grd = (Grid)re.RootElement.FindName("SuperGralifragilisticaGrilla"); //(Grid)re.RootElement.FindVisualRoot();
                    grd.RowDefinitions.Add(new StripDefinition());

                    Button nwButton = null;
                    SpriteFromSheet spSht = null;

                    //float topBase = -125;
                    //float leftBase = -625;

                    int i = 1;
                    int j = 0;
                    int m = 0;
                    SpriteSheet lastSpriteSheet = new SpriteSheet();
                    List<Pares<SpriteSheet, Sprite>> sprites = new List<Pares<SpriteSheet, Sprite>>();

                    foreach (SpriteSheet spriteSheet in l_Tileset)
                    {
                        foreach (Sprite item in spriteSheet.Sprites)
                        {
                            if (!item.Name.Contains("Tilesets/Tiles"))
                            {
                                sprites.Add(new Pares<SpriteSheet, Sprite>(spriteSheet, item));
                            }
                        }
                    }

                    for (int k = 0; k < Math.Ceiling(Convert.ToDecimal(sprites.Count / 2)); k++)
                    {
                        grd.RowDefinitions.Add(new StripDefinition());
                    }

                    foreach (Pares<SpriteSheet, Sprite> item in sprites)
                    {
                        if (!lastSpriteSheet.Equals(item.Item1))
                        {
                            lastSpriteSheet = item.Item1;
                            m = 0;
                        }

                        nwButton = new Button();
                        nwButton.Name = "btn" + item.Item2.Name;
                        nwButton.SetGridRow(j);

                        spSht = SpriteFromSheet.Create(item.Item1, item.Item2.Name);
                        spSht.Sheet = item.Item1;
                        spSht.CurrentFrame = m;
                        nwButton.NotPressedImage = (ISpriteProvider)spSht;

                        nwButton.Width = 92;
                        nwButton.Height = 38;

                        grd.Children.Add(nwButton);

                        nwButton.SetGridColumn(0);

                        if (i % 2 == 0 && i != 0)
                        {
                            nwButton.SetGridColumn(1);
                            j++;
                        }

                        nwButton.Click += (s, e) =>
                        {
                            NameOfSelectedType = item.Item2.Name;
                            Console.WriteLine("NameOfSelectedType: " + NameOfSelectedType);
                        };

                        i++;
                        m++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error PrepareUI: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, Stride.UI.Events.RoutedEventArgs e)
        {
            try
            {
                txtX.IsSelectionActive = false;
                txtY.IsSelectionActive = false;
                txtName.IsSelectionActive = false;

                WorldModels.World empty = null;
                if (WorldController.dic_worlds.Count == 0)
                {
                    return;
                }

                if (WorldController.TestWorld != null)
                {
                    WorldController.dic_worlds.Remove(WorldController.TestWorld.Name, out empty);

                    foreach (Tile item in WorldController.TestWorld.dic_worldTiles.Values)
                    {
                        this.Entity.Scene.Entities.Remove(item.Entity);
                    }
                    WorldController.TestWorld.dic_worldTiles.Clear();
                    WorldController.TestWorld = null;
                    WorldController.dic_worlds.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BtnDelete_Click: " + ex.Message);
            }
        }

        private void BtnCreate_Click(object sender, Stride.UI.Events.RoutedEventArgs e)
        {
            try
            {
                int resultX = 0;
                int resultY = 0;
                txtX.IsSelectionActive = false;
                txtY.IsSelectionActive = false;
                txtName.IsSelectionActive = false;

                if (string.IsNullOrWhiteSpace(txtX.Text))
                {
                    Console.WriteLine("Error: X cannot be empty");
                    return;
                }

                if (!int.TryParse(txtX.Text, out resultX))
                {
                    Console.WriteLine("Error: X must be a valid, positive, natural number");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtY.Text))
                {
                    Console.WriteLine("Error: Y cannot be empty");
                    return;
                }

                if (!int.TryParse(txtY.Text, out resultY))
                {
                    Console.WriteLine("Error: X must be a valid, positive, natural number");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    Console.WriteLine("Error: Name cannot be empty");
                    return;
                }

                if (WorldController.TestWorld == null)
                {
                    WorldController.TestWorld = new Map_Editor_HoD.WorldModels.BaseWorld();
                    WorldController.TestWorld.WestEast = resultX;
                    WorldController.TestWorld.FrontBack = resultY;
                    WorldController.TestWorld.RegisterWorld(txtName.Text);
                    WorldController.TestWorld.FillWorld("Grass");
                    WorldController.TestWorld.InstanceWorldEditorReqMechanics();
                }
                //string b = TestWorld.ToJson();
                //World c = World.CreateFromJson(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BtnCreate_Click: " + ex.Message);
            }
        }

        private void BtnLoad_Click(object sender, Stride.UI.Events.RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    Console.WriteLine("Error: Name cannot be empty");
                    return;
                }

                // Get the path to the directory where the program is executing
                string directoryPath = Path.GetDirectoryName(typeof(Game).Assembly.Location);
                // Define the subfolder name and full path to the subfolder
                string subfolderName = "Worlds";
                string subfolderPath = Path.Combine(directoryPath, subfolderName);

                // Check if the subfolder exists; if not, create it
                if (!Directory.Exists(subfolderPath))
                {
                    Directory.CreateDirectory(subfolderPath);
                    return; //Porque en este caso no hay ningún archivo que cargar
                }

                // Define the filename and full path to the text file
                string filename = txtName.Text + ".wrld";
                string filepath = Path.Combine(subfolderPath, filename);

                string fileContents = string.Empty;
                using (StreamReader reader = new StreamReader(filepath))
                {
                    fileContents = reader.ReadToEnd();
                }
                //Aca se carga el archivo y se carga el mundo
                WorldController.TestWorld = WorldModels.World.CreateFromJson(fileContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BtnLoad_Click: " + ex.Message);
                WorldController.TestWorld = new BaseWorld();
            }
        }

        private void BtnSave_Click(object sender, Stride.UI.Events.RoutedEventArgs e)
        {
            try
            {
                string fileName = string.Empty;
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    Console.WriteLine("Error: Name cannot be empty");
                    return;
                }

                if (WorldController.TestWorld == null)
                {
                    Console.WriteLine("Error: You need to have a World created in order to been able to save it");
                    return;
                }

                fileName = WorldController.TestWorld.Name + ".wrld";

                // Get the path to the directory where the program is executing
                string directoryPath = Path.GetDirectoryName(typeof(Game).Assembly.Location);

                // Define the subfolder name and full path to the subfolder
                string subfolderName = "Worlds";
                string subfolderPath = Path.Combine(directoryPath, subfolderName);

                // Check if the subfolder exists; if not, create it
                if (!Directory.Exists(subfolderPath))
                {
                    Directory.CreateDirectory(subfolderPath);
                }

                // Define the filename and full path to the text file within the subfolder
                string filepath = Path.Combine(subfolderPath, fileName);

                // Write the string to the text file
                using (StreamWriter writer = new StreamWriter(filepath))
                {
                    writer.Write(WorldController.TestWorld.ToJson());
                }

                //Aca se crea el archivo donde se guarda el mundo
                //string b = TestWorld.ToJson();
                //World c = World.CreateFromJson(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BtnSave_Click: " + ex.Message);
            }
        }
        #endregion

        #region Cameras
        //Prepare the Original suit of Cameras to work, you may want to add all the starting cameras in this method at start unless that cameras will be added at runtime, in such a case Register camera is the way.
        public void PreparingCameras()
        {
            foreach (Entity item in SceneSystem.SceneInstance.RootScene.Entities)
            {
                CameraComponent a = item.Get<CameraComponent>();
                if (a != null)
                {
                    Services.GetService<Controller>().RegisterCamera(a);
                }
            }
            //Services.GetService<Controller>().RegisterCamera(SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(a => a.Name == "IntroCamera")?.Get<CameraComponent>());
        }

        public void UpdatingCamera()
        {

        }

        //Make the Camera procesed by this method the only Active Camera
        public void ActivateCamera(CameraComponent cameraComponent)
        {
            if (cameraComponent == null)
            {
                Log.Error("Entity attempted to activate a camera when no camera is attached.");
                return;
            }

            foreach (var camera in cameraDb)
            {
                camera.Enabled = false;
            }

            cameraComponent.Enabled = true;
            Log.Info($"{cameraComponent.Entity.Name} has been activated.");
        }

        //Make a Camera searched by his name the only Active Camera
        public void ActivateCamera(string cameraComponentName)
        {
            if (string.IsNullOrWhiteSpace(cameraComponentName))
            {
                Log.Error("Camera couldn't be found, are you certain it was registered using the RegisterCamera method?");
                return;
            }

            foreach (var camera in cameraDb)
            {
                if (string.IsNullOrWhiteSpace(camera.Name))
                {
                    camera.Name = "";
                }

                if (camera.Name.ToUpper() == cameraComponentName.ToUpper())
                {
                    camera.Enabled = true;
                    break;
                }
            }

            foreach (var camera in cameraDb)
            {
                if (camera.Name.ToUpper() != cameraComponentName.ToUpper())
                {
                    camera.Enabled = false;
                }
            }
            //Log.Info($"{cameraComponent.Entity.Name} has been activated.");
        }

        //Change the active camera to the next Camera in the list, also return the new active Camera.
        public CameraComponent NextCamera()
        {
            try
            {
                CameraComponent cameraComponent = default(CameraComponent);
                int i = 0;
                foreach (var camera in cameraDb)
                {
                    if (camera.Enabled == true)
                    {
                        camera.Enabled = false;
                        if (i >= (cameraDb.Count - 1))
                        {
                            i = 0;
                            cameraDb[i].Enabled = true;
                            cameraComponent = cameraDb[i];
                            Log.Info($"{cameraDb[i].Entity.Name} has been activated.");
                            break;
                        }
                        cameraDb[i + 1].Enabled = true;
                        cameraComponent = cameraDb[i + 1];
                        Log.Info($"{cameraDb[i + 1].Entity.Name} has been activated.");
                        break;
                    }
                    i++;
                }
                return cameraComponent;
            }
            catch (Exception ex)
            {
                Log.Error("Error NextCamera(): " + ex.Message);
                return default(CameraComponent);
            }
        }

        //internal static void SetInstrucciones(string returned)
        //{
        //    instrucciones = returned;

        //    //TODO: Acá se separan las instrucciones para cada controlador para después repartirlas entre ellos
        //    PlayerController.SetInstrucciones(instrucciones);
        //}

        //Return the Active Camera from the list of Cameras, if non, it will return a default.
        public CameraComponent GetActiveCamera()
        {
            try
            {
                CameraComponent cameraComponent = default(CameraComponent);
                foreach (var camera in cameraDb)
                {
                    if (camera.Enabled == true)
                    {
                        return camera;
                    }
                }
                return cameraComponent;
            }
            catch (Exception ex)
            {
                Log.Error("Error GetActiveCamera(): " + ex.Message);
                return default(CameraComponent);
            }
        }

        //It allow to add a Camera to the list of Cameras, avoid repetitions.
        public void RegisterCamera(CameraComponent cameraComponent)
        {
            if (cameraComponent == null)
            {
                Log.Error("Entity attempted to register a camera when no camera is attached.");
                return;
            }

            if (!cameraDb.Contains(cameraComponent))
            {
                if (cameraDb.Count > 0)
                {
                    if (GetActiveCamera().Name != cameraComponent.Name)
                    {
                        cameraComponent.Enabled = false;
                    }
                }

                cameraDb.Add(cameraComponent);
                Log.Info($"{cameraComponent?.Name} camera has been registered with camera db.");
            }
        }
        #endregion

        #region Auto-Extractores
        //    l_Models = GetItemsFromVirtualGameFolder<Model>("Models");
        //    l_Textures = GetItemsFromVirtualGameFolder<Texture>("Textures");
        //    l_Materials = GetItemsFromVirtualGameFolder<Material>("Materials");

        public List<Entity> GetPrefab(string prefabName)
        {
            try
            {
                Prefab prefab = null;
                prefab = this.l_Prefabs.Where(D => D.Entities[0].Name.ToUpper() == prefabName.ToUpper()).FirstOrDefault();
                if (prefab != default(Prefab))
                {
                    return prefab.Instantiate();
                }
                return new List<Entity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new List<Entity>();
            }
        }

        /*public List<Entity> GetModel(string modelName)
        {
            try
            {
                //Model model = null;
                //model = this.l_Models.Where(D => D.Instantiate()..ToUpper() == modelName.ToUpper()).FirstOrDefault();
                //if (model != default(Model))
                //{
                //    return prefab.Instantiate();
                //}
                foreach (var item in l_Models)
                {
                    Console.WriteLine("-----");
                    Console.WriteLine(item);
                    Console.WriteLine("-----");
                }
                return new List<Entity>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new List<Entity>();
            }
        }*/

        public SpriteSheet GetSpriteSheet(string spriteSheetName)
        {
            try
            {
                SpriteSheet spriteSheet = null;
                spriteSheet = this.l_Sprites.Where(D => D.Sprites[(D.Sprites.Count - 1)].Name.ToUpper() == spriteSheetName.ToUpper()).FirstOrDefault();

                if (spriteSheet != default(SpriteSheet))
                {
                    return spriteSheet;
                }
                return new SpriteSheet();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetSpriteSheet(string): " + ex.Message);
                return new SpriteSheet();
            }
        }

        /*public Texture GetTexture(string textureName)
        {
            try
            {
                //Model model = null;
                //model = this.l_Models.Where(D => D.Instantiate()..ToUpper() == modelName.ToUpper()).FirstOrDefault();
                //if (model != default(Model))
                //{
                //    return prefab.Instantiate();
                //}
                foreach (var item in l_Materials)
                {
                    Console.WriteLine("-----");
                    Console.WriteLine(item);
                    Console.WriteLine("-----");
                }
                return new Texture();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPrefab(string): " + ex.Message);
                return new Texture();
            }
        }*/
        #endregion
        #endregion

    }
}
