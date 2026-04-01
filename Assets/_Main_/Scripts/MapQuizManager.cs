using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapQuizManager : MonoBehaviour
{
    [Header("UI")]
    public Image mapImage;
    public RectTransform playerMarker;
    public RectTransform correctMarker;
    public TMP_Text questionText;
    public TMP_Text scoreText;
    public TMP_Text resultText;

    [Header("Data")]
    public List<MapLocation> locations;

    private MapLocation currentLocation;
    private List<MapLocation> remainingLocations;
    private int score = 0;

    // Clean India bounds (tuned)
    const float MIN_LAT = 8.0f;
    const float MAX_LAT = 34.5f;
    const float MIN_LON = 72.0f;
    const float MAX_LON = 91.5f;

    void Start()
    {
        // Prevent repeats
        remainingLocations = new List<MapLocation>(locations);

        // Auto-fix UI setup
        mapImage.preserveAspect = true;

        playerMarker.SetParent(mapImage.rectTransform);
        correctMarker.SetParent(mapImage.rectTransform);

        playerMarker.anchorMin = playerMarker.anchorMax = new Vector2(0.5f, 0.5f);
        correctMarker.anchorMin = correctMarker.anchorMax = new Vector2(0.5f, 0.5f);

        playerMarker.pivot = correctMarker.pivot = new Vector2(0.5f, 0.5f);

        playerMarker.gameObject.SetActive(false);
        correctMarker.gameObject.SetActive(false);

        scoreText.text = "Score: 0";

        LoadNext();
    }

    void LoadNext()
    {
        if (remainingLocations.Count == 0)
        {
            questionText.text = "Quiz complete! Final score: " + score;
            return;
        }

        int index = Random.Range(0, remainingLocations.Count);
        currentLocation = remainingLocations[index];
        remainingLocations.RemoveAt(index);

        questionText.text = "Locate this " + currentLocation.type + ":\n" + currentLocation.locationName;
        resultText.text = "";

        playerMarker.gameObject.SetActive(false);
        correctMarker.gameObject.SetActive(false);
    }

    public void OnMapClick()
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapImage.rectTransform,
            Input.mousePosition,
            null, // IMPORTANT: Overlay canvas fix
            out localPoint
        );

        // Show player guess
        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = localPoint;

        Rect rect = mapImage.rectTransform.rect;

        float xPercent = (localPoint.x - rect.xMin) / rect.width;
        float yPercent = (localPoint.y - rect.yMin) / rect.height;

        // Convert to lat/lon
        float guessedLon = Mathf.Lerp(MIN_LON, MAX_LON, xPercent);
        float guessedLat = Mathf.Lerp(MIN_LAT, MAX_LAT, yPercent);

        float correctLat = currentLocation.latitude;
        float correctLon = currentLocation.longitude;

        float distance = HaversineKm(guessedLat, guessedLon, correctLat, correctLon);

        int points = Mathf.Max(0, 1000 - (int)distance);
        score += points;

        scoreText.text = "Score: " + score;

        // Show correct location
        correctMarker.gameObject.SetActive(true);
        correctMarker.anchoredPosition = LatLonToMapPos(correctLat, correctLon);

        resultText.text = Mathf.RoundToInt(distance) + " km away! (+" + points + " pts)";

        Invoke("LoadNext", 2.5f);
    }

    Vector2 LatLonToMapPos(float lat, float lon)
    {
        Rect rect = mapImage.rectTransform.rect;

        float xPercent = Mathf.InverseLerp(MIN_LON, MAX_LON, lon);
        float yPercent = Mathf.InverseLerp(MIN_LAT, MAX_LAT, lat);

        float x = rect.xMin + xPercent * rect.width;
        float y = rect.yMin + yPercent * rect.height;

        return new Vector2(x, y);
    }

    float HaversineKm(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371f;

        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);

        float a =
            Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
            Mathf.Cos(Mathf.Deg2Rad * lat1) *
            Mathf.Cos(Mathf.Deg2Rad * lat2) *
            Mathf.Sin(dLon / 2) *
            Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c;
    }
}