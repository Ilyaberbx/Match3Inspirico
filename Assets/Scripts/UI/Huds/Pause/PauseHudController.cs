﻿using Better.Commons.Runtime.Extensions;
using Better.Locators.Runtime;
using EndlessHeresy.Gameplay.Services.Pause;
using EndlessHeresy.UI.MVC;
using EndlessHeresy.UI.Popups.Pause;
using EndlessHeresy.UI.Services.Popups;

namespace EndlessHeresy.UI.Huds.Pause
{
    public sealed class PauseHudController : BaseController<PauseHudModel, PauseHudView>
    {
        private IPauseService _pauseService;
        private IPopupsService _popupsService;

        protected override void Show(PauseHudModel model, PauseHudView view)
        {
            base.Show(model, view);

            _pauseService = ServiceLocator.Get<PauseService>();
            _popupsService = ServiceLocator.Get<PopupsService>();
            View.OnPauseClicked += OnPauseClicked;
        }

        protected override void Hide()
        {
            base.Hide();

            View.OnPauseClicked -= OnPauseClicked;
        }

        private void OnPauseClicked()
        {
            if (_pauseService.IsPaused)
            {
                return;
            }

            _pauseService.Pause();
            _popupsService.ShowAsync<PausePopupController, PausePopupModel>(PausePopupModel.New()).Forget();
        }
    }
}