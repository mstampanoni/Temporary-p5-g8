using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OrbitalCinematic : MonoBehaviour
{
    [SerializeField]
    private float speedRotate;

    [SerializeField] CinemachineFreeLook _cam;

    [SerializeField] private float timeBeforeSwitch;

    [SerializeField] private float timeTransition;

    private float camMode = 1f;

    private int index = 0;

    private List<float> modes = new List<float> { 0f, 0.5f, 1 };

    private Coroutine coroutine = null;

    private void Reset()
    {
        speedRotate = 2f;
        timeBeforeSwitch = 10f;
        timeTransition = 0.5f;
        _cam = GetComponent<CinemachineFreeLook>();
    }


    void Start()
    {
    }

    void Update()
    {
        _cam.m_XAxis.Value += speedRotate * Time.fixedDeltaTime;

        if (coroutine == null)
        {
            coroutine = StartCoroutine(switchCamMode());
        }
    }

    private IEnumerator switchCamMode()
    {
        yield return new WaitForSeconds(timeBeforeSwitch);

        index += 1;

        if (index > modes.Count - 1)
        {
            index = 0;
        }

        float startValue = _cam.m_YAxis.Value;
        float elapsedTime = 0f;

        while (elapsedTime < timeTransition)
        {
            elapsedTime += Time.deltaTime;
            _cam.m_YAxis.Value = Mathf.Lerp(startValue, modes[index], elapsedTime / timeTransition);
            yield return null;
        }

        _cam.m_YAxis.Value = modes[index];

        coroutine = null;
    }
}