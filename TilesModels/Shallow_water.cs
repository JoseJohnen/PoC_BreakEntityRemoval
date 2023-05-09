using System.Numerics;

namespace Map_Editor_HoD.TilesModels
{
    public class Shallow_water : Map_Editor_HoD.TilesModels.Tile
    {
        public Shallow_water(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Shallow_water() 
        {
        }
    }
}
