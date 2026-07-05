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
        Persisting.Instance.PlaySFX(SFX.ButtonClick);
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
        "Rajasthan is the largest state in India by area, while Uttar Pradesh is the largest by population.",
        "Madhya Pradesh is the second-largest state in India by area.",
        "Drass in Ladakh is considered one of the coldest inhabited places in India.",
        "Goa is the smallest state in India by land area.",
        "The Arabian Sea and the Bay of Bengal are the major oceanic bodies flanking the Indian peninsula.",
        "The Sundarbans Trench (Swatch of No Ground) is a deep canyon in the Bay of Bengal.",
        "Kangchenjunga is the highest mountain peak located fully or partially within India.",
        "The Ganga is the longest river flowing through India.",
        "The Brahmaputra River carries the highest volume of water of any river in India.",
        "The Thar Desert is the largest hot desert in India.",
        "Ladakh is a high-altitude cold desert that receives very little precipitation.",
        "The Himalayas are the highest mountain range in northern India.",
        "The Western Ghats are one of the longest continuous mountain ranges in India.",
        "The Western Ghats and the Northeast rain forests are home to India's major tropical rainforests.",
        "Majuli is the largest river island in India (and the world).",
        "Rajasthan is the largest state in India by land area.",
        "Goa is the smallest state in India by land area.",
        "Gujarat has the longest coastline of any Indian state.",
        "The Tropic of Cancer divides India into nearly equal northern and southern halves.",
        "The Indian Standard Meridian (82°30' E) passes through Mirzapur, Uttar Pradesh.",
        "Latitude lines determine the climatic zones of India from south to north.",
        "Longitude lines determine the time difference between western Gujarat and eastern Arunachal Pradesh.",
        "The Tropic of Cancer passes through eight states in India.",
        "The southern tip of mainland India, Indira Point, lies closest to the equator.",
        "The northernmost point of India is Indira Col near the Siachen Glacier.",
        "The southern tip of mainland India is Kanyakumari.",
        "India has 28 states and 8 union territories.",
        "India is bounded by three major water bodies: the Arabian Sea, the Bay of Bengal, and the Indian Ocean.",
        "The Indian Ocean washes the southern shores of the Indian peninsula.",
        "The Arabian Sea separates India from the Arabian Peninsula.",
        "The Lakshadweep Sea surrounds the Lakshadweep archipelago.",
        "The Netrani Island reef and Gulf of Mannar contain major coral reef systems in India.",
        "Lake Sambhar in Rajasthan is one of the saltiest inland bodies of water in India.",
        "Chilika Lake is the largest brackish water coastal lagoon in India.",
        "Wular Lake in Jammu and Kashmir is one of the largest freshwater lakes in India.",
        "Gobind Sagar and Shivaji Sagar are among the deepest man-made reservoirs in India.",
        "The Great Lakes of the Himalayas, like Pangong Tso and Tso Moriri, hold a vast amount of high-altitude water.",
        "The Cold Desert of Spiti Valley lies in Himachal Pradesh.",
        "The Rann of Kutch is a massive salt desert located in western India.",
        "The Thar Desert is one of the most densely populated desert regions in the world.",
        "The Indian Peninsula is the prominent peninsula shaping the geography of South Asia.",
        "India is the core landmass located on the Indian subcontinent.",
        "The Laccadive Sea lies between the Lakshadweep islands and the western coast of India.",
        "The Palk Strait lies between India and Sri Lanka.",
        "The National Waterway 1 connects the Ganga and Hooghly rivers for inland navigation.",
        "The Buckingham Canal connects Andhra Pradesh to Tamil Nadu.",
        "The Palk Strait separates India from Sri Lanka.",
        "The Ten Degree Channel separates the Andaman Islands from the Nicobar Islands.",
        "The Pamban Channel separates mainland India from Rameswaram Island.",
        "The Aravalli Range is one of the oldest mountain ranges in India.",
        "The Western Ghats stretch along the western coast of India.",
        "The Vindhya and Satpura ranges are ancient block mountains in central India.",
        "The Purvanchal Range forms the eastern boundary between India and Myanmar.",
        "The Eastern Ghats run discontinuously along India's eastern coast.",
        "The Himalayan Seismic Zone is an active earthquake belt running along northern India.",
        "Barren Island in the Andaman Sea is India's only active volcano.",
        "The Lakshadweep islands consist of thousands of coral islets and reefs.",
        "The Andaman and Nicobar Islands form a major archipelago in the Bay of Bengal.",
        "Diarmid and Netrani are tiny islands off the southwestern coast of India.",
        "Sri Lanka is the large island nation located just south of India.",
        "Barren Island sits on a volcanic arc in the Andaman Sea.",
        "The Ninety East Ridge is a massive underwater mountain range in the Indian Ocean.",
        "The collision of the Indian Plate with the Eurasian Plate formed the Himalayas.",
        "Earthquakes often occur along the Himalayan fault lines.",
        "Geothermal springs are often found near tectonic fault lines in Himachal Pradesh and Ladakh.",
        "The Sundarbans Delta forms where the Ganga and Brahmaputra deposit sediment into the Bay of Bengal.",
        "The Gandikota Canyon in Andhra Pradesh was formed by erosion over long periods of time.",
        "The Gandikota Gorge was carved mainly by the Pennar River.",
        "A glacier-carved valley can be found in the high regions of Kashmir and Sikkim.",
        "Himalayan glaciers store most of India’s fresh river water.",
        "The Indian Peninsula is a vast landmass surrounded by water on three sides.",
        "The Isthmus of Pamban connects mainland India to Pamban Island.",
        "The Lakshadweep Islands are a prominent coral archipelago of India.",
        "The Deccan Plateau is a large, flat area of land at a high elevation in southern India.",
        "The Indo-Gangetic Plain is a broad area of fertile, flat land in northern India.",
        "The Kashmir Valley is a famous low area nestled between the Pir Panjal and Himalayan mountain ranges.",
        "The Chilika Basin is a coastal basin where water and sediment collect in Odisha.",
        "The Ganga Basin is the largest watershed draining land in India.",
        "The source of the Ganga River is at Gaumukh (Gangotri Glacier).",
        "The mouth of the Ganga River empties into the Bay of Bengal.",
        "The Indo-Gangetic Basin is the largest drainage basin in northern India.",
        "The Godavari Basin contains some of India's largest riverine forest covers.",
        "The Narmada Rift Valley runs between the Vindhya and Satpura ranges in central India.",
        "The Terai is a famous marshy grassland region in northern India and Nepal.",
        "The Shola grasslands are fertile, high-altitude grasslands found in the Western Ghats.",
        "The Bugyals are alpine pasturelands that stretch across the high altitudes of Uttarakhand.",
        "The Himalayan alpine tundra is a cold biome with low-growing vegetation in northern India.",
        "The Himalayan subtropical pine forests are dominated by coniferous trees.",
        "The Western Ghats receive heavy rainfall throughout the southwest monsoon season.",
        "The Terai-Duar savannas are grasslands with scattered trees in northern India.",
        "Monsoons are seasonal winds that bring heavy rains to the Indian subcontinent.",
        "Southern India receives more direct sunlight throughout the year than northern India.",
        "Places in southern India, like Kerala, usually have warmer, tropical climates year-round.",
        "Altitude affects climate in India because hill stations like Shimla and Ooty are much cooler than the plains.",
        "Coastal Mumbai has a much milder climate than inland Delhi.",
        "The Indian Ocean Dipole and ocean currents influence the monsoon climate of India.",
        "The West India Coastal Current carries water along the western coast of the peninsula.",
        "Indian Standard Time (IST) is calculated from the meridian passing through Prayagraj/Mirzapur.",
        "Time zones in India are kept uniform nationwide under a single standard time (IST).",
        "The Survey of India uses precise scales to create official national topographical maps.",
        "An Ashoka Chakra or standard directional pointer shows orientation on Indian maps."
    });
    }
}
