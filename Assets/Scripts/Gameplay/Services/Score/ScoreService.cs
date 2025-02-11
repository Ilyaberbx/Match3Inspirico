﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Better.Services.Runtime;
using UnityEngine;

namespace EndlessHeresy.Gameplay.Services.Score
{
    [Serializable]
    public sealed class ScoreService : PocoService, IScoreService
    {
        protected override Task OnInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        protected override Task OnPostInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public event Action<int> OnScoreChanged;
        public int Score { get; private set; }

        public void AddScore(int score)
        {
            Score = Mathf.Clamp(Score + score, 0, int.MaxValue);
            Debug.Log("Score: " + Score);
            OnScoreChanged?.Invoke(Score);
        }
    }
}