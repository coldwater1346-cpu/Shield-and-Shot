using Shield_Shot.InputSystem.Data;

namespace Shield_Shot.InputSystem
{
    public interface IInputContextTransformer
    {
        InputContext Transform(InputContext context);
    }
}