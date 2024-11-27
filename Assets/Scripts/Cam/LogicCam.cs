using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class LogicCam : MonoBehaviour
{

    [SerializeField]
    private List<CinemachineVirtualCamera> mVirtualCams;

    [SerializeField]
    private CinemachineFreeLook mOrbitalCam;

    private List<Character> mCharacters;
    
    public void Init()
    {
        SwitchTarget(mCharacters[0].name);
    }

    void Update()
    {
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
    public void SwitchTarget(string targetName)
    {
        GameObject newCharacter = GetCharacterByName(targetName);

        mOrbitalCam.Follow = newCharacter.transform;
        mOrbitalCam.LookAt = newCharacter.transform;

        foreach (var cam in mVirtualCams)
        {

            if (cam.gameObject.GetComponent<VirtualCamInfo>().UseFollow())
            {
                cam.Follow = newCharacter.transform;
            }

            if (cam.gameObject.GetComponent<VirtualCamInfo>().UseLookAt())
            {
                cam.LookAt = newCharacter.transform;
            }
        }
    }

    private GameObject GetCharacterByName(string name)
    {
        foreach (var character in mCharacters)
        {
            if(character.name == name)
            {
                return character.gameObject;
            }
        }
        return null;
    }

    public void RegisterCharcater(List<Character> listToRegister)
    {
        mCharacters = listToRegister;
    }
}