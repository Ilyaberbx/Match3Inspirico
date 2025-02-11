﻿using System.Threading;
using System.Threading.Tasks;
using EndlessHeresy.Gameplay.Modules;

namespace EndlessHeresy.Gameplay.States
{
    public sealed class BoardMiniGameState : BaseGameplayState
    {
        public override async Task EnterAsync(CancellationToken token)
        {
            await AddModuleAsync<InitializeBoardModule>();
        }

        public override Task ExitAsync(CancellationToken token)
        {
            DisposeAllModules();
            return Task.CompletedTask;
        }
    }
}