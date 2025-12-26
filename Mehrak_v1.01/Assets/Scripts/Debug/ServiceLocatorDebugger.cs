using UnityEngine;
using System.Linq;

public class ServiceLocatorDebugger : MonoBehaviour
{
    void Start()
    {
        var inv = ServiceLocator.Get<IInventoryService>();
        var bus = ServiceLocator.Get<IEventBus>();
        Debug.Log($"ServiceLocatorDebugger: IInventoryService = {(inv!=null?inv.ToString():"null")}, IEventBus = {(bus!=null?bus.ToString():"null")}");

        if (inv != null)
        {
            try
            {
                var arr = inv.GetAllCounts();
                Debug.Log("Inventory counts: " + string.Join(",", arr.Select(x => x.ToString()).ToArray()));
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Cannot call GetAllCounts(): " + ex);
            }
        }
    }
}