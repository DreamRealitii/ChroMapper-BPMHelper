using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private float numberOfBeats = 4;

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

        private async void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex != EditorSceneBuildIndex)
                return;

            mapEditorUI = null;
            bpmChangeGridContainer = null;
            audioTimeSyncController = null;
            await FindGameObjects();
            ui.AddMenu(mapEditorUI);
        }

        // Places 1000bpm at the cursor
        public void AddInitialBPM()
        {
            Debug.Log($"Adding 1000BPM event to cursor at beat {audioTimeSyncController.CurrentJsonTime}");
            BaseBpmEvent bpmEvent = new BaseBpmEvent(audioTimeSyncController.CurrentJsonTime, 1000f);
            bpmChangeGridContainer.SpawnObject(bpmEvent);
            bpmChangeGridContainer.RefreshModifiedBeat();
        }

        // Changes the closest bpm behind the cursor to (60 * numberOfBeats / (cursor time - last bpm time))
        // Spawns error dialog box and returns false if there is no bpm event behind cursor
        public bool AddFinalBPM()
        {
            Debug.Log("Updating previous BPM event");
            float currentBeats = audioTimeSyncController.CurrentJsonTime;
            BaseBpmEvent bpmEvent = GetClosestBehindBPMEvent(currentBeats);
            if (bpmEvent == null)
            {
                Debug.Log("No BPM events found behind cursor");
                SpawnErrorBox("No BPM events found behind cursor");
                return false;
            }

            Debug.Log($"Previous BPM event found at beat {bpmEvent.JsonTime}");
            float newBPM = bpmEvent.Bpm * numberOfBeats / (currentBeats - bpmEvent.JsonTime);
            Debug.Log($"Setting its BPM to {newBPM:N2}");
            bpmEvent.Bpm = newBPM;
            bpmChangeGridContainer.RefreshModifiedBeat();
            Debug.Log($"Moving cursor to {bpmEvent.JsonTime + numberOfBeats:N2}");
            audioTimeSyncController.MoveToJsonTime(bpmEvent.JsonTime + numberOfBeats);
            return true;
        }

        // Does AddFinalBPM and AddInitialBPM together
        public void AddMiddleBPM()
        {
            if (AddFinalBPM())
                AddInitialBPM();
        }

        public void AdjustBPM()
        {
            Debug.Log("Adjusting BPM events");
            
            float currentBeats = audioTimeSyncController.CurrentJsonTime;
            if (CursorOnBPMEvent(currentBeats)) {
                Debug.Log("No reason to adjust if cursor is already on top of a BPM change");
                SpawnErrorBox("No reason to adjust if cursor is already on top of a BPM change");
                return;
            }

            BaseBpmEvent[] bpmEvents = GetFourClosestBPMEvents(currentBeats, out int closestBeatIndex);
            if (bpmEvents == null) {
                Debug.Log("Need at least one BPM event behind cursor and two in map to adjust");
                SpawnErrorBox("Need at least one BPM event behind cursor and two in map to adjust");
                return;
            }
            for (int i = 0; i < 4; i++)
                if (bpmEvents[i] != null)
                    Debug.Log($"BPM event found at {bpmEvents[i].JsonTime:N2}");
            Debug.Log($"Closest BPM event determined to be the one at {bpmEvents[closestBeatIndex].JsonTime:N2}");

            BaseBpmEvent backBpm = null, movedBpm = null, frontBpm = null;
            if (bpmEvents[0] != null && bpmEvents[1] != null && bpmEvents[2] == null && bpmEvents[3] == null)
            {
                Debug.Log("Adjusting second bpm behind and replacing first one");
                backBpm = bpmEvents[0];
                movedBpm = bpmEvents[1];
            }
            if ((bpmEvents[0] == null && bpmEvents[1] != null && bpmEvents[2] != null && bpmEvents[3] == null) ||
                (bpmEvents[0] != null && bpmEvents[1] != null && bpmEvents[2] != null && bpmEvents[3] == null && closestBeatIndex == 2))
            {
                Debug.Log("Adjusting first bpm behind and replacing first one ahead");
                backBpm = bpmEvents[1];
                movedBpm = bpmEvents[2];
            }
            if (bpmEvents[0] != null && bpmEvents[1] != null && bpmEvents[2] != null && closestBeatIndex == 1)
            {
                Debug.Log("Adjusting two bpms behind and replacing first one behind/first one ahead");
                backBpm = bpmEvents[0];
                movedBpm = bpmEvents[1];
                frontBpm = bpmEvents[2];
            }
            if (bpmEvents[1] != null && bpmEvents[2] != null && bpmEvents[3] != null && closestBeatIndex == 2)
            {
                Debug.Log("Adjusting first bpm behind/first one ahead and replacing two ahead");
                backBpm = bpmEvents[1];
                movedBpm = bpmEvents[2];
                frontBpm = bpmEvents[3];
            }

            float currentSeconds = audioTimeSyncController.CurrentSeconds;

            if (frontBpm != null)
            {
                float laterBeatDifference = frontBpm.JsonTime - movedBpm.JsonTime;
                float frontBpmSeconds = audioTimeSyncController.GetSecondsFromBeat(JsonTimeToGlobalBPMTime(frontBpm.JsonTime));
                Debug.Log($"currentSeconds: {currentSeconds:N2}, frontBpmSeconds: {frontBpmSeconds:N2}");
                bpmChangeGridContainer.DeleteObject(frontBpm);

                movedBpm.Bpm = 60f * laterBeatDifference / (frontBpmSeconds - currentSeconds);
                Debug.Log($"Setting BPM event at {movedBpm.JsonTime:N2} to {movedBpm.Bpm:N2}");
                bpmChangeGridContainer.RefreshModifiedBeat();

                bpmChangeGridContainer.SpawnObject(frontBpm);
                bpmChangeGridContainer.RefreshModifiedBeat();
            }

            float beatDifference = movedBpm.JsonTime - backBpm.JsonTime;
            float backBpmSeconds = audioTimeSyncController.GetSecondsFromBeat(JsonTimeToGlobalBPMTime(backBpm.JsonTime));
            bpmChangeGridContainer.DeleteObject(movedBpm);

            backBpm.Bpm = 60f * beatDifference / (currentSeconds - backBpmSeconds);
            Debug.Log($"Setting BPM event at {backBpm.JsonTime:N2} to {backBpm.Bpm:N2}");
            bpmChangeGridContainer.RefreshModifiedBeat();

            bpmChangeGridContainer.SpawnObject(movedBpm);
            bpmChangeGridContainer.RefreshModifiedBeat();

            Debug.Log($"Moving cursor back to {currentSeconds:N2} seconds");
            audioTimeSyncController.MoveToTimeInSeconds(currentSeconds);
        }

        public void UpdateNumberOfBeats(string newBeatString)
        {
            if (float.TryParse(newBeatString, out float newBeats) && newBeats > 0)
                numberOfBeats = newBeats;
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

        private bool CursorOnBPMEvent(float currentBeats)
        {
            return bpmChangeGridContainer.MapObjects
                .Where(b => Math.Abs(b.JsonTime - currentBeats) < 0.001)
                .Count() > 0;
        }

        private BaseBpmEvent GetClosestBehindBPMEvent(float currentBeats)
        {
            IEnumerable<BaseBpmEvent> bpmEvents = bpmChangeGridContainer.MapObjects
                .Where(b => b.JsonTime < currentBeats).OrderBy(b => b.JsonTime).Reverse();
            if (bpmEvents.Count() == 0) return null;
            return bpmEvents.First();
        }

        private float JsonTimeToGlobalBPMTime(float jsonTime) {
            return BeatSaberSongContainer.Instance.Map.JsonTimeToSongBpmTime(jsonTime).Value;
        }

        // Gets the two closest BPM events behind the cursor, and the two closest in front of the cursor, in chronological order.
        // Also outputs the index of the BPM event closest to the cursor.
        // Returns null if there are no BPM events behind cursor, or less than two BPM events total.
        private BaseBpmEvent[] GetFourClosestBPMEvents(float currentBeats, out int closestBeatIndex)
        {
            closestBeatIndex = 0;
            IEnumerable<BaseBpmEvent> allBpmEvents = bpmChangeGridContainer.MapObjects;
            if (allBpmEvents.Count() < 2) return null;

            List<BaseBpmEvent> behindBpmEvents = allBpmEvents
                .Where(b => b.JsonTime < currentBeats).OrderBy(b => b.JsonTime).Reverse().ToList();
            if (behindBpmEvents.Count == 0) return null;
            List<BaseBpmEvent> frontBpmEvents = allBpmEvents
                .Where(b => b.JsonTime > currentBeats).OrderBy(b => b.JsonTime).ToList();
            BaseBpmEvent[] result = new BaseBpmEvent[4];
            for (int i = 0; i < 4; i++)
                result[i] = null;

            if (behindBpmEvents.Count > 1) result[0] = behindBpmEvents[1];
            if (behindBpmEvents.Count > 0) result[1] = behindBpmEvents[0];
            if (frontBpmEvents.Count > 0) result[2] = frontBpmEvents[0];
            if (frontBpmEvents.Count > 1) result[3] = frontBpmEvents[1];

            float minBeatDifference = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                if (result[i] != null) {
                    float beatDifference = Math.Abs(result[i].JsonTime - currentBeats);
                    if (beatDifference < minBeatDifference) {
                        minBeatDifference = beatDifference;
                        closestBeatIndex = i;
                    }
                }
            }

            return result;
        }

        private void SpawnErrorBox(string message)
        {
            DialogBox db = PersistentUI.Instance.CreateNewDialogBox().WithTitle("BPM Helper Error");
            TextComponent text = db.AddComponent<TextComponent>().WithInitialValue(message);
            db.AddFooterButton(() => {}, "Close");
            db.Open();
        }
    }
}
