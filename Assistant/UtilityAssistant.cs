using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Text.Json;
using Stride.Rendering.Sprites;
using Stride.Graphics;
using Quaternion = Stride.Core.Mathematics.Quaternion;
using Vector3 = Stride.Core.Mathematics.Vector3;
using Vector2 = Stride.Core.Mathematics.Vector2;
using Map_Editor_HoD.Controllers;
using Map_Editor_HoD.Enums;

namespace Map_Editor_HoD.Assistants
{
    public class UtilityAssistant : StartupScript
    {
        public static bool ScreenPositionToWorldPositionRaycast(Vector2 screenPos, CameraComponent camera, Simulation simulation, out ClickResult clickResult)
        {
            try
            {
                Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

                Vector3 sPos;
                sPos.X = screenPos.X * 2f - 1f;
                sPos.Y = 1f - screenPos.Y * 2f;

                sPos.Z = 0f;
                var vectorNear = Vector3.Transform(sPos, invViewProj);
                vectorNear /= vectorNear.W;

                sPos.Z = 1f;
                var vectorFar = Vector3.Transform(sPos, invViewProj);
                vectorFar /= vectorFar.W;

                clickResult.ClickedEntity = null;
                clickResult.WorldPosition = Vector3.Zero;
                clickResult.Type = ClickType.Empty;
                clickResult.HitResult = new HitResult();

                var minDistance = float.PositiveInfinity;

                var result = new FastList<HitResult>();
                if(simulation == null)
                {
                    return false;
                }

                simulation.RaycastPenetrating(vectorNear.XYZ(), vectorFar.XYZ(), result);
                foreach (var hitResult in result)
                {
                    ClickType type = ClickType.Empty;

                    var staticBody = hitResult.Collider as StaticColliderComponent;
                    if (staticBody != null)
                    {
                        if (staticBody.CollisionGroup == CollisionFilterGroups.CustomFilter1)
                            type = ClickType.Ground;

                        if (staticBody.CollisionGroup == CollisionFilterGroups.CustomFilter2)
                            type = ClickType.LootCrate;

                        if (type != ClickType.Empty)
                        {
                            var distance = (vectorNear.XYZ() - hitResult.Point).LengthSquared();
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                clickResult.Type = type;
                                clickResult.HitResult = hitResult;
                                clickResult.WorldPosition = hitResult.Point;
                                clickResult.ClickedEntity = hitResult.Collider.Entity;
                            }
                        }
                    }
                }

                return (clickResult.Type != ClickType.Empty);

            }
            catch (Exception ex)
            {
                clickResult = default(ClickResult);
                return false;
            }
        }

        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer all compacted in a Vector3
        /// </summary>
        /// <param name="ValueA">Is a Vector 3, First value to compare by his X,Y and Z values with the ValueB</param>
        /// <param name="ValueB">Is a Vector 3, First value to compare by his X,Y and Z values with the ValueA</param>
        /// <param name="ValueC">(Optional) Is a float used in every comparison (X, Y and Z), a margin of tolerance which inside that range of difference it consider ValueA and ValueB equal, if not setted is consider 0</param>
        /// <returns>Returns a Vector3 with the result of each directional difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByVectorComparison(Vector3 ValueA, Vector3 ValueB, float ValueC = 0)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByAxis(ValueA.X, ValueB.X, ValueC);
            result.Y = DistanceModifierByAxis(ValueA.Y, ValueB.Y, ValueC);
            result.Z = DistanceModifierByAxis(ValueA.Z, ValueB.Z, ValueC);
            return result;
        }

        /// <summary>
        /// Compare Vector3s, usually used in position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of his distance to 0, all compacted in a Vector3.
        /// </summary>
        /// <param name="ValueA"></param>
        /// <param name="ValueB"></param>
        /// <returns>Returns a Vector3 with the result of each directional cartesian difference (1,0,-1) between ValueA and ValueB on each of his axis independantly</returns>
        public static Vector3 DistanceModifierByCartesianVectorComparison(Vector3 ValueA, Vector3 ValueB)
        {
            Vector3 result = Vector3.Zero;
            result.X = DistanceModifierByCartesianAxis(ValueA.X, ValueB.X);
            result.Y = DistanceModifierByCartesianAxis(ValueA.Y, ValueB.Y);
            result.Z = DistanceModifierByCartesianAxis(ValueA.Z, ValueB.Z);
            return result;
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the cartesian distance (i.e. which one is closer or farther to 0) 
        /// </summary>
        /// <param name="ValueA">a flota</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>returns 1 if ValueA is farther to 0 or -1 if ValueB is farther to 0, if they are equal it will return 0</returns>
        public static float DistanceModifierByCartesianAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if ((ValueA < 0 && ValueB > 0) || (ValueA > 0 && ValueB < 0))
                {
                    if (ValueA > ValueB)
                    {
                        evaluator = -1;
                    }
                    else if (ValueA < ValueB)
                    {
                        evaluator = 1;
                    }
                    return evaluator;
                }

                float a, b, c;
                //Determinar si números de mismo signo son positivos o negativos
                if (ValueA > 0)
                {
                    c = 1;
                }
                else if (ValueA < 0)
                {
                    c = -1;
                }
                else
                {
                    if (ValueB > 0)
                    {
                        c = 1;
                    }
                    else if (ValueB < 0)
                    {
                        c = -1;
                    }
                    else
                    {
                        //Si llega acá es porque ambos números son 0
                        return 0;
                    }
                }

                //Si son de igual signo y no son 0 ambos
                a = ValueA < 0 ? ValueA * -1 : ValueA;
                b = ValueB < 0 ? ValueB * -1 : ValueB;

                if (c > 0)
                {
                    if (a > b)
                    {
                        evaluator = 1;
                    }
                    else if (a < b)
                    {
                        evaluator = -1;
                    }
                }
                else if (c < 0)
                {
                    if (a > b)
                    {
                        evaluator = -1;
                    }
                    else if (a < b)
                    {
                        evaluator = 1;
                    }
                }

                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByCartesianAxis(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Compare floats, usually used in floats of position, to determine what is the directional difference between them, it will return a -1, 0 or 1 depending of the imaginary nature of the answer 
        /// </summary>
        /// <param name="ValueA">a float</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <param name="ValueC">(Optional) a margin of tolerance which inside that range of difference it consider ValueA and ValueB equal, if not setted is consider 0</param>
        /// <returns>returns 1 if ValueA is bigger or -1 if ValueB is bigger, if they are equal it will return 0</returns>
        public static float DistanceModifierByAxis(float ValueA, float ValueB, float ValueC = 0)
        {
            try
            {
                float evaluator = 0;
                float difference = 0;

                if (ValueC == 0)
                {
                    if (Math.Round(ValueA) > Math.Round(ValueB))
                    {
                        evaluator = 1;
                    }
                    else if (Math.Round(ValueA) < Math.Round(ValueB))
                    {
                        evaluator = -1;
                    }
                }
                else
                {
                    if (ValueA > ValueB)
                    {
                        difference = ValueA - ValueB;
                    }
                    else if (ValueA < ValueB)
                    {
                        difference = ValueB - ValueA;
                    }

                    if (difference > ValueC)
                    {
                        if (Math.Round(ValueA) > Math.Round(ValueB))
                        {
                            evaluator = 1;
                        }
                        else if (Math.Round(ValueA) < Math.Round(ValueB))
                        {
                            evaluator = -1;
                        }
                    }
                    //Else, evaluator remains to be 0, hence, i don't need to set it to 0
                }

                /*Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("DistanceModifierByAxis evaluator: " + evaluator);
                Console.WriteLine("ValueA: " + ValueA);
                Console.WriteLine("ValueB: " + ValueB);
                Console.ResetColor();*/

                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceModifierByAxis(): " + ex.Message);
                return 0;
            }
        }

        #region Quaternion Related
        /// <summary>
        /// Recibe un string en formato 'X:# Y:# Z:# W:#' y lo convierte en un Quaternion con dichos parámetros, luego retorna dicho Quaternion.
        /// </summary>
        /// <param name="information">String containing the quaternion information in format X:# Y:# Z:# W:#</param>
        /// <returns></returns>
        public static Quaternion StringToQuaternion(string information)
        {
            try
            {
                string sQuaternion = "(" + information.Replace(",", ".").Replace(" ", ",").Replace("}", "").Replace("{", "") + ")";
                // Remove the parentheses
                if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
                {
                    sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
                }

                // split the items
                string[] sArray = sQuaternion.Split(',');

                // store as a Vector3
                Quaternion result = new Quaternion(
                    float.Parse(sArray[0].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[1].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[2].Replace(".", ",").Substring(2)),
                    float.Parse(sArray[3].Replace(".", ",").Substring(2)));

                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Quaternion StringToQuaternion(string): " + ex.Message, ConsoleColor.Red);
                return Quaternion.Identity;
            }
        }

        public static Quaternion ToQuaternion(Vector3 v)
        {
            try
            {
                float cy = (float)Math.Cos(v.Z * 0.5);
                float sy = (float)Math.Sin(v.Z * 0.5);
                float cp = (float)Math.Cos(v.Y * 0.5);
                float sp = (float)Math.Sin(v.Y * 0.5);
                float cr = (float)Math.Cos(v.X * 0.5);
                float sr = (float)Math.Sin(v.X * 0.5);

                return new Quaternion
                {
                    W = (cr * cp * cy + sr * sp * sy),
                    X = (sr * cp * cy - cr * sp * sy),
                    Y = (cr * sp * cy + sr * cp * sy),
                    Z = (cr * cp * sy - sr * sp * cy)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Quaternion ToQuaternion(Vector3): " + ex.Message, ConsoleColor.Red);
                return Quaternion.Identity;
            }
        }

        public static string QuaternionToXml(Quaternion quaternion)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Quaternion));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, String.Empty);
                string result = string.Empty;
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, quaternion, ns);
                    return textWriter.ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string QuaternionToXml(Quaternion): " + ex.Message);
                return String.Empty;
            }
        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            try
            {
                Vector3 angles = new();

                // roll / x
                double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
                double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
                angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

                // pitch / y
                double sinp = 2 * (q.W * q.Y - q.Z * q.X);
                if (Math.Abs(sinp) >= 1)
                {
                    angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
                }
                else
                {
                    angles.Y = (float)Math.Asin(sinp);
                }

                // yaw / z
                double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
                double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
                angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

                return angles;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Vector3 ToEulerAngles(Quaternion): " + ex.Message);
                return Vector3.Zero;
            }
        }
        #endregion

        #region Extract Values
        public static string ExtractValues(string instructions, string particle, out string part1, out string part2)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = String.Empty;
                    part2 = String.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = String.Empty;
                part2 = String.Empty;
                return String.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle, out string part1, out string part2, out string part3)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    part1 = String.Empty;
                    part2 = String.Empty;
                    part3 = String.Empty;
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = b.Contains("r/n/") ? b.Substring(0, b.IndexOf("r/n/")) : b;

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                part1 = specificRelevantInstruction.Substring(0, 2);
                part2 = specificRelevantInstruction.Substring(2, 2);
                part3 = specificRelevantInstruction.Substring(4);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string, out string, out string): " + ex.Message);
                part1 = String.Empty;
                part2 = String.Empty;
                part3 = String.Empty;
                return String.Empty;
            }
        }

        public static string ExtractValues(string instructions, string particle)
        {
            try
            {
                if (!instructions.Contains(particle))
                {
                    return instructions;
                }

                //Extract relevant part
                string particleswithdots = particle + ":";
                string b = instructions.Substring(instructions.IndexOf(particle));
                string d = String.Empty;
                if (b.Contains("\r\n"))
                {
                    d = b.Substring(0, b.IndexOf("\r\n"));
                }
                else
                {
                    d = b;
                }

                //Process relevant part
                string specificRelevantInstruction = d.Substring(particleswithdots.Length);
                return specificRelevantInstruction;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return String.Empty;
            }
        }

        /// <summary>
        /// Extract the value of the specific field in the Json, it eliminates everything else
        /// </summary>
        /// <param name="instruction">the JSON from which its gonna extract the value</param>
        /// <param name="valueName">the name of the field to extract</param>
        /// <returns>the value extacted</returns>
        public static string ExtractValue(string instruction, string valueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    return string.Empty;
                }

                string result = instruction;
                if (instruction.Contains("\u0022"))
                {
                    result = Interfaz.Utilities.UtilityAssistant.CleanJSON(instruction);
                }

                if (result.Contains("\"" + valueName + "\":"))
                {
                    result = result.Substring(result.IndexOf("\"" + valueName + "\":"));
                }
                else if (result.Contains(valueName))
                {
                    result = result.Substring(result.IndexOf(valueName));
                }

                if (result.Contains(","))
                {
                    result = result.Replace(result.Substring(result.IndexOf(",")), "");
                }
                string aarg = "\"" + valueName + "\":";
                result = result.Replace(aarg, "");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return String.Empty;
            }
        }

        public static string ExtractAIInstructionData(string instruction, string valueName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    return string.Empty;
                }

                string result = instruction;
                result = result.Substring(result.IndexOf("\"" + valueName + "\":"));
                result = result.Replace(result.Substring(result.IndexOf(",")), "");
                string aarg = "\"" + valueName + "\":";
                result = result.Replace(aarg, "");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string ExtractValues(string, string): " + ex.Message);
                return String.Empty;
            }
        }
        #endregion

        /// <summary>
        /// Cuidado, no funciona con objetos dentro de objetos TODO: Arreglar eso
        /// </summary>
        /// <param name="jsonToCut">el string en formato JSON a cortar</param>
        /// <returns>un string array que contiene el json ya cortado en sub-jsons</returns>
        public static string[] CutJson(string jsonToCut)
        {
            try
            {
                string[] result = null;
                if (!string.IsNullOrWhiteSpace(jsonToCut))
                {
                    string tempString = jsonToCut.Replace("{", "").Replace("}", "");


                    if (tempString.Contains(", "))
                    {
                        result = tempString.Split(", ");
                        int i = 0;
                        foreach (string str in result)
                        {
                            result[i] = str.Substring(str.IndexOf(":") + 1).Replace("\"", "");
                            i++;
                        }
                        return result;
                    }

                    if (tempString.Contains(" "))
                    {
                        result = tempString.Split(" ");
                        return result;
                    }
                }

                result = new string[0];
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error string[] CutJson(string): " + ex.Message);
                return new string[0];
            }
        }

        /// <summary>
        /// Deserializa los string XML de vuelta a objetos de la clase T
        /// </summary>
        /// <param name="xml">el string en formato XML a deserializar</param>
        /// <returns>un objeto de la clase T</returns>
        public static T XmlToClass<T>(string xml)
        {
            try
            {
                string toProcess = xml.Replace("xmlns:xsi=http://www.w3.org/2001/XMLSchema-instance xmlns:xsd=http://www.w3.org/2001/XMLSchema", "").Replace("1.0", "\"1.0\"").Replace("utf-16", "\"utf-16\"").Replace("UTF-8", "\"UTF-8\"");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (StringReader textReader = new StringReader(toProcess))
                {
                    return (T)xmlSerializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error T XmlToClass<T>: " + ex.Message);
                return default(T);
            }
        }

        public static Stride.Core.Mathematics.Quaternion ConvertSystemNumericsToStrideQuaternion(System.Numerics.Quaternion quaternion)
        {
            try
            {
                return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error System.Numerics.Quaternion ConvertSystemNumericsToStrideQuaternion(Stride.Core.Mathematics.Quaternion): " + ex.Message);
                return Quaternion.Identity;
            }
        }

        public static System.Numerics.Quaternion ConvertStrideToSystemNumericsQuaternion(Stride.Core.Mathematics.Quaternion quaternion)
        {
            try
            {
                return new System.Numerics.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Stride.Core.Mathematics.Quaternion ConvertStrideToSystemNumericsQuaternion(System.Numerics.Quaternion): " + ex.Message);
                return System.Numerics.Quaternion.Identity;
            }
        }

        /// <summary>
        /// Compare floats, usually used in Vectors of position, to determine what is the distance between both numerically, it will return a positive
        /// </summary>
        /// <param name="ValueA">a float</param>
        /// <param name="ValueB">another float to make the comparison with</param>
        /// <returns>the distance between the two</returns>
        public static float DistanceComparitorByAxis(float ValueA, float ValueB)
        {
            try
            {
                float evaluator = 0;
                if (ValueA > ValueB)
                {
                    evaluator = ValueA - ValueB;
                }
                else
                {
                    evaluator = ValueB - ValueA;
                }
                return evaluator;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceComparitorByAxis(): " + ex.Message);
                return 0;
            }
        }

        public enum LockDimension { None, X, Y, Z }
        public static Vector3 ScreenToMapPosition(Vector2 mouseScreenPosition, LockDimension lockDimension = LockDimension.None, float ammountBlock = 1.5f)
        {
            ClickResult clickResult = new ClickResult();
            bool result = UtilityAssistant.ScreenPositionToWorldPositionRaycast(mouseScreenPosition, Controller.controller.GetActiveCamera(), Controller.controller.GetSimulation(), out clickResult);

            if (lockDimension == LockDimension.X)
            {
                clickResult.WorldPosition.X = ammountBlock;
            }

            if (lockDimension == LockDimension.Y)
            {
                clickResult.WorldPosition.Y = ammountBlock;
            }

            if (lockDimension == LockDimension.Z)
            {
                clickResult.WorldPosition.Z = ammountBlock;
            }

            return clickResult.WorldPosition;
        }

        /// <summary>
        /// Return the position of the world where the Mouse was Clicked (works with any perspective) but require something to "impact"
        /// </summary>
        /// <param name="mouseScreenPos">The position of the mouse in the creen</param>
        /// <param name="camera">(Optional) The camere from where te calculation will be done</param>
        /// <returns>Returns a Vector3 with the 3D position of the mouse in the world</returns>
        public static Vector3 ScrenToMapPoint(Vector2 mouseScreenPos, CameraComponent camera = null)
        {
            try
            {
                Controller ent = Controller.controller;
                CameraComponent camra = camera != null ? camera : Controller.controller.GetActiveCamera();

                // Preparing for the data
                FastList<HitResult> resultList = new FastList<HitResult>();

                // Borrowed from Stride documentation
                var invViewProj = Matrix.Invert(camra.ViewProjectionMatrix);
                Vector3 sPos;
                sPos.X = mouseScreenPos.X * 2f - 1f;
                sPos.Y = 1f - mouseScreenPos.Y * 2f;
                sPos.Z = 0f;

                var vectorNear = Vector3.Transform(sPos, invViewProj);
                vectorNear /= vectorNear.W;
                sPos.Z = 1f;
                var vectorFar = Vector3.Transform(sPos, invViewProj);
                vectorFar /= vectorFar.W;

                resultList.Clear();
                ent.GetSimulation().RaycastPenetrating(vectorNear.XYZ(), vectorFar.XYZ(),
                resultList, CollisionFilterGroups.DefaultFilter, CollisionFilterGroupFlags.CustomFilter10);

                if (!resultList.Any())
                {
                    //Console.WriteLine($"Destination point could not be found, are you clicking ground?");
                    return Vector3.Zero;
                }

                var targetPosition = resultList.First().Point;
                //Console.WriteLine($"Sphere position has been updated. (Vector3 of position clicked returned successfully)");
                return new Vector3(targetPosition.X, .5f, targetPosition.Z);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceComparitorByAxis(): " + ex.Message);
                return Vector3.Zero;
            }
        }

        public enum AxistToIgnore { X, Y, Z }

        /// <summary>
        /// Convert a number (preferably grades of an angle) than exist between the rank -180 to 180 for his equivalent in 0 to 360
        /// </summary>
        /// <param name="numberInRangeOf180">double, the grade than is gonna be converted from the range of -180 to 180</param>
        /// <returns>double, the angle already converted to the new range between 0 to 360</returns>
        public static double Convert180to360(double numberInRangeOf180)
        {
            double result = numberInRangeOf180;
            if (numberInRangeOf180 < 0)
            {
                result = 180 + (180 + numberInRangeOf180); //It's a sum because (numberInRangeOf180) it's always negative
            }

            return result;
        }

        /// <summary>
        /// Obtain the angule between two sides who are builded from 2 Vector2 coords
        /// </summary>
        /// <param name="touch">Vector2, The position of the free point</param>
        /// <param name="center">Vector2, The position of the center from where you are calculating</param>
        /// <returns>Returns the angle between both sides as a double</returns>
        public static double CalculateAngle(Vector2 touch, Vector2 center)
        {
            if (touch != Vector2.Zero)
            {
                var a = "";
                var b = a;
            }

            double deltaX = center.X - touch.X;
            double deltaY = center.Y - touch.Y;

            double radians = Math.Atan2(deltaY, deltaX);
            return (180 / Math.PI) * radians;
        }

        /// <summary>
        /// Obtain the distance between two numbers in a double, which also be a positive.
        /// </summary>
        /// <param name="c">float, one of the numbers in the axis</param>
        /// <param name="d">float, the other number in the axis</param>
        /// <returns>Returns the distance between both points in positive numbers as a double</returns>
        public static double DistanceBetweenVectorsAxis(float c, float d)
        {
            try
            {
                //float x1 = ent.Entity.Transform.position.X;
                //float y1 = ent.Entity.Transform.position.Y;

                //float x2 = plyr.Transform.position.X;
                //float y2 = plyr.Transform.position.Y;

                double a = c;
                double b = d;

                double distX = 0;
                //float distY = 0;
                double varNumb = 0;

                if (a > 0 && b < 0)
                {
                    varNumb = b * -1;
                    distX = varNumb + a;
                }
                if (a < 0 && b > 0)
                {
                    varNumb = a * -1;
                    distX = varNumb + b;
                }

                if (a <= 0 && b <= 0)
                {
                    double numberTemp = 0;
                    if (a > b)
                    {
                        numberTemp = a - b;
                    }
                    else if (b > a)
                    {
                        numberTemp = b - a;
                    }

                    distX = numberTemp < 0 ? numberTemp * -1 : numberTemp;
                }
                if (a >= 0 && b >= 0)
                {
                    if (a > b)
                    {
                        distX = a - b;
                    }
                    else if (b > a)
                    {
                        distX = b - a;
                    }
                }

                return distX;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error DistanceBetweenVectorsAxis(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Obtain the result of the LawOfSine
        /// </summary>
        /// <param name="AngleA">The Angle you have already</param>
        /// <param name="SideA">One of the Sides you already have</param>
        /// <param name="SideB">The other Side you already have</param>
        /// <returns>a positive double, the "AngleB" of the triangle, in grades</returns>
        public static double LawOfSine(double AngleA, double SideA, double SideB)
        {
            try
            {
                //LawofSines
                //Remember, AngleB is affected by Sin
                double part1 = (Math.Sin(Math.PI / 180 * AngleA) * SideB); //Sin works in radians, not degrees
                double part2 = part1 / SideA;
                double AngleB = (180 / Math.PI) * Math.Asin(part2);

                return AngleB;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LawOfSine: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Obtain the result of the LawOfCosine (Side, Angle, Side)
        /// </summary>
        /// <param name="AngleA">The Angle you have already</param>
        /// <param name="SideB">One of the Sides you already have</param>
        /// <param name="SideC">The other Side you already have</param>
        /// <returns>The size of the side you are lacking in the triangle, as a positive double in grades</returns>
        public static double LawOfCosineSAS(double AngleA, double SideB, double SideC)
        {
            try
            {
                //Law of Cosines (S A S)
                //Remember, SideA should be to the pow2
                double part1 = Math.Pow(SideB, 2) + Math.Pow(SideC, 2);
                double part2 = -2 * SideB * SideC;
                double part3 = Math.Cos(AngleA * (Math.PI / 180.0)); //Math.Cos use radians, convert them from degrees

                double nextpart1 = part2 * part3;
                double nextpart2 = part1 + nextpart1;

                //Remember, SideA should be to the pow2
                double SideA = Math.Sqrt(nextpart2);
                return SideA;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error LawOfCosineSAS(): " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Obtain the angule between two sides who are builded from 2 Vector2 coords, it can be axis locked.
        /// </summary>
        /// <param name="touch">Vector2, The position of the free point</param>
        /// <param name="center">Vector2, The position of the center from where you are calculating</param>
        /// <returns>Returns the angle between both sides as a double</returns>
        public static double GetAngle(Vector3 v1, Vector3 v2, AxistToIgnore axistToIgnore = AxistToIgnore.Y)
        {
            if (axistToIgnore == AxistToIgnore.X)
            {
                return GetAngle(new Vector2(v1.Y, v1.Z), new Vector2(v2.Y, v2.Z));
            }
            if (axistToIgnore == AxistToIgnore.Y)
            {
                return GetAngle(new Vector2(v1.X, v1.Z), new Vector2(v2.X, v2.Z));
            }
            return GetAngle(new Vector2(v1.X, v1.Y), new Vector2(v2.X, v2.Y));
        }

        /// <summary>
        /// Obtain the angule between two sides who are builded from 2 Vector2 coords
        /// </summary>
        /// <param name="touch">Vector2, The position of the free point</param>
        /// <param name="center">Vector2, The position of the center from where you are calculating</param>
        /// <returns>Returns the angle between both sides as a double</returns>
        public static double GetAngle(Vector2 v1, Vector2 v2)
        {
            double X1 = Convert.ToDouble(v1.X);
            double X2 = Convert.ToDouble(v2.X);
            double Y1 = Convert.ToDouble(v1.Y);
            double Y2 = Convert.ToDouble(v2.Y);

            double dot = X1 * X2 + Y1 * Y2; // dot product
            double det = X1 * Y2 - Y1 * X2;  // determinant
            double angle = Math.Atan2(det, dot);  // atan2(y, x) or atan2(sin, cos)
            return angle;
        }

        //arctangent
        //TD
        public static double AngleRotationY(Entity obj, Vector3 destination)
        {
            Vector3 procDest = destination;
            if ((procDest.X - obj.Transform.Position.X) == 0)
            {
                return 0f;
            }
            //double slope = Convert.ToDouble(((procDest.Y - obj.FindChild("Gun").Transform.position.Y) / (procDest.X - obj.FindChild("Gun").Transform.position.X)));
            //double radiant = Math.Atan(slope);
            double radiant = Math.Atan2((procDest.Y - obj.Transform.Position.Y), (procDest.X - obj.Transform.Position.X));
            return radiant; //((radiant * 180) / Math.PI); //Degrees
        }

        public static float AngleRotationX(Entity obj, Vector3 destination)
        {
            Vector3 procDest = destination;
            if ((procDest.X - obj.Transform.Rotation.X) == 0)
            {
                return 0f;
            }
            return ((float)((procDest.Y - obj.Transform.Rotation.Y) / (procDest.X - obj.Transform.Rotation.X)));
        }


        //TD
        public static float AngleRotationZ(Entity obj, Vector3 destination)
        {
            Vector3 procDest = destination;
            if ((procDest.X - obj.FindChild("Gun").Transform.Rotation.X) == 0)
            {
                return 0f;
            }
            return ((float)((procDest.Y - obj.FindChild("Gun").Transform.Rotation.Y) / (procDest.X - obj.FindChild("Gun").Transform.Rotation.X)));
        }

        /// <summary>
        /// Rotate the position of the entity in the direction of the destination
        /// </summary>
        /// <param name="obj">The entity than is going to be rotated</param>
        /// <param name="destination">The direction than the entity should be rotated to</param>
        public static Quaternion RotateToDirection(Entity obj, Vector3 destination)
        {
            Vector3 direction = destination - obj.Transform.Position;
            Quaternion rotation = Quaternion.BetweenDirections(obj.Transform.WorldMatrix.TranslationVector, direction);
            obj.Transform.Rotation = Quaternion.Lerp(obj.Transform.Rotation, rotation, 1);
            return Quaternion.Lerp(obj.Transform.Rotation, rotation, 1);
        }

        //Rotate the position of the entity in the direction of the destination, do his job but it's suceptible of euler lock
        public static Quaternion RotateTo(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 towardsPlayer = obj.Transform.Position - destination;
                //towardsPlayer.Y = 0; //avoid up/down rotation
                towardsPlayer.Normalize();
                Quaternion result = Quaternion.BetweenDirections(Vector3.UnitZ, towardsPlayer);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: RotateTo(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        //Rotate the position of the entity in the direction of the destination, however, only works in 2D, it's also suceptible of euler lock
        public static Quaternion RotateTo2D(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 enemyPos = obj.Transform.Position;
                enemyPos.Y = destination.Y;
                Matrix lookAt = Matrix.LookAtRH(enemyPos, destination, Vector3.UnitY);
                Matrix resultMatrix = Matrix.Transpose(lookAt);
                //Quaternion result = Quaternion.RotationMatrix(resultMatrix);
                Quaternion result = Quaternion.Zero;
                Vector3 scale = Vector3.Zero;
                Vector3 translation = Vector3.Zero;
                resultMatrix.Decompose(out scale, out result, out translation);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: RotateTo(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        //Rotate the position of the entity in the direction of the destination (worldrotation) [ChildLookAtAlt]
        public enum Axis { X, Y, Z }

        public static Quaternion LookAtWithOnlyOneFreeAxis(Entity obj, Vector3 destination, Axis axis = Axis.Y)
        {
            try
            {
                Vector3 objPos = obj.Transform.Position;

                if (axis == Axis.X)
                {
                    objPos.X = obj.Transform.Position.X;
                }
                if (axis == Axis.Y)
                {
                    objPos.Y = obj.Transform.Position.Y;
                }
                if (axis == Axis.Z)
                {
                    objPos.Z = obj.Transform.Position.Z;
                }

                //objPos.Y = destination.Y;
                Matrix lookAt = Matrix.LookAtRH(objPos, destination, Vector3.UnitY);
                Matrix resultMatrix = Matrix.Transpose(lookAt);
                //Quaternion result = Quaternion.RotationMatrix(resultMatrix);
                Quaternion result = Quaternion.Zero;
                Vector3 scale = Vector3.Zero;
                Vector3 translation = Vector3.Zero;
                resultMatrix.Decompose(out scale, out result, out translation);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: LookAtAlt(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        public static Quaternion LookArWithBlockedAxis(Entity obj, Vector3 destination, Axis axis = Axis.Y)
        {
            try
            {
                Vector3 objPos = obj.Transform.Position;

                if (axis == Axis.X)
                {
                    objPos.X = obj.Transform.Position.X;
                }
                if (axis == Axis.Y)
                {
                    objPos.Y = obj.Transform.Position.Y;
                }
                if (axis == Axis.Z)
                {
                    objPos.Z = obj.Transform.Position.Z;
                }

                //objPos.Y = destination.Y;
                Matrix lookAt = Matrix.LookAtRH(objPos, destination, Vector3.UnitY);
                Matrix resultMatrix = Matrix.Transpose(lookAt);
                //Quaternion result = Quaternion.RotationMatrix(resultMatrix);
                Quaternion result = Quaternion.Zero;
                Vector3 scale = Vector3.Zero;
                Vector3 translation = Vector3.Zero;
                resultMatrix.Decompose(out scale, out result, out translation);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: LookAtAlt(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        //Rotate the position of the entity in the direction of the destination (worldrotation) [ChildLookAtAlt]
        public static Quaternion LookAtAlt2(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 enemyPos = obj.Transform.Position;
                enemyPos.Y = destination.Y;
                Matrix lookAt = Matrix.LookAtRH(enemyPos, destination, Vector3.UnitY);
                Matrix resultMatrix = Matrix.Transpose(lookAt);
                //Quaternion result = Quaternion.RotationMatrix(resultMatrix);
                Quaternion result = Quaternion.Zero;
                Vector3 scale = Vector3.Zero;
                Vector3 translation = Vector3.Zero;
                resultMatrix.Decompose(out scale, out result, out translation);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: LookAtAlt(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        //Rotate the position of the entity in the direction of the destination
        public static Quaternion LookAtAlt(Entity obj, Vector3 destination)
        {
            try
            {
                Vector3 enemyPos = obj.Transform.Position;
                enemyPos.Y = destination.Y;
                Matrix lookAt = Matrix.LookAtRH(enemyPos, destination, Vector3.UnitY);
                Matrix resultMatrix = Matrix.Transpose(lookAt);
                Quaternion result = Quaternion.RotationMatrix(resultMatrix);
                obj.Transform.Rotation = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: LookAtAlt(Entity, Vector3): " + ex.Message);
                return Quaternion.Zero;
            }
        }

        /// <summary>
        /// Rotate the position of the entity in the direction of the destination
        /// </summary>
        /// <param name="obj">The entity than is going to be rotated</param>
        /// <param name="target">The direction than the entity should be rotated to</param>
        public static void ManualLookAt(Entity obj, Vector3 target)
        {
            float x = target.X - obj.Transform.Position.X;
            float y = target.Z - obj.Transform.Position.Z;
            double newAngle = MathF.Atan2(y, x);
            newAngle *= UtilityAssistant.ConvertRadiansToDegrees(newAngle);
            obj.Transform.RotationEulerXYZ = new Vector3(0, (float)newAngle, 0);
        }

        /*public static void OrientedTo(Entity obj, Vector3 target)
        {
            Quaternion rotation = obj.Transform.Rotation;
            Vector3 characterMatrix = obj.Transform.WorldMatrix.TranslationVector;
            Quaternion tempRot = Quaternion.Slerp(rotation, rotation * Quaternion.RotationAxis(characterMatrix.Y, angle), elapsed * RotationSpeed);
            obj.Transform.Rotation *= tempRot;
        }*/

        /*void ManualLookAt(Entity obj, Vector3 target)
        {

            if (target != obj.Transform.position)
            {
                Vector3 viewForward = Vector3.Zero;
                Vector3 viewUp = Vector3.Zero;
                Vector3 viewRight = Vector3.Zero;


                // Create viewVector
                viewForward = target - obj.Transform.position;

                // normalize viewVector
                viewForward.Normalize();



                // Now we get the perpendicular projection of the viewForward vector onto the world up vector
                // Uperp = U - ( U.V / V.V ) * V
                viewUp = Vector3.up - (Vector3.Project(viewForward, Vector3.up));
                viewUp.Normalize();

                // Alternatively for getting viewUp you could just use:
                // viewUp = thisTransform.TransformDirection(thisTransform.up);
                // viewUp.Normalize();


                // Calculate rightVector using Cross Product of viewOut and viewUp
                // this is order is because we use left-handed coordinates
                viewRight = Vector3.Cross(viewUp, viewForward);


                // set new vectors
                thisTransform.right = new Vector3(viewRight.x, viewRight.y, viewRight.z);
                thisTransform.up = new Vector3(viewUp.x, viewUp.y, viewUp.z);
                thisTransform.forward = new Vector3(viewForward.x, viewForward.y, viewForward.z);
            }

            else
            {
                print("position vectors are equal. No rotation needed");
            }

        }*/

        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        public static double ConvertDegreesToRadiants(double degrees)
        {
            double radians = degrees * (Math.PI / 180);
            return (radians);
        }

        public static void MatrixFromYawPitchRoll()
        {
            return;
        }

        public static void YawForMatrix(double theta)
        {
            double result = 1 * 0 * 0 * 0 * Math.Cos(theta) * -Math.Sin(theta) * 0 * Math.Sin(theta) * Math.Cos(theta);
        }

        public static void PitchForMatrix(double theta)
        {
            double result = 1 * 0 * 0 * 0 * Math.Cos(theta) * -Math.Sin(theta) * 0 * Math.Sin(theta) * Math.Cos(theta);
        }

        public static void RollForMatrix()
        {

        }

        public static Stride.Core.Mathematics.Vector3 ConvertVector3NumericToStride(System.Numerics.Vector3 v3ToConvert)
        {
            return new Stride.Core.Mathematics.Vector3(v3ToConvert.X, v3ToConvert.Y, v3ToConvert.Z);
        }

        public static System.Numerics.Vector3 ConvertVector3StrideToNumeric(Stride.Core.Mathematics.Vector3 v3ToConvert)
        {
            return new System.Numerics.Vector3(v3ToConvert.X, v3ToConvert.Y, v3ToConvert.Z);
        }

        public static Stride.Core.Mathematics.Vector2 ConvertVector2NumericToStride(System.Numerics.Vector2 v2ToConvert)
        {
            return new Stride.Core.Mathematics.Vector2(v2ToConvert.X, v2ToConvert.Y);
        }

        public static System.Numerics.Vector2 ConvertVector2StrideToNumeric(Stride.Core.Mathematics.Vector2 v2ToConvert)
        {
            return new System.Numerics.Vector2(v2ToConvert.X, v2ToConvert.Y);
        }
    }

    public class EntityConverter : System.Text.Json.Serialization.JsonConverter<Entity>
    {
        public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                string strEntity = reader.GetString();
                string[] a = UtilityAssistant.CutJson(strEntity);
                Entity ent = new Entity(a[0]);
                ent.Transform.Position = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();
                ent.Transform.Rotation = UtilityAssistant.XmlToClass<Quaternion>(a[2]);
                return ent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvert) Read(): " + ex.Message);
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, Entity entity, JsonSerializerOptions options)
        {
            try
            {
                string name = "\"" + entity.Name + "\"";
                string position = new SerializedVector3(entity.Transform.Position).ToXML();
                string rotation = UtilityAssistant.QuaternionToXml(entity.Transform.Rotation);

                string resultJson = "{name:" + name + ", " + "position:" + position + ", rotation:" + rotation + "}";
                writer.WriteStringValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvert) Write(): " + ex.Message);
            }
        }
    }

    /*public class EntityConverterJSON : Newtonsoft.Json.JsonConverter<Entity>
    {
        public override void WriteJson(JsonWriter writer, Entity entity, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                string name = "\"" + entity.Name + "\"";
                string a = string.Empty;

                string position = String.Empty;
                string rotation = String.Empty;
                if (entity.GetAll<EntityComponent>().Count() > 0)
                {
                    a += "entro";
                    foreach (EntityComponent child in entity.GetAll<EntityComponent>())
                    {
                        Console.WriteLine(child.GetType().Name);
                        switch (child.GetType().Name)
                        {
                            case "TransformComponent":
                                position = new SerializedVector3(entity.Transform.Position).ToXML();
                                rotation = UtilityAssistant.QuaternionToXml(entity.Transform.Rotation);
                                Console.WriteLine("Entro en TransformComponent");
                                break;
                            case "SpriteComponent":
                                Console.WriteLine(child.GetType().Name);
                                Console.ForegroundColor = ConsoleColor.Green;

                                SpriteFromSheet spr = ((SpriteComponent)child).SpriteProvider as SpriteFromSheet;
                                int currentFrame = spr.CurrentFrame;

                                Console.WriteLine(spr.CurrentFrame);
                                Console.WriteLine("Comienzo muestra de interior de la Sheet");

                                foreach (Sprite item in spr.Sheet.Sprites)
                                {
                                    Console.WriteLine(item.ToString());
                                    Console.WriteLine(item);
                                }

                                Console.WriteLine("Fin muestra interior de la Sheet");
                                Console.ResetColor();

                                //Content.Load<Prefab>("Tileset/" + child.GetType().Name);
                                break;
                            case "RigidbodyComponent":
                                Console.WriteLine(child.GetType().Name);
                                break;
                            default:
                                Console.WriteLine("default: " + child.GetType().Name);
                                break;
                        }
                        //TransformComponent;
                        //SpriteComponent;
                        //RigidbodyComponent;
                    }
                }


                string b = a;

                string resultJson = "{name:" + name + ", " + "position:" + position + ", rotation:" + rotation + "}";
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvert) Write(): " + ex.Message);
            }
        }

        public override Entity ReadJson(JsonReader reader, Type objectType, Entity existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                string strEntity = (string)reader.Value;
                string[] a = UtilityAssistant.CutJson(strEntity);
                Entity ent = new Entity(a[0]);
                ent.Transform.Position = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();
                ent.Transform.Rotation = UtilityAssistant.XmlToClass<Quaternion>(a[2]);
                return ent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvertJSON) Read(): " + ex.Message);
                return null;
            }
        }
    }*/

    /*public class FurnitureConverterJSON : Newtonsoft.Json.JsonConverter<Furniture>
    {
        public override void WriteJson(JsonWriter writer, Furniture furniture, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                string name = "\"" + furniture.Entity.Name + "\"";
                string position = new SerializedVector3(furniture.Entity.Transform.Position).ToXML();
                string rotation = UtilityAssistant.QuaternionToXml(furniture.Entity.Transform.Rotation);
                string type = furniture.GetType().ToString();

                string resultJson = "{name:" + name + ", " + "position:" + position + ", rotation:" + rotation + ", type:" + type + "}";
                writer.WriteValue(resultJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvert) Write(): " + ex.Message);
            }
        }

        public override Furniture ReadJson(JsonReader reader, Type objectType, Furniture existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                string strEntity = (string)reader.Value;
                string[] a = UtilityAssistant.CutJson(strEntity);

                //Type typ = Furniture.TypesOfFurniture().Where(c => c.FullName == a[3]).FirstOrDefault();
                //object obtOfType = Activator.CreateInstance(typ); //Requires parameterless constructor.

                //((Furniture)obtOfType).Entity.Name = a[0];
                //((Furniture)obtOfType).Entity.Transform.position = UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3();
                //((Furniture)obtOfType).Entity.Transform.Rotation = UtilityAssistant.XmlToClass<Quaternion>(a[2]);

                //Controller.controller.worldController.FurnitureCreate(a[3], position: UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3(), Rotation: UtilityAssistant.XmlToClass<Quaternion>(a[2]));
                //Controller.ActiveScene.Entities.Add(((Furniture)obtOfType).Entity);
                string typeName = a[3].Replace("MMO_Client.Models.FurnitureModels.", "");

                return Controller.controller.worldController.FurnitureCreate(typeName, Position: UtilityAssistant.XmlToClass<SerializedVector3>(a[1]).ConvertToVector3(), Rotation: UtilityAssistant.XmlToClass<Quaternion>(a[2])); ;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (EntityConvertJSON) Read(): " + ex.Message);
                return null;
            }
        }
    }*/

    public static class ExtendedClasses
    {
        //This work like a charm
        public static void ChangeSpriteSheet(this SpriteComponent sprComp, SpriteSheet sprite)
        {
            try
            {
                SpriteFromSheet nwSfS = SpriteFromSheet.Create(sprite, sprite[0].Name);
                sprComp.SpriteProvider = (ISpriteProvider)nwSfS;
                SpriteFromSheet a = sprComp.SpriteProvider as SpriteFromSheet;
                a.CurrentFrame = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: (ExtendedClasses) ChangeSpriteSheet(SpriteSheet): " + ex.Message);
            }
        }

        public static void ChangeSpriteSheet(this SpriteComponent sprComp, Texture texture)
        {
            SpriteFromTexture nwSfT = (SpriteFromTexture)new Sprite(texture);
            SpriteSheet sprSheet = new SpriteSheet();
            sprSheet.Sprites.Add(nwSfT.GetSprite());
            sprComp.ChangeSpriteSheet(sprSheet);
        }
    }
}
