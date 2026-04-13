using UnityEngine;

public class ChatResponsiveLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bottomDesktop;
    [SerializeField] private GameObject bottomMobile;
    [SerializeField] private RectTransform scrollViewRect;

    [Header("Settings")]
    [SerializeField] private float mobileAspectThreshold = 0.8f;
    [SerializeField] private float desktopScrollBottom = 170f;
    [SerializeField] private float mobileScrollBottom = 240f;

    private void Start()
    {
        ApplyLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        bool isMobileLayout = IsMobileLayout();

        if (bottomDesktop != null)
        {
            bottomDesktop.SetActive(!isMobileLayout);
        }

        if (bottomMobile != null)
        {
            bottomMobile.SetActive(isMobileLayout);
        }

        if (scrollViewRect != null)
        {
            Vector2 offsetMin = scrollViewRect.offsetMin;
            offsetMin.y = isMobileLayout ? mobileScrollBottom : desktopScrollBottom;
            scrollViewRect.offsetMin = offsetMin;
        }
    }

    private bool IsMobileLayout()
    {
        float aspect = (float)Screen.width / Screen.height;
        return aspect < mobileAspectThreshold;
    }
}