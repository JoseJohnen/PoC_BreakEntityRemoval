//using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Text.Json;

namespace Interfaz.Utilities
{
    //TODO: TODO BASICAMENTE; ESTAS CLASES NO ESTAN LISTAS
    public class Vector3Converter : System.Text.Json.Serialization.JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string strJson = string.Empty;
            try
            {
                //TODO: Corregir, testear y terminar
                //strJson = reader.GetString();
                JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
                strJson = jsonDoc.RootElement.GetRawText();
                strJson = strJson.Replace("\"", "").Replace("{a:", "").Replace("{ a:", "").Replace("}", "").Trim();

                if (strJson.Contains(".�M�"))
                {
                    //Because it's incomplete
                    return Vector3.Zero;
                }

                string[] secondStep = strJson.Replace("<", "").Replace("u003C", "").Replace(">", "").Replace("u003E", "").Replace("\\", "").Replace("\"", "").Split("|");

                string a = secondStep[0].Replace(".", ",").Trim();
                string b = secondStep[1].Replace(".", ",").Trim();
                string c = secondStep[2].Replace(".", ",").Trim();


                //string[] strArray = strJson.Replace("{","").Replace("}","").Split(',');

                Vector3 vector3 = new Vector3(Convert.ToSingle(a), Convert.ToSingle(b), Convert.ToSingle(c));
                return vector3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3Converter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, Vector3 vector3, JsonSerializerOptions options)
        {
            try
            {
                /*string[] x = vector3.ToString().Replace("<","").Replace(">","").Split(".");
                string y1 = x[0].Replace(",",".");
                string y2 = x[1].Replace(",",".");
                string y3 = x[2].Replace(",",".");
                char[] a = { '"' }; G9 <0,0. 0,0. 0,0>
                string wr = @String.Concat("{ ", new string(a), "X", new string(a), ":", y1, ", ",new string(a), "Y", new string(a), ":" , y2, ", " , new string(a), "Z", new string(a), ":", y3, "}");*/
                //Podría ser R también en lugar de G9, pero G9 garantiza siempre la misma desconversión, R la regula según necesidad
                //Por tanto R, potencialmente -no lo he probado- podría generar problemas de deserialización
                //Igual G9 al final no fue suficiente, tuve que subirle a 20, debería probablemente subir a 32

                string wr = "<" + vector3.X.ToString("G13");
                wr += "|" + vector3.Y.ToString("G13");
                wr += "|" + vector3.Z.ToString("G13") + ">";
                wr = wr.Replace(",", "."); //Este antiguo replace ".Replace(".","|")" lo removí porque estoy haciendo lo mismo pero a mano ahora mas arriba
                writer.WriteStringValue(wr);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3Converter) Write(): " + ex.Message);
            }
        }

        public static Vector3 Converter(string vector3Json)
        {
            string strJson = vector3Json;
            try
            {
                //TODO: Corregir, testear y terminar
                //strJson = reader.GetString();
                strJson = strJson.Replace("\"", "").Replace("{a:", "").Replace("{ a:", "").Replace("}", "").Trim();

                if (strJson.Contains(".�M�"))
                {
                    //Because it's incomplete
                    return Vector3.Zero;
                }

                //strJson = strJson.Replace("´┐¢M´┐¢", "").Replace(".�M�","");
                string[] secondStep = strJson.Replace("<", "").Replace("u003C", "").Replace(">", "").Replace("u003E", "").Replace("\\", "").Replace("\"", "").Split("|");

                string a = secondStep[0].Replace(".", ",").Trim();
                string b = secondStep[1].Replace(".", ",").Trim();
                string c = secondStep[2].Replace(".", ",").Trim();

                //string[] strArray = strJson.Replace("{","").Replace("}","").Split(',');

                Vector3 vector3 = new Vector3((float)Convert.ToDouble(a), (float)Convert.ToDouble(b), (float)Convert.ToDouble(c));
                return vector3;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3Converter) Read(): {0} Message: {1}", strJson, ex.Message);
                return default;
            }
        }
    }

    /*public class Vector3ConverterJSON : Newtonsoft.Json.JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 shot, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                ////TODO: Corregir, testear y terminar
                //string Id = "\"" + shot.Id + "\"";
                //string LauncherName = "\"" + shot.LN + "\"";
                //string Type = "\"" + shot.Type + "\"";
                ////string LauncherPos = new SerializedVector3(shot.OrPos).ToXML();
                ////string WeaponPos = new SerializedVector3(shot.WPos).ToXML();
                ////string Moddif = new SerializedVector3(shot.Mdf).ToXML();

                //string LauncherPos = new SerializedVector3(shot.OrPos).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);
                //string WeaponPos = new SerializedVector3(shot.WPos).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);
                //string Moddif = new SerializedVector3(shot.Mdf).ToJson(SerializedVector3.TextOrNewtonsoft.Newtonsoft);

                //string resultJson = "{Id:" + Id + ", LN:" + LauncherName + ", Type:" + Type + ", OrPos:" + LauncherPos + ", WPos:" + WeaponPos + ", Mdf:" + Moddif + "}";
                writer.WriteValue("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3ConverterJSON) WriteJson(): " + ex.Message);
            }
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            string strJson = string.Empty;
            try
            {
                strJson = (string)reader.Value;
                //TODO: Corregir, testear y terminar

                Shot shot = new Shot();
                string[] a = UtilityAssistant.CutJson(strJson);

                shot.Id = a[0];
                shot.LN = a[1];
                shot.Type = a[2];

                //shot.OrPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[3]).ConvertToVector3();
                //shot.WPos = UtilityAssistant.XmlToClass<SerializedVector3>(a[4]).ConvertToVector3();
                //shot.Mdf = UtilityAssistant.XmlToClass<SerializedVector3>(a[5]).ConvertToVector3();

                shot.OrPos = JsonConvert.DeserializeObject<SerializedVector3>(a[3]).ConvertToVector3();
                shot.WPos = JsonConvert.DeserializeObject<SerializedVector3>(a[4]).ConvertToVector3();
                shot.Mdf = JsonConvert.DeserializeObject<SerializedVector3>(a[5]).ConvertToVector3();

                return new Vector3();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (Vector3ConverterJSON) ReadJson(): {0} Message: {1}", strJson, ex.Message);
                return default(Vector3);
            }
        }
    }*/
}
