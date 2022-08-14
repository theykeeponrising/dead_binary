using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour
{
    // Used by parent Tile script to locate any appplicable cover objects

    public enum CoverSize { half, full }
    public CoverSize coverSize;

    public int CoverBonus()
    {
        // Returns dodge chance percent bonus provided by cover

        if (coverSize == CoverSize.half)
            return 15;
        else if (coverSize == CoverSize.full)
            return 30;
        return 0;
    }
}
