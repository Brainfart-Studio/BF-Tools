using System.Collections;
using UnityEngine;

namespace BFTools.Systems.SceneManager
{
    public abstract class BFTransitionBehaviour : MonoBehaviour, ITransition
    {
        public abstract IEnumerator PlayOut();
        public abstract IEnumerator PlayIn();
    }
}