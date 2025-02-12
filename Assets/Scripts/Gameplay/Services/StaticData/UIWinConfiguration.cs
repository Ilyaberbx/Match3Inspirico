﻿using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeresy.Gameplay.Services.StaticData
{
    [CreateAssetMenu(menuName = "Configs/Match3/UIWin", fileName = "UIWinConfiguration", order = 0)]
    public sealed class UIWinConfiguration : ScriptableObject
    {
        [SerializeField] private List<Color> _colorsForStars;
        [SerializeField] private List<Sprite> _iconsForStars;
        [SerializeField] private List<string> _descriptionsForStars;

        public List<Color> ColorsForStars => _colorsForStars;
        public List<Sprite> IconsForStars => _iconsForStars;
        public List<string> DescriptionsForStars => _descriptionsForStars;
    }
}