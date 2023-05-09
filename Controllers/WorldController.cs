using System.Collections.Concurrent;
using System;
using Stride.Engine;
using Map_Editor_HoD.WorldModels;
using Stride.Core;

namespace Map_Editor_HoD.Controllers
{
    public class WorldController : StartupScript
    {
        [DataMemberIgnore]
        public static ConcurrentDictionary<string, World> dic_worlds = new ConcurrentDictionary<string, World>();
        public static Game game = null;

        public static World TestWorld = null;
        public WorldController worldController = null;

        public override void Start()
        {
            try
            {
                Services.AddService(this);
                worldController = this;
                game = (Game)this.Game;

                WorldController_OnStart();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController Start() Error: " + ex.Message);
            }
        }

        public static void LoadWorld(string textOriginal)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textOriginal))
                {
                    if (textOriginal.Contains("WM:"))
                    {
                        string tempString = Interfaz.Utilities.UtilityAssistant.ExtractValues(textOriginal, "WM");
                        if (!string.IsNullOrWhiteSpace(tempString))
                        {
                            World world = World.CreateFromJson(textOriginal);
                            world.InstanceWorld();
                            world.InstanceWorldEditorReqMechanics();
                            dic_worlds.TryAdd(world.Name, World.CreateFromJson(textOriginal));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController LoadWorld(string) Error: " + ex.Message);
            }
        }

        public void WorldController_OnStart()
        {
            try
            {
                /*TestWorld = new Map_Editor_HoD.WorldModels.BaseWorld();
                TestWorld.WestEast = 15;
                TestWorld.FrontBack = 15;
                TestWorld.RegisterWorld("NombreDePrueba");
                TestWorld.FillWorld("Grass");*/
                /*string b = TestWorld.ToJson();
                World c = World.CreateFromJson(b);*/
                //dic_worlds.Add("World1", BaseWorld.CreateWorld(prefab));
                //dic_worlds["World1"].Entity.Transform.Position = new Vector3(0, 0, 0);
                //dic_worlds["World1"].Save();
                //gmobj.Transform.parent = WorldController.Instance.Transform;
                //wrld.Empty();
                //wrld.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController_OnStart() Error: " + ex.Message);
            }
        }

        public static void WorldController_Tick()
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController_Tick() Error: " + ex.Message);
            }
        }

    }
}