using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PositionType = UnityEngine.UIElements.Position;

namespace UnityEditor.VFX.UIElements
{
    class VFXColorField : ValueControl<Color>
    {
        VisualElement m_ColorDisplay;

        VisualElement m_AlphaDisplay;
        VisualElement m_NotAlphaDisplay;
        VisualElement m_AlphaContainer;

        VisualElement m_HDRLabel;

        VisualElement m_IndeterminateLabel;

        VisualElement m_Container;

        VisualElement CreateColorContainer()
        {
            m_Container = new VisualElement();

            m_Container.style.flexDirection = FlexDirection.Column;
            m_Container.style.alignItems = Align.Stretch;
            m_Container.style.flexGrow = 1f;
            m_Container.AddToClassList("colorcontainer");

            m_ColorDisplay = new VisualElement();
            m_ColorDisplay.style.height = 10;
            m_ColorDisplay.AddToClassList("colordisplay");

            m_AlphaDisplay = new VisualElement();
            m_AlphaDisplay.style.height = 3;
            m_AlphaDisplay.style.backgroundColor = Color.white;

            m_AlphaDisplay.AddToClassList("alphadisplay");

            m_NotAlphaDisplay = new VisualElement();
            m_NotAlphaDisplay.style.height = 3;
            m_NotAlphaDisplay.style.backgroundColor = Color.black;

            m_NotAlphaDisplay.AddToClassList("notalphadisplay");

            m_AlphaContainer = new VisualElement();
            m_AlphaContainer.style.flexDirection = FlexDirection.Row;
            m_AlphaContainer.style.height = 3;

            m_ColorDisplay.AddManipulator(new Clickable(OnColorClick));
            m_AlphaDisplay.AddManipulator(new Clickable(OnColorClick));


            m_HDRLabel = new Label() {
                pickingMode = PickingMode.Ignore,
                text = "HDR"
            };

            m_IndeterminateLabel = new Label() {
                pickingMode = PickingMode.Ignore,
                name = "indeterminate",
                text = VFXControlConstants.indeterminateText
            };

            m_HDRLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_HDRLabel.style.position = PositionType.Absolute;
            m_HDRLabel.style.top = 0f;
            m_HDRLabel.style.bottom = 0f;
            m_HDRLabel.style.left = 0f;
            m_HDRLabel.style.right = 0f;

            m_IndeterminateLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            m_IndeterminateLabel.style.position = PositionType.Absolute;
            m_IndeterminateLabel.style.top = 0f;
            m_IndeterminateLabel.style.bottom = 0f;
            m_IndeterminateLabel.style.left = 0f;
            m_IndeterminateLabel.style.right = 0f;

            m_HDRLabel.AddToClassList("hdr");

            m_Container.Add(m_ColorDisplay);
            m_Container.Add(m_AlphaContainer);
            m_Container.Add(m_HDRLabel);
            m_Container.Add(m_IndeterminateLabel);

            m_AlphaContainer.Add(m_AlphaDisplay);
            m_AlphaContainer.Add(m_NotAlphaDisplay);


            return m_Container;
        }

        bool m_ShowAlpha = true;

        public bool showAlpha
        {
            get { return m_ShowAlpha; }
            set
            {
                if (m_ShowAlpha != value)
                {
                    m_ShowAlpha = value;
                    if (m_ShowAlpha)
                    {
                        m_Container.Add(m_AlphaContainer);
                    }
                    else
                    {
                        m_AlphaContainer.RemoveFromHierarchy();
                    }
                }
            }
        }

        void OnColorClick()
        {
            if (enabledInHierarchy)
                ColorPicker.Show(OnColorChanged, m_Value, m_ShowAlpha, true);
        }

        VisualElement CreateEyeDropper()
        {
            Texture2D eyeDropperIcon = EditorGUIUtility.IconContent("EyeDropper.Large").image as Texture2D;
            var eyeDropper = new VisualElement();

            eyeDropper.style.backgroundImage = eyeDropperIcon;
            eyeDropper.style.width = eyeDropperIcon.width /2;
            eyeDropper.style.height = eyeDropperIcon.height /2;

            eyeDropper.RegisterCallback<MouseDownEvent>(OnEyeDropperStart);

            return eyeDropper;
        }

        IVisualElementScheduledItem m_EyeDropperScheduler;
        void OnEyeDropperStart(MouseDownEvent e)
        {
            EyeDropper.Start(OnGammaColorChanged);
            m_EyeDropperScheduler = this.schedule.Execute(OnEyeDropperMove).Every(10).StartingIn(10);
            m_EyeDropper.UnregisterCallback<MouseDownEvent>(OnEyeDropperStart);
        }

        void OnEyeDropperMove(TimerState state)
        {
            Color pickerColor = EyeDropper.GetPickedColor();
            if (pickerColor != GetValue())
            {
                SetValue(pickerColor.linear);
            }
        }

        VisualElement m_EyeDropper;

        public VFXColorField(string label) : base(label)
        {
            VisualElement container = CreateColorContainer();
            Add(container);

            m_EyeDropper = CreateEyeDropper();
            Add(m_EyeDropper);
        }

        public VFXColorField(Label existingLabel) : base(existingLabel)
        {
            VisualElement container = CreateColorContainer();
            Add(container);

            m_EyeDropper = CreateEyeDropper();
            Add(m_EyeDropper);
        }

        void OnGammaColorChanged(Color color)
        {
            OnColorChanged(color.linear);
        }

        void OnColorChanged(Color color)
        {
            SetValue(color);

            if (m_EyeDropperScheduler != null)
            {
                m_EyeDropperScheduler.Pause();
                m_EyeDropperScheduler = null;
                m_EyeDropper.RegisterCallback<MouseDownEvent>(OnEyeDropperStart);
            }

            if (OnValueChanged != null)
                OnValueChanged();
        }

        bool m_Indeterminate;

        public bool indeterminate
        {
            get {return m_Indeterminate; }
            set
            {
                m_Indeterminate = value;
                ValueToGUI(true);
            }
        }

        protected override void ValueToGUI(bool force)
        {
            if (indeterminate)
            {
                m_ColorDisplay.style.backgroundColor = VFXControlConstants.indeterminateTextColor;
                m_AlphaDisplay.style.flexGrow = 1f;
                m_NotAlphaDisplay.style.flexGrow = 0f;
                m_NotAlphaDisplay.style.flexShrink = 0f;
                m_HDRLabel.RemoveFromHierarchy();
                if (m_IndeterminateLabel.parent == null)
                    m_Container.Add(m_IndeterminateLabel);
            }
            else
            {
                m_IndeterminateLabel.RemoveFromHierarchy();
                Color displayedColor = (new Color(m_Value.r, m_Value.g, m_Value.b, 1)).gamma;
                m_ColorDisplay.style.backgroundColor = displayedColor;
                m_AlphaDisplay.style.flexGrow = m_Value.a;
                m_AlphaDisplay.style.flexShrink = 0f;
                m_NotAlphaDisplay.style.flexGrow = 1 - m_Value.a;
                m_NotAlphaDisplay.style.flexShrink = 0f;

                bool hdr = m_Value.r > 1 || m_Value.g > 1 || m_Value.b > 1;
                if ((m_HDRLabel.parent != null) != hdr)
                {
                    if (hdr)
                    {
                        if (m_HDRLabel.parent == null)
                            m_Container.Add(m_HDRLabel);
                    }
                    else
                        m_HDRLabel.RemoveFromHierarchy();
                }
            }
        }
    }
}
