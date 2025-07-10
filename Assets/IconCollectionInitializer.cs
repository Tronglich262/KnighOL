using Assets.HeroEditor.Common.CommonScripts;
using UnityEngine;

public class IconCollectionInitializer : MonoBehaviour
{
    public IconCollection iconCollection;

    void Awake()
    {
        IconCollection.Active = iconCollection;
    }
}
    