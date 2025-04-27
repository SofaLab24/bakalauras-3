using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRunDataPersistence
{
    void LoadData(RunData data);
    void SaveData(ref RunData data);
}
