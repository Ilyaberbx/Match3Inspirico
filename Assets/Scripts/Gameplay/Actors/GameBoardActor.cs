﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Better.Commons.Runtime.Extensions;
using Better.Locators.Runtime;
using DG.Tweening;
using Inspirio.Extensions;
using Inspirio.Gameplay.Core;
using Inspirio.Gameplay.Services.Factory;
using Inspirio.Gameplay.Services.Input;
using Inspirio.Gameplay.Services.Level;
using Inspirio.Gameplay.Services.StaticDataManagement;
using Inspirio.Gameplay.Systems;
using Inspirio.Utilities;
using UnityEngine;
using Random = System.Random;
using Sequence = DG.Tweening.Sequence;

namespace Inspirio.Gameplay.Actors
{
    public sealed class GameBoardActor : MonoActor
    {
        private IGameplayFactoryService _gameplayFactoryService;
        private IGameplayStaticDataService _gameplayStaticDataService;
        private ILevelService _levelService;
        private IInputService _inputService;

        private Random _random;
        private readonly List<ItemActor> _selectedItems = new();

        private SizeStorageComponent _sizeStorage;
        private GridStorageComponent _gridStorage;
        private TilesManagerComponent _tilesManager;

        private int ItemsCount => _gameplayStaticDataService.GetItemsConfiguration().Items.Length;

        protected override async Task OnInitializeAsync()
        {
            await base.OnInitializeAsync();

            _gameplayFactoryService = ServiceLocator.Get<GameplayFactoryService>();
            _gameplayStaticDataService = ServiceLocator.Get<GameplayStaticDataService>();
            _inputService = ServiceLocator.Get<InputService>();
            _levelService = ServiceLocator.Get<LevelService>();
            _tilesManager = GetComponent<TilesManagerComponent>();
            _sizeStorage = GetComponent<SizeStorageComponent>();
            _gridStorage = GetComponent<GridStorageComponent>();

            InitializeRandom();
            await InitializeBoardAsync();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            foreach (var tile in _tilesManager.Tiles)
            {
                var item = tile.Item;
                item.OnSelected -= OnItemSelected;
                _gameplayFactoryService.Dispose(tile);
            }
        }

        private void InitializeRandom()
        {
            var idStorage = GetComponent<IdentifierStorageComponent>();
            var idHashCode = idStorage.Value.GetHashCode();
            _random = new Random(idHashCode);
        }

        private async Task InitializeBoardAsync()
        {
            var tiles = await InitializeTilesAsync();
            await InitializeItemsAsync(tiles);
        }

        private async Task<List<TileActor>> InitializeTilesAsync()
        {
            var gridRoot = _gridStorage.Group.transform;
            var width = _sizeStorage.Width;
            var height = _sizeStorage.Height;

            var tiles = new List<TileActor>();
            _tilesManager.Initialize(width, height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var tile = await _gameplayFactoryService.CreateTileAsync(x, y, gridRoot);
                    tiles.Add(tile);
                    _tilesManager.AddTile(tile, x, y);
                }
            }

            return tiles;
        }

        private async Task InitializeItemsAsync(List<TileActor> tiles)
        {
            var items = new List<ItemActor>();

            foreach (var tile in tiles)
            {
                var item = await GetRandomItemAsync(tile.transform);
                tile.SetItem(item);
                item.OnSelected += OnItemSelected;
                items.Add(item);
            }

            foreach (var tile in _tilesManager.Tiles)
            {
                tile.SetManager(_tilesManager);
            }

            var noPossibleMoves = !MatchUtility.HasPossibleMoves(_tilesManager.Tiles);
            var canPop = MatchUtility.CanPop(_tilesManager.Tiles);

            if (canPop || noPossibleMoves)
            {
                Dispose(items);
                await InitializeItemsAsync(tiles);
            }
        }

        private int GetRandomItemIndex() => _random.Next(0, ItemsCount);
        private void OnItemSelected(ItemActor item) => SelectItemAsync(item).Forget();

        private async Task ShuffleBoardAsync()
        {
            _inputService.Lock();

            var items = _tilesManager.Tiles.Cast<TileActor>().Select(t => t.Item).ToList();

            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }

            var sequence = DOTween
                .Sequence()
                .SetId(this);

            var index = 0;

            foreach (var oldTile in _tilesManager.Tiles)
            {
                var oldTileTransform = oldTile.transform;
                var newItem = items[index++];
                var oldItem = oldTile.Item;
                var newTile = GetTileOf(newItem);
                var newTileTransform = newTile.transform;
                var newItemShuffleTween =
                    newItem.transform.DOMove(oldTileTransform.position, GameBoardConstants.TweenDuration);
                var oldItemShuffleTween =
                    oldItem.transform.DOMove(newTileTransform.position, GameBoardConstants.TweenDuration);

                sequence.Join(newItemShuffleTween)
                    .Join(oldItemShuffleTween);

                oldTile.SetItem(newItem);
                newTile.SetItem(oldItem);
            }

