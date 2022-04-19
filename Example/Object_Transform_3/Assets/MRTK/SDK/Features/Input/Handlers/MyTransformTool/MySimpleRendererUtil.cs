using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace MyTransformTool
{
    [RequireComponent(typeof(Renderer))]
    public class MySimpleRendererUtil : MonoBehaviour
    {
        public Color activationColor = Color.yellow;

        private Renderer _renderer;
        private Material _materialInstance;
        private Outline _outline;

        private Color _originalColor;

        void Start()
        {
            _renderer = GetComponent<Renderer>();
            _outline = GetComponent<Outline>();
            if (_outline != null)
            {
                _outline.enabled = false;
            }
            
            _materialInstance = _renderer.material;
            _originalColor = _materialInstance.color;
        }

        public void SetToActivationColor()
        {
            if(_outline != null)
            {
                _outline.enabled = true;
            }
            else
            {
                _materialInstance.color = activationColor;
            }
        }

        public void SetToOriginalColor()
        {
            if (_outline != null)
            {
                _outline.enabled = false;
            }
            else
            {
                _materialInstance.color = _originalColor;
            }        }

        public void ShowRenderer()
        {
            if(_renderer != null)
            {
                _renderer.enabled = true;
            }
            
        }

        public void HideRenderer()
        {
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }
    }
}


