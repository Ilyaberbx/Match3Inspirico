﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Locators.Runtime;
using Better.Services.Runtime;
using Inspirio.Gameplay.Services.StaticDataManagement;
using Inspirio.Gameplay.StaticData;
using UnityEngine;

namespace Inspirio.Gameplay.Services.Sprites
{
    [Serializable]
    public sealed class SpriteService : PocoService, ISpriteService
    {
        private IGameplayStaticDataService _gameplayConfigurationService;
        private TilesConfiguration _tilesConfiguration;

        protected override Task OnInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected override Task OnPostInitializeAsync(CancellationToken cancellationToken)
        {
            _gameplayConfigurationService = ServiceLocator.Get<GameplayStaticDataService>();
            _tilesConfiguration = _gameplayConfigurationService.GetTilesConfiguration();
            return Task.CompletedTask;
        }

        public Sprite GetTileSprite(Vector2Int point)
        {
            var value = Mathf.Abs(point.x + point.y);
            var isEven = value % 2 == 0;
            return isEven ? _tilesConfiguration.EvenSprite : _tilesConfiguration.OddSprite;
        }

        public Sprite GetItemSprite(int id) => _gameplayConfigurationService.GetItemConfiguration(id).Sprite;
    }
}