            await ValidateAndAwait(sequence);

            _inputService.Unlock();

            var noPossibleMoves = !MatchUtility.HasPossibleMoves(_tilesManager.Tiles);
            var canPop = MatchUtility.CanPop(_tilesManager.Tiles);

            if (noPossibleMoves || canPop)
            {
                await ShuffleBoardAsync();
            }
        }

        private Task ValidateAndAwait(Sequence sequence)
        {
            return destroyCancellationToken.IsCancellationRequested
                ? Task.CompletedTask
                : sequence.AsTask(destroyCancellationToken);
        }

        private async Task SelectItemAsync(ItemActor item)
        {
            if (!CanSelect(item))
            {
                _selectedItems.Clear();
                return;
            }

            _selectedItems.Add(item);

            if (!MatchUtility.CanSwap(_selectedItems))
            {
                return;
            }

            var firstSelected = _selectedItems[0];
            var secondSelected = _selectedItems[1];
            _inputService.Lock();

            await SwapAsync(firstSelected, secondSelected);

            if (MatchUtility.TryGetTilesToPop(_tilesManager.Tiles, out var toPop))
            {
                await PopTilesAsync(toPop);
            }
            else
            {
                await SwapAsync(secondSelected, firstSelected);
            }

            _levelService.FireMove();
            _selectedItems.Clear();
            _inputService.Unlock();
        }

        private bool CanSelect(ItemActor item)
        {
            if (_selectedItems.Contains(item))
            {
                return false;
            }

            if (_selectedItems.Count <= 0)
            {
                return true;
            }

            var firstSelected = GetTileOf(_selectedItems[0]);
            var tryToSelect = GetTileOf(item);
            return firstSelected.IsNeighbor(tryToSelect);
        }

        private async Task PopTilesAsync(IReadOnlyList<TileActor> connected)
        {
            while (true)
            {
                _levelService.FirePreDeflate(_tilesManager.Tiles, connected);
                await DeflateTilesAsync(connected);
                _levelService.FireItemsPopped(connected.Select(t => t.Item));
                await InflateTilesAsync(connected);
                _levelService.FirePostInflate(_tilesManager.Tiles, connected);

                if (MatchUtility.TryGetTilesToPop(_tilesManager.Tiles, out var toPop))
                {
                    connected = toPop;
                    continue;
                }

                break;
            }

            var noPossibleMoves = !MatchUtility.HasPossibleMoves(_tilesManager.Tiles);

            if (noPossibleMoves)
            {
                await ShuffleBoardAsync();
            }
        }

        private async Task DeflateTilesAsync(IReadOnlyList<TileActor> tiles)
        {
            var sequence = DOTween
                .Sequence()
                .SetId(this);

            var items = tiles.Select(temp => temp.Item).ToArray();

            foreach (var item in items)
            {
                var tween = item.transform.DOScale(Vector3.zero, GameBoardConstants.TweenDuration);
                sequence.Join(tween);
            }

            await ValidateAndAwait(sequence);

            Dispose(items);
        }

        private void Dispose(IEnumerable<ItemActor> items)
        {
            foreach (var item in items)
            {
                item.OnSelected -= OnItemSelected;
                _gameplayFactoryService.Dispose(item);
            }
        }

        private async Task InflateTilesAsync(IReadOnlyList<TileActor> tiles)
        {
            var sequence = DOTween
                .Sequence()
                .SetId(this);

            foreach (var tile in tiles)
            {
                var item = await GetRandomItemAsync(tile.transform);
                item.OnSelected += OnItemSelected;
                tile.SetItem(item);

                var tween = item.transform.DOScale(Vector3.zero, GameBoardConstants.TweenDuration).From();
                sequence.Join(tween);
            }

            await ValidateAndAwait(sequence);
        }

        private async Task SwapAsync(ItemActor first, ItemActor second)
        {
            var firstTransform = first.transform;
            var secondTransform = second.transform;
            var firstTile = GetTileOf(first);
            var secondTile = GetTileOf(second);

            var firstTween = firstTransform
                .DOMove(secondTile.transform.position, GameBoardConstants.TweenDuration);
            var secondTween = secondTransform
                .DOMove(firstTile.transform.position, GameBoardConstants.TweenDuration);

            firstTransform.SetParent(transform);
            secondTransform.SetParent(transform);

            var sequence = DOTween
                .Sequence()
                .Join(firstTween)
                .Join(secondTween)
                .SetId(this);

            await ValidateAndAwait(sequence);

            firstTile.SetItem(second);
            secondTile.SetItem(first);
        }

        private Task<ItemActor> GetRandomItemAsync(Transform parent)
        {
            var index = GetRandomItemIndex();
            return _gameplayFactoryService.CreateItemAsync(index, parent);
        }

        private TileActor GetTileOf(ItemActor item) => _tilesManager.GetTileOf(item);
        public TileActor GetTileActor(int x, int y) => _tilesManager.GetTileActor(x, y);
    }
}