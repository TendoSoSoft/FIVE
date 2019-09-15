﻿using System;
using UnityEngine;

namespace FIVE.UI.OptionMenu
{
    public class OptionMenuViewModel : ViewModel<OptionMenuView, OptionMenuViewModel>
    {

        public string TestInputFieldText { get; set; }
        public OptionMenuViewModel() : base()
        {

            binder.Bind(view => view.LoadButton.onClick).
            To(viewModel => viewModel.OnLoadButtonClicked);

            binder.Bind(view => view.SaveButton.onClick).
            To(viewModel => viewModel.OnSaveButtonClicked);

            binder.Bind(view => view.GameOptionButton.onClick).
            To(viewModel => viewModel.OnGameOptionButtonClicked);

            binder.Bind(view => view.ExitGameButton.onClick).
            To(viewModel => viewModel.OnExitButtonClicked);

            binder.Bind(view => view.VideoOptionButton.onClick).
            To(viewModel => viewModel.OnVideoOptionButtonClicked);

            binder.Bind(view => view.SoundOptionButton.onClick).
            To(viewModel => viewModel.OnSoundOptionButtonClicked);

            binder.Bind(view => view.ResumeButton.onClick).
            To(viewModel => viewModel.OnResumeButtonClicked);
        }

        private void OnLoadButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnLoadButtonClicked));
        }
        private void OnSaveButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnSaveButtonClicked));
        }
        private void OnGameOptionButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnGameOptionButtonClicked));
        }
        private void OnExitButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnExitButtonClicked));
        }
        private void OnResumeButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnResumeButtonClicked));
        }
        private void OnVideoOptionButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnVideoOptionButtonClicked));
        }
        private void OnSoundOptionButtonClicked(object sender, EventArgs eventArgs)
        {
            Debug.Log(nameof(OnSoundOptionButtonClicked));
        }

    }
}