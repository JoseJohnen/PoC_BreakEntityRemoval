using Interfaz.Utilities;
using Map_Editor_HoD.Code.Models;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Map_Editor_HoD
{ 
    [Serializable]
    [Stride.Core.DataContract]
    public class AreaDefiner
    {
        public string NombreArea { get => nombreArea; set => nombreArea = value; }
        private Pares<string, SerializedVector3> point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
        private string nombreArea;

        public Pares<string, SerializedVector3> Point
        {
            get
            {
                if (point == null)
                {
                    point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
                }
                return point;
            }
            set
            {
                if (point == null)
                {
                    point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
                }
                point = value;
            }
        }

        [JsonConstructor]
        public AreaDefiner(string name = "")
        {
            NombreArea = name;
            Point = new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3());
        }

        public AreaDefiner(Pares<string, SerializedVector3> point, string name = "")
        {
            NombreArea = name;
            Point = point;
        }

        public AreaDefiner()
        {

        }

        #region Suplementary Functions
        public static bool isBreakingInside(List<AreaDefiner> l_areaDefiners, Entity entity)
        {
            try
            {
                bool result = true;
                foreach (AreaDefiner arDef in l_areaDefiners)
                {
                    result = AreaDefiner.isBreakingInside(arDef, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingInside(List<AreaDefiner> l_areaDefiners, List<Entity> l_entitys): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingInside(AreaDefiner areaDefiner, Entity entity)
        {
            try
            {
                if (entity.Transform.Position.Y > areaDefiner.Point.Item2.Y) //South Limit
                {
                    if (entity.Transform.Position.Y <= areaDefiner.Point.Item2.Y) //North Limit
                    {
                        if (entity.Transform.Position.X > areaDefiner.Point.Item2.X) //East Limit
                        {
                            if (entity.Transform.Position.X <= areaDefiner.Point.Item2.X) //West Limit
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingInside(AreaDefiner areaDefiner, Entity entity): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingOutside(List<AreaDefiner> l_areaDefiners, Entity entity)
        {
            try
            {
                bool result = true;
                foreach (AreaDefiner arDef in l_areaDefiners)
                {
                    result = AreaDefiner.isBreakingOutside(arDef, entity);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingOutside(List<AreaDefiner> l_areaDefiners, List<Entity> l_entitys): " + ex.Message);
                return false;
            }
        }

        public static bool isBreakingOutside(AreaDefiner areaDefiner, Entity entity)
        {
            try
            {
                if (entity.Transform.Position.Y <= areaDefiner.Point.Item2.Y) //South Limit
                {
                    if (entity.Transform.Position.Y > areaDefiner.Point.Item2.Y) //North Limit
                    {
                        if (entity.Transform.Position.X <= areaDefiner.Point.Item2.X) //East Limit
                        {
                            if (entity.Transform.Position.X > areaDefiner.Point.Item2.X) //West Limit
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error isBreakingOutside(AreaDefiner areaDefiner, Entity entity): " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Métodos JSON
        public string ToJson()
        {
            try
            {
                string narea = !string.IsNullOrWhiteSpace(NombreArea) ? NombreArea : "null";
                string strItem1 = !string.IsNullOrWhiteSpace(Point.Item1) ? Point.Item1 : "null";
                string json = "{ \"NombreArea\":\"" + narea + "\",";
                json += "\"Point\":{ \"Item1\":\"" + strItem1 + "\","; //it's a string
                json += "\"Item2\":" + Point.Item2.ToJson() + "}}"; //it's a SerializeVector3

                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (AreaDefiner) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public AreaDefiner FromJson(string json)
        {
            string txt = json;
            try
            {
                txt = UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));

                string nombreArea = UtilityAssistant.ExtractValue(txt, "NombreArea");
                this.NombreArea = nombreArea;

                string pointJson = UtilityAssistant.ExtractValue(txt, "Point");

                string item1 = UtilityAssistant.ExtractValue(pointJson, "Item1");
                string item2 = UtilityAssistant.ExtractValue(pointJson, "Item2");

                this.point = new Pares<string, SerializedVector3>(item1, new SerializedVector3(item2));

                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (AreaDefiner) FromJson(): " + json + " ex.Message: " + ex.Message);
                return null;
            }
        }

        public static AreaDefiner CreateFromJson(string json)
        {
            try
            {
                string txt = UtilityAssistant.CleanJSON(json);
                if(string.IsNullOrWhiteSpace(txt))
                {
                    txt = json;
                }
                AreaDefiner areaDefiner = new AreaDefiner();
                return areaDefiner.FromJson(txt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (AreaDefiner) CreateFromJson(): " + ex.Message);
                return null;
            }
        }
        #endregion

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }*/

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
