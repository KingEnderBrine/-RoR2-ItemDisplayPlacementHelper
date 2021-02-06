using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    [AddComponentMenu("UI/Searchable Dropdown", 35)]
    [RequireComponent(typeof(RectTransform))]
    public class SearchableDropdown : TMP_InputField, IEventSystemHandler, ICancelHandler
    {
        [SerializeField]
        private RectTransform m_Template;
        [Space]
        [SerializeField]
        private TMP_Text m_ItemText;
        [SerializeField]
        private Image m_ItemImage;
        [SerializeField]
        private TMP_Text m_SelectText;
        [Space]
        private GameObject m_Dropdown;
        private GameObject m_Blocker;
        private List<DropdownItem> m_Items = new List<DropdownItem>();
        private FloatTweenRunner m_AlphaTweenRunner;
        private bool validTemplate;
        private Canvas rootCanvas;
        private Vector2 initialContentSizeDelta;
        private Vector2 initialDropdownSizeDelta;

        public RectTransform template
        {
            get => this.m_Template;
            set
            {
                this.m_Template = value;
            }
        }

        public TMP_Text itemText
        {
            get => this.m_ItemText;
            set
            {
                this.m_ItemText = value;
            }
        }

        public Image itemImage
        {
            get => this.m_ItemImage;
            set
            {
                this.m_ItemImage = value;
            }
        }

        public List<OptionData> Options { get; set; } = new List<OptionData>();

        [SerializeField]
        private DropdownEvent m_OnItemSelected;
        public DropdownEvent OnItemSelected
        {
            get => m_OnItemSelected;
            set => m_OnItemSelected = value;
        }

        public bool IsExpanded => m_Dropdown != null;

        protected SearchableDropdown()
        {
        }

        protected override void Awake()
        {
            base.Awake();

            m_AlphaTweenRunner = new FloatTweenRunner();
            m_AlphaTweenRunner.Init(this);
            if (!m_Template)
            {
                return;
            }
            m_Template.gameObject.SetActive(false);
            onValueChanged.AddListener(ApplyOptionsFilter);
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.ImmediateDestroyDropdownList();
            if (!this.m_Blocker)
                this.DestroyBlocker(this.m_Blocker);
            this.m_Blocker = null;
        }

        public void AddOptions(List<OptionData> options)
        {
            this.Options.AddRange(options);
        }

        public void AddOptions(Dictionary<object, string> options)
        {
            foreach (var option in options)
            {
                this.Options.Add(new OptionData(option.Key, option.Value));
            }
        }

        public void AddOptions(Dictionary<object, Sprite> options)
        {
            foreach (var option in options)
            {
                this.Options.Add(new OptionData(option.Key, option.Value));
            }
        }

        public void ClearOptions()
        {
            this.Options.Clear();
        }

        private void SetupTemplate()
        {
            validTemplate = false;
            if (!m_Template)
            {
                Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", (UnityEngine.Object)this);
            }
            else
            {
                GameObject gameObject = this.m_Template.gameObject;
                gameObject.SetActive(true);
                this.validTemplate = true;
                var itemButton = gameObject.GetComponentInChildren<Button>();
                if (!itemButton || itemButton.transform == template)
                {
                    this.validTemplate = false;
                    Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Button component serving as the item.", (UnityEngine.Object)this.template);
                }
                else if (!(itemButton.transform.parent is RectTransform))
                {
                    this.validTemplate = false;
                    Debug.LogError("The dropdown template is not valid. The child GameObject with a Button component (the item) must have a RectTransform on its parent.", (UnityEngine.Object)this.template);
                }
                else if (itemText != null && !this.itemText.transform.IsChildOf(itemButton.transform))
                {
                    this.validTemplate = false;
                    Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", (UnityEngine.Object)this.template);
                }
                else if (itemImage != null && !this.itemImage.transform.IsChildOf(itemButton.transform))
                {
                    this.validTemplate = false;
                    Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", (UnityEngine.Object)this.template);
                }
                if (!this.validTemplate)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    Canvas orAddComponent = GetOrAddComponent<Canvas>(gameObject);
                    orAddComponent.overrideSorting = true;
                    orAddComponent.sortingOrder = 30000;
                    GetOrAddComponent<GraphicRaycaster>(gameObject);
                    GetOrAddComponent<CanvasGroup>(gameObject);
                    gameObject.SetActive(false);
                    this.validTemplate = true;
                }
            }
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T obj = go.GetComponent<T>();
            if (!obj)
                obj = go.AddComponent<T>();
            return obj;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            this.Show();
            textViewport.gameObject.SetActive(true);
            m_SelectText.gameObject.SetActive(false);
            base.OnPointerClick(eventData);
        }

        public virtual void OnCancel(BaseEventData eventData) => Hide();

        public void Show()
        {
            if (!IsActive() || !IsInteractable() || m_Dropdown)
                return;
            var canvasList = this.gameObject.GetComponentsInParent<Canvas>(false);
            if (canvasList.Length == 0)
                return;
            rootCanvas = canvasList[canvasList.Length - 1];
            for (int index = 0; index < canvasList.Length; ++index)
            {
                if (canvasList[index].isRootCanvas)
                {
                    rootCanvas = canvasList[index];
                    break;
                }
            }
            canvasList = null;

            if (!this.validTemplate)
            {
                this.SetupTemplate();
                if (!this.validTemplate)
                    return;
            }
            this.m_Template.gameObject.SetActive(true);
            this.m_Template.GetComponent<Canvas>().sortingLayerID = rootCanvas.sortingLayerID;
            this.m_Dropdown = this.CreateDropdownList(this.m_Template.gameObject);
            this.m_Dropdown.name = "Dropdown List";
            this.m_Dropdown.SetActive(true);
            RectTransform dropdownTransform = this.m_Dropdown.transform as RectTransform;
            dropdownTransform.SetParent(this.m_Template.transform.parent, false);
            DropdownItem dropdownItem = this.m_Dropdown.GetComponentInChildren<DropdownItem>();
            RectTransform contentTransform = dropdownItem.rectTransform.parent.gameObject.transform as RectTransform;

            initialContentSizeDelta = contentTransform.sizeDelta;
            initialDropdownSizeDelta = dropdownTransform.sizeDelta;

            dropdownItem.rectTransform.gameObject.SetActive(true);
            
            this.m_Items.Clear();
            for (int index = 0; index < this.Options.Count; ++index)
            {
                var item = AddItem(Options[index], dropdownItem, this.m_Items);
                if (item.button)
                {
                    item.button.onClick.AddListener(() => OnSelectItem(item.Value));
                }
            }

            RecalculateDropdownBounds();

            this.AlphaFadeList(0.15f, 0.0f, 1f);
            this.m_Template.gameObject.SetActive(false);
            dropdownItem.gameObject.SetActive(false);
            this.m_Blocker = this.CreateBlocker(rootCanvas);
        }

        private void RecalculateDropdownBounds()
        {
            if (!m_Dropdown)
            {
                return;
            }
            RectTransform dropdownTransform = this.m_Dropdown.transform as RectTransform;
            DropdownItem dropdownItem = this.m_Dropdown.GetComponentInChildren<DropdownItem>(true);
            RectTransform contentTransform = dropdownItem.rectTransform.parent.gameObject.transform as RectTransform;
            var activeItems = m_Items.Where(el => el.gameObject.activeSelf);
            
            contentTransform.sizeDelta = initialContentSizeDelta;
            dropdownTransform.sizeDelta = initialDropdownSizeDelta;

            Rect contentRect = contentTransform.rect;
            Rect dropdownItemRect = dropdownItem.rectTransform.rect;
            Vector2 vector2_1 = dropdownItemRect.min - contentRect.min + (Vector2)dropdownItem.rectTransform.localPosition;
            Vector2 vector2_2 = dropdownItemRect.max - contentRect.max + (Vector2)dropdownItem.rectTransform.localPosition;
            Vector2 size = dropdownItemRect.size;
            Vector2 sizeDelta = contentTransform.sizeDelta;
            sizeDelta.y = size.y * activeItems.Count() + vector2_1.y - vector2_2.y;
            contentTransform.sizeDelta = sizeDelta;
            Rect rect3 = dropdownTransform.rect;
            double height1 = rect3.height;
            rect3 = contentTransform.rect;
            double height2 = rect3.height;
            float num1 = (float)(height1 - height2);
            if (num1 > 0.0)
                dropdownTransform.sizeDelta = new Vector2(dropdownTransform.sizeDelta.x, dropdownTransform.sizeDelta.y - num1);
            Vector3[] fourCornersArray = new Vector3[4];
            dropdownTransform.GetWorldCorners(fourCornersArray);
            RectTransform transform3 = rootCanvas.transform as RectTransform;
            Rect rect4 = transform3.rect;
            for (int index1 = 0; index1 < 2; ++index1)
            {
                bool flag = false;
                for (int index2 = 0; index2 < 4; ++index2)
                {
                    Vector3 vector3 = transform3.InverseTransformPoint(fourCornersArray[index2]);
                    double num2 = vector3[index1];
                    Vector2 vector2_3 = rect4.min;
                    double num3 = vector2_3[index1];
                    if (num2 < num3)
                    {
                        double num4 = vector3[index1];
                        vector2_3 = rect4.min;
                        double num5 = vector2_3[index1];
                        if (!Mathf.Approximately((float)num4, (float)num5))
                            goto label_28;
                    }
                    double num6 = vector3[index1];
                    vector2_3 = rect4.max;
                    double num7 = vector2_3[index1];
                    if (num6 > num7)
                    {
                        double num4 = vector3[index1];
                        vector2_3 = rect4.max;
                        double num5 = vector2_3[index1];
                        if (Mathf.Approximately((float)num4, (float)num5))
                            continue;
                    }
                    else
                        continue;
                    label_28:
                    flag = true;
                    break;
                }
                if (flag)
                    RectTransformUtility.FlipLayoutOnAxis(dropdownTransform, index1, false, false);
            }
            for (int index = 0; index < activeItems.Count(); ++index)
            {
                RectTransform rectTransform = activeItems.ElementAt(index).rectTransform;
                rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0.0f);
                rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0.0f);
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, (float)(vector2_1.y + size.y * (double)(activeItems.Count() - 1 - index) + size.y * (double)rectTransform.pivot.y));
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y);
            }
        }

        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            GameObject gameObject = new GameObject("Blocker");
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(rootCanvas.transform, false);
            rectTransform.anchorMin = (Vector2)Vector3.zero;
            rectTransform.anchorMax = (Vector2)Vector3.one;
            rectTransform.sizeDelta = Vector2.zero;
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            Canvas component = this.m_Dropdown.GetComponent<Canvas>();
            canvas.sortingLayerID = component.sortingLayerID;
            canvas.sortingOrder = component.sortingOrder - 1;
            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<Image>().color = Color.clear;
            gameObject.AddComponent<Button>().onClick.AddListener(new UnityAction(this.Hide));
            return gameObject;
        }

        protected virtual void DestroyBlocker(GameObject blocker) => DestroyImmediate(blocker);

        protected virtual GameObject CreateDropdownList(GameObject template) => Instantiate(template);

        protected virtual void DestroyDropdownList(GameObject dropdownList) => DestroyImmediate(dropdownList);

        protected virtual DropdownItem CreateItem(DropdownItem itemTemplate) => Instantiate(itemTemplate);

        protected virtual void DestroyItem(DropdownItem item) { }

        private DropdownItem AddItem(
            OptionData data,
            DropdownItem itemTemplate,
            List<DropdownItem> items)
        {
            var dropdownItem = CreateItem(itemTemplate);
            dropdownItem.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);
            dropdownItem.gameObject.SetActive(true);
            dropdownItem.gameObject.name = "Item " + items.Count + (data.Text != null ? (": " + data.Text) : "");
            if (dropdownItem.text)
                dropdownItem.text.text = data.Text;
            if (dropdownItem.image)
            {
                dropdownItem.image.sprite = data.Image;
                dropdownItem.image.enabled = dropdownItem.image.sprite;
            }
            dropdownItem.Value = data.Value;
            items.Add(dropdownItem);
            return dropdownItem;
        }

        private void AlphaFadeList(float duration, float alpha)
        {
            CanvasGroup component = this.m_Dropdown.GetComponent<CanvasGroup>();
            this.AlphaFadeList(duration, component.alpha, alpha);
        }

        private void AlphaFadeList(float duration, float start, float end)
        {
            if (end.Equals(start))
                return;
            FloatTween info = new FloatTween()
            {
                duration = duration,
                startValue = start,
                targetValue = end
            };
            info.AddOnChangedCallback(new UnityAction<float>(this.SetAlpha));
            info.ignoreTimeScale = true;
            this.m_AlphaTweenRunner.StartTween(info);
        }

        private void SetAlpha(float alpha)
        {
            if (!(bool)(UnityEngine.Object)this.m_Dropdown)
                return;
            this.m_Dropdown.GetComponent<CanvasGroup>().alpha = alpha;
        }

        public void Hide()
        {
            if (m_Dropdown)
            {
                AlphaFadeList(0.15f, 0.0f);
                if (IsActive())
                    StartCoroutine(DelayedDestroyDropdownList(0.15f));
            }
            if (m_Blocker)
                DestroyBlocker(m_Blocker);
            m_Blocker = null;


            textViewport.gameObject.SetActive(false);
            m_SelectText.gameObject.SetActive(true);
            SetTextWithoutNotify(String.Empty);
        }

        private IEnumerator DelayedDestroyDropdownList(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ImmediateDestroyDropdownList();
        }

        private void ImmediateDestroyDropdownList()
        {
            for (int index = 0; index < this.m_Items.Count; ++index)
            {
                if (this.m_Items[index])
                    this.DestroyItem(this.m_Items[index]);
            }
            this.m_Items.Clear();
            if (m_Dropdown)
                this.DestroyDropdownList(this.m_Dropdown);
            this.m_Dropdown = null;
        }

        private void OnSelectItem(object value)
        {
            m_OnItemSelected?.Invoke(value);
            this.Hide();
        }

        private void ApplyOptionsFilter(string filter)
        {
            foreach (var item in m_Items)
            {
                item.gameObject.SetActive(item.text.text.ContainsInSequence(filter, "||"));
            }
            RecalculateDropdownBounds();
        }

        public class OptionData
        {
            public object Value { get; set; }
            public string Text { get; set; }
            public Sprite Image { get; set; }

            public OptionData()
            {
            }

            public OptionData(object value, string text)
            {
                Value = value;
                Text = text;
            }

            public OptionData(object value, Sprite image)
            {
                Value = value;
                Image = image;
            }

            public OptionData(object value, string text, Sprite image)
            {
                Value = value;
                Text = text;
                Image = image;
            }
        }

        [Serializable]
        public class DropdownEvent : UnityEvent<object>
        {
        }

        internal class FloatTweenRunner
        {
            protected MonoBehaviour m_CoroutineContainer;
            protected IEnumerator m_Tween;

            private static IEnumerator Start(FloatTween tweenInfo)
            {
                if (tweenInfo.ValidTarget())
                {
                    float elapsedTime = 0.0f;
                    while ((double)elapsedTime < (double)tweenInfo.duration)
                    {
                        elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                        tweenInfo.TweenValue(Mathf.Clamp01(elapsedTime / tweenInfo.duration));
                        yield return (object)null;
                    }
                    tweenInfo.TweenValue(1f);
                }
            }

            public void Init(MonoBehaviour coroutineContainer) => this.m_CoroutineContainer = coroutineContainer;

            public void StartTween(FloatTween info)
            {
                if (this.m_CoroutineContainer == null)
                {
                    Debug.LogWarning((object)"Coroutine container not configured... did you forget to call Init?");
                }
                else
                {
                    this.StopTween();
                    if (!this.m_CoroutineContainer.gameObject.activeInHierarchy)
                    {
                        info.TweenValue(1f);
                    }
                    else
                    {
                        this.m_Tween = FloatTweenRunner.Start(info);
                        this.m_CoroutineContainer.StartCoroutine(this.m_Tween);
                    }
                }
            }

            public void StopTween()
            {
                if (this.m_Tween == null)
                    return;
                this.m_CoroutineContainer.StopCoroutine(this.m_Tween);
                this.m_Tween = (IEnumerator)null;
            }
        }

        internal struct FloatTween
        {
            private FloatTween.FloatTweenCallback m_Target;
            private float m_StartValue;
            private float m_TargetValue;
            private float m_Duration;
            private bool m_IgnoreTimeScale;

            public float startValue
            {
                get => this.m_StartValue;
                set => this.m_StartValue = value;
            }

            public float targetValue
            {
                get => this.m_TargetValue;
                set => this.m_TargetValue = value;
            }

            public float duration
            {
                get => this.m_Duration;
                set => this.m_Duration = value;
            }

            public bool ignoreTimeScale
            {
                get => this.m_IgnoreTimeScale;
                set => this.m_IgnoreTimeScale = value;
            }

            public void TweenValue(float floatPercentage)
            {
                if (!this.ValidTarget())
                    return;
                this.m_Target.Invoke(Mathf.Lerp(this.m_StartValue, this.m_TargetValue, floatPercentage));
            }

            public void AddOnChangedCallback(UnityAction<float> callback)
            {
                if (this.m_Target == null)
                    this.m_Target = new FloatTween.FloatTweenCallback();
                this.m_Target.AddListener(callback);
            }

            public bool GetIgnoreTimescale() => this.m_IgnoreTimeScale;

            public float GetDuration() => this.m_Duration;

            public bool ValidTarget() => this.m_Target != null;

            public class FloatTweenCallback : UnityEvent<float>
            {
            }
        }
    }
}
