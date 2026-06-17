using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Input;

namespace GrayHare.Asteroids.Systems;

internal static class PlayerControl
{
    public static bool IsThrottling(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        InputActionMap actionMap = host.InputActions!;
        return actionMap.IsActionDown(GameConstants.Throttle, host.Input) || actionMap.GetAxisValue(GameConstants.Throttle, host.Input) > 0;
    }

    public static bool IsTurningLeft(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        InputActionMap actionMap = host.InputActions!;
        return actionMap.IsActionDown(GameConstants.Left, host.Input) || actionMap.GetAxisValue(GameConstants.LeftRight, host.Input) < 0;
    }

    public static bool IsTurningRight(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        InputActionMap actionMap = host.InputActions!;
        return actionMap.IsActionDown(GameConstants.Right, host.Input) || actionMap.GetAxisValue(GameConstants.LeftRight, host.Input) > 0;
    }

    internal static bool IsFiring(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        InputActionMap actionMap = host.InputActions!;
        return actionMap.WasActionPressed(GameConstants.Fire, host.Input);
    }
}
