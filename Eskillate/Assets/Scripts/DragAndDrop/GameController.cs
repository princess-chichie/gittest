﻿using UnityEngine;
using Core;
using System.Collections.Generic;
using UnityEngine.UI;

namespace DragAndDrop
{
    public class GameController : MonoBehaviour, IGameController
    {
        private int _correctlyPlacedCounter = 0;
        private MoveableShape[] _shapeArray;
        private List<Level> _levels;
        private int _loadedLevelId;

        // Start is called before the first frame update
        void Start()
        {
            _shapeArray = FindObjectsOfType<MoveableShape>();

            _levels = new List<Level>();

            var highScores = HighScoreHelper.GetHighScores(MiniGameId.DragAndDrop);
            var level1 = new Level()
            {
                MiniGameId = MiniGameId.DragAndDrop,
                LevelId = 1,
                Name = "Level1",
                Description = "This is level 1.",
                HighScore = highScores[1]
            };

            _levels.Add(level1);

            LoadHelper.LoadSceneAdditively(this, "LevelSelectionMenu", FillLevelSelectionMenu);

            // Add menus
            LoadHelper.LoadGenericMenus(this);
        }

        private void FillLevelSelectionMenu()
        {

            var levelSelectionCanvas = GameObject.Find("LevelSelectionCanvas");
            var scrollListContent = levelSelectionCanvas.transform.Find("ScrollList").transform.Find("ScrollListViewport").transform.Find("ScrollListContent");

            foreach (Transform child in scrollListContent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            int levelCountForThisLevelTrio = 0;
            GameObject currentLevelTrio = new GameObject();
            foreach (var level in _levels)
            {
                levelCountForThisLevelTrio = levelCountForThisLevelTrio % 3;
                if (levelCountForThisLevelTrio == 0)
                {
                    currentLevelTrio = Instantiate(Resources.Load("Core\\Prefabs\\LevelSelection\\LevelTrio")) as GameObject;
                    currentLevelTrio.transform.SetParent(scrollListContent.transform, false);
                }
                var levelGO = currentLevelTrio.transform.GetChild(levelCountForThisLevelTrio);
                var scoreGO = levelGO.GetChild(0);
                scoreGO.GetComponent<TMPro.TextMeshProUGUI>().text = level.HighScore.ToString();
                var nameGO = levelGO.GetChild(1);
                nameGO.GetComponent<TMPro.TextMeshProUGUI>().text = level.Name;
                var starsGO = levelGO.GetChild(2);
                var selectButtonGO = levelGO.GetChild(3);
                selectButtonGO.GetComponent<Button>().onClick.AddListener(delegate { OnLevelSelected(level.LevelId); });

                levelCountForThisLevelTrio++;
            }

            while(levelCountForThisLevelTrio != 3)
            {
                GameObject.Destroy(currentLevelTrio.transform.GetChild(levelCountForThisLevelTrio).gameObject);
                levelCountForThisLevelTrio++;
            }

            // Set Y pos to 1 to be at scrolled to the top
            scrollListContent.transform.parent.transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        }

        private void OnLevelSelected(int levelId)
        {
            UnloadLevel();
            LoadLevel(levelId-1);
            GameObject.Find("LevelSelectionCanvas").SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPlacedCorrectly()
        {
            if (++_correctlyPlacedCounter >= _shapeArray.Length)
                LevelCompleted();
        }

        public void LoadLevel(int id)
        {
            _levels[id].Load();
            _loadedLevelId = id;
            // For now, this mini game only has 1 level, so lets just restart it instead of unloading and reloading it
            RestartLevel();
        }

        private void UnloadLevel()
        {
            // Nothing to unload in this minigame
        }

        public void RestartLevel()
        {
            _correctlyPlacedCounter = 0;

            foreach (var moveableShape in _shapeArray)
            {
                moveableShape.Restart();
            }
        }

        void LevelCompleted()
        {
            Debug.Log("Level Completed");
            var levelCompletionGO = GameObject.FindGameObjectWithTag("LevelCompletionMenu");
            levelCompletionGO.GetComponent<LevelCompletionMenu>().OnLevelCompleted(_levels[_loadedLevelId], 100);
        }
    }
}