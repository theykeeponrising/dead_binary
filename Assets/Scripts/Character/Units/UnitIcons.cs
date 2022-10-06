using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIcons
{
    // Used as a quick way to designate icons to units

    const string icon_dummy = "Icons/icon_dummy";
    const string icon_scrapbot = "Icons/icon_scrapbot";

    static Dictionary<UnitIconEnum, string> UnitIconsDict = new Dictionary<UnitIconEnum, string>() {
        { UnitIconEnum.DUMMY, icon_dummy },
        { UnitIconEnum.SCRAPBOT, icon_scrapbot },
    };

    public static string GetIcon(UnitIconEnum match)
    {
        // Returns the icon of the provided unit

        return UnitIconsDict[match];
    }
}

public enum UnitIconEnum { DUMMY, SCRAPBOT };
