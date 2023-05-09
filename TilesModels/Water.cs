using System.Numerics;

namespace Map_Editor_HoD.TilesModels
{
    public class Water : Map_Editor_HoD.TilesModels.Tile
    {
        public Water(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Water() 
        {
        }
    }
}
