using System;
using System.IO;
using System.Xml.Serialization;

namespace Map_Editor_HoD
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
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public SerializedVector3(string strVector3)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(strVector3))
                {
                    string tempString = strVector3.Replace("{", "").Replace("}", "").Replace("\"","");
                    string[] b = tempString.Trim().Split(' ');
                    this.X = float.Parse(b[0].Substring(2));
                    this.Y = float.Parse(b[1].Substring(2));
                    this.Z = float.Parse(b[2].Substring(2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Constructor SerializedVector3(string): "+ex.Message, ConsoleColor.Red);
            }
        }

        public SerializedVector3(System.Numerics.Vector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

        public SerializedVector3(Stride.Core.Mathematics.Vector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

        public SerializedVector3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public override string ToString()
        {
            string serialized = "X:" + this.x + " Y:" + this.y + " Z:" + this.z;
            return serialized;
        }

        public static SerializedVector3 operator *(Stride.Core.Mathematics.Quaternion quat, SerializedVector3 vec)
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

        public static SerializedVector3 operator *(System.Numerics.Quaternion quat, SerializedVector3 vec)
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

        public Stride.Core.Mathematics.Vector3 ConvertToVector3()
        {
            return new Stride.Core.Mathematics.Vector3(this.x, this.y, this.z);
        }

        public System.Numerics.Vector3 ConvertToVector3SN()
        {
            return new System.Numerics.Vector3(this.x, this.y, this.z);
        }

        public static Stride.Core.Mathematics.Vector3 ConvertToVector3(SerializedVector3 serializedVector3)
        {
            return new Stride.Core.Mathematics.Vector3(serializedVector3.x, serializedVector3.y, serializedVector3.z);
        }

        public static System.Numerics.Vector3 ConvertToVector3SN(SerializedVector3 serializedVector3)
        {
            return new System.Numerics.Vector3(serializedVector3.x, serializedVector3.y, serializedVector3.z);
        }

        public string ToXML()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces(); 
            ns.Add(String.Empty, String.Empty);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedVector3));
            string result = string.Empty;
            using(StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, this, ns);
                return textWriter.ToString();
            }
        }

        public static string ToXML(SerializedVector3 serializedVector3)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedVector3));
            string result = string.Empty;
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, serializedVector3);
                return textWriter.ToString();
            }
        }
       
        public string ToJson()
        {
            return "{ \"X\":"+this.X+", \"Y\":"+this.Y+", \"Z\":"+this.Z+" }";
        }

        public static string ToJson(SerializedVector3 serializedVector3)
        {
            return "{ \"X\":" + serializedVector3.X + ", \"Y\":" + serializedVector3.Y + ", \"Z\":" + serializedVector3.Z + " }";
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
        => (a.X == b.X && a.Y == b.Y && a.Z == b.Z);

        public static bool operator !=(SerializedVector3 a, SerializedVector3 b)
        => (a.X != b.X && a.Y != b.Y && a.Z != b.Z);

        public override bool Equals(Object sv3)
        => (X != ((SerializedVector3)sv3).X && Y != ((SerializedVector3)sv3).Y && Z != ((SerializedVector3)sv3).Z);

        public override int GetHashCode() { return 0; }
    }
}
