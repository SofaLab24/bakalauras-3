using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettingsPersistence
{
    void LoadSettings(List<BuildingSettings> buildingPresets);
    List<BuildingSettings> SaveSettings();
}
