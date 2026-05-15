using UnityEngine;

namespace ColorBlockJamClone.UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}