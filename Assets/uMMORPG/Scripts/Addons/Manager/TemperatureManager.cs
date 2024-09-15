using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.Universal;
using System;

// Pool di oggetti
public class ObjectPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private int poolSize;

    public ObjectPool(GameObject prefab, int size)
    {
        this.prefab = prefab;
        this.poolSize = size;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public void ReturnPooledObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public void DeactivateAll()
    {
        foreach (GameObject obj in pool)
        {
            if (obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }
    }
}

public class TemperatureManager : NetworkBehaviour
{

    public static TemperatureManager singleton;

    [SyncVar]
    public string season = "Winter";
    public string nextSeason;
    public bool isComing;
    [SyncVar]
    public float actualSafeCover;

    [SyncVar(hook = (nameof(ChangeTimeUI)))]
    public string time;
    [SyncVar(hook = (nameof(ChangeLightColor)))]
    public string colorSync;
    public Light2D light2D;

    //[SyncVar]
    //public string ambientColorSync;


    [SyncVar(hook = (nameof(CheckRainParticle)))]
    public bool isRainy;
    [SyncVar]
    public bool isWindy;
    [SyncVar(hook = (nameof(CheckSunnyParticle)))]
    public bool isSunny;
    [SyncVar(hook = (nameof(CheckSnowParticle)))]
    public bool isSnowy;

    [Space(5)]

    public int seconds = 00;
    public int minutes = 00;
    public int hours = 00;
    public int days = 0;

    [Space(5)]

    public int WinterDayDuration;
    public int SpringDayDuration;
    public int SummerDuration;
    public int AutumnDayDuration;

    [Space(5)]

    public int winterSunset;
    public int winterStayAtNight;
    public int winterLightestPoint;
    public int winterSunrise;

    [Space(5)]

    public int springSunset;
    public int springStayAtNight;
    public int springLightestPoint;
    public int springSunrise;

    [Space(5)]

    public int summerSunset;
    public int summerStayAtNight;
    public int summerLightestPoint;
    public int summerSunrise;

    [Space(5)]

    public int autumunSunset;
    public int autumunStayAtNight;
    public int autumunLightestPoint;
    public int autumunSunrise;

    [Space(5)]

    public float requiredCoverWinter = 1.0f;
    public float requiredCoverSpring = 0.5f;
    public float requiredCoverSummer = 0.2f;
    public float requiredCoverAutumn = 0.6f;

    [Space(5)]
    public string winter = "Winter";
    public string spring = "Spring";
    public string summer = "Summer";
    public string autumn = "Autumn";

    [Space(5)]

    public float winterProbabilityOfRainStart = 1.0f;
    public float winterProbabilityOfRainEnd = 1.0f;
    public float winterProbabilityOfSunStart = 0.5f;
    public float winterProbabilityOfSunEnd = 0.5f;
    public float winterProbabilityOfSnowStart = 0.2f;
    public float winterProbabilityOfSnowEnd = 0.5f;
    public float winterProbabilityWindy = 0.1f;

    [Space(5)]

    public float springProbabilityOfRainStart = 1.0f;
    public float springProbabilityOfRainEnd = 1.0f;
    public float springProbabilityOfSunStart = 0.5f;
    public float springProbabilityOfSunEnd = 0.5f;
    public float springProbabilityOfSnowStart = 0.2f;
    public float springProbabilityOfSnowEnd = 0.5f;
    public float springProbabilityWindy = 0.1f;

    [Space(5)]

    public float summerProbabilityOfRainStart = 1.0f;
    public float summerProbabilityOfRainEnd = 1.0f;
    public float summerProbabilityOfSunStart = 0.5f;
    public float summerProbabilityOfSunEnd = 0.5f;
    public float summerProbabilityOfSnowStart = 0.2f;
    public float summerProbabilityOfSnowEnd = 0.5f;
    public float summerProbabilityWindy = 0.1f;

    [Space(5)]

    public float autumnProbabilityOfRainStart = 1.0f;
    public float autumnProbabilityOfRainEnd = 1.0f;
    public float autumnProbabilityOfSunStart = 0.5f;
    public float autumnProbabilityOfSunEnd = 0.5f;
    public float autumnProbabilityOfSnowStart = 0.2f;
    public float autumnProbabilityOfSnowEnd = 0.5f;
    public float autumnProbabilityWindy = 0.1f;

    private string secondTime;
    private string minuteTime;
    private string hourTime;

    public Color sunsetSunriseColor;

    public Color halfDayColor;

    public Color MidnightColor;

    public Color desiredColor;

    private bool prevRainy;
    private bool prevWindy;
    private bool prevSunny;
    private bool prevSnowy;

    private int _Sunset;
    private int _StayAtNight;
    private int _LightestPoint;
    private int _Sunrise;

    public List<double> changeTime = new List<double>();

    public RainController rainObject;
    public SnowController snowObject;

    private Color color;

    public float timeBetweenWeatherChange = 360.0f;
    [SyncVar(hook = (nameof(ChangeMusic)))]
    public bool nightMusic;

    public List<Aquifer> actualAcquifer = new List<Aquifer>();

    public float basicSmooth = 45;
    private float desiredSmooth = 45;

    [SyncVar(hook = (nameof(CheckSnow)))] public float snowPercent;
    public Material snowMaterial;
    public Material snowGroundMaterial;

    public AudioSource audioSource;
    public AudioClip nightClip;
    public AudioClip dayClip;

    [SyncVar(hook = (nameof(RainClouds)))] public float tempLimitRain;
    [SyncVar(hook = (nameof(SnowClouds)))] public float tempLimitSnow;
    [SyncVar(hook = (nameof(SunClouds)))] public float tempLimitSunny;

    public float cloudThreshold = 0.05f;
    public Material cloudShadowMaterial;

    public Color colorToActivateLamp;

    public float transp;
    private ObjectPool rainEffectPool;
    public GameObject rainSlplashObject;

    public LayerMask layerMask;

    void Awake()
    {
        if (!singleton) singleton = this;
        rainEffectPool = new ObjectPool(rainSlplashObject.gameObject, 200);
        ChangeMusic(nightMusic, nightMusic);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        //ManagerAssign.singleton.Assign(this.gameObject);
        CheckRainParticle(isRainy, isRainy);
        CheckSnowParticle(isSnowy, isSnowy);
        SunClouds(tempLimitSunny, tempLimitSunny);
        SnowClouds(tempLimitSnow, tempLimitSnow);
        RainClouds(tempLimitRain, tempLimitRain);
        cloudShadowMaterial.SetFloat("_TransparencyThreshold", tempLimitSunny);
    }

    public void CheckSnow(float oldValue, float newValue)
    {
        snowMaterial.SetFloat("_FullAlphaDissolveFade", newValue);
        snowGroundMaterial.SetFloat("_FullAlphaDissolveFade", newValue);
    }

    public void ChangeMusic(bool oldValue, bool newValue)
    {
        if(!audioSource) audioSource = FindObjectOfType<SoundManager>().GetComponent<AudioSource>();
        audioSource.clip = newValue ? nightClip : dayClip;
        audioSource.Play();
    }

    public void ChangeVolume(bool newValue)
    {
        if (!audioSource) audioSource = FindObjectOfType<SoundManager>().GetComponent<AudioSource>();
        audioSource.volume = Player.localPlayer.playerOptions.blockSound ? 0f : 0.2f;
    }

    public void ChangeTimeUI(string oldValue, string newValue)
    {
        if (UITime.singleton) UITime.singleton.Set(newValue);
    }

    public void CheckRainParticle(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (Player.localPlayer)
            {
                snowObject.snowIntensity = 0f;
                rainObject.rainIntensity = Mathf.Lerp(0.0f, 1.0f, 3.0f);
                Invoke(nameof(ManageRainParticle), 0.01f);
            }
        }
        else
        {
            // Disattiva tutti gli oggetti attivi nel pool
            rainEffectPool.DeactivateAll();
            CancelInvoke(nameof(ManageRainParticle));
        }
    }

    public void ManageRainParticle()
    {
        GameObject rainEffect = rainEffectPool.GetPooledObject();
        if (rainEffect != null)
        {
            rainEffect.transform.position = GetValidSpawnPoint(); // Imposta la posizione al punto generato casualmente
            rainEffect.SetActive(true);

            // Se l'oggetto ha un'animazione, riproducila qui
            Animator animator = rainEffect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Splash"); // Assicurati di avere l'animazione corretta impostata
            }

            StartCoroutine(DeactivateAfterTime(rainEffect, 1.0f)); // Disattiva dopo 2 secondi
            Invoke(nameof(ManageRainParticle), 0.01f);
        }

    }

    //private Vector2 GetRandomScreenPosition()
    //{
    //    // Genera una posizione casuale sullo schermo
    //    Vector2 randomPoint = Camera.main.ScreenToWorldPoint(new Vector2(UnityEngine.Random.Range(0, Screen.width), UnityEngine.Random.Range(0, Screen.height)), layerMask);
    //    return randomPoint;
    //}

    Vector2 GetValidSpawnPoint()
    {
        int maxAttempts = 100; // Numero massimo di tentativi
        for (int i = 0; i < maxAttempts; i++)
        {
            // Step 1: Genera un punto casuale sullo schermo
            Vector2 screenPoint = new Vector2(UnityEngine.Random.Range(0, Screen.width), UnityEngine.Random.Range(0, Screen.height));

            // Step 2: Converte il punto dello schermo in coordinate del mondo
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);

            // Step 3: Controlla se il punto è libero da oggetti con i layer specificati
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint, layerMask);

            if (hitCollider == null)
            {
                // Se non ci sono oggetti nel punto, il punto è valido
                return worldPoint;
            }
        }

        // Se non è stato trovato un punto valido dopo maxAttempts, ritorna Vector2.zero
        return Vector2.zero;
    }

    IEnumerator DeactivateAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        rainEffectPool.ReturnPooledObject(obj);
    }

    public void SunClouds(float oldValue, float newValue)
    {
        if (tempLimitSunny == 0) return;
        transp = cloudShadowMaterial.GetFloat("_TransparencyThreshold");

        if(transp > tempLimitSunny + cloudThreshold)
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp - 0.01f);
        else if(transp < tempLimitSunny + (-cloudThreshold))
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp + 0.01f);

        Invoke(nameof(CallSunClouds), 0.2f);
    }

    public void CallSunClouds()
    {
        SunClouds(tempLimitSunny, tempLimitSunny);
    }

    public void RainClouds(float oldValue, float newValue)
    {
        if (tempLimitRain == 0) return;
        transp = cloudShadowMaterial.GetFloat("_TransparencyThreshold");

        if (transp > tempLimitRain + cloudThreshold)
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp - 0.01f);
        else if (transp < tempLimitRain + (-cloudThreshold))
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp + 0.01f);

        Invoke(nameof(CallRainClouds), 0.2f);
    }

    public void CallRainClouds()
    {
        RainClouds(tempLimitRain, tempLimitRain);
    }

    public void SnowClouds(float oldValue, float newValue)
    {
        if (tempLimitSnow == 0) return;
        transp = cloudShadowMaterial.GetFloat("_TransparencyThreshold");

        if (transp > tempLimitSnow + cloudThreshold)
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp - 0.01f);
        else if (transp < tempLimitSnow + (-cloudThreshold))
            cloudShadowMaterial.SetFloat("_TransparencyThreshold", transp + 0.01f);

        Invoke(nameof(CallSnowClouds), 0.2f);
    }

    public void CallSnowClouds()
    {
        SnowClouds(tempLimitSnow, tempLimitSnow);
    }

    public void CheckSunnyParticle(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (Player.localPlayer)
            {
                snowObject.snowIntensity = 0f;
                rainObject.rainIntensity = 0f;
            }
        }
    }

    public void CheckSnowParticle(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (Player.localPlayer)
            {
                rainObject.rainIntensity = 0f;
                snowObject.snowIntensity = Mathf.Lerp(0.0f, 1.0f, 3.0f);
            }
        }
    }

    public bool CheckZombieAggresivity()
    {
        switch (season)
        {
            case "Winter":
                return hours >= winterSunrise && hours <= winterSunset;
            case "Summer":
                return hours >= summerSunrise && hours <= summerSunset;
            case "Spring":
                return hours >= springSunrise && hours <= springSunset;
            case "Autumn":
                return hours >= autumunSunrise && hours <= autumunSunset;
        }

        return false;
    }

    public void SetParameters()
    {
        switch (season)
        {
            case "Winter":
                _Sunset = winterSunset;
                _StayAtNight = winterStayAtNight;
                _LightestPoint = winterLightestPoint;
                _Sunrise = winterSunrise;
                break;
            case "Summer":
                _Sunset = summerSunset;
                _StayAtNight = summerStayAtNight;
                _LightestPoint = summerLightestPoint;
                _Sunrise = summerSunrise;
                break;
            case "Spring":
                _Sunset = springSunset;
                _StayAtNight = springStayAtNight;
                _LightestPoint = springLightestPoint;
                _Sunrise = springSunrise;
                break;
            case "Autumn":
                _Sunset = autumunSunset;
                _StayAtNight = autumunStayAtNight;
                _LightestPoint = autumunLightestPoint;
                _Sunrise = autumunSunrise;
                break;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //ManagerAssign.singleton.Assign(this.gameObject);
        SetParameters();
        InvokeRepeating(nameof(TimeManager), 0.01f, 0.01f);
        InvokeRepeating(nameof(ChangeWeatherConditions), 0.0f, timeBetweenWeatherChange);
        InvokeRepeating(nameof(ChangeWindConditions), 0.0f, 30.0f);
        InvokeRepeating(nameof(ChargeAquifer), 0.0f, 30.0f);
        InvokeRepeating(nameof(ChargeWaterContainer), 0.0f, 20.0f);
        prevRainy = isRainy;
        prevWindy = isWindy;
        prevSunny = isSunny;
        prevSnowy = isSnowy;
    }

    public void ChangeLightColor(string oldColor, string newColor)
    {
        if (Player.localPlayer)
        {
            if (ColorUtility.TryParseHtmlString("#" + newColor, out color))
                light2D.color = color;
        }
    }

    private void UpdateColor(Color targetColor)
    {
        if (targetColor.r > desiredColor.r)
        {
            desiredColor.r += Time.deltaTime / desiredSmooth;
        }
        else if (targetColor.r < desiredColor.r)
        {
            desiredColor.r -= Time.deltaTime / desiredSmooth;
        }

        if (targetColor.g > desiredColor.g)
        {
            desiredColor.g += Time.deltaTime / desiredSmooth;
        }
        else if (targetColor.g < desiredColor.g)
        {
            desiredColor.g -= Time.deltaTime / desiredSmooth;
        }

        if (targetColor.b > desiredColor.b)
        {
            desiredColor.b += Time.deltaTime / desiredSmooth;
        }
        else if (targetColor.b < desiredColor.b)
        {
            desiredColor.b -= Time.deltaTime / desiredSmooth;
        }
    }

    float CalculateLuminance(Color color)
    {
        return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
    }

    // Function to compare two colors and check if color1 is darker than color2
    bool IsColorDarker(Color color1, Color color2)
    {
        float luminance1 = CalculateLuminance(color1);
        float luminance2 = CalculateLuminance(color2);

        return luminance1 < luminance2;
    }



    void Update()
    {

        if (minutes < 10)
            minuteTime = "0" + minutes.ToString();
        else
            minuteTime = minutes.ToString();

        if (hours < 10)
            hourTime = "0" + hours.ToString();
        else
            hourTime = hours.ToString();

        ManageColor();

        colorSync = ColorUtility.ToHtmlStringRGBA(desiredColor);

        if (isServer)
        {
            for (int i = 0; i < ModularBuildingManager.singleton.lamps.Count; i++)
            {
                if(ModularBuildingManager.singleton.lamps[i].isServer || ModularBuildingManager.singleton.lamps[i].isClient)
                    ModularBuildingManager.singleton.lamps[i].isActive = IsColorDarker(desiredColor, colorToActivateLamp);
            }
        }


        if (isSnowy)
        {
            if (snowPercent < 0.8)
            {
                snowPercent += Convert.ToSingle(0.005 * Time.deltaTime);
            }
        }
        else
        {
            if (snowPercent > 0)
            {
                snowPercent -= Convert.ToSingle(0.005 * Time.deltaTime);
            }
        }

        if (isComing)
        {
            time = hourTime + " : " + minuteTime + "  " + nextSeason + " is coming...";
        }
        else
        {
            time = hourTime + " : " + minuteTime + "  " + season;
        }
    }

    public void ManageColor()
    {
        if (isServer || isServerOnly)
        {
            // Notte fino a che non inizia a far luce
            if (hours >= 0 && hours < _StayAtNight)
            {
                desiredSmooth = basicSmooth * _StayAtNight;
                UpdateColor(MidnightColor);
                nightMusic = true;
            }
            // Inizio alba
            else if (hours >= _StayAtNight && hours < _Sunrise)
            {
                desiredSmooth = basicSmooth * (_Sunrise - _StayAtNight);
                UpdateColor(sunsetSunriseColor);
                nightMusic = true;
            }
            // Alba fino a mezzogiorno
            else if (hours >= _Sunrise && hours < _LightestPoint)
            {
                desiredSmooth = basicSmooth * (_LightestPoint - _Sunrise);
                UpdateColor(halfDayColor);
                nightMusic = false;
            }
            // Effetto tramonto
            else if (hours >= _Sunset && hours < _Sunset + 1)
            {
                desiredSmooth = basicSmooth;
                UpdateColor(sunsetSunriseColor);
                nightMusic = false;
            }
            // Effetto tramonto
            else if (hours >= _Sunset + 1 && hours < _Sunset + 2)
            {
                desiredSmooth = basicSmooth * ((_Sunset + 2) - (_Sunset + 1));
                UpdateColor(MidnightColor);
                nightMusic = true;
            }
            // Tramonto fino a sera dello stesso giorno
            else if (hours >= _Sunset + 2 && hours <= 23)
            {
                desiredSmooth = basicSmooth * (23 - (_Sunset + 2));
                UpdateColor(MidnightColor);
                nightMusic = true;
            }
        }
    }

    public void TimeManager()
    {
        if (seconds < 60)
        {
            seconds++;
        }
        else
        {
            seconds = 00;
            if (minutes < 59)
            {
                minutes++;
            }
            else
            {
                if (hours < 23)
                {
                    hours++;
                    SetParameters();
                }
                else
                {
                    if (days >= (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
                    {
                        days = 0;
                    }
                    else
                    {
                        days++;
                    }
                    seconds = 0;
                    minutes = 0;
                    hours = 0;
                }
                minutes = 00;
            }
        }

        if (days <= WinterDayDuration)
        {
            season = winter;
            actualSafeCover = requiredCoverWinter;
            nextSeason = spring;

            if (days == WinterDayDuration)
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration))
        {
            season = spring;
            actualSafeCover = requiredCoverSpring;
            nextSeason = summer;

            if (days == (WinterDayDuration + SpringDayDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration + SummerDuration))
        {
            season = summer;
            actualSafeCover = requiredCoverSummer;
            nextSeason = autumn;

            if (days == (WinterDayDuration + SpringDayDuration + SummerDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }
        if (days <= (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
        {
            season = autumn;
            actualSafeCover = requiredCoverAutumn;
            nextSeason = winter;

            if (days == (WinterDayDuration + SpringDayDuration + SummerDuration + AutumnDayDuration))
            {
                isComing = true;
            }
            else
            {
                isComing = false;
            }

            return;
        }


    }

    public void ChangeWeatherConditions()
    {
        float nextweather = UnityEngine.Random.Range(0.0f, 1.0f);

        //Debug.Log("Weather change : " + nextweather);

        bool tempRainy = false;
        bool tempSunny = false;
        bool tempSnowy = false;

        if (season == winter)
        {
            if (nextweather >= winterProbabilityOfRainStart && nextweather <= winterProbabilityOfRainEnd)
            {
                tempRainy = true;
                tempSunny = false;
                tempSnowy = false;
            }
            if (nextweather >= winterProbabilityOfSunStart && nextweather <= winterProbabilityOfSunEnd)
            {
                tempRainy = false;
                tempSunny = true;
                tempSnowy = false;
            }
            if (nextweather >= winterProbabilityOfSnowStart && nextweather <= winterProbabilityOfSnowEnd)
            {
                tempRainy = false;
                tempSunny = false;
                tempSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == spring)
        {
            if (nextweather >= springProbabilityOfRainStart && nextweather <= springProbabilityOfRainEnd)
            {
                tempRainy = true;
                tempSunny = false;
                tempSnowy = false;
            }
            if (nextweather >= springProbabilityOfSunStart && nextweather <= springProbabilityOfSunEnd)
            {
                tempRainy = false;
                tempSunny = true;
                tempSnowy = false;
            }
            if (nextweather >= springProbabilityOfSnowStart && nextweather <= springProbabilityOfSnowEnd)
            {
                tempRainy = false;
                tempSunny = false;
                tempSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == summer)
        {
            if (nextweather >= summerProbabilityOfRainStart && nextweather <= summerProbabilityOfRainEnd)
            {
                tempRainy = true;
                tempSunny = false;
                tempSnowy = false;
            }
            if (nextweather >= summerProbabilityOfSunStart && nextweather <= summerProbabilityOfSunEnd)
            {
                tempRainy = false;
                tempSunny = true;
                tempSnowy = false;
            }
            if (nextweather >= summerProbabilityOfSnowStart && nextweather <= summerProbabilityOfSnowEnd)
            {
                tempRainy = false;
                tempSunny = false;
                tempSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (season == autumn)
        {
            if (nextweather >= autumnProbabilityOfRainStart && nextweather <= autumnProbabilityOfRainEnd)
            {
                tempRainy = true;
                tempSunny = false;
                tempSnowy = false;
            }
            if (nextweather >= autumnProbabilityOfSunStart && nextweather <= autumnProbabilityOfSunEnd)
            {
                tempRainy = false;
                tempSunny = true;
                tempSnowy = false;
            }
            if (nextweather >= autumnProbabilityOfSnowStart && nextweather <= autumnProbabilityOfSnowEnd)
            {
                tempRainy = false;
                tempSunny = false;
                tempSnowy = true;
            }
            changeTime.Add(NetworkTime.time);
        }

        if (tempRainy) 
        { 
            tempLimitRain = 0.29f; tempLimitSnow = 0.0f; tempLimitSunny = 0.0f;
        }
        if (tempSnowy) 
        { 
            tempLimitRain = 0.0f; tempLimitSnow = 0.29f; tempLimitSunny = 0.0f; 
        }
        if (tempSunny) 
        { 
            tempLimitRain = 0.0f; tempLimitSnow = 0.0f; tempLimitSunny = 0.85f; 
        }

        Invoke(nameof(ChangeWeather), 10.0f);
    }

    public void ChangeWeather()
    {
        if (tempLimitRain > 0.0f) isRainy = true; else isRainy = false;
        if (tempLimitSnow > 0.0f) isSnowy = true; else isSnowy = false;
        if (tempLimitSunny > 0.0f) isSunny = true;else isSunny = false;
    }

    public void ChangeWindConditions()
    {
        float nextweather = UnityEngine.Random.Range(0.0f, 1.0f);

        if (season == winter)
        {
            if (nextweather <= winterProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == spring)
        {
            if (nextweather <= springProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == summer)
        {
            if (nextweather <= summerProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }

        if (season == autumn)
        {
            if (nextweather <= autumnProbabilityWindy)
            {
                isWindy = true;
            }
            else
                isWindy = false;
        }
    }

    public void ChargeAquifer()
    {
        if (isRainy)
        {
            for (int i = 0; i < actualAcquifer.Count; i++)
            {
                actualAcquifer[i].actualWater += 10;
                if (actualAcquifer[i].actualWater > actualAcquifer[i].maxWater) actualAcquifer[i].actualWater = actualAcquifer[i].maxWater;
            }
        }
    }

    public void ChargeWaterContainer()
    { 
        if (isRainy)
        {
            for (int i = 0; i < ModularBuildingManager.singleton.waterContainers.Count; i++)
            {
                ModularBuildingManager.singleton.waterContainers[i].water += 2;
                if (ModularBuildingManager.singleton.waterContainers[i].water > 200) ModularBuildingManager.singleton.waterContainers[i].water = 200;
            }
        }
    }
}
