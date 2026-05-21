using UnityEngine;
using UnityEngine.UI;

namespace Mikmod
{
    public class FSButtonToggle : MonoBehaviour
    {
        private Image[] images;

        public void Init(Image[] imgs)
        {
            images = imgs;

            SetColor(FSIngameHooks.IsHookEnabled
                ? Color.red
                : Color.green);
        }

        public void Toggle()
        {
            FSIngameHooks.IsHookEnabled =
                !FSIngameHooks.IsHookEnabled;

            SetColor(FSIngameHooks.IsHookEnabled
                ? Color.red
                : Color.green);
        }

        private void SetColor(Color color)
        {
            if (images == null)
                images = GetComponentsInChildren<Image>(true);

            foreach (var img in images)
            {
                if (img != null)
                    img.color = color;
            }
        }
    }
}