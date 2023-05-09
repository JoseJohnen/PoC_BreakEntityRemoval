using Interfaz.Utilities;
using Map_Editor_HoD.Code.Models;
using Map_Editor_HoD.Controllers;
using Map_Editor_HoD.TilesModels;
using Stride.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Map_Editor_HoD.WorldModels
{
    public abstract class World
    {
        [DataMemberIgnore]
        private System.Numerics.Vector3 location = Vector3.Zero;
        private string name = string.Empty;

        public float FrontBack;
        public float WestEast;
        public float Height;

        public new ConcurrentDictionary<string, Tile> dic_worldTiles = new ConcurrentDictionary<string, Tile>();

        public System.Numerics.Vector3 Location {
            get
            {
                return location;
            }
            set 
            {
                Location = value;
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (Area != null)
                {
                    Area.Name = "Area_" + name;
                }
            }
        }
        //= new System.Numerics.Vector3(0, 0, 0);


        //Pares<string, SerializedVector3> point, string name = ""

        public World()
        { }

        public new Area Area = new Area(new List<AreaDefiner>() {
            new AreaDefiner(new Pares<string,SerializedVector3>("NW",new SerializedVector3(Vector3.Zero)), "NW"),
            new AreaDefiner(new Pares<string, SerializedVector3>("NE",new SerializedVector3(Vector3.Zero)), "NE"),
            new AreaDefiner(new Pares<string, SerializedVector3>("SW",new SerializedVector3(Vector3.Zero)), "SW"),
            new AreaDefiner(new Pares<string, SerializedVector3>("SE",new SerializedVector3(Vector3.Zero)), "SE"),
        }, "AreaWorld");

        public World(int westEast = 3, int height = 1, int frontBack = 3, string name = "")
        {
            WestEast = westEast;
            Height = height;
            FrontBack = frontBack;
            Name = name;
            Area = new Area(new List<AreaDefiner>() {
                new AreaDefiner(new Pares<string,SerializedVector3>("NW",new SerializedVector3(Vector3.Zero)), "NW"),
                new AreaDefiner(new Pares<string, SerializedVector3>("NE",new SerializedVector3(Vector3.Zero)), "NE"),
                new AreaDefiner(new Pares<string, SerializedVector3>("SW",new SerializedVector3(Vector3.Zero)), "SW"),
                new AreaDefiner(new Pares<string, SerializedVector3>("SE",new SerializedVector3(Vector3.Zero)), "SE"),
            }, "AreaWorld");
        }

        public virtual new World RegisterWorld(string nameOfTheWorld = "")
        {
            try
            {
                string name = "World_" + WorldController.dic_worlds.Count;
                if (nameOfTheWorld != "")
                {
                    name = nameOfTheWorld;
                }
                this.Name = name;
                WorldController.dic_worlds.TryAdd(name, this);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error RegisterWorld(string): " + ex.Message);
                return null;
            }
        }

        public new virtual World FillWorld(string TileClass = "")
        {
            try
            {
                Type typ = null;
                if (!string.IsNullOrWhiteSpace(TileClass))
                {
                    typ = Tile.TypesOfTiles().Where(c => c.Name == TileClass).FirstOrDefault();
                    if (typ == null)
                    {
                        typ = Tile.TypesOfTiles().Where(c => c.FullName == TileClass).FirstOrDefault();
                    }
                }
                else
                {
                    typ = Tile.TypesOfTiles().Where(c => c.Name == "Black").FirstOrDefault();
                }

                float inicialX = Location.X - (WestEast / 2);
                float inicialZ = Location.Z - (WestEast / 2);
                Tile tile = null;

                float xMod = inicialX;
                float yMod = Location.Y;
                float zMod = inicialZ;

                float x = 0, y = 0, z = 0;
                do
                {
                    if (typ != null)
                    {
                        object obtOfType = Activator.CreateInstance(typ);
                        tile = ((Tile)obtOfType);
                        tile.InstanceTile("Tile_" + x + "_" + y + "_" + z, new System.Numerics.Vector3(xMod, yMod, zMod), new System.Numerics.Vector3(x, y, z));
                        dic_worldTiles.TryAdd(tile.Name, tile);
                    }
                    else
                    {
                        dic_worldTiles.TryAdd("Tile_" + x + "_" + y + "_" + z, new Grass("Tile_" + x + "_" + y + "_" + z));
                    }

                    if (x == WestEast - 1)
                    {
                        x = 0;
                        y++;
                        zMod += tile.spriteSize.Y;
                        xMod = inicialX;

                        if (y == Height)
                        {
                            y = 0;
                            z++;

                            if (z == FrontBack)
                            {
                                break;
                            }
                        }
                    }
                    else if (x < WestEast)
                    {
                        x++;
                        xMod += tile.spriteSize.X;
                    }
                }
                while (x <= WestEast - 1 && y <= Height - 1 && z <= FrontBack - 1);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error FillWorld: " + ex.Message);
                return null;
            }
        }

        public new static List<Type> TypesOfWorlds()
        {
            List<Type> myTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(World)) && !type.IsAbstract).ToList();
            return myTypes;
        }

        #region Métodos JSON
        public virtual new string ToJson()
        {
            try
            {

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter()
                    },
                };
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //    AllowTrailingCommas = true,

                return JsonSerializer.Serialize(this, serializeOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public virtual new World FromJson(string json)
        {
            string txt = json;
            try
            {
                //txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                //txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt);

                //json = UtilityAssistant.CleanJSON(json);

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new WorldConverter(),
                        new Vector3Converter()
                    },
                };

                //AllowTrailingCommas = true,
                //ReadCommentHandling = JsonCommentHandling.Skip,
                //json = UtilityAssistant.CleanJSON(json);
                World wrldObj = JsonSerializer.Deserialize<World>(txt, serializeOptions);//, serializeOptions);
                //this = prgObj;

                if (wrldObj != null)
                {
                    this.WestEast = wrldObj.WestEast;
                    this.Height = wrldObj.Height;
                    this.FrontBack = wrldObj.FrontBack;
                    this.dic_worldTiles = wrldObj.dic_worldTiles;
                }

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (World) FromJson(): " + json + " ex.Message: " + ex.Message);
                return null;
            }
        }

        public static new World CreateFromJson(string json)
        {
            try
            {
                string prep = UtilityAssistant.PrepareJSON(json);
                string clase = UtilityAssistant.ExtractAIInstructionData(prep, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                }

                object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                  //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list

                World prgObj = ((World)obtOfType);
                prgObj.FromJson(json);
                prgObj.RegisterWorld();
                prgObj.InstanceWorld();
                prgObj.InstanceWorldEditorReqMechanics();
                return prgObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (World) CreateFromJson(): " + ex.Message);
                return null;
            }
        }

        public void InstanceWorld()
        {
            try
            {
                foreach (Tile item in this.dic_worldTiles.Values)
                {
                    item.InstanceTile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController LoadWorld(string) Error: " + ex.Message);
            }
        }

        public void InstanceWorldEditorReqMechanics()
        {
            try
            {
                foreach (Tile item in this.dic_worldTiles.Values)
                {
                    item.InstanceEditorReqMechanics();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WorldController LoadWorld(string) Error: " + ex.Message);
            }
        }
        #endregion
    }

    public class WorldConverter : System.Text.Json.Serialization.JsonConverter<World>
    {
        public override World Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string[] strJsonArray = new string[1];
            string[] strStrArr = new string[1];
            //string[] strStrArr2 = new string[1];
            //string[] strStrArr3 = new string[1];
            string readerReceiver = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //readerReceiver = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                string tempString = jsonDoc.RootElement.GetRawText();

                //string clase = UtilityAssistant.CleanJSON(tempString);
                string clase = UtilityAssistant.ExtractValue(tempString, "Class").Replace("\"", "");

                Type typ = World.TypesOfWorlds().Where(c => c.Name == clase).FirstOrDefault();
                if (typ == null)
                {
                    typ = World.TypesOfWorlds().Where(c => c.FullName == clase).FirstOrDefault();
                    
                }

                World wrldObj = new BaseWorld();
                if (typ != null)
                {
                    object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.
                                                                      //TODO: System to determine the type of enemy to make the object, prepare stats and then add it to the list
                    wrldObj = ((World)obtOfType);
                }

                string strValue = UtilityAssistant.ExtractValue(tempString, "WestEast");
                wrldObj.WestEast = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "Height");
                wrldObj.Height = Convert.ToInt32(strValue);
                strValue = UtilityAssistant.ExtractValue(tempString, "FrontBack");
                wrldObj.FrontBack = Convert.ToInt32(strValue);
                wrldObj.Name = UtilityAssistant.ExtractValue(tempString, "Name");
                strValue = UtilityAssistant.ExtractValue(tempString, "Location");
                wrldObj.Location = Vector3Converter.Converter(strValue);

                //strValue = UtilityAssistant.ExtractValue(tempString, "Area");
                strValue = tempString.Substring(tempString.IndexOf("Area"));
                string strValue2 = strValue.Substring(strValue.IndexOf("dic_worldTiles"));
                strValue = strValue.Replace(strValue2, "");
                strValue = UtilityAssistant.PrepareJSON(strValue);
                int c = strValue.IndexOf("}]}");
                string b = strValue.Substring(c + 3);
                strValue = strValue.Replace("}]}"+b, "}]}");
                strValue = strValue.Replace("Area\":{", "{"); //ek "{" esta ahí porque hay valores al interior del string que se llaman "NombreArea", entonces para evitar que se afecten lugares del string que no deberían
                strValue = UtilityAssistant.CleanJSON(strValue);
                //strValue = strValue.Replace("Y\"", ", \"Y\"").Replace("Z\"", ", \"Z\"").Replace("\"Name\":", "\"Name\":"+ wrldObj.Name +",").Replace("\",\",", "\",");
                wrldObj.Area = Area.CreateFromJson(strValue);

                //string tempValue = UtilityAssistant.PrepareJSON(tempString);
                //strValue = UtilityAssistant.ExtractValue(tempValue, "Area");
                //wrldObj.Area = Area.CreateFromJson(strValue);


                /*if (string.IsNullOrWhiteSpace(readerReceiver) || readerReceiver.Equals("\"{\""))
                {
                    return null;
                }
                
                strJsonArray = tempString.Split("],");
                if (strJsonArray.Length > 1)
                {
                    strJsonArray[0] += "]";
                    strJsonArray[1] += "]";
                }*/

                strJsonArray[0] = tempString; //UtilityAssistant.CleanJSON(tempString);

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("dic_worldTiles")).Replace("dic_worldTiles", "");
                Tile tile = null;
                strTemp = UtilityAssistant.PrepareJSON(strTemp);
                strTemp = strTemp.Replace("\"\"", "");
                strTemp = strTemp.Substring(strTemp.IndexOf("[")+1);
                strTemp = strTemp.Substring(0,strTemp.IndexOf("]"));
                strTemp = strTemp.Replace("},{", "}|°|{");
                List<string> l_string = new List<string>(strTemp.Split("|°|", StringSplitOptions.RemoveEmptyEntries));
                foreach (string item in l_string)
                {
                    //strTemp = UtilityAssistant.ExtractValue(item, "Value");
                    /*strTemp = UtilityAssistant.CleanJSON(item);
                    if(strTemp.Contains("\"Value\""))
                    {
                        strTemp = strTemp.Substring(strTemp.IndexOf("\"Value\""));
                        strTemp = strTemp.Replace("\"Value\":", "").Replace("}}]}", "}");
                    }
                    tile = Tile.CreateFromJson(strTemp);
                    wrldObj.dic_worldTiles.TryAdd(tile.Name, tile);*/
                    strTemp = item.Substring(item.IndexOf("\"Value\""));
                    strTemp = strTemp.Replace("\"Value\":\"", "").Replace("}\"}","}"); //.Replace("}}]}", "}");
                    tile = Tile.CreateFromJson(strTemp);
                    wrldObj.dic_worldTiles.TryAdd(tile.Name, tile);
                }
                //strTemp = strTemp.Substring(4).Replace("[", "").Replace("]", "").Replace("}}", "}");

                /*string str_tiles_to_create = string.Empty;
                if(!strTemp.Equals("}"))
                {
                    str_tiles_to_create = strTemp;
                }

                if (!string.IsNullOrWhiteSpace(str_tiles_to_create))
                {
                    //Array.Clear(strStrArr, 0, strStrArr.Length);
                    str_tiles_to_create = str_tiles_to_create.Replace("},{", "}|°|{");
                    strStrArr = str_tiles_to_create.Split("|°|");
                    foreach (string item1 in strStrArr)
                    {
                        //wwrldObj.dic_worldTiles.TryAdd(item1);
                    }

                    //wrldObj.LoadShots();
                }*/

                return wrldObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Read(): {0} Message: {1}", strJsonArray[0], ex.Message);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, World wldObj, JsonSerializerOptions options)
        {
            try
            {
                string strTemp = string.Empty;//"{";
                int i = 0;
                int last = 0;
                strTemp += "\"dic_worldTiles\" : [";
                last = wldObj.dic_worldTiles.Count;
                foreach (KeyValuePair<string, Tile> item in wldObj.dic_worldTiles)
                {
                    strTemp += "{\"Key\":\"" + item.Key + "\",\"Value\":\"" + item.Value.ToJson() + "\"}";
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]"; //,";
                //strTemp += "}";

                //strTemp = UtilityAssistant.CleanJSON(strTemp);

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                string WestEast = wldObj.WestEast.ToString();
                string Height = wldObj.Height.ToString();
                string FrontBack = wldObj.FrontBack.ToString();
                string Name = string.IsNullOrWhiteSpace(wldObj.Name) ? "null" : wldObj.Name;

                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new Vector3Converter()
                        ,new NullConverter()
                    },
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };

                string Location = System.Text.Json.JsonSerializer.Serialize(wldObj.Location, serializeOptions);
                string Area = wldObj.Area.ToJson();
                string Class = wldObj.GetType().Name;

                char[] a = { '"' };

                string wr = string.Concat("{", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                    ", ", new string(a), "Class", new string(a), ":", new string(a), Class, new string(a),
                    ", ", new string(a), "WestEast", new string(a), ":", WestEast,
                    ", ", new string(a), "Height", new string(a), ":", Height,
                    ", ", new string(a), "FrontBack", new string(a), ":", FrontBack,
                    ", ", new string(a), "Location", new string(a), ":", Location,
                    ", ", new string(a), "Area", new string(a), ":", Area,
                    ", ", strTemp,
                    "}");

                string resultJson = Regex.Replace(wr, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

                writer.WriteStringValue(wr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (WorldConverter) Write(): " + ex.Message);
            }
        }
    }
}