using System.Collections;

namespace BFTools.Systems.SceneManager
{
    public interface ITransition
    {
        IEnumerator PlayOut();
        IEnumerator PlayIn();
    }
}