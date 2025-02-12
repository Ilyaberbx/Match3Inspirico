﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Better.Services.Runtime;
using EndlessHeresy.Gameplay.Actors;

namespace EndlessHeresy.Gameplay.Services.Level
{
    [Serializable]
    public sealed class LevelService : PocoService, ILevelService
    {
        public event Action<IEnumerable<ItemActor>> OnItemsPopped;
        public event Action OnMove;
        protected override Task OnInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        protected override Task OnPostInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public int SelectedLevelIndex { get; private set; }
        public void FireItemsPopped(IEnumerable<ItemActor> items) => OnItemsPopped?.Invoke(items);
        public void FireMove() => OnMove?.Invoke();
        public void SelectLevel(int index) => SelectedLevelIndex = index;
    }
}