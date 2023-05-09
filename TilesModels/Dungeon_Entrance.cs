using System.Numerics;

namespace Map_Editor_HoD.TilesModels
{
    public class Dungeon_Entrance : Map_Editor_HoD.TilesModels.Tile
    {
        public Dungeon_Entrance(string name = "", Vector3 position = default, Vector3 inworldpos = default) : base(name, position, inworldpos)
        {
        }

        public Dungeon_Entrance() 
        {
        }
    }
}
