using UnityEngine;

public enum ItemType
{
    Invalid,
    Key,
    Door,
    Entity,
    Mace,
    Money,
    PastaFrola,
}

public class Item : MonoBehaviour
{
    public ItemType type;
    private Waypoint _wp;
    private bool _insideInventory;
    private bool _onFloor;
    public bool pickedUp;


    public void OnInventoryAdd()
    {
        Destroy(GetComponent<Rigidbody>());
        _insideInventory = true;
        if (_wp)
            _wp.nearbyItems.Remove(this);
    }

    public void OnInventoryRemove()
    {
        if (gameObject.GetComponent<Rigidbody>() == null)
            gameObject.AddComponent<Rigidbody>();
        _insideInventory = false;
    }

    private void Start()
    {
        _wp = Navigation.instance.NearestTo(transform.position);
        if (_wp != null)
        {
            _wp.nearbyItems.Add(this);
        }
    }

    public void Kill()
    {
        var ent = GetComponent<Entidad>();
        if (ent != null)
        {
            foreach (var it in ent.RemoveAllitems())
                it.transform.parent = null;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _wp.nearbyItems.Remove(this);
    }

    private void Update()
    {
        if (!_insideInventory)
        {
           
             _wp.nearbyItems.Remove(this);
            
             _wp = Navigation.instance.NearestTo(transform.position);
          
            
            _wp.nearbyItems.Add(this);

            _onFloor = true;

        }
    }
}
