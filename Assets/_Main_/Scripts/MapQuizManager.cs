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
    private int score = 0;

    void Start()
    {
        playerMarker.gameObject.SetActive(false);
        correctMarker.gameObject.SetActive(false);

        scoreText.text = "Score: 0";

        LoadNext();
    }

    void LoadNext()
    {
        playerMarker.gameObject.SetActive(false);
        correctMarker.gameObject.SetActive(false);

        currentLocation = locations[Random.Range(0, locations.Count)];

        questionText.text = "Locate this " + currentLocation.type + ":\n" + currentLocation.locationName;
        resultText.text = "";
    }

    public void OnMapClick()
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapImage.rectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );

        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = localPoint;

        float xPercent = (localPoint.x + mapImage.rectTransform.rect.width / 2) / mapImage.rectTransform.rect.width;
        float yPercent = (localPoint.y + mapImage.rectTransform.rect.height / 2) / mapImage.rectTransform.rect.height;

        float guessedLat = Mathf.Lerp(34.5f, 9.0f, yPercent);
        float guessedLon = Mathf.Lerp(70.0f, 94.5f, xPercent);

        float correctLat = currentLocation.latitude;
        float correctLon = currentLocation.longitude;

        float distance = CalculateDistance(guessedLat, guessedLon, correctLat, correctLon);

        int points = Mathf.Max(0, 1000 - (int)distance);
        score += points;

        scoreText.text = "Score: " + score;

        Vector2 correctPos = LatLonToMapPosition(correctLat, correctLon);
        correctMarker.gameObject.SetActive(true);
        correctMarker.anchoredPosition = correctPos;

        resultText.text = "You were " + Mathf.RoundToInt(distance) + " km away";

        Invoke("LoadNext", 2f);
    }

    Vector2 LatLonToMapPosition(float lat, float lon)
    {
        float xPercent = Mathf.InverseLerp(68f, 97f, lon);
        float yPercent = Mathf.InverseLerp(37f, 8f, lat);

        float x = (xPercent * mapImage.rectTransform.rect.width) - mapImage.rectTransform.rect.width / 2;
        float y = (yPercent * mapImage.rectTransform.rect.height) - mapImage.rectTransform.rect.height / 2;

        return new Vector2(x, y);
    }

    float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
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