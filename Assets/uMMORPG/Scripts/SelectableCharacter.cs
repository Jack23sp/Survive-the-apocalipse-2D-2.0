// small helper script that is added to character selection previews at runtime
using UnityEngine;
using Mirror;

public class SelectableCharacter : MonoBehaviour
{
    // index will be set by networkmanager when creating this script
    public int index = -1;
    public Transform characterToRotate;

    public void OnEnable()
    {
        characterToRotate = GetComponent<PlayerCharacterCreation>().playerChildObject.transform;
    }

    void OnMouseDown()
    {
        // set selection index
        ((NetworkManagerMMO)NetworkManager.singleton).selection = index;
    }

    void Update()
    {
        // selected?
        bool selected = ((NetworkManagerMMO)NetworkManager.singleton).selection != index;

        // set name overlay font style as indicator
        Player player = GetComponent<Player>();
        if (!player.isClient && !player.isServer)
        {
            characterToRotate.transform.rotation = new Quaternion(0.0f, 180.0f, 0.0f, 0.0f);
        }
        player.nameOverlay.fontStyle = selected ? FontStyle.Normal : FontStyle.Bold;
    }
}
