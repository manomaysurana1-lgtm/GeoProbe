using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup panelGroup;
    public Image loadingImage;
    public TMP_Text waitText;
    public TMP_Text factText;

    [Header("Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float imagePulseScale = 1.05f;
    public float imagePulseDuration = 1.5f;
    public float factChangeInterval = 6f;
    public float waitTextInterval = 0.5f; // time between dot updates

    private bool isAnimating = false;
    private Coroutine factRoutine;
    private Coroutine waitTextRoutine;
    private List<string> spaceFacts = new List<string>();

    void Start()
    {
        panelGroup.alpha = 0f;
        waitText.alpha = 0f;
        factText.alpha = 0f;
        loadingImage.transform.localScale = Vector3.one;
        LoadSpaceFacts();
    }

    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadingSequence(sceneName));
    }

    private IEnumerator LoadingSequence(string sceneName)
    {
        ShowLoadingScreen();

        yield return new WaitForSeconds(5f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        asyncLoad.allowSceneActivation = true;
        HideLoadingScreen();
        yield return new WaitForSeconds(fadeOutDuration);
    }

    public void ShowLoadingScreen()
    {
        panelGroup.blocksRaycasts = true;
        panelGroup.DOFade(1f, fadeInDuration);

        waitText.text = "Please wait";
        waitText.DOFade(1f, fadeInDuration);
        factText.DOFade(1f, fadeInDuration);

        // Animate loading image — rotate clockwise continuously
        loadingImage.transform.DORotate(new Vector3(0, 0, -360f), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // Pulse the image for a bit of life
        loadingImage.transform.DOScale(imagePulseScale, imagePulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        if (!isAnimating)
        {
            factRoutine = StartCoroutine(ChangeFactRoutine());
            waitTextRoutine = StartCoroutine(AnimateWaitText());
        }
    }

    public void HideLoadingScreen()
    {
        if (factRoutine != null) StopCoroutine(factRoutine);
        if (waitTextRoutine != null) StopCoroutine(waitTextRoutine);
        isAnimating = false;

        panelGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            panelGroup.blocksRaycasts = false;
            loadingImage.transform.DOKill();
        });
    }

    private IEnumerator AnimateWaitText()
    {
        isAnimating = true;
        string baseText = "Please wait";
        int dotCount = 0;

        while (isAnimating)
        {
            waitText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // cycles 0–3 dots
            yield return new WaitForSeconds(waitTextInterval);
        }
    }

    public void ChangeRandomFact()
    {
        if (spaceFacts.Count == 0) return;

        string newFact = spaceFacts[Random.Range(0, spaceFacts.Count)];

        factText.DOFade(0f, 0.4f).OnComplete(() =>
        {
            factText.text = newFact;
            factText.DOFade(1f, 0.4f);
        });
    }

    private IEnumerator ChangeFactRoutine()
    {
        isAnimating = true;
        ChangeRandomFact();

        while (isAnimating)
        {
            yield return new WaitForSeconds(factChangeInterval);
            ChangeRandomFact();
        }
    }

    private void LoadSpaceFacts()
    {
        // 100+ astrophysics and space facts
        spaceFacts.AddRange(new string[]
    {
        "Asia is the largest continent on Earth by both area and population.",
        "Africa is the second-largest continent by area.",
        "Antarctica is the coldest continent on Earth.",
        "Australia is the smallest continent by land area.",
        "The Pacific Ocean is the largest and deepest ocean on Earth.",
        "The Mariana Trench is the deepest known ocean trench.",
        "Mount Everest is the highest mountain above sea level.",
        "The Nile River is often considered the longest river in the world.",
        "The Amazon River carries more water than any other river.",
        "The Sahara Desert is the largest hot desert in the world.",
        "Antarctica is the largest desert on Earth because it receives very little precipitation.",
        "The Himalayas are the highest mountain range in the world.",
        "The Andes are the longest continental mountain range.",
        "The Amazon Rainforest is the largest tropical rainforest.",
        "Greenland is the largest island in the world that is not a continent.",
        "Russia is the largest country in the world by land area.",
        "Vatican City is the smallest country in the world by area.",
        "Canada has the longest coastline of any country.",
        "The equator divides Earth into the Northern and Southern Hemispheres.",
        "The Prime Meridian passes through Greenwich, England.",
        "Latitude lines run east to west.",
        "Longitude lines run north to south.",
        "The Tropic of Cancer lies north of the equator.",
        "The Tropic of Capricorn lies south of the equator.",
        "The Arctic Circle is near the North Pole.",
        "The Antarctic Circle surrounds Antarctica.",
        "Earth has seven continents.",
        "Earth has five major oceans.",
        "The Indian Ocean is the third-largest ocean.",
        "The Atlantic Ocean separates the Americas from Europe and Africa.",
        "The Southern Ocean surrounds Antarctica.",
        "The Great Barrier Reef is the largest coral reef system in the world.",
        "The Dead Sea is one of the saltiest bodies of water on Earth.",
        "The Caspian Sea is the largest inland body of water.",
        "Lake Superior is the largest freshwater lake by surface area.",
        "Lake Baikal is the deepest freshwater lake in the world.",
        "The Great Lakes contain a large share of Earth’s surface fresh water.",
        "The Gobi Desert lies in Asia.",
        "The Kalahari Desert is located in southern Africa.",
        "The Atacama Desert is one of the driest places on Earth.",
        "The Arabian Peninsula is the largest peninsula in the world.",
        "India is located on the Indian subcontinent.",
        "The Mediterranean Sea lies between Europe, Africa, and Asia.",
        "The Red Sea lies between Africa and the Arabian Peninsula.",
        "The Suez Canal connects the Mediterranean Sea and the Red Sea.",
        "The Panama Canal connects the Atlantic and Pacific Oceans.",
        "The Strait of Gibraltar separates Europe from Africa.",
        "The Bering Strait separates Asia from North America.",
        "The English Channel separates southern England from northern France.",
        "The Alps are a major mountain range in Europe.",
        "The Rocky Mountains stretch through western North America.",
        "The Appalachian Mountains are among the oldest mountains in North America.",
        "The Ural Mountains are often used as a boundary between Europe and Asia.",
        "The Andes run along the western edge of South America.",
        "The Ring of Fire is a zone of frequent earthquakes and volcanoes around the Pacific Ocean.",
        "Japan lies along the Pacific Ring of Fire.",
        "Indonesia has thousands of islands.",
        "The Philippines is an archipelago in Southeast Asia.",
        "New Zealand is located in the southwestern Pacific Ocean.",
        "Madagascar is the fourth-largest island in the world.",
        "Iceland sits on the Mid-Atlantic Ridge.",
        "The Mid-Atlantic Ridge is an underwater mountain range.",
        "Plate tectonics explains the movement of Earth’s crustal plates.",
        "Earthquakes often occur along tectonic plate boundaries.",
        "Volcanoes are often found near plate boundaries.",
        "A delta forms where a river deposits sediment near its mouth.",
        "A canyon is formed by erosion over long periods of time.",
        "The Grand Canyon was carved mainly by the Colorado River.",
        "A fjord is a deep, narrow inlet carved by glaciers.",
        "Glaciers store most of Earth’s fresh water.",
        "A peninsula is land surrounded by water on three sides.",
        "An isthmus is a narrow strip of land connecting two larger land areas.",
        "An archipelago is a group or chain of islands.",
        "A plateau is a large, flat area of land at high elevation.",
        "A plain is a broad area of relatively flat land.",
        "A valley is a low area between hills or mountains.",
        "A basin is a low area where water and sediment can collect.",
        "A watershed is an area of land that drains into a common water body.",
        "A river’s source is where it begins.",
        "A river’s mouth is where it empties into another body of water.",
        "The Amazon Basin is the largest drainage basin in the world.",
        "The Congo Basin contains one of the world’s largest rainforests.",
        "The Great Rift Valley runs through eastern Africa.",
        "The Serengeti is a famous grassland region in East Africa.",
        "The Pampas are fertile grasslands in South America.",
        "The Eurasian Steppe stretches across parts of Europe and Asia.",
        "The tundra is a cold biome with low-growing vegetation.",
        "Taiga forests are dominated by coniferous trees.",
        "Tropical rainforests receive heavy rainfall throughout the year.",
        "Savannas are grasslands with scattered trees.",
        "Monsoons are seasonal winds that bring heavy rains to some regions.",
        "The equator receives direct sunlight throughout the year.",
        "Places near the equator usually have warmer climates.",
        "Altitude affects climate because higher places are usually cooler.",
        "Coastal areas often have milder climates than inland areas.",
        "Ocean currents can influence the climate of nearby land.",
        "The Gulf Stream carries warm water across the North Atlantic.",
        "The International Date Line is mostly located in the Pacific Ocean.",
        "Time zones are based roughly on lines of longitude.",
        "Maps use scale to show real-world distances.",
        "A compass rose shows directions on a map."
    });
    }
}
