using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RocketMan
{
    public struct GridTempModel
    {
        public IntVec3 Cell;

        public Map Map;

        public GridTempModel(IntVec3 cell, Map map)
        {
            Cell = cell;
            Map = map;
        }
    }

    public class GridTempModelComparer : EqualityComparer<GridTempModel>
    {
        public static GridTempModelComparer Instance = new GridTempModelComparer();

        private GridTempModelComparer()
        {
        }

        #region Overrides of EqualityComparer<GridTempModel>

        public override bool Equals(GridTempModel x, GridTempModel y)
        {
            return x.Cell == y.Cell && x.Map == y.Map;
        }

        public override int GetHashCode(GridTempModel obj)
        {
            unchecked
            {
                int hash;
                hash = HashUtility.HashOne(obj.Cell.GetHashCode());
                hash = HashUtility.HashOne(obj.Map.GetHashCode(), hash);

                return hash;
            }
        }

        #endregion
    }
}