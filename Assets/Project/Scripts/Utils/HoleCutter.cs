using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class HoleCutter : Image
{
    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        Material toReturn = base.GetModifiedMaterial(baseMaterial);
        var created = new Material(toReturn);

        // This 'NotEqual' creates the hole effect in the stencil buffer
        created.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
        return created;
    }
}