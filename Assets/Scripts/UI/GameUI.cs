using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense
{
    /// <summary>
    /// Construiește și actualizează interfața jocului prin cod: bani, vieți, val, nivel,
    /// magazinul de ture, bannere (nivel/boss) și ecranul final.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        Text moneyText, livesText, waveText, levelText;
        Text bannerText;
        GameObject endPanel;
        Text endText;
        Font font;
        List<TurretBlueprint> blueprints;
        readonly List<Button> shopButtons = new List<Button>();
        Coroutine bannerRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Init(List<TurretBlueprint> blueprints)
        {
            this.blueprints = blueprints;
            font = GetFont();
            EnsureEventSystem();
            BuildCanvas();
            HookEvents();
        }

        void Update()
        {
            var lm = LevelManager.Instance;
            if (lm != null)
            {
                if (levelText != null) levelText.text = $"Nivel: {lm.CurrentLevel}/{lm.TotalLevels}";
                if (waveText != null && lm.CurrentSpawner != null)
                    waveText.text = $"Val: {lm.CurrentSpawner.CurrentWave}/{lm.CurrentSpawner.TotalWaves}";
            }
            HighlightSelected();
        }

        void HighlightSelected()
        {
            if (BuildManager.Instance == null) return;
            for (int i = 0; i < shopButtons.Count; i++)
            {
                bool sel = BuildManager.Instance.Selected == blueprints[i];
                var img = shopButtons[i].GetComponent<Image>();
                var c = blueprints[i].color;
                img.color = sel ? c : new Color(c.r * 0.55f, c.g * 0.55f, c.b * 0.55f, 1f);
            }
        }

        /// <summary>Afișează un mesaj mare temporar în centru (ex. „NIVELUL 2", „BOSS!").</summary>
        public void ShowBanner(string text, float duration)
        {
            if (bannerText == null) return;
            bannerText.text = text;
            bannerText.gameObject.SetActive(true);
            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(HideBanner(duration));
        }

        IEnumerator HideBanner(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (bannerText != null) bannerText.gameObject.SetActive(false);
        }

        Font GetFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 16);
            return f;
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Bară de sus.
            var bar = MakeRect("TopBar", canvas.transform);
            bar.anchorMin = new Vector2(0, 1);
            bar.anchorMax = new Vector2(1, 1);
            bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, 90);
            bar.anchoredPosition = Vector2.zero;
            bar.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.55f);

            moneyText = MakeText(canvas.transform, "Bani: 0", 38, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.2f));
            Place(moneyText.rectTransform, new Vector2(0, 1), new Vector2(30, -45), new Vector2(360, 70));

            livesText = MakeText(canvas.transform, "Vieti: 0", 38, TextAnchor.MiddleLeft, new Color(1f, 0.45f, 0.45f));
            Place(livesText.rectTransform, new Vector2(0, 1), new Vector2(400, -45), new Vector2(360, 70));

            waveText = MakeText(canvas.transform, "Val: 0/0", 38, TextAnchor.MiddleRight, Color.white);
            Place(waveText.rectTransform, new Vector2(1, 1), new Vector2(-400, -45), new Vector2(360, 70));

            levelText = MakeText(canvas.transform, "Nivel: 1/3", 38, TextAnchor.MiddleRight, new Color(0.6f, 0.9f, 1f));
            Place(levelText.rectTransform, new Vector2(1, 1), new Vector2(-30, -45), new Vector2(360, 70));

            // Banner central (ascuns implicit).
            bannerText = MakeText(canvas.transform, "", 96, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.3f));
            Place(bannerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1600, 220));
            bannerText.gameObject.SetActive(false);

            // Magazin (jos).
            float bw = 360, bh = 110, gap = 30;
            float totalW = blueprints.Count * bw + (blueprints.Count - 1) * gap;
            float startX = -totalW / 2f + bw / 2f;
            for (int i = 0; i < blueprints.Count; i++)
            {
                var bp = blueprints[i];
                float x = startX + i * (bw + gap);
                var btn = MakeButton(canvas.transform, $"{bp.name}\n{bp.cost} $",
                    new Vector2(0.5f, 0), new Vector2(x, 80), new Vector2(bw, bh), bp.color);
                btn.onClick.AddListener(() => BuildManager.Instance.Select(bp));
                shopButtons.Add(btn);
            }

            // Ecran final (ascuns implicit).
            endPanel = MakeRect("EndPanel", canvas.transform).gameObject;
            var ep = endPanel.GetComponent<RectTransform>();
            ep.anchorMin = Vector2.zero;
            ep.anchorMax = Vector2.one;
            ep.offsetMin = Vector2.zero;
            ep.offsetMax = Vector2.zero;
            endPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.78f);

            endText = MakeText(endPanel.transform, "", 90, TextAnchor.MiddleCenter, Color.white);
            Place(endText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(1400, 220));

            var restart = MakeButton(endPanel.transform, "Reia jocul", new Vector2(0.5f, 0.5f),
                new Vector2(0, -90), new Vector2(440, 120), new Color(0.2f, 0.5f, 0.8f));
            restart.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

            endPanel.SetActive(false);
        }

        void HookEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnMoneyChanged += m => moneyText.text = $"Bani: {m}";
            gm.OnLivesChanged += l => livesText.text = $"Vieti: {l}";
            gm.OnStateChanged += OnState;
            moneyText.text = $"Bani: {gm.Money}";
            livesText.text = $"Vieti: {gm.Lives}";
        }

        void OnState(GameManager.GameState s)
        {
            if (s == GameManager.GameState.Playing) return;
            endPanel.SetActive(true);
            if (s == GameManager.GameState.Victory)
            {
                endText.text = "AI CASTIGAT!";
                endText.color = new Color(0.4f, 1f, 0.5f);
            }
            else
            {
                endText.text = "INFRANGERE";
                endText.color = new Color(1f, 0.4f, 0.4f);
            }
        }

        // ---------- Helperi pentru construirea UI ----------

        RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        Text MakeText(Transform parent, string content, int size, TextAnchor anchor, Color color)
        {
            var rt = MakeRect("Text", parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var rt = MakeRect("Button", parent);
            Place(rt, anchor, pos, size);
            rt.gameObject.AddComponent<Image>().color = color;
            var btn = rt.gameObject.AddComponent<Button>();
            var txt = MakeText(rt, label, 28, TextAnchor.MiddleCenter, Color.white);
            Place(txt.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            return btn;
        }

        void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
    }
}
