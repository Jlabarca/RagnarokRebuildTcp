using System.Collections;
using System.Collections.Generic;
using RebuildSharedData.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public GameObject ContentContainer;
    public GameObject Viewport;

    public Image MapImage;
    public Material OverworldMaterial;
    public Material DungeonMaterial;
    public Sprite PlayerIcon;
    public Slider ZoomSlider;
    
    private GameObject playerMapIconObject;

    public float ObjectScaleFactor = 1f;

    public float minScale;
    public float maxScale;

    public float curSize;
    public float lastZoom;

    private float offsetX;
    private float offsetY;

    private static MinimapController instance;

    private Coroutine loadCoroutine;

    private Sprite mapSprite;
    public Sprite walkSprite;

    public static MinimapController Instance
    {
        get
        {
            if (instance != null)
                return instance;
            instance = FindObjectOfType<MinimapController>();
            return instance;
        }
    }

    public void SetPlayerPosition(Vector2Int pos, float angle)
    {
        if (!gameObject.activeInHierarchy || MapImage == null || mapSprite == null)
            return;

        if (playerMapIconObject == null)
        {
            playerMapIconObject = new GameObject("PlayerIcon");
            playerMapIconObject.transform.SetParent(MapImage.transform, false);


            var img = playerMapIconObject.AddComponent<Image>();
            img.sprite = PlayerIcon;

         
            //var w = mapSprite.texture.width;
            //var h = mapSprite.texture.height;
         
        }
        //Debug.Log(pos + " " + new Vector3(pos.x * 10f, pos.y * 10f, 0f));

        var r = playerMapIconObject.GetComponent<RectTransform>();

        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.zero;

        var w = mapSprite.texture.height;
        var h = mapSprite.texture.height;

        r.localPosition = new Vector3(pos.x * 10f / 2f, pos.y * 10f / 2f - h, 0f);

        //ScrollRect.horizontalNormalizedPosition = pos.x / (float)w;

        var px = (pos.x * 10f / 2f + offsetX) * curSize;
        var py = ((h - pos.y * 10f / 2f) + offsetY) * curSize;

        var scrollx = px - 125f;
        var scrolly = py - 125f;



        var maxScroll = ((Mathf.Max(w, h) * curSize - 250f));


        scrollx = Mathf.Clamp(-scrollx, -maxScroll, 0);
        scrolly = Mathf.Clamp(scrolly, 0, maxScroll);

        Debug.Log($"{curSize} {px} {py} {scrollx} {scrolly} {maxScroll}");

        ContentContainer.GetComponent<RectTransform>().anchoredPosition = new Vector3(scrollx, scrolly, 0f);

        //playerMapIconObject.transform.localPosition = new Vector3(pos.x * 10f, pos.y * 10f, 0f);
        playerMapIconObject.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);

        var s = 0.3f * ObjectScaleFactor * (1 / curSize);
        
        playerMapIconObject.transform.localScale = Vector3.one * s;
    }

    public void LoadMinimap(string mapName, string mapType)
    {
        if(loadCoroutine != null)
            StopCoroutine(loadCoroutine);

        gameObject.SetActive(true);
        ContentContainer.SetActive(false);
        //if(mapSprite != null)
        //    Destroy(mapSprite);
        mapSprite = null;
        //if(walkSprite != null)
        //    Destroy(walkSprite);
        walkSprite = null;

        loadCoroutine = StartCoroutine(LoadMinimapCoroutine(mapName, mapType));
    }

    public void SetZoom(float zoom)
    {
        zoom = Mathf.Clamp(zoom, minScale, maxScale);

        curSize = zoom;

        if (mapSprite == null)
            return;

        var w = mapSprite.texture.width;
        var h = mapSprite.texture.height;

        MapImage.rectTransform.sizeDelta = new Vector2(w, h);

        var containerRect = ContentContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = MapImage.rectTransform.sizeDelta;
        containerRect.localScale = new Vector3(curSize, curSize, curSize);

        offsetX = 0f;
        offsetY = 0f;

        if (w != h && (w * curSize < 250 || h * curSize < 250))
        {

            if (w > h)
                offsetY = -(w - h) / 2f;

            else
                offsetX = (h - w) / 2f;

            MapImage.transform.localPosition = new Vector3(offsetX, offsetY, 0);
        }
        else
            MapImage.transform.localPosition = Vector3.zero;

        lastZoom = curSize;
    }

    public void UpdateZoomFromSlider()
    {
        SetZoom(ZoomSlider.value.Remap(0, 1, minScale, maxScale));
    }

    public IEnumerator LoadMinimapCoroutine(string mapName, string mapType)
    {
        yield return new WaitForEndOfFrame();

        var loadMap = Addressables.LoadAssetAsync<Sprite>($"Assets/Maps/minimap/{mapName}.png");
        var loadWalk = Addressables.LoadAssetAsync<Sprite>($"Assets/Maps/minimap/{mapName}_walkmask.png");

        yield return loadMap;
        yield return loadWalk;

        if (!loadWalk.IsDone || !loadWalk.IsValid() || !loadMap.IsDone || !loadMap.IsValid())
        {
            Debug.LogWarning("Could not load minimap.");
            yield break; //give up
        }

        //var map = loadMap.Result;

        mapSprite = loadMap.Result;
        walkSprite = loadWalk.Result;

        MapImage.sprite = mapSprite;

        if (mapType == "Dungeon")
        {
            MapImage.sprite = walkSprite;
            MapImage.material = DungeonMaterial;
        }
        else
        {
            if (mapType == "Town")
            {
                //dungeon material but with regular map, so no highlighted walk
                MapImage.material = DungeonMaterial;
            }
            else
            {
                MapImage.material = OverworldMaterial;
                OverworldMaterial.SetTexture("_SecondaryTex", walkSprite.texture);
            }
            
        }

        minScale = 250f / mapSprite.texture.width;
        maxScale = 1f;

        if (250f / mapSprite.texture.height < minScale)
            minScale = 250f / mapSprite.texture.height;

        if (minScale > 1f)
            maxScale = 1f;

        //var sprite = Sprite.Create(map, new Rect(0, 0, map.width, map.height), new Vector2(0, 1), 1);

        ContentContainer.transform.localPosition = new Vector3(0, 0, 0f);

        UpdateZoomFromSlider();

        ContentContainer.gameObject.SetActive(true);
    }

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Approximately(curSize, lastZoom))
            SetZoom(curSize);
    }
}
