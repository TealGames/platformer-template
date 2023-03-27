using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : Trigger
{

    [Header("Scene Load Trigger")]
    [SerializeField] private LevelSO loadLevelData;
    [SerializeField][Tooltip("On the scene change, the location to teleport the player")] private Transform teleportPlayerPoint;

    private bool sceneTriggered = false;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnEnter(collider);
        if (collider != null)
        {
            if (collider.gameObject.TryGetComponent<PlayerCharacter>(out PlayerCharacter playerScript))
            {
                inTrigger = true;

                if (teleportPlayerPoint != null && loadLevelData != null)
                {
                    UnityEngine.Debug.Log("The player has entered the hitbox");
                    StartCoroutine(GameManager.Instance.LoadSceneAsync(loadLevelData, true, teleportPlayerPoint));
                    //sceneTriggered = true;
                    //StartCoroutine(SceneCooldown());
                }
            }
        }
    }
}
