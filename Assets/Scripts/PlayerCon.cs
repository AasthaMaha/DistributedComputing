using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCon : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public Material defaultMaterial;
    public Material itMaterial;

    private NetworkVariable<bool> isIt = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsServer && IsOwner)
        {
            isIt.Value = true;
            UpdatePlayerMaterial();
            Debug.Log($"{name} is initialized as 'It'");
        }
        else
        {
            Debug.Log($"{name} is NOT initialized as 'It'");
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Only allow local player movement

        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir.z += 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z -= 1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collision detected with: {other.name}");
        if (other.CompareTag("Player") && isIt.Value && IsOwner)
        {
            Debug.Log($"{name} is tagging {other.name}");
            PlayerCon otherPlayer = other.GetComponent<PlayerCon>();
            if (otherPlayer != null)
            {
                isIt.Value = false; // Current player is no longer "It"
                UpdatePlayerMaterial();
                otherPlayer.TagAsItServerRpc();
            }
            else
            {
                Debug.LogError($"{other.name} does not have a PlayerCon component");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TagAsItServerRpc()
    {
        Debug.Log($"{name} has been tagged as 'It'");
        isIt.Value = true;
        UpdatePlayerMaterial();
    }

    private void UpdatePlayerMaterial()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = isIt.Value ? itMaterial : defaultMaterial;
            Debug.Log($"{name} material updated to {(isIt.Value ? "It" : "Default")}");
        }
        else
        {
            Debug.LogError($"{name} does not have a MeshRenderer");
        }
    }

    public void DisableMovement()
    {
        Debug.Log($"{name} movement disabled");
        enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
