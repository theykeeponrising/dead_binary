using System.Collections.Generic;

public class UnitIcons
{
    // Used as a quick way to designate icons to units

    const string icon_dummy = "Icons/icon_dummy";
    const string icon_scrapbot = "Icons/icon_scrapbot";
    const string icon_drone = "Icons/icon_drone";

    static Dictionary<UnitIconEnum, string> UnitIconsDict = new Dictionary<UnitIconEnum, string>() {
        { UnitIconEnum.DUMMY, icon_dummy },
        { UnitIconEnum.SCRAPBOT, icon_scrapbot },
        { UnitIconEnum.DRONE, icon_drone },
    };

    public static string GetIcon(UnitIconEnum match)
    {
        // Returns the icon of the provided unit

        return UnitIconsDict[match];
    }
}

public enum UnitIconEnum { DUMMY, SCRAPBOT, DRONE };
