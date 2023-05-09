using System.Numerics;

namespace Map_Editor_HoD.TilesModels
{
    public class Grass : Map_Editor_HoD.TilesModels.Tile
    {
        public Grass(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Grass() 
        {
        }
    }
}
