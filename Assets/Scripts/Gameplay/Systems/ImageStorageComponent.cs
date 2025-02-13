﻿using EndlessHeresy.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessHeresy.Gameplay.Systems
{
    public sealed class ImageStorageComponent : MonoComponent
    {
        [SerializeField] private Image _image;

        public void SetSprite(Sprite sprite)
        {
            if (_image == null)
            {
                return;
            }

            _image.sprite = sprite;
        }

        public void SetColor(Color color) => _image.color = color;
    }
}