using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InfoItemDatabase", menuName = "Scriptable Objects/InfoItemDatabase")]
public class InfoItemDatabase : ScriptableObject
{
    public Sprite[] prefileImages;
    public Sprite[] frameImages;

    public Sprite GetProfileSprite(int index) => (index >= 0 && 
        index < prefileImages.Length) ? prefileImages[index] : null;
    public Sprite GetFrameSprite(int index) => (index >= 0 && 
        index < frameImages.Length) ? frameImages[index] : null;
}
