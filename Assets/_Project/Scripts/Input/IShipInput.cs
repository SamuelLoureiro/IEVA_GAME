using UnityEngine;

namespace VARG.Input
{
    public interface IShipInput
    {
        Vector2 Steer { get; }
        bool DescendPressed { get; }
        bool CalibratePressed { get; }
    }
}
