using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Harmonika.Tools;

namespace Harmonika.Menu
{
    public class MenuManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private string _initialMenu;
        [SerializeField] private CanvasGroup _menuBackground;
        [SerializeField] private MenuPanel[] _menus;
        private Coroutine _fadeAnimationRoutine;

        private void Start()
        {
            OpenInitialMenu();
        }

        public void Initialize(CanvasGroup canvasGroup, MenuPanel[] menuArray)
        {
            _menuBackground = canvasGroup;
            _menus = menuArray;
        }

        public void OpenMenu(string menuName, Turn turnBackground = Turn.on)
        {
            MenuPanel m = Array.Find(_menus, menu => menu.Name == menuName);
            if (m == null)
            {
                Debug.LogError("There is no one Menu with the name " + menuName + "!\n" +
                    "Please, use a valid menu name");
                return;
            }

            StartCoroutine(SwitchCanvasGroupAnimated(_menuBackground, Turn.on));
            foreach (MenuPanel menu in _menus)
            {
                if (menu.Name == menuName)
                {
                    StartCoroutine(SwitchCanvasGroupAnimated(menu.Group, Turn.on));
                }
                else
                {
                    StartCoroutine(SwitchCanvasGroupAnimated(menu.Group, Turn.off));
                }
            }
        }

        public void CloseMenus()
        {
            StartCoroutine(SwitchCanvasGroupAnimated(_menuBackground, Turn.off));
            foreach (MenuPanel menu in _menus)
            {
                StartCoroutine(SwitchCanvasGroupAnimated(menu.Group, Turn.off));
            }
        }

        private void OpenInitialMenu()
        {
            if (string.IsNullOrEmpty(_initialMenu))
                CloseMenus();
            else
                OpenMenu(_initialMenu);
        }

        IEnumerator SwitchCanvasGroupAnimated(CanvasGroup group, Turn turn, float duration = .1f)
        {
            if (group == null) 
                yield break;

            float targetAlpha = (turn == Turn.on) ? 1 : 0;
            float startAlpha = group.alpha;
            float time = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
            while (time < duration)
            {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            group.alpha = targetAlpha;
            if (turn == Turn.on)
            {
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }
    }
}