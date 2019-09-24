using System;
using FIVE.UI.Background;
using FIVE.UI.MainGameDisplay;
using FIVE.UI.OptionsMenu;
using FIVE.UI.StartupMenu;
using System.Collections.Generic;
using System.Linq;
using FIVE.EventSystem;
using FIVE.GameStates;
using FIVE.UI.SplashScreens;
using UnityEngine;
using UnityEngine.UI;

namespace FIVE.UI
{
    public class UIManager : MonoBehaviour
    {
        private static readonly Dictionary<string, ViewModel> NameToVMs = new Dictionary<string, ViewModel>();
        private static readonly Dictionary<Type, SortedSet<ViewModel>> TypeToVMs = new Dictionary<Type, SortedSet<ViewModel>>();
        private static readonly SortedSet<ViewModel> LayerSortedVMs = new SortedSet<ViewModel>(new ViewModelComparer());

        public static bool TryGetViewModel(string name, out ViewModel viewModel) => NameToVMs.TryGetValue(name, out viewModel);

        public static T GetViewModel<T>() where T : ViewModel => (T)TypeToVMs[typeof(T)].FirstOrDefault();

        public static SortedSet<ViewModel> GetViewModels<T>() where T : ViewModel => TypeToVMs[typeof(T)];

        private void Awake()
        {
            var canvasGameObject = new GameObject { name = "LoadingSplashCanvas" };
            Canvas canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();
            var startUpScreen = new StartUpScreen(canvasGameObject);
            StartCoroutine(startUpScreen.OnTransitioning());
            EventManager.Subscribe<OnLoadingFinished>((sender, args) => startUpScreen.DoFadingOut());
        }

        private void Start()
        {
        }

        public static T AddViewModel<T>(string name = null) where T : ViewModel, new()
        {
            var newViewModel = new T();
            NameToVMs.Add(name ?? typeof(T).Name, newViewModel);
            if (!TypeToVMs.ContainsKey(typeof(T)))
            {
                TypeToVMs.Add(typeof(T), new SortedSet<ViewModel>(new ViewModelComparer()));
            }
            TypeToVMs[typeof(T)].Add(newViewModel);
            LayerSortedVMs.Add(newViewModel);
            return newViewModel;
        }

        private class ViewModelComparer : IComparer<ViewModel>
        {
            public int Compare(ViewModel x, ViewModel y)
            {
                return x.SortingOrder - y.SortingOrder;
            }
        }
    }
}
