using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BPMHelper
{
    [Plugin("BPM Helper")]
    public class BPMHelper
    {
        private const int EditorSceneBuildIndex = 3;
        private const int PollIntervalMilliseconds = 1000; // poll interval
        private float numberOfBeats = 1;

        private UI ui = null;
        private MapEditorUI mapEditorUI = null;
        private BPMChangeGridContainer bpmChangeGridContainer = null;
        private AudioTimeSyncController audioTimeSyncController = null;

        [Init]
        private void Init()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            ui = new UI(this);
            Debug.Log("BPM Helper has loaded!");
        }

        private async void SceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.buildIndex != EditorSceneBuildIndex)
                return;

            await FindGameObjects();
        }

        // Places 1000bpm at the cursor
        public void AddInitialBPM()
        {
            float currentTime = audioTimeSyncController.CurrentJsonTime;
            BaseBpmEvent bpmEvent = new BaseBpmEvent(currentTime, 1000f);
            bpmChangeGridContainer.SpawnObject(bpmEvent);
            bpmChangeGridContainer.RefreshModifiedBeat();
        }

        // Changes the closest bpm behind the cursor to (60 * numberOfBeats / (cursor time - last bpm time))
        // Spawns error dialog box if there is no bpm event behind cursor
        public void AddFinalBPM()
        {
            float currentTime = audioTimeSyncController.CurrentJsonTime;
            BaseBpmEvent bpmEvent = GetClosestBehindBPMEvent(currentTime);
            if (bpmEvent == null) {
                SpawnErrorBox("No BPM events found behind cursor");
                return;
            }

            float newBPM = 60 * numberOfBeats / (currentTime - bpmEvent.JsonTime);
            bpmEvent.JsonTime = newBPM;
            bpmChangeGridContainer.RefreshModifiedBeat();
        }

        // Does AddFinalBPM and AddInitialBPM together
        public void AddMiddleBPM()
        {
            AddFinalBPM();
            AddInitialBPM();
        }

        public void UpdateNumberOfBeats(string newBeats)
        {
            numberOfBeats = float.Parse(newBeats);
        }

        private async Task FindGameObjects()
        {
            while (mapEditorUI == null || bpmChangeGridContainer == null || audioTimeSyncController == null)
            { 
                await Task.Delay(PollIntervalMilliseconds);
                mapEditorUI = mapEditorUI ?? UnityEngine.Object.FindFirstObjectByType<MapEditorUI>();
                bpmChangeGridContainer = bpmChangeGridContainer ?? UnityEngine.Object.FindFirstObjectByType<BPMChangeGridContainer>();
                audioTimeSyncController = audioTimeSyncController ?? UnityEngine.Object.FindFirstObjectByType<AudioTimeSyncController>();
            }
        }

        private BaseBpmEvent GetClosestBehindBPMEvent(float currentTime)
        {
            IEnumerable<BaseBpmEvent> bpmEvents = bpmChangeGridContainer.MapObjects
                .Where(b => b.JsonTime < currentTime).OrderBy(b => b.JsonTime).Reverse();
            if (bpmEvents.Count() == 0) return null;
            return bpmEvents.First();
        }

        private void SpawnErrorBox(string message)
        {
            DialogBox db = PersistentUI.Instance.CreateNewDialogBox();
            TextComponent text = db.AddComponent<TextComponent>().WithInitialValue(message);
            db.AddFooterButton(() => {}, "Close");
        }

        [Exit]
        private void Exit()
        {
            Debug.Log("Application has closed!");
        }
    }
}
