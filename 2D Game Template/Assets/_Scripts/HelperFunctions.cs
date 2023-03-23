using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    //since cameras only have 1 instance and will not be destroyed, we get main camera from start
    private static Camera mainCamera;
    public static Camera MainCamera
    {
        get
        {
            //finding main camera is very expensive, so we only do it once from start
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera;
        }
    }

    //this is an extension function meaning that it can be called without static class and since it changes gameObject's pos, that argument can be
    //omitted because is implied when calling it on a gameObject (ex. call gameObject.SetObjectPos(Vector2.Zero) )
    public static void SetObjectPos(this GameObject gameObject, Vector2 position) => gameObject.transform.position = position;

    //public static void InstantiateGameObject(this GameObject prefab, Vector2 position) => Instantiate(prefab, position, Quaternion.identity);

    public static int GenerateRandomPrecentage() => UnityEngine.Random.Range(0, 100);

    public static void PlaySound(this AudioClip audioClip, AudioSource audioSource, float minPitch, float maxPitch, float volume)
    {
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(audioClip, volume);
    }

    public static Quaternion LookAt2D(Vector2 forward) => Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
      
    //gets the world coordinates of a UI elment give its rect transform (UI transform) (1 use case: so that you can position non-UI objects
    //to match behind UI elements, like particle effects)
    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, MainCamera, out var result);
        return result;
    }

    public static void DestroyChildren(this Transform parentTransform)
    {
        foreach (Transform child in parentTransform) Object.Destroy(child.gameObject);
    }

    //Note: this currently may not work
    public static T GetKey<T, V>(Dictionary<T, V> dictionary, V dictionaryValue)
    {
        T Key = default;
        foreach (KeyValuePair<T, V> pair in dictionary)
        {
            if (EqualityComparer<V>.Default.Equals(pair.Value, dictionaryValue))
            {
                Key = pair.Key;
                break;
            }
        }
        return Key;
    }
}
