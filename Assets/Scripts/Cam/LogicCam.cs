using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class LogicCam : MonoBehaviour
{

    [SerializeField]
    private List<CinemachineVirtualCamera> mVirtualCams;

    [SerializeField]
    private CinemachineFreeLook mOrbitalCam;

    [SerializeField]
    private List<GameObject> mCharacters;

    private GameObject currentCharacter;

    void Start()
    {
        currentCharacter = mCharacters[0];
    }

    void Update()
    {

        // test need game logic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchTarget();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // use right tag
            SwitchCamera("FrontCam");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // use right tag
            SwitchCamera("BackCam");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // use right tag
            SwitchCamera("OrbitalCam");
        }
    }

    private void SwitchCamera(string targetTag)
    {
        if (targetTag == mOrbitalCam.tag)
        {
            mOrbitalCam.Priority = 2;
            return;
        }

        CinemachineVirtualCamera targetCam = mVirtualCams.Find(obj => obj.CompareTag(targetTag));

        foreach (var cam in mVirtualCams)
        {
            if (cam != targetCam)
            {
                cam.Priority = 0;
            }
            else
            {
                cam.Priority = 1;
            }
        }

        mOrbitalCam.Priority = 0;
    }
    private void SwitchTarget()
    {
        GameObject newCharacter = GetNextCharacter();

        mOrbitalCam.Follow = newCharacter.transform;
        mOrbitalCam.LookAt = newCharacter.transform;

        foreach (var cam in mVirtualCams)
        {

            if (cam.gameObject.GetComponent<VirtualCamInfo>().UseFollow())
            {
                Debug.Log("Follow changed");
                cam.Follow = newCharacter.transform;
            }

            if (cam.gameObject.GetComponent<VirtualCamInfo>().UseLookAt())
            {
                Debug.Log("Look at changed");
                cam.LookAt = newCharacter.transform;
            }
        }

        currentCharacter = newCharacter;
    }

    private GameObject GetNextCharacter()
    {
        int currentIndex = mCharacters.IndexOf(currentCharacter);
        int nextIndex = (currentIndex + 1) % mCharacters.Count;
        return mCharacters[nextIndex];
    }
}