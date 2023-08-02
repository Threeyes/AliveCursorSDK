using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Threeyes.Steamworks.Whiteboard;

namespace Threeyes.Steamworks
{
    public class WhiteboardToolbar : MonoBehaviour
    {
        public Image imagePreview;//Func: show color/Size/Brush, etc
        public Text textR;
        public Slider sliderR;
        public Text textG;
        public Slider sliderG;
        public Text textB;
        public Slider sliderB;
        public Text textA;
        public Slider sliderA;

        public Text textSize;
        public Slider sliderSize;

        public Toggle togglePenType_Pen;
        public Toggle togglePenType_Eraser;
        public Button buttonClear;

        public Toggle toggleEffectType_None;
        public Toggle toggleEffectType_Smear;

        //Runtime
        Whiteboard WhiteboardInstance { get { return Whiteboard.Instance; } }
        WhiteboardPen PenInstance { get { return WhiteboardPen.Instance; } }
        public Color32 defaultPenColor { get { return WhiteboardInstance.Config.defaultPenColor; } }
        public Color32 curColor32;
        public int curSize = 1;
        private void Start()
        {
            sliderSize.minValue = WhiteboardInstance.Config.penSizeRange.x;
            sliderSize.maxValue = WhiteboardInstance.Config.penSizeRange.y;
            sliderR.onValueChanged.AddListener(OnRSliderChanged);
            sliderG.onValueChanged.AddListener(OnGSliderChanged);
            sliderB.onValueChanged.AddListener(OnBSliderChanged);
            sliderA.onValueChanged.AddListener(OnASliderChanged);

            sliderSize.onValueChanged.AddListener(OnSizeSliderChanged);
            buttonClear.onClick.AddListener(OnClearButtonClick);
            togglePenType_Pen.onValueChanged.AddListener(OnPenType_PenToggleChanged);
            togglePenType_Eraser.onValueChanged.AddListener(OnPenType_EraserToggleChanged);
            toggleEffectType_None.onValueChanged.AddListener(OnEffectType_NoneToggleChanged);
            toggleEffectType_Smear.onValueChanged.AddListener(OnEffectType_SmearToggleChanged);

            sliderR.value = defaultPenColor.r;
            sliderG.value = defaultPenColor.g;
            sliderB.value = defaultPenColor.b;
            sliderA.value = defaultPenColor.a;
            sliderSize.value = WhiteboardInstance.Config.defaultPenSize;
        }
        private void OnRSliderChanged(float value)
        {
            byte bValue = (byte)value;
            curColor32.r = bValue;
            UpdateVisualFunc();
        }
        private void OnGSliderChanged(float value)
        {
            byte bValue = (byte)value;
            curColor32.g = bValue;
            UpdateVisualFunc();
        }
        private void OnBSliderChanged(float value)
        {
            byte bValue = (byte)value;
            curColor32.b = bValue;
            UpdateVisualFunc();
        }
        private void OnASliderChanged(float value)
        {
            byte bValue = (byte)value;
            curColor32.a = bValue;
            UpdateVisualFunc();
        }

        void OnSizeSliderChanged(float value)
        {
            curSize = (int)value;
            UpdateVisualFunc();
        }
        void OnClearButtonClick()
        {
            Whiteboard.Instance.Clear();
        }
        private void OnPenType_PenToggleChanged(bool isOn)
        {
            if (isOn)
                PenInstance.SetPenType(PenType.Pen);
        }
        private void OnPenType_EraserToggleChanged(bool isOn)
        {
            if (isOn)
                PenInstance.SetPenType(PenType.Eraser);
        }
        private void OnEffectType_NoneToggleChanged(bool isOn)
        {
            if (isOn)
                PenInstance.SetEffect(EffectType.None);
        }
        private void OnEffectType_SmearToggleChanged(bool isOn)
        {
            if (isOn)
                PenInstance.SetEffect(EffectType.Smear);
        }
        void UpdateVisualFunc()
        {
            textR.text = $"R[{(int)sliderR.value}]";
            textG.text = $"G[{(int)sliderG.value}]";
            textB.text = $"B[{(int)sliderB.value}]";
            textA.text = $"A[{(int)sliderA.value}]";
            textSize.text = $"Size[{(int)sliderSize.value}]";

            imagePreview.color = curColor32;
            PenInstance.SetPenColor(curColor32);
            PenInstance.SetPenSize(curSize);
        }
    }
}