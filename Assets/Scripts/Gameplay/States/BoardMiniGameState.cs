﻿using System.Threading;
using System.Threading.Tasks;
using Better.Locators.Runtime;
using EndlessHeresy.Gameplay.Modules;
using EndlessHeresy.UI.Services.Huds;
using EndlessHeresy.UI.Services.Popups;

namespace EndlessHeresy.Gameplay.States
{
    public sealed class BoardMiniGameState : BaseGameplayState
    {
        private IPopupsService _popupsService;
        private IHudsService _hudsService;

        public override async Task EnterAsync(CancellationToken token)
        {
            _popupsService = ServiceLocator.Get<PopupsService>();
            _hudsService = ServiceLocator.Get<HudsService>();

            await AddModuleAsync<InitializeBoardModule>();
            await AddModuleAsync<PauseModule>();
            await AddModuleAsync<ScoreModule>();
            await AddModuleAsync<GameExodusModule>();
        }

        public override Task ExitAsync(CancellationToken token)
        {
            DisposeAllModules();
            _popupsService.Hide();
            _hudsService.HideAll();
            return Task.CompletedTask;
        }
    }
}