using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Environement : MonoBehaviour
{

    [SerializeField]
    private GameObject mLightningPrefab;

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private List<Transform> lightningPos;

    [SerializeField]
    private Material defaultSkyBox;

    [SerializeField]
    private Material stormSkyBox;

    private Coroutine coroutine;


    // Update is called once per frame
    void Update()
    {
        if (gameManager.isInUltMode())
        {
            if (coroutine == null)
            {
                coroutine = StartCoroutine(LightningStrikes());
            }
        }
        else
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;

                RenderSettings.skybox = defaultSkyBox;
            }

        }
    }

    private IEnumerator LightningStrikes()
    {
        RenderSettings.skybox = stormSkyBox;

        while (gameManager.isInUltMode())
        {

            float randomNumber = Random.RandomRange(1, lightningPos.Count);

            List<int> numbers = GenerateUniqueRandomNumbers(0, lightningPos.Count-1);

            for (int i = 0; i < randomNumber - 1; i++)
            {
                GameObject lightningInstance = Instantiate(mLightningPrefab, lightningPos[numbers[i]].position, mLightningPrefab.transform.rotation, null);
                Destroy(lightningInstance, lightningInstance.GetComponent<VisualEffect>().GetFloat("Lifetime"));
            }

            randomNumber = 1f;

            yield return new WaitForSeconds(randomNumber);
        }
    }

    List<int> GenerateUniqueRandomNumbers(int min, int max)
    {
        List<int> numbers = new List<int>();
        for (int i = min; i <= max; i++)
        {
            numbers.Add(i);
        }

        // Shuffle the list
        for (int i = 0; i < numbers.Count; i++)
        {
            int randomIndex = Random.Range(0, numbers.Count);
            int temp = numbers[i];
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }

        return numbers;
    }
}
