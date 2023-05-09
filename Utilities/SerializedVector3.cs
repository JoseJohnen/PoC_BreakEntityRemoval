using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Xml.Serialization;

namespace Interfaz.Utilities
{
    [Serializable]
    public class SerializedVector3
    {
        private float x;
        private float y;
        private float z;

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public float Z { get => z; set => z = value; }

        /// <summary>
        /// Returns the vector (0,0,0).
        /// </summary>
        public static SerializedVector3 Zero { get => new SerializedVector3(); }
        /// <summary>
        /// Returns the vector (1,1,1).
        /// </summary>
        public static SerializedVector3 One { get => new SerializedVector3(1.0f, 1.0f, 1.0f); }
        /// <summary>
        /// Returns the vector (1,0,0).
        /// </summary>
        public static SerializedVector3 UnitX { get => new SerializedVector3(1.0f, 0.0f, 0.0f); }
        /// <summary>
        /// Returns the vector (0,1,0).
        /// </summary>
        public static SerializedVector3 UnitY { get => new SerializedVector3(0.0f, 1.0f, 0.0f); }
        /// <summary>
        /// Returns the vector (0,0,1).
        /// </summary>
        public static SerializedVector3 UnitZ { get => new SerializedVector3(0.0f, 0.0f, 1.0f); }

        public SerializedVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SerializedVector3(string strVector3)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(strVector3))
                {
                    string[] b = strVector3.Trim().Split(' ');
                    X = float.Parse(b[0].Substring(2));
                    Y = float.Parse(b[1].Substring(2));
                    Z = float.Parse(b[2].Substring(2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Constructur SerializedVector3(string): " + ex.Message, ConsoleColor.Red);
            }
        }

        public SerializedVector3(Vector3 vector3)
        {
            X = IsNaN(vector3.X) ? 0 : vector3.X;
            Y = IsNaN(vector3.Y) ? 0 : vector3.Y;
            Z = IsNaN(vector3.Z) ? 0 : vector3.Z;
        }

        public SerializedVector3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public override string ToString()
        {
            string serialized = "X:" + x + " Y:" + y + " Z:" + z;
            return serialized;
        }

        public static SerializedVector3 operator *(Quaternion quat, SerializedVector3 vec)
        {
            float num = quat.X * 2f;
            float num2 = quat.Y * 2f;
            float num3 = quat.Z * 2f;
            float num4 = quat.X * num;
            float num5 = quat.Y * num2;
            float num6 = quat.Z * num3;
            float num7 = quat.X * num2;
            float num8 = quat.X * num3;
            float num9 = quat.Y * num3;
            float num10 = quat.W * num;
            float num11 = quat.W * num2;
            float num12 = quat.W * num3;
            SerializedVector3 result = new SerializedVector3();
            result.X = (1f - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
            result.Y = (num7 + num12) * vec.x + (1f - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
            result.Z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1f - (num4 + num5)) * vec.z;
            return result;
        }

        public static unsafe bool IsNaN(float f)
        {
            int binary = *(int*)&f;
            return (binary & 0x7F800000) == 0x7F800000 && (binary & 0x007FFFFF) != 0;
        }

        public Vector3 ConvertToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static Vector3 ConvertToVector3(SerializedVector3 serializedVector3)
        {
            return new Vector3(serializedVector3.x, serializedVector3.y, serializedVector3.z);
        }

        public string ToXML()
        {
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedVector3));
                string result = string.Empty;
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, this, ns);
                    result = textWriter.ToString();
                    return result;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ToXML: " + ex.Message);
                return string.Empty;
            }
        }

        public static string ToXML(SerializedVector3 serializedVector3)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedVector3));
                string result = string.Empty;
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, serializedVector3);
                    result = textWriter.ToString();
                    return result;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static string ToXML: " + ex.Message);
                return string.Empty;
            }
        }

        /*public enum TextOrNewtonsoft { Text = 0, Newtonsoft = 1}
        public string ToJson(TextOrNewtonsoft ton = TextOrNewtonsoft.Text)
        {
            try
            {
                if(ton == TextOrNewtonsoft.Text)
                {
                    return JsonSerializer.Serialize(this);
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this);
                }
                //return "{ \"X\":" + this.X + ", \"Y\":" + this.Y + ", \"Z\":" + this.Z + " }";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ToJson: " + ex.Message);
                return string.Empty;
            }
        }*/

        public string ToJson()
        {
            try
            {
                return JsonSerializer.Serialize(this);
                //return "{ \"X\":" + this.X + ", \"Y\":" + this.Y + ", \"Z\":" + this.Z + " }";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ToJson: " + ex.Message);
                return string.Empty;
            }
        }

        public static string ToJson(SerializedVector3 serializedVector3)//, TextOrNewtonsoft ton = TextOrNewtonsoft.Text)
        {
            try
            {
                return serializedVector3.ToJson();//ton);
                //return "{ \"X\":" + serializedVector3.X + ", \"Y\":" + serializedVector3.Y + ", \"Z\":" + serializedVector3.Z + " }";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static string ToJson(SerializedVector3): " + ex.Message);
                return string.Empty;
            }
        }

        public static SerializedVector3 FromJson(string strJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strJson))
                {
                    return new SerializedVector3();
                }

                string jsonExtract = strJson.ReplaceFirst("{", "").ReplaceLast("}", "");
                string[] arrResult = jsonExtract.Split(",");

                float[] fltArry = new float[arrResult.Length];
                for (int i = 0; i < arrResult.Length; i++)
                {
                    string d = arrResult[i].Substring(arrResult[i].IndexOf(":") + 1) + "Listo";
                    if (!float.TryParse(arrResult[i].Substring(arrResult[i].IndexOf(":") + 1), out fltArry[i]))
                    {
                        throw new Exception("Conversion Failed in position: " + i + " |Asociated Values Total: " + arrResult[i] + " |Specific Values: " + arrResult[i].Substring(arrResult[i].IndexOf(":") + 1));
                    }
                }
                string[] a = arrResult;

                return new SerializedVector3();
                //return "{ \"X\":" + serializedVector3.X + ", \"Y\":" + serializedVector3.Y + ", \"Z\":" + serializedVector3.Z + " }";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error static SerializedVector3 FromJson(string): " + ex.Message);
                return new SerializedVector3();
            }
        }

        public static SerializedVector3 operator +(SerializedVector3 a, SerializedVector3 b)
        => new SerializedVector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static SerializedVector3 operator -(SerializedVector3 a, SerializedVector3 b)
        => new SerializedVector3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static SerializedVector3 operator *(SerializedVector3 a, SerializedVector3 b)
        => new SerializedVector3(a.x * b.x, a.y * b.y, a.z * b.z);

        public static SerializedVector3 operator /(SerializedVector3 a, SerializedVector3 b)
        => new SerializedVector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static bool operator ==(SerializedVector3 a, SerializedVector3 b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        public static bool operator !=(SerializedVector3 a, SerializedVector3 b)
        => a.X != b.X && a.Y != b.Y && a.Z != b.Z;

        public override bool Equals(Object sv3)
        => (X != ((SerializedVector3)sv3).X && Y != ((SerializedVector3)sv3).Y && Z != ((SerializedVector3)sv3).Z);

        public override int GetHashCode() { return 0; }
    }
}
