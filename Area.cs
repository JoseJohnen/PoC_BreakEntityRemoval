using Map_Editor_HoD.Assistants;
using Map_Editor_HoD.Code.Models;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;

namespace Map_Editor_HoD
{
    [Serializable]
    [Stride.Core.DataContract]
    //[JsonObjectAttribute]
    public class Area : List<AreaDefiner>
    {
        //: IEnumerable<AreaDefiner>
        public string Name { get => name; set => name = value; }
        public List<AreaDefiner> L_AreaDefiners { get => l_AreaDefiners; set => l_AreaDefiners = value; }

        //[JsonProperty("L_AreaDefiners")]
        private List<AreaDefiner> l_AreaDefiners = new List<AreaDefiner>();
        private string name;

        #region Constructores
        //[JsonConstructor]
        public Area(string name = "")
        {
            this.Name = name;
            this.L_AreaDefiners = new List<AreaDefiner>();
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
        }

        //Forma cúbica de área TODO: falta evaluación arriba y abajo entre los dos cuadrados
        public Area(Vector3 NWU, Vector3 SEU, Vector3 NWD, Vector3 SED, string name = "")
        {
            //S Z++
            //E X++

            SerializedVector3 nwu = new SerializedVector3(NWU);
            SerializedVector3 neu = new SerializedVector3(new Vector3(SEU.X, NWU.Y, NWU.Z));
            SerializedVector3 swu = new SerializedVector3(new Vector3(NWU.X, SEU.Y, SEU.Z));
            SerializedVector3 seu = new SerializedVector3(SEU);

            SerializedVector3 nwd = new SerializedVector3(NWD);
            SerializedVector3 ned = new SerializedVector3(new Vector3(SED.X, NWD.Y, NWD.Z));
            SerializedVector3 swd = new SerializedVector3(new Vector3(NWD.X, SED.Y, SED.Z));
            SerializedVector3 sed = new SerializedVector3(SED);

            this.Name = name;
            this.L_AreaDefiners = new List<AreaDefiner>();

            //Cuadrado arriba
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NWU", nwu), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NEU", neu), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SWU", swu), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SEU", seu), name));

            //Cuadrado abajo
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NWD", nwd), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NED", ned), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SWD", swd), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SED", sed), name));
        }

        //Forma cuadrada de área (Para TOP-DOWN)
        public Area(Vector3 NW, Vector3 SE, string name = "")
        {
            //S Z++
            //E X++

            SerializedVector3 nw = new SerializedVector3(NW);
            SerializedVector3 ne = new SerializedVector3(new Vector3(SE.X, NW.Y, NW.Z));
            SerializedVector3 sw = new SerializedVector3(new Vector3(NW.X, SE.Y, SE.Z));
            SerializedVector3 se = new SerializedVector3(SE);

            this.Name = name;
            this.L_AreaDefiners = new List<AreaDefiner>();

            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NW", nw), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("NE", ne), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SW", sw), name));
            this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>("SE", se), name));
        }

        public Area(List<AreaDefiner> L_AreaDefiners, string Name = "")
        {
            this.Name = Name;
            this.L_AreaDefiners = L_AreaDefiners;
        }

        public Area(List<Vector3> l_vector3, string Name = "")
        {
            this.L_AreaDefiners = new List<AreaDefiner>();
            foreach (Vector3 item in l_vector3)
            {
                this.L_AreaDefiners.Add(new AreaDefiner(new Pares<string, SerializedVector3>(string.Empty, new SerializedVector3(item)), Name));
            }
            this.Name = Name;
        }

        public Area(IEnumerable<AreaDefiner> collection) : base(collection)
        {
        }

        public Area()
        {
            this.L_AreaDefiners = new List<AreaDefiner>();
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
            this.L_AreaDefiners.Add(new AreaDefiner());
        }
        #endregion

        #region Suplementary Functions
        //Check if the move of one specific entity is valid inside the area generated by the list, and if it is, allow it to move, otherwise will return the original position of the entity.
        public static Vector3 MoveMeThereOnlyIfValidCheckEverything(List<Area> l_areas, Entity entity, Vector3 MovementModifier)
        {
            try
            {
                Vector3 VectorArea = new Vector3();
                foreach (Area area in l_areas)
                {
                    VectorArea = MoveMeThereOnlyIfValid(area.L_AreaDefiners, entity, MovementModifier, UtilityAssistant.Axis.Y);
                    if (VectorArea == entity.Transform.Position)
                    {
                        return entity.Transform.Position;
                    }
                }
                return VectorArea;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MoveMeThereOnlyIfValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier): " + ex.Message);
                return entity.Transform.Position;
            }
        }

        //Check if the move of one specific entity is valid inside the area generated by the list, and if it is, allow it to move, otherwise will return the original position of the entity.
        public static Vector3 MoveMeThereOnlyIfValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier, UtilityAssistant.Axis IgnoredAxis = UtilityAssistant.Axis.Z)
        {
            try
            {
                AreaDefiner NW = null;
                AreaDefiner NE = null;
                AreaDefiner SW = null;
                AreaDefiner SE = null;

                foreach (AreaDefiner areaDefiner in l_areaDefiners)
                {
                    if (areaDefiner.Point.Item1 == "NW")
                    {
                        NW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "NE")
                    {
                        NE = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SW")
                    {
                        SW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SE")
                    {
                        SE = areaDefiner;
                    }
                }

                Vector3 SouthLine = new Vector3(); //South + SouthVariance;
                Vector3 NorthLine = new Vector3(); //North + NorthVariance;
                Vector3 EastLine = new Vector3(); //East + EastVariance;
                Vector3 WestLine = new Vector3(); //West + WestVariance;

                if(IgnoredAxis == UtilityAssistant.Axis.Z)
                {
                    SouthLine.Y = (((SE.Point.Item2.Y - SW.Point.Item2.Y) / 2) + SW.Point.Item2.Y);
                    NorthLine.Y = (((NE.Point.Item2.Y - NW.Point.Item2.Y) / 2) + NW.Point.Item2.Y);

                    EastLine.X = (((NE.Point.Item2.X - SE.Point.Item2.X) / 2) + SE.Point.Item2.X);
                    WestLine.X = (((NW.Point.Item2.X - SW.Point.Item2.X) / 2) + SW.Point.Item2.X);
                }
                else
                {
                    SouthLine.Y = (((SE.Point.Item2.Z - SW.Point.Item2.Z) / 2) + SW.Point.Item2.Z);
                    NorthLine.Y = (((NE.Point.Item2.Z - NW.Point.Item2.Z) / 2) + NW.Point.Item2.Z);

                    EastLine.X = (((NE.Point.Item2.X - SE.Point.Item2.X) / 2) + SE.Point.Item2.X);
                    WestLine.X = (((NW.Point.Item2.X - SW.Point.Item2.X) / 2) + SW.Point.Item2.X);
                }
                
                bool sur = false;
                bool norte = false;
                bool este = false;
                bool oeste = false;

                Vector3 futureEntityPosition = entity.Transform.Position;
                if (IgnoredAxis == UtilityAssistant.Axis.Z)
                {
                    if (MovementModifier.Y > 0)
                    {
                        futureEntityPosition.Y += (MovementModifier).Y;
                    }
                    else if (MovementModifier.Y < 0)
                    {
                        futureEntityPosition.Y += (MovementModifier).Y;
                    }

                    //Si cae dentro de la zona, no se mueve
                    if ((futureEntityPosition).Y >= SouthLine.Y) //South Limit
                    {
                        if ((entity.Transform.Position).Y < NorthLine.Y)
                        {
                            //Console.WriteLine("Desde el sur entro");
                            sur = true;

                        }
                    }

                    if ((futureEntityPosition).Y <= NorthLine.Y) //North Limit
                    {
                        if ((entity.Transform.Position).Y > SouthLine.Y)
                        {
                            //Console.WriteLine("Desde el norte entro");
                            norte = true;
                        }
                    }
                }
                else
                {
                    if (MovementModifier.Z > 0)
                    {
                        futureEntityPosition.Z += (MovementModifier).Z;
                    }
                    else if (MovementModifier.Z < 0)
                    {
                        futureEntityPosition.Z += (MovementModifier).Z;
                    }

                    //Si cae dentro de la zona, no se mueve
                    if ((futureEntityPosition).Z >= SouthLine.Z) //South Limit
                    {
                        if ((entity.Transform.Position).Z < NorthLine.Z)
                        {
                            //Console.WriteLine("Desde el sur entro");
                            sur = true;

                        }
                    }

                    if ((futureEntityPosition).Z <= NorthLine.Z) //North Limit
                    {
                        if ((entity.Transform.Position).Z > SouthLine.Z)
                        {
                            //Console.WriteLine("Desde el norte entro");
                            norte = true;
                        }
                    }
                }

                if (MovementModifier.X > 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }
                else if (MovementModifier.X < 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }

                if ((futureEntityPosition).X <= EastLine.X) //East Limit
                {
                    if ((entity.Transform.Position).X > WestLine.X)
                    {
                        //Console.WriteLine("Desde el este entro");
                        este = true;
                    }
                }

                if ((futureEntityPosition).X >= WestLine.X) //West Limit
                {
                    if ((entity.Transform.Position).X < EastLine.X)
                    {
                        //Console.WriteLine("Desde el oeste entro");
                        oeste = true;
                    }
                }

                if (norte && sur && este && oeste)
                {
                    return entity.Transform.Position;
                }

                return entity.Transform.Position + MovementModifier;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MoveMeThereOnlyIfValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier): " + ex.Message);
                return entity.Transform.Position;
            }
        }

        //Check if the move of one specific entity is valid inside the area generated by the list, and if it is, allow it to move, otherwise will bounce the entity to his original position.
        public static Vector3 MoveMeThereInsideIfValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier)
        {
            try
            {
                AreaDefiner NW = null;
                AreaDefiner NE = null;
                AreaDefiner SW = null;
                AreaDefiner SE = null;

                foreach (AreaDefiner areaDefiner in l_areaDefiners)
                {
                    if (areaDefiner.Point.Item1 == "NW")
                    {
                        NW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "NE")
                    {
                        NE = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SW")
                    {
                        SW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SE")
                    {
                        SE = areaDefiner;
                    }
                }

                Vector3 SouthLine = new Vector3(); //South + SouthVariance;
                Vector3 NorthLine = new Vector3(); //North + NorthVariance;
                Vector3 EastLine = new Vector3(); //East + EastVariance;
                Vector3 WestLine = new Vector3(); //West + WestVariance;

                SouthLine.Y = (((SE.Point.Item2.Y - SW.Point.Item2.Y) / 2) + SW.Point.Item2.Y);
                NorthLine.Y = (((NE.Point.Item2.Y - NW.Point.Item2.Y) / 2) + NW.Point.Item2.Y);

                EastLine.X = (((NE.Point.Item2.X - SE.Point.Item2.X) / 2) + SE.Point.Item2.X);
                WestLine.X = (((NW.Point.Item2.X - SW.Point.Item2.X) / 2) + SW.Point.Item2.X);

                bool sur = false;
                bool norte = false;
                bool este = false;
                bool oeste = false;

                //Si cae dentro de la zona, no se mueve
                if ((entity.Transform.Position).Y >= SouthLine.Y) //South Limit
                {
                    if ((entity.Transform.Position).Y < NorthLine.Y)
                    {
                        Console.WriteLine("Desde el sur entro");
                        sur = true;

                    }
                }

                if ((entity.Transform.Position).Y <= NorthLine.Y) //North Limit
                {
                    if ((entity.Transform.Position).Y > SouthLine.Y)
                    {
                        Console.WriteLine("Desde el norte entro");
                        norte = true;
                    }
                }

                if ((entity.Transform.Position).X <= EastLine.X) //East Limit
                {
                    if ((entity.Transform.Position).X > WestLine.X)
                    {
                        Console.WriteLine("Desde el este entro");
                        este = true;
                    }
                }

                if ((entity.Transform.Position).X >= WestLine.X) //West Limit
                {
                    if ((entity.Transform.Position).X < EastLine.X)
                    {
                        Console.WriteLine("Desde el oeste entro");
                        oeste = true;
                    }
                }

                if (norte && sur && este && oeste)
                {
                    if (MovementModifier.X > 0)
                    {
                        do
                        {
                            entity.Transform.Position.X -= MovementModifier.X;
                        } while (entity.Transform.Position.X >= WestLine.X - .25f);
                    }
                    else if (MovementModifier.X < 0)
                    {
                        do
                        {
                            entity.Transform.Position.X -= MovementModifier.X;
                        } while (entity.Transform.Position.X <= EastLine.X + .25f);
                    }
                    if (MovementModifier.Y > 0)
                    {
                        do
                        {
                            entity.Transform.Position.Y -= MovementModifier.Y;
                        } while (entity.Transform.Position.Y >= SouthLine.Y - .25f);
                    }
                    else if (MovementModifier.Y < 0)
                    {
                        do
                        {
                            entity.Transform.Position.Y -= MovementModifier.Y;
                        } while (entity.Transform.Position.Y <= NorthLine.Y + .25f);
                    }
                }

                return entity.Transform.Position + MovementModifier;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error MoveMeThereInsideIfValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier): " + ex.Message);
                return entity.Transform.Position;
            }
        }

        //Check if the move of one specific entity is valid inside the area generated by the list, if it is return true, otherwise false.
        public static bool isMoveMeThereInsideValid(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier)
        {
            try
            {
                AreaDefiner NW = null;
                AreaDefiner NE = null;
                AreaDefiner SW = null;
                AreaDefiner SE = null;

                foreach (AreaDefiner areaDefiner in l_areaDefiners)
                {
                    if (areaDefiner.Point.Item1 == "NW")
                    {
                        NW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "NE")
                    {
                        NE = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SW")
                    {
                        SW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SE")
                    {
                        SE = areaDefiner;
                    }
                }

                Vector3 SouthLine = new Vector3(); //South + SouthVariance;
                Vector3 NorthLine = new Vector3(); //North + NorthVariance;
                Vector3 EastLine = new Vector3(); //East + EastVariance;
                Vector3 WestLine = new Vector3(); //West + WestVariance;

                SouthLine.Y = (((SE.Point.Item2.Y - SW.Point.Item2.Y) / 2) + SW.Point.Item2.Y);
                NorthLine.Y = (((NE.Point.Item2.Y - NW.Point.Item2.Y) / 2) + NW.Point.Item2.Y);

                EastLine.X = (((NE.Point.Item2.X - SE.Point.Item2.X) / 2) + SE.Point.Item2.X);
                WestLine.X = (((NW.Point.Item2.X - SW.Point.Item2.X) / 2) + SW.Point.Item2.X);

                bool sur = false;
                bool norte = false;
                bool este = false;
                bool oeste = false;

                //Prepara la posicion futura para evaluar.
                Vector3 futureEntityPosition = entity.Transform.WorldMatrix.TranslationVector;
                if (MovementModifier.Y > 0)
                {
                    futureEntityPosition.Y += (MovementModifier).Y;
                }
                else if (MovementModifier.Y < 0)
                {
                    futureEntityPosition.Y += (MovementModifier).Y;
                }

                if (MovementModifier.X > 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }
                else if (MovementModifier.X < 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }
                //Fin preparacion posicion futura

                //Si cae dentro de la zona, no se mueve
                if ((futureEntityPosition).Y >= SouthLine.Y) //South Limit
                {
                    if ((entity.Transform.Position).Y < NorthLine.Y)
                    {
                        sur = true;
                    }
                }

                if ((futureEntityPosition).Y <= NorthLine.Y) //North Limit
                {
                    if ((entity.Transform.Position).Y > SouthLine.Y)
                    {
                        norte = true;
                    }
                }

                if ((futureEntityPosition).X <= EastLine.X) //East Limit
                {
                    if ((entity.Transform.Position).X > WestLine.X)
                    {
                        este = true;
                    }
                }

                if ((futureEntityPosition).X >= WestLine.X) //West Limit
                {
                    if ((entity.Transform.Position).X < EastLine.X)
                    {
                        oeste = true;
                    }
                }

                if (norte && sur && este && oeste)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error bool isBreakingInside(AreaDefiner areaDefiner, Entity entity): " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Check if the entity is inside the area defined and avoid it to traspass it.
        /// </summary>
        /// <param name="l_areaDefiners">Areas defined like the limits who cannot be passed</param>
        /// <param name="entity">Entity restricted by this function</param>
        /// <param name="MovementModifier">Direction of the movement</param>
        /// <returns>bool: true if the new position is safe to move, false if it will traspass the defined bounds</returns>
        public static bool AvoidEntityLeave(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier)
        {
            try
            {
                AreaDefiner NW = null;
                AreaDefiner NE = null;
                AreaDefiner SW = null;
                AreaDefiner SE = null;

                foreach (AreaDefiner areaDefiner in l_areaDefiners)
                {
                    if (areaDefiner.Point.Item1 == "NW")
                    {
                        NW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "NE")
                    {
                        NE = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SW")
                    {
                        SW = areaDefiner;
                    }

                    if (areaDefiner.Point.Item1 == "SE")
                    {
                        SE = areaDefiner;
                    }
                }

                Vector3 SouthLine = new Vector3(); //South + SouthVariance;
                Vector3 NorthLine = new Vector3(); //North + NorthVariance;
                Vector3 EastLine = new Vector3(); //East + EastVariance;
                Vector3 WestLine = new Vector3(); //West + WestVariance;

                SouthLine.Y = (((SE.Point.Item2.Y - SW.Point.Item2.Y) / 2) + SW.Point.Item2.Y);
                NorthLine.Y = (((NE.Point.Item2.Y - NW.Point.Item2.Y) / 2) + NW.Point.Item2.Y);

                EastLine.X = (((NE.Point.Item2.X - SE.Point.Item2.X) / 2) + SE.Point.Item2.X);
                WestLine.X = (((NW.Point.Item2.X - SW.Point.Item2.X) / 2) + SW.Point.Item2.X);

                bool horizontal = false;
                bool vertical = false;

                //Prepara la posicion futura para evaluar.
                Vector3 futureEntityPosition = entity.Transform.Position;
                if (MovementModifier.Y > 0)
                {
                    futureEntityPosition.Y += (MovementModifier).Y;
                }
                else if (MovementModifier.Y < 0)
                {
                    futureEntityPosition.Y += (MovementModifier).Y;
                }

                if (MovementModifier.X > 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }
                else if (MovementModifier.X < 0)
                {
                    futureEntityPosition.X += (MovementModifier).X;
                }
                //Fin preparacion posicion futura

                //Si intenta salir de la zona, no se mueve
                if (((futureEntityPosition).Y <= SouthLine.Y) || ((futureEntityPosition).Y >= NorthLine.Y)) //Vertical Limits
                {
                    horizontal = true;
                }

                if (((futureEntityPosition).X >= EastLine.X) || ((futureEntityPosition).X <= WestLine.X)) //Horizontal Limits
                {
                    vertical = true;
                }

                if (horizontal || vertical)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error AvoidEntityLeave(List<AreaDefiner> l_areaDefiners, Entity entity, Vector3 MovementModifier): " + ex.Message);
                return false;
            }
        }

        //Obtiene los valores necesarios para construír un borde desde dos AreaDefiners (Norte y Sur).
        public static Vector3 BorderAreaDefinerNS(AreaDefiner a, AreaDefiner b, ref Vector3 variance)
        {
            try
            {
                if ((a.Point.Item2.Y < 0 && b.Point.Item2.Y < 0) || (a.Point.Item2.Y > 0 && b.Point.Item2.Y > 0))
                {
                    if (a.Point.Item2.Y > b.Point.Item2.Y)
                    {
                        SerializedVector3 tempVect3 = new SerializedVector3(b.Point.Item2.X, b.Point.Item2.Y * -1, b.Point.Item2.Z);
                        variance = SerializedVector3.ConvertToVector3(tempVect3 - a.Point.Item2);
                        return a.Point.Item2.ConvertToVector3();
                    }
                    else if (a.Point.Item2.Y < b.Point.Item2.Y)
                    {
                        SerializedVector3 tempVect3 = new SerializedVector3(a.Point.Item2.X, a.Point.Item2.Y * -1, a.Point.Item2.Z);
                        variance = SerializedVector3.ConvertToVector3(tempVect3 - b.Point.Item2);
                        return b.Point.Item2.ConvertToVector3();
                    }
                }
                else
                {
                    if (a.Point.Item2.Y > b.Point.Item2.Y)
                    {
                        variance = SerializedVector3.ConvertToVector3(b.Point.Item2 - a.Point.Item2);
                        return a.Point.Item2.ConvertToVector3();
                    }
                    else if (a.Point.Item2.Y < b.Point.Item2.Y)
                    {
                        variance = SerializedVector3.ConvertToVector3(a.Point.Item2 - b.Point.Item2);
                        return b.Point.Item2.ConvertToVector3();
                    }
                }
                variance = Vector3.Zero;
                return a.Point.Item2.ConvertToVector3();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BorderAreaDefiner(AreaDefiner a, AreaDefiner b, ref Vector3 variance): " + ex.Message);
                return Vector3.Zero;
            }
        }

        //Obtiene los valores necesarios para construír un borde desde dos AreaDefiners (Oeste y Este).
        public static Vector3 BorderAreaDefinerOE(AreaDefiner a, AreaDefiner b, ref Vector3 variance)
        {
            try
            {
                if ((a.Point.Item2.X < 0 && b.Point.Item2.X < 0) || (a.Point.Item2.X > 0 && b.Point.Item2.X > 0))
                {
                    if (a.Point.Item2.X > b.Point.Item2.X)
                    {
                        SerializedVector3 tempVect3 = new SerializedVector3(b.Point.Item2.X * -1, b.Point.Item2.Y, b.Point.Item2.Z);
                        variance = SerializedVector3.ConvertToVector3(tempVect3 - a.Point.Item2);
                        return a.Point.Item2.ConvertToVector3();
                    }
                    else if (a.Point.Item2.X < b.Point.Item2.X)
                    {
                        SerializedVector3 tempVect3 = new SerializedVector3(a.Point.Item2.X * -1, a.Point.Item2.Y, a.Point.Item2.Z);
                        variance = SerializedVector3.ConvertToVector3(tempVect3 - b.Point.Item2);
                        return b.Point.Item2.ConvertToVector3();
                    }
                }
                else
                {
                    if (a.Point.Item2.X > b.Point.Item2.X)
                    {
                        variance = SerializedVector3.ConvertToVector3(b.Point.Item2 - a.Point.Item2);
                        return a.Point.Item2.ConvertToVector3();
                    }
                    else if (a.Point.Item2.X < b.Point.Item2.X)
                    {
                        variance = SerializedVector3.ConvertToVector3(a.Point.Item2 - b.Point.Item2);
                        return b.Point.Item2.ConvertToVector3();
                    }
                }
                variance = Vector3.Zero;
                return a.Point.Item2.ConvertToVector3();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error BorderAreaDefiner(AreaDefiner a, AreaDefiner b, ref Vector3 variance): " + ex.Message);
                return Vector3.Zero;
            }
        }
        #endregion

        #region Métodos JSON
        public string ToJson()
        {
            try
            {
                string strTemp = string.Empty;//"{";
                int i = 0;
                int last = 0;
                strTemp += "\"L_AreaDefiners\" : [";
                last = this.L_AreaDefiners.Count;
                foreach (AreaDefiner item in L_AreaDefiners)
                {
                    strTemp += item.ToJson();
                    if (i < last - 1)
                    {
                        strTemp += ",";
                    }
                    i++;
                }
                strTemp += "]"; //,";

                while (strTemp.Contains("\"\""))
                {
                    strTemp = strTemp.Replace("\"\"", "\"");
                }

                while (strTemp.Contains("\\"))
                {
                    strTemp = strTemp.Replace("\\", "");
                }

                char[] a = { '"' };

                string nstring = !string.IsNullOrWhiteSpace(Name) ? Name : "null"; 
                string wr = string.Concat("{", new string(a), "Name", new string(a), ":", new string(a), Name, new string(a),
                   ", ", strTemp,
                   "}");

                return wr;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Area) ToJson(): " + ex.Message);
                return string.Empty;
            }
        }

        public Area FromJson(string json)
        {
            string txt = json;
            try
            {
                txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(txt.Replace("\u002B", "+"));
                string[] strJsonArray = new string[1];
                string[] strStrArr = new string[1];

                this.Name = UtilityAssistant.ExtractValue(txt, "Name").Replace("\"","");

                strJsonArray[0] = txt;

                string strTemp = strJsonArray[0].Substring(strJsonArray[0].IndexOf("L_AreaDefiners")).Replace("L_AreaDefiners", "");
                strTemp = strTemp.Substring(strTemp.IndexOf("[{")+1);
                //strTemp = strTemp.Replace("\"Nombre\"", "\"Nombre\":\"");
                strTemp = strTemp.Replace("]}", "");
                strTemp = strTemp.Replace("},{", "}|°|{");
                AreaDefiner areaDefiner = null;
                List<string> l_string = new List<string>(strTemp.Split("|°|", StringSplitOptions.RemoveEmptyEntries));
                foreach (string item in l_string)
                {
                    areaDefiner = AreaDefiner.CreateFromJson(item);
                    this.l_AreaDefiners.Add(areaDefiner);
                }

                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nError (Area) FromJson(): " + json + " ex.Message: " + ex.Message);
                return null;
            }
        }

        public static Area CreateFromJson(string json)
        {
            try
            {
                string txt = Interfaz.Utilities.UtilityAssistant.CleanJSON(json);
                Area area = new Area();
                return area.FromJson(txt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (Area) CreateFromJson(): " + ex.Message);
                return null;
            }
        }
        #endregion

        #region ForEach Compatibility
        /*public IEnumerator<AreaDefiner> GetEnumerator()
        {
            //return (IEnumerator)this;
            foreach (AreaDefiner o in L_AreaDefiners)
            {
                if (o == null)
                {
                    break;
                }
                yield return o;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //return (IEnumerator)this;
            foreach (AreaDefiner o in L_AreaDefiners)
            {
                if (o == null)
                {
                    break;
                }
                yield return o;
            }
        }*/

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
