using UnityEngine;
using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common.Enums;

public class MegapackCharacterInitializer : MonoBehaviour
{
    public Character character;

    void Start()
    {
        if (character == null)
            character = GetComponent<Character>();

        // 👉 MEGAPACK: luôn reset
     //  character.ResetCharacterToNaked();
    }
}
