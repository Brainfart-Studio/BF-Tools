using System.Collections;

namespace BFTools.Core.SceneManager
{
    public interface ITransition
    {
        IEnumerator PlayOut();
        IEnumerator PlayIn();
    }
}