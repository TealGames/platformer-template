using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraEffects : MonoBehaviour
{

    private Animator animator;


    [SerializeField] private CinemachineVirtualCamera vcam;
    private float defaultLensSize;


    public enum CameraType
    {
        None,
        Default,
        Stationary,
    }
    [SerializeField] private CameraType cameraType;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if (cameraType == CameraType.Default)
        {
            GameManager.Instance.OnSceneLoadEnded += () =>
            {
                Collider2D newConfinerCollider = GameObject.FindObjectOfType<Confiner>().GetComponent<Collider2D>();
                ChangeConfiner(newConfinerCollider);
            };
        }

        

        if (cameraType==CameraType.Default) vcam.Follow = PlayerCharacter.Instance.transform;

        defaultLensSize = vcam.m_Lens.OrthographicSize;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator ShakeCamera(float percentageOfLensSize, float cameraShakeDuration)
    {
        vcam.m_Lens.OrthographicSize = (percentageOfLensSize / 100) * vcam.m_Lens.OrthographicSize;
        yield return new WaitForSeconds(cameraShakeDuration);
        vcam.m_Lens.OrthographicSize = defaultLensSize;
        //animator.SetTrigger("shakeCamera_trig");
    }
    public void ChangeOrthoSize(float newLensSize) => vcam.m_Lens.OrthographicSize = newLensSize;

    public void ChangeConfiner(Collider2D newCollider) => vcam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = newCollider;

    //gets the approximate maximum value (a bit over) of the positive world x coordinate that the camera can see in its view
    public float GetMaxXCameraView()=> transform.position.x + (2f * vcam.m_Lens.OrthographicSize);

    public float GetMinXCameraView() => transform.position.x - (2f * vcam.m_Lens.OrthographicSize);
}